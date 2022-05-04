/**
 *  Directory iterators to centralize all iteration code.
 *  Copyright (C) 2019  Jonathan Whitlock
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;
using static reusable.fs.FTest;
using static reusable.fs.PathOp;
using reusable.utility;

namespace reusable.fs
{
    internal static class common
    {
        public static string exception_display(in Exception e)
        {
            return ("Message: " + e.Message + "\r\n" +
                    "Source: " + e.Source + "\r\n" +
                    "HRESULT: " + e.HResult + "\r\n" +
                    "Help: " + e.HelpLink + "\r\n\r\n" +
                    "Stack Trace: " + e.StackTrace);
        }
    }

    public static class DirectoryIterator
    {
        /// <summary>
        /// All the filesystem operations involved in recursively copying a folder
        /// or even just copying a single file can result in a plethora of errors
        /// from unautorized access, to file being currently in use by another program, 
        /// or it simply just doesn't exist for some reason.  Whenever an
        /// unrecoverable error happens in the middle of such an operation,
        /// a copy_iterator_result containing information about the error is passed up the stack
        /// to be returned to the user.
        /// 
        /// All operations within these algorithms should be
        /// exception free, and any errors that occur should be handled at the discretion of the
        /// user.  To this end copy_iterator_result is provided.
        /// </summary>
        public struct copy_iterator_result
        {
            public copy_iterator_result(in copy_iterator_result d)
            {
                src = new string(d.src);
                dest = new string(d.dest);
                success = d.success;
                e = new Exception(d.e?.Message, d.e?.InnerException);
            }

            public override string ToString()
            {
                string temps = (success ? "Success: " : "Failure: ");
                if (!success && (e != null)) temps += common.exception_display(e);
                return temps;
            }

            public string src, dest;
            public bool success;
            public Exception e;
        }

        public delegate bool copy_predicate(in string src, in string dest);
        public delegate bool iterator_predicate(in string path);

        /// <summary>
        /// Iterates an entire directory tree.  Made to simplify complicated
        /// filesystem operations by removing this burden from the programmer.
        /// </summary>
        /// <param name="dir">The directory to iterate.</param>
        /// <param name="do_return">A predicate that can be used to skip certain directories or
        /// paths.  Return true to iterate over it, false to skip.</param>
        /// <returns cref="IEnumerable">The path as a string.</returns>
        public static IEnumerable iterator(DirectoryInfo dir, iterator_predicate do_return)
        {
            /* DON'T CHANGE THIS!
               I know it's memory-inefficient to use recursion, but there are
            serious problems with error handling if you use enumeration.*/
            if(dir.Exists)
            {
                string[] directories = null, files = null;
                try
                {
                    files = Directory.GetFiles(dir.FullName);
                }
                catch(UnauthorizedAccessException)
                {
                }

                if(files != null)
                {
                    foreach(string file in files)
                    {
                        if(do_return(file)) yield return file;
                    }
                }

                try
                {
                    directories = Directory.GetDirectories(dir.FullName);
                }
                catch(UnauthorizedAccessException)
                {
                }

                if(directories != null)
                {
                    foreach(string d in directories)
                    {
                        if (do_return(d)) yield return d;
                        var info = new_directoryinfo(d, out Exception e);
                        if((e == null) && FTest.is_folder(d))
                        {
                            foreach(string s in iterator(info, do_return))
                            {
                                if (do_return(d)) yield return s;
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable iterator(DirectoryInfo dir)
        {
            foreach(string s in DirectoryIterator.iterator(dir, (in string p) => {return true;}))
            {
                yield return s;
            }
        }

        /// <summary>
        /// An iterator that iterates only over directories.  This is a more efficient method than
        /// using a predicate-iterator as it skips iterating over irrelevant paths entirely.
        /// </summary>
        /// <param name="dir">The folder to iterate over.</param>
        /// <returns>Each fully qualified path within the directory that is a folder.</returns>
        public static IEnumerable dirs_only_iterator(DirectoryInfo dir)
        {
            DirectoryInfo[] dirs = null;

            //iterate the directory tree first:
            try
            {
                dirs = dir.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {//do we care?
            }
            catch (SecurityException)
            {
            }
            if (dirs != null)
            {
                foreach (DirectoryInfo directory in dirs)
                {
                    yield return directory.FullName;
                    foreach(string result in DirectoryIterator.dirs_only_iterator(directory))
                    {
                        yield return result;
                    }
                }
            }
        }

        /// <summary>
        /// An iterator that iterates only over files.  This is a more efficient method than
        /// using a predicate-iterator as it skips irrelevant paths.
        /// </summary>
        /// <param name="dir">The folder to iterate over.</param>
        /// <returns>Each fully qualified path within the directory that is a file.</returns>
        public static IEnumerable files_only_iterator(DirectoryInfo dir)
        {
            FileInfo[] files = null;
            DirectoryInfo[] dirs = null;

            try
            {
                files = dir.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
            }
            if (files != null)
            {
                foreach(FileInfo file in files) yield return file.FullName;
            }

            try
            {
                dirs = dir.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {//do we care?
            }
            catch (SecurityException)
            {
            }

            // for some reason dirs is null somtimes...
            dirs ??= new DirectoryInfo[0]; // if it's null, garuntee it has a value

            foreach(DirectoryInfo d in dirs)
            {
                foreach(string s in DirectoryIterator.files_only_iterator(d)) yield return s;
            }
        }

        /// <summary>
        /// Copies an entire folder.
        /// </summary>
        /// <param name="src">The source directory.  A fully qualified path name.</param>
        /// <param name="dest">The destination directory.  A fully qualified path name.</param>
        /// <param name="predicate">A predicate functor.  If this function call returns
        /// false, the path currently being operated on will not be copied.  This can be used to filter
        /// paths to copy.</param>
        /// <param name="newname">The new name of the folder at the destination.  This can be used to
        /// change the dirname of the folder at the destination so there aren't any name collisions
        /// in the destination.</param>
        /// <param name="dry">If set to true no filesystem-modifying calls will be made.</param>
        /// <returns>A copy_iterator_result containing the results of the operation.</returns>
        public static IEnumerable copy_iterator(
            DirectoryInfo  src, 
            DirectoryInfo  dest, 
            copy_predicate predicate, 
            string         newname = null, 
            bool           dry = false)
        {
            //Set up the destination.  The destination should be the the name of the src
            //folder, but concatonated to the destination path.
            if(src.Root.FullName == src.FullName)
            {
                //so if the src is an entire fucking drive... we'll just use the drive letter
                //for the destination folder since semicolons are not valid fsobject names.
                dest = new DirectoryInfo(concat_path(dest.FullName, char.ToString(src.FullName[0])));
            }
            else if(newname == null)
            {
                dest = new DirectoryInfo(concat_path(dest.FullName, src.Name));
            }

            //and if we specify newname, then none of that matters... we will just use
            //the new foldername:
            else if(newname != null)
            {
                dest = new DirectoryInfo(concat_path(dest.FullName, newname));
            }
            
            //first make the destination directory... copied exactly:
            yield return FilesystemOperation.exact_copyto(src, dest, dry);

            bool execute_copy = false;
            Exception tempe = null;

            //this is the meat of the algorithm.
            //We start by copying the directory tree:
            foreach(string path in DirectoryIterator.iterator(src))
            {
                tempe = null;
                try
                {
                    execute_copy = predicate(in path, 
                        concat_path(dest.FullName, split_path(src.FullName, path).second));
                }
                catch(Exception e)
                {
                    tempe = e;
                }

                if(tempe == null)
                {
                    if(execute_copy) yield return sub_copypath(src, dest, path, dry);
                }
                else
                {
                    copy_iterator_result r = new copy_iterator_result();
                    r.src = src.FullName;
                    r.dest = dest.FullName;
                    r.success = false;
                    r.e = tempe;
                    yield return r;
                }
            }
        }

        public static IEnumerable copy_iterator(
            DirectoryInfo src, 
            DirectoryInfo dest, 
            string        newname = null, 
            bool          dry = false)
        {
            foreach(copy_iterator_result res in copy_iterator(
                src, 
                dest, 
                (in string s, in string d)=>{
                    return true;
                },
                newname,
                dry))
            {
                yield return res;
            }
        }

        public static IEnumerable copy_iterator(
            string src, 
            string dest, 
            string newname = null, 
            bool   dry = false)
        {
            DirectoryInfo source = new DirectoryInfo(src), destination = new DirectoryInfo(dest);
            foreach (copy_iterator_result res in copy_iterator(
                source, 
                destination, 
                (in string s, in string d) => {
                    return true;
                },
                newname,
                dry))
            {
                yield return res;
            }
        }

        /// <summary>
        /// Copies a path from one folder into another.  It is assumed the path exists.
        /// </summary>
        /// <param name="src">The source directory.</param>
        /// <param name="dest">The destination directory.  The operation will copy the target
        /// into this one.</param>
        /// <param name="path">The path.  It must be a child path of the src.</param>
        /// <returns>The result of the operation.</returns>
        public static copy_iterator_result sub_copypath(
            in DirectoryInfo src, 
            in DirectoryInfo dest, 
            in string path,
            in bool dry)
        {
            copy_iterator_result result = new copy_iterator_result();
            result.src = src.FullName;
            result.dest = dest.FullName;
            result.success = false;

            if(is_child(src.FullName, path))
            {
                //if the parent of the destination path does not currently exist, don't execute the copy_parents
                if(!FTest.exists(PathOp.parent(PathOp.concat_path(dest.FullName, split_path(src.FullName, path).second))))
                {
                    FilesystemOperation.copy_parents(
                        src.FullName, 
                        dest.FullName, 
                        split_path(src.FullName, path).second,
                        dry);
                }

                if (is_file(path)) return sub_copyfile(src, dest, path, dry);
                else if (is_folder(path)) return sub_copydirectory(src, dest, path, dry);
                result.e = new FileNotFoundException("Path is niether a file or a folder: \"" + path + "\"");
                return result;
            }

            result.e = new Exception("@sub_copypath(DirectoryInfo, DirectoryInfo, string): The path is not a child of it's given parent.  \nParent = \"" + src.FullName + 
                "\"\nChild = \"" + path + "\"");
            return result;
        }
        
        private static copy_iterator_result sub_copyfile(in DirectoryInfo src, in DirectoryInfo dest, in string path, in bool dry)
        {
            copy_iterator_result result = new copy_iterator_result();
            FileInfo newdest = null;

            result.src = src.FullName;
            result.dest = dest.FullName;
            result.success = false;

            {
                newdest = new_fileinfo(
                    concat_path(dest.FullName, split_path(src.FullName, path).second), 
                    out Exception e);
                if(newdest == null)
                {
                    result.e = e;
                    return result;
                }
            }// e dies here.  rest in peace, e.

            if(FTest.exists(path))
            {
                try
                {
                    if(FTest.exists(newdest.FullName)) 
                    {
                        if(!dry) newdest.Delete();
                    }
                }
                catch(Exception e)
                {
                    result.e = e;
                    return result;
                }

                {
                    FileInfo newpath = new_fileinfo(path, out Exception e);
                    if(newpath != null)
                    {
                        result = FilesystemOperation.exact_copyto(newpath, newdest, dry);
                        return result;
                    }
                    result.e = e;
                }
                return result;
            }
            throw new FileNotFoundException("DirectoryIterator marked \"" +
                path + "\" as a file, but it existence tested negative!");
        }

        private static copy_iterator_result sub_copydirectory(in DirectoryInfo src, in DirectoryInfo dest, in string path, in bool dry)
        {
            copy_iterator_result result = new copy_iterator_result();

            result.src = src.FullName;
            result.dest = dest.FullName;
            result.success = false;

            DirectoryInfo newdest = new_directoryinfo(
                concat_path(dest.FullName, split_path(src.FullName, path).second), 
                out Exception e);
            if(newdest == null)
            {
                result.e = e;
                return result;
            }

            if(FTest.exists(path))
            {
                result = FilesystemOperation.exact_copyto(new DirectoryInfo(path), newdest, dry);
                return result;
            }
            throw new DirectoryNotFoundException("DirectoryIterator marked \"" +
                path + "\" as a folder, but it existence tested negative!");
        }
        
        /// <summary>
        /// Creates a new fileinfo in a way in which any errors generated can be silently ignored if the users wants.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e">The exception generated, if any.  If no exception was generated, then null.</param>
        /// <returns>null if an error occured.</returns>
        public static FileInfo new_fileinfo(in string s, out Exception e)
        {
            FileInfo f = null;
            e = null;
            try
            {
                f = new FileInfo(s);
                return f;
            }
            catch(Exception x)
            {
                e = x;
                return null;
            }
            throw new Exception("well....  shit.");
        }

        /// <summary>
        /// Creates a new directoryinfo in a way in which any errors generated can be silently ignored if the users wants.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e">The exception generated, if any.  If no exception was generated, then null.</param>
        /// <returns>null if an error occured.</returns>
        public static DirectoryInfo new_directoryinfo(in string s, out Exception e)
        {
            e = null;
            DirectoryInfo d = null;
            try
            {
                d = new DirectoryInfo(s);
                return d;
            }
            catch(Exception x)
            {
                e = x;
                return null;
            }
            throw new Exception("Well...  shit.");
        }

        /// <summary>
        /// Checks that one path exists within another.
        /// </summary>
        /// <returns>True if parent is a root of child.</returns>
        private static bool is_child(in string parent, in string child)
        {
            if(child.Length < parent.Length) return false;
            return (child.Substring(0, parent.Length) == parent);
        }


    }

    public static class FilesystemOperation
    {
        /// <summary>
        /// Copies all the parents of a sub-path given its ext relative to the source, 
        /// fully-qualifieds source path, and fully qualified destination path.
        /// </summary>
        /// <param name="source">Fully qualified path representing the source directory or file.</param>
        /// <param name="destination">A fully qualified path representing the destination folder.</param>
        /// <param name="ext">The sub-path as split from its parent source by split_path</param>
        /// <param name="dry">determines whether any system calls are made that modify the filesystem.</param>
        /// <returns>the result of the operation.  If any single copy operation fails, the entire
        /// operation fails.</returns>
        public static DirectoryIterator.copy_iterator_result copy_parents(
            in string source, 
            in string destination, 
            in string ext,
            bool dry = false)
        {
            DirectoryIterator.copy_iterator_result result = new DirectoryIterator.copy_iterator_result();
            result.dest = concat_path(destination, ext);
            result.src = concat_path(source, ext);
            result.e = null;
            result.success = false;

            string newsource = "", newdest = "";

            if(!FTest.exists(PathOp.parent(PathOp.concat_path(destination, ext))))
            {
                foreach(string temps in PathOp.parent_path_iterator(ext))
                {
                    newsource = concat_path(source, temps);
                    newdest = concat_path(destination, temps);
                    if(!FTest.exists(newdest)) result = FilesystemOperation.exact_copyto(newsource, newdest, dry);
                    else result.success = true;
                    if(!result.success) return result;
                }
            }
            else
            {
                result.success = true;
            }
            return result;
        }

        public static DirectoryIterator.copy_iterator_result exact_copyto(in string source, in string dest, in bool dry = false)
        {
            DirectoryIterator.copy_iterator_result result = new DirectoryIterator.copy_iterator_result();
            result.dest = dest;
            result.src = source;
            result.e = null;
            result.success = false;
            if((source == null) || (dest == null))
            {
                result.e = new ArgumentNullException("@concat_path(string, string): source or dest strings is null");
                return result;
            }


            if(FTest.is_file(source))
            {
                FileInfo tempf = DirectoryIterator.new_fileinfo(source, out result.e);
                if(result.e != null) return result;
                FileInfo tempdf = DirectoryIterator.new_fileinfo(dest, out result.e);
                if(result.e != null) return result;
                return FilesystemOperation.exact_copyto(tempf, tempdf, dry);
            }
            else if(FTest.is_folder(source))
            {
                DirectoryInfo tempf = DirectoryIterator.new_directoryinfo(source, out result.e);
                if(result.e != null) return result;
                DirectoryInfo tempdf = DirectoryIterator.new_directoryinfo(dest, out result.e);
                if(result.e != null) return result;
                return FilesystemOperation.exact_copyto(tempf, tempdf, dry);
            }
            result.e = new InvalidOperationException("@exact_copyto(string, string): source does not register as a file or a folder.");
            return result;
        }

        /// <summary>
        /// Copies a file and along with all of its attributes.  The destination
        /// to which the file is being copied should not already exist, and its parent directory
        /// should already exist.
        /// </summary>
        /// <param name="src">The source path. </param>
        /// <param name="dest">The destination file.</param>
        public static  DirectoryIterator.copy_iterator_result exact_copyto(in FileInfo src, in FileInfo dest, in bool dry = false)
        {
            DirectoryIterator.copy_iterator_result result = new DirectoryIterator.copy_iterator_result();
            result.src = src.FullName;
            result.dest = dest.FullName;
            result.success = false;
            result.e = null;

            try
            {
                if(!dry) src.CopyTo(dest.FullName);
                if(FTest.exists(dest.FullName)) result.e = copy_properties(src, dest, dry);
                result.success = (result.e == null) || dry;
            }
            catch (Exception e)
            {
                result.e = e;
            }
            return result;
        }
        
        /// <summary>
        /// Copies a folder to a fully qualified destination.  The destination folder can already exist.  In that
        /// case, only the sources properties will be copied.
        /// </summary>
        /// <param name="src">The source directory.</param>
        /// <param name="dest">The destination directory.</param>
        /// <returns>The result of the operation.</returns>
        public static DirectoryIterator.copy_iterator_result exact_copyto(in DirectoryInfo src, in DirectoryInfo dest, in bool dry = false)
        {
            DirectoryIterator.copy_iterator_result result = new DirectoryIterator.copy_iterator_result();
            result.success = false;
            result.src = src.ToString();
            result.dest = dest.ToString();
            result.e = null;
            try
            {
                if(!dry) dest.Create();
                if(FTest.exists(dest.FullName))
                {
                    result.e = copy_properties(src, dest, dry);
                    result.success = (result.e == null) || dry;
                }
            }
            catch (Exception e)
            {
                result.e = e;
                result.success = false;
            }
            return result;
        }

        public static Exception copy_properties(in DirectoryInfo src, in DirectoryInfo dest, in bool dry = false)
        {
            if(dry) return null;
            try
            {
                Directory.SetCreationTime(dest.FullName, src.CreationTime);
                Directory.SetCreationTimeUtc(dest.FullName, src.CreationTimeUtc);
                //dest.Attributes = src.Attributes;
                Directory.SetLastAccessTime(dest.FullName, src.LastAccessTime);
                Directory.SetLastAccessTimeUtc(dest.FullName, src.LastAccessTimeUtc);
                Directory.SetLastWriteTime(dest.FullName, src.LastWriteTime);
                Directory.SetLastWriteTimeUtc(dest.FullName, src.LastWriteTimeUtc);
            }
            catch(Exception e)
            {
                return e;
            }
            return null;
        }

        public static Exception copy_properties(in FileInfo src, in FileInfo dest, in bool dry = false)
        {
            if(dry) return null;
            try
            {
                dest.CreationTime = src.CreationTime;
                dest.CreationTimeUtc = src.CreationTimeUtc;
                dest.Attributes = src.Attributes;
                dest.LastAccessTime = src.LastAccessTime;
                dest.LastAccessTimeUtc = src.LastAccessTimeUtc;
                dest.LastWriteTime = src.LastWriteTime;
                dest.LastWriteTimeUtc = src.LastWriteTimeUtc;
                dest.IsReadOnly = src.IsReadOnly;
            }
            catch(UnauthorizedAccessException)
            {
                //this happens quite a bit for some reason.
                //If this happens, there is really nothing that we can do.
                //Debug.WriteLine(" [" + typeof(FilesystemOperation).FullName + ".copy_properties(FileInfo, FileInfo)] " + 
                //    e.Message);
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }
        
        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="f">A fully qualified path to a file.</param>
        /// <returns>An exception if there was an error, null if successful.</returns>
        public static Exception delete_file(in string f)
        {
            try
            {
                if(FTest.exists(f)) File.Delete(f);
            }
            catch(Exception e)
            {
                return e;
            }
            return null;
        }
        

    }

    public static class PathOp
    {
        /// <summary>
        /// This function operates on a path.  Given the stem belongs under the root,
        /// the Pair returned will consist of the root, and the second half of the stem under the root.
        /// Used primarily for constructing new destination paths.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="stem"></param>
        /// <returns>A Pair containing the root, and the part of the stem that
        /// is under the root.  The first of the pair is garunteed not to end with a path
        /// separator, and the second of the pair is garunteed not to start with one.</returns>
        public static pair<string, string> split_path(in string root, in string stem)
        {
            if (root.Length == stem.Length) return new pair<string, string>(root, "");
            return new pair<string, string>(
                ((root[root.Length - 1] == Path.DirectorySeparatorChar) ? root.Substring(0, (root.Length - 1)) : root),
                stem.Substring(root.Length + ((root[root.Length - 1] == Path.DirectorySeparatorChar) ? 0 : 1)));
        }

        /// <summary>
        /// Concatonates two paths.  This is meant to centralize concatenation code, 
        /// as well to ensure that path separator standards are followed throughout the codebase.
        /// </summary>
        /// <param name="p1">A fully qualified path.</param>
        /// <param name="p2">Another fully qualified path.</param>
        /// <returns>The two paths concatonated.</returns>
        public static string concat_path(string p1, string p2)
        {
            //like all the rest of my code, this thing chews bubble gun and kicks ass...
            // but it's all outta guns...
            //                                    0_0
            if ((p1 == null) || (p2 == null)) throw new ArgumentNullException("@concat_path: one or both of the arguments is NULL");
            if (p1.Length > 0)
            {
                if (p1[p1.Length - 1] == Path.DirectorySeparatorChar) p1 = p1.Remove((p1.Length - 1), 1);
            }
            if (p2.Length > 0)
            {
                if (p2[0] == Path.DirectorySeparatorChar) p2.Remove(0, 1);
                if (p2.Length > 0)
                {
                    p2 = p2.Replace(':', ' ');
                }
            }
            string temps = (p1 + Path.DirectorySeparatorChar + p2);
            //now we do one last thing: if the path is not a root, it shouldn't have a terminating
            //directory separator character.
            if((temps[(temps.Length - 1)] == Path.DirectorySeparatorChar) && 
                (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? (temps.Length > 3) : (temps.Length > 1)))
            {
                temps = temps.Remove((temps.Length - 1), 1); //pop the sep char
            }
            return temps;
        }

        /// <summary>
        /// Iterates up the directory tree.  Provides the same results as 
        /// calling Directory.GetParent on a path repeatedly, except as a much
        /// more useful iterator.  It actually operates on any string with
        /// directory separators in it, so it can be used in more complex 
        /// path operations such as those involving recursive-parent copy operations.
        /// </summary>
        /// <param name="path">A path, any path.</param>
        /// <returns></returns>
        public static IEnumerable parent_path_iterator(string path)
        {
            string temps = null;
            string[] ancestors = path.Split(Path.DirectorySeparatorChar);
            for (int x = (ancestors.Length - 2); x >= 0; --x)
            {
                temps = "";
                for (uint y = 0; y <= (uint)x; ++y)
                {
                    temps += (ancestors[y] + Path.DirectorySeparatorChar);
                }
                yield return ((x == 0) ? temps : temps.Substring(0, (temps.Length - 1)));
            }
        }

        public static string parent(in string p)
        {
            string[] tempsplit = p.Split(Path.DirectorySeparatorChar);
            if(tempsplit.Length > 1)
            {
                string temps = "";
                for(uint x = 0; x < (tempsplit.Length - 1); ++x)
                {
                    temps += (tempsplit[x] + Path.DirectorySeparatorChar);
                }
                return temps.Remove(temps.Length - 1);
            }
            else if(tempsplit.Length > 0) return (tempsplit[0] + Path.DirectorySeparatorChar);
            return p;
        }


    }

    public static class FTest
    {

        public static bool is_file(in string path)
        {
            if(path.Length == 0) return false;
            if(!exists(path)) return false;
            try
            {
                return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch(FileNotFoundException)
            {
            }
            return false;
        }

        public static bool is_folder(in string path)
        {
            if(path.Length == 0) return false;
            if(!exists(path)) return false;
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory) && 
                    !is_symlink(path);
            }
            catch(ArgumentException)
            {
            }
            catch(UnauthorizedAccessException)
            {
            }
            catch(FileNotFoundException)
            {
            }
            catch (IOException)
            {
            }
            return false;
        }

        public static bool is_symlink(in string path)
        {
            if(path.Length == 0) return false;
            if(!exists(path)) return false;
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
            }
            catch(FileNotFoundException)
            {
            }
            return false;
        }

        public static bool exists(in string path)
        {
            if(path.Length == 0) return false;
            return (Directory.Exists(path) || File.Exists(path));
        }
    }


}