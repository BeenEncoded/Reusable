using System.IO;
using System.Text;
using System;

using reusable.fs;
using reusable.utility;

namespace reusable.data
{
    public interface RWObject
    {
        void write(BinaryWriter s, Encoding encoding);
        bool read(BinaryReader s, Encoding encoding);

        string filename { get; } //must be public
    }

    public static class FilesystemObject
    {
        private static DirectoryInfo _root = new DirectoryInfo(Environment.CurrentDirectory);

        /**
         * <summary>Sets the root directory that save/load operate within.</summary>
         * <param name="value">DirectoryInfo to set the new root to.</param>
         */
        public static DirectoryInfo root {set => _root = value;}

        public static void save<type>(type t, string folder = null) where type : RWObject
        {
            if (!_root.Exists) _root.Create();
            string file = string.Empty;
            if(folder == null)
            {
                file = PathOp.concat_path(_root.ToString(), t.filename);
            }
            else
            {
                file = PathOp.concat_path(folder, t.filename);
            }
            using(BinaryWriter stream = new BinaryWriter(
                File.Open(file, FileMode.Create), 
                constant.defaultEncoding))
            {
                t.write(stream, constant.defaultEncoding);
            }
        }

        /// <summary>
        /// Loads an object from its file.
        /// </summary>
        /// <typeparam name="type">The type of object to load.</typeparam>
        /// <param name="t">The object to load.</param>
        /// <param name="folder">A new folder to override root with.</param>
        /// <returns>true if, and only if, data was read into the object.  False otherwise.</returns>
        public static bool load<type>(out type t, string folder = null) where type : RWObject
        {
            t = default(type);
            if (!_root.Exists)
            {
                _root.Create();
                return false;
            }
            string file = string.Empty;
            if(folder == null)
            {
                file = PathOp.concat_path(_root.ToString(), t.filename);
            }
            else
            {
                file = PathOp.concat_path(folder, t.filename);
            }
            if(!File.Exists(file)) return false;
            using(BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open), constant.defaultEncoding))
            {
                t.read(reader, constant.defaultEncoding);
            }
            return true;
        }
    }


}