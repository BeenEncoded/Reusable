using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using System.Diagnostics;

using reusable.utility;
using reusable.fs;
using reusable.tests.data;

namespace reusable.tests
{
    [TestClass]
    public class filesystem_tests
    {
        delegate pair<string, string> splitpath_del(string root, string stem);

        [TestMethod]
        public void parent_path_test()
        {
            Console.WriteLine("Current Directory: \"" + Environment.CurrentDirectory + "\"");
            foreach (string s in PathOp.parent_path_iterator(Environment.CurrentDirectory))
            {
                Console.WriteLine(new string(' ', 20) + s);
            }
        }

        [TestMethod]
        public void iterate_directory_test()
        {
            string path = Path.GetPathRoot(Environment.CurrentDirectory);
            Assert.IsTrue(FTest.is_folder(path));
            uint x = 0;
            foreach (string s in DirectoryIterator.iterator(new DirectoryInfo(path)))
            {
                ++x;
            }
            Console.WriteLine("Number of paths: " + x);
        }

        [TestMethod]
        public void create_directory_test()
        {
            DirectoryInfo current_path = new DirectoryInfo(Environment.CurrentDirectory +
                Path.DirectorySeparatorChar + $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}" + 
                $"create_folder_test{Path.DirectorySeparatorChar}blag{Path.DirectorySeparatorChar}" + 
                $"ragtag{Path.DirectorySeparatorChar}private_folder"),
                folder = new DirectoryInfo(Environment.CurrentDirectory + Path.DirectorySeparatorChar +
                    "unit_test_folder");

            string testfolder = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}create_folder_test";
            if (FTest.exists(in testfolder))
            {
                Directory.Delete(testfolder, true);
            }
            bool exists = FTest.exists(current_path.FullName);

            Console.WriteLine("Test Folder: \"" + current_path.FullName + "\"");

            current_path.Create();
            Thread.Sleep(1000);
            Assert.IsTrue(exists != FTest.exists(current_path.FullName));
        }

        [TestMethod]
        public void path_combine_test()
        {
            DirectoryInfo current_path = new DirectoryInfo(Environment.CurrentDirectory);
            string[] blags = current_path.FullName.Split(new char[] { '\\' });
            Console.WriteLine("Blags: ");
            foreach (string s in blags) Console.WriteLine("    \"" + s + "\"");
            Console.WriteLine("current_path = \"" + current_path.FullName);
            Console.WriteLine("Combined path: \"" + Path.Combine(blags) + "\"");
            Console.WriteLine("Path Separator: \'" + Path.DirectorySeparatorChar + "\'");
        }

        [TestMethod]
        public void split_path_test()
        {
            splitpath_del split_path = (string root, string stem) => {
                if (root.Length == stem.Length) return new pair<string, string>(root, "");
                return new pair<string, string>(
                    ((root[root.Length - 1] == Path.DirectorySeparatorChar) ? root.Substring(0, (root.Length - 1)) : root),
                    stem.Substring(root.Length + ((root[root.Length - 1] == Path.DirectorySeparatorChar) ? 0 : 1)));
            };

            DirectoryInfo current_path = new DirectoryInfo(Environment.CurrentDirectory);
            pair<string, string> result = new pair<string, string>();

            foreach (string s in RandomTestData.parent_path_iterator(current_path.FullName))
            {
                result = split_path(s, current_path.FullName);
                //Console.WriteLine("Current Directory: \"" + current_path.FullName + "\"");
                //Console.WriteLine("Parent Path:        \"" + s + "\"");
                //Console.WriteLine("Split path first, second: ");
                //Console.WriteLine("                   \"" +
                //    split_path(s, current_path.FullName).first + "\"");
                //Console.WriteLine("                   \"" +
                //    split_path(s, current_path.FullName).second + "\"");
                Assert.AreEqual(
                    result.first +
                    ((s.Length == current_path.FullName.Length) ? "" : (Path.DirectorySeparatorChar + result.second)),
                    current_path.FullName);
                if (s == current_path.Root.FullName) Assert.AreEqual(s, (result.first + Path.DirectorySeparatorChar));
                else Assert.AreEqual(s, result.first);
            }
        }

        /// <summary>
        /// Tests whether calling .Root on a path will still retrieve the actual
        /// root when the path the getter is called on is actually the root itself.
        /// </summary>
        [TestMethod]
        public void rootPathTest()
        {
            var d = new DirectoryInfo(Directory.GetCurrentDirectory()).Root;
            Console.WriteLine("Current Directory:  \"" + d + "\"");
            Console.WriteLine("Root: " + d.Root);
            Assert.IsTrue(d.FullName == d.Root.FullName);
        }

        [TestMethod]
        public void childPathTest()
        {
            DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory()), ancestor = d.Parent.Parent.Parent;
            Console.WriteLine("d = \"" + d.FullName + "\"");
            Console.WriteLine("ancestor = \"" + ancestor.FullName + "\"");
            string temps1 = d.FullName, temps2 = ancestor.FullName;
            Assert.IsTrue(temps2 == temps1.Substring(0, temps2.Length));
        }

        [TestMethod]
        public void dry_run_copy_test()
        {
            string src = PathOp.concat_path(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget"),
                    dest = PathOp.concat_path(Environment.CurrentDirectory, "test_destination");

            Debug.WriteLine($"Source Directory: {src}");
            Debug.WriteLine($"Destination Directory: {dest}");

            foreach(DirectoryIterator.copy_iterator_result result in DirectoryIterator.copy_iterator(src, dest, null, true))
            {
                Debug.WriteLine($"{result.src}  ->  {result.dest}");
            }
            Assert.IsFalse(FTest.exists(dest));
        }

        //[Fact]
        //public void copy_iterator_test()
        //{
        //    string source = TestGlobals.current_directory + "\\random_project",
        //        dest = TestGlobals.current_directory + "\\backup_target";
        //    Assert.IsTrue(Directory.Exists(source) && Directory.Exists(dest));
        //    List<string> paths = new List<string>();

        //    { //delete the destination if contents are left over from a previous run...
        //        string[] names = source.Split(Path.DirectorySeparatorChar);
        //        DirectoryInfo newdest = new DirectoryInfo(PathOp.concat_path(dest, names[names.Length - 1]));
        //        if (FTest.exists(newdest.FullName))
        //        {
        //            try
        //            {
        //                newdest.Delete(true);
        //            }
        //            catch
        //            {
        //            }
        //        }
        //        if (FTest.exists(PathOp.concat_path(dest, names[names.Length - 1])))
        //        {
        //            Console.WriteLine("Destination path exists!");
        //        }
        //    } //names and newdest die here... rest in pieces...

        //    foreach (DirectoryIterator.copy_iterator_result result in DirectoryIterator.copy_iterator(source, dest))
        //    {
        //        if (result.success)
        //        {
        //            Console.WriteLine("copied " + result.src);
        //            paths.Add(result.dest);
        //        }
        //        else Console.WriteLine("Failed: " + result.e.Message);
        //    }
        //    foreach (string s in paths)
        //    {
        //        Assert.IsTrue(FTest.exists(s));
        //    }
        //}

        //[TestMethod]
        //public void iterate_directory_test()
        //{
        //    DirectoryIterator.iterator_predicate pred = (string p) =>
        //    {
        //        return (FTest.exists(p) && FTest.is_folder(p));
        //    };
        //    foreach (string s in
        //        DirectoryIterator.iterator(
        //            new DirectoryInfo("D:\\documents\\CSharp\\Current_projects\\fsbackup"),
        //            pred))
        //    {
        //        Assert.IsTrue(FTest.is_folder(s));
        //    }
        //}

    }
}
