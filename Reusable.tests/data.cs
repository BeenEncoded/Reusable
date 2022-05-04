using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

using reusable.fs;
using reusable.utility;
using reusable.data;

namespace reusable.tests.data
{
    public static class TestGlobals
    {
        public static readonly Random random = new Random();

        public static readonly string current_directory = PathOp.parent(PathOp.parent(Environment.CurrentDirectory));
    }

    public static class Display
    {
        public static string display_string(this byte[] b)
        {
            StringBuilder temps = new StringBuilder();

            temps.Append("[");
            for (int x = 0; x < b.Length; ++x)
            {
                temps.Append(b[x]);
                if (x < (b.Length - 1)) temps.Append(", ");
            }
            temps.Append("]");
            return temps.ToString();
        }


    }

    public static class TestAlgo
    {
        /** <summary>
         *     Performs a read/write to memory and returns
         *     the read information.
         *  </summary>
         *  <param name="s">The string to write.</param>
         *  <param name="writer">The stream writer to use.</param>
         *  <param name="reader">The stream reader to use.</param>
         *  <param name="stream">The base stream.</param>
         *  <param name="encoding">The encoding to use.  The default is UTF8.</param>
         *  <returns>
         *      The string that was read.  It should be equal
         *      to the parameter passed as the argument.
         *  </returns>
         *  <exception cref="IOException">If anything goes wrong with the streams.</exception>
         */
        public static T read_write<T>(
            in T value,
            in BinaryWriter writer,
            in BinaryReader reader,
            in Stream stream,
            Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            io.write(value, writer, encoding);
            stream.Seek(0, SeekOrigin.Begin);
            io.read(out T read_object, reader, encoding);
            return read_object;
        }

        public static T read_write<T>(in T value, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, encoding))
            using (BinaryReader reader = new BinaryReader(ms, encoding))
            {
                return read_write(value, writer, reader, ms, encoding);
            }
        }

        /// <summary>
        /// Writes a list using the reusable.data.io functions and then reads
        /// it with those same functions and the returns the result.  The reuslt
        /// should be the same as the value passed.
        /// </summary>
        /// <typeparam name="T">the Type of the list.  Restricted to non-null fundamental types or strings.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <param name="writer">the writer to use</param>
        /// <param name="reader">the reader to use.</param>
        /// <param name="stream">the underlying stream.</param>
        /// <param name="encoding">the encoding to use.</param>
        /// <returns></returns>
        public static List<T> read_writelist<T>(
            in List<T> value,
            in BinaryWriter writer,
            in BinaryReader reader,
            in Stream stream,
            Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;

            io.writelist(in value, in writer, encoding);
            stream.Seek(0, SeekOrigin.Begin);
            io.readlist(out List<T> new_read_list, in reader, encoding);
            return new_read_list;
        }

        public static List<T> read_writelist<T>(in List<T> value, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, encoding))
            using (BinaryReader reader = new BinaryReader(ms, encoding))
            {
                return read_writelist(in value, in writer, in reader, ms, encoding);
            }
        }
    }

    public static class RandomTestData
    {
        public delegate IEnumerable iterator_function(DirectoryInfo root);

        public static List<string> random_folders(string root, int max)
        {
            return random_fselements(root, max, DirectoryIterator.dirs_only_iterator);
        }

        public static List<string> random_files(string root, int max)
        {
            return random_fselements(root, max, DirectoryIterator.files_only_iterator);
        }

        private static List<string> random_fselements(string root, int max, iterator_function iter)
        {
            int count = (TestGlobals.random.Next(max) + 1);
            bool chosen = false;
            List<string> folders = new List<string>();
            IEnumerator e = iter(new DirectoryInfo(root)).GetEnumerator();

            for (int x = 0; x < count; ++x)
            {
                while (!chosen)
                {
                    while (e.MoveNext())
                    {
                        // 50/50 chance of being picked:
                        if ((TestGlobals.random.Next() % 2) == 1)
                        {
                            chosen = true;
                            folders.Add((string)e.Current);
                            break;
                        }
                    }
                    if (!chosen) e = iter(new DirectoryInfo(root)).GetEnumerator();
                }
                chosen = false;
            }
            return folders;
        }

        public static char random_char()
        {
            //return CHARS[TestGlobals.random.Next(0, CHARS.Length)];
            return (char)TestGlobals.random.Next(char.MinValue, 0xd800);
        }

        public static int random_int()
        {
            return TestGlobals.random.Next();
        }

        public static bool random_bool()
        {
            return TestGlobals.random.Next() % 2 == 1;
        }

        public static List<string> random_stringlist(int max_len = 50)
        {
            int len = random_int() % max_len;
            List<string> s = new List<string>();
            for (uint x = 0; x < len; ++x)
            {
                s.Add(random_string(10, 100));
            }
            return s;
        }

        public static string random_string(uint min, uint max)
        {
            string temps = "";
            uint size = (uint)TestGlobals.random.Next((int)min, (int)max);
            for (uint x = 0; x < size; ++x) temps += random_char();
            return temps;
        }

        public static IEnumerable parent_path_iterator(string path)
        {
            string temps = null;
            string[] ancestors = path.Split(Path.DirectorySeparatorChar);
            for (int x = (ancestors.Length - 1); x >= 0; --x)
            {
                temps = "";
                for (uint y = 0; y <= (uint)x; ++y)
                {
                    temps += (ancestors[y] + Path.DirectorySeparatorChar);
                }
                yield return ((x == 0) ? temps : temps.Substring(0, (temps.Length - 1)));
            }
        }

        public static id_mapping<string> random_directory_mapping()
        {
            int size = random_int() % 100;
            id_mapping<string> map = new id_mapping<string>();
            for (int x = 0; x < size; ++x) map.add(random_string(10, 50));
            return map;
        }


    }
}