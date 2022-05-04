///Extensions for arrays.  Not compatible with Linq.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace reusable
{
    /// <summary>
    /// These extensions to array allow the user to operate on fixed-length arrays
    /// such as those representing memory buffers.
    /// 
    /// Please make sure you remember that these functions work with null-terminated arrays
    /// and are not compatible if you're using Linq.  Don't be a dummy.
    /// </summary>
    public static class ArrayExtensions
    {
        public delegate string to_string_func<T>(T t);

        /// <summary>
        /// This function actually appends the value to the end of the array.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="t">The t</param>
        /// <param name="value">The value</param>
        public static void Append<T>(this T[] t, T value)
        {
            if (t == null) throw new ArgumentException("Null array passed!");
            for (int x = 0; x < t.Length; ++x)
            {
                if (t[x] == null)
                {
                    t[x] = value;
                    break;
                }
                else if (x == (t.Length - 1))
                {
                    Trace.TraceWarning("Appending to null-terminated array -- can not append: array full!");
                }
            }
        }

        /// <summary>
        /// Creates a string representation of all the elements concatenated
        /// against eachother.  ex: [1, 2, 3] -> "123"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string StringRep<T>(this T[] t, to_string_func<T> str = null)
        {
            str ??= (T t) => t.ToString();

            StringBuilder builder = new StringBuilder();
            for(int x = 0; x < t.Length; ++x)
            {
                if(t[x] is not null)
                {
                    builder.Append(str(t[x]));
                }
                else
                {
                    break;
                }
            }
            return builder.ToString();
        }

        public static string DisplayString<T>(this T[] t, to_string_func<T> tostr = null)
        {
            tostr ??= (T t) => t.ToString();

            if(t == null) return "null";
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            for(int x = 0; x < t.Length; ++x)
            {
                builder.Append(tostr(t[x]));
                if(x < (t.Length - 1)) builder.Append(", ");
            }
            builder.Append("]");
            return builder.ToString();
        }

        /// <summary>
        /// Shifts the entire array left N places filling with null.
        /// Please pass a nullable object[] thanks.  :)
        /// </summary>
        /// <typeparam name="T">T type</typeparam>
        /// <param name="t">array fo type T -- must be nullable</param>
        public static void ShiftLeft<T>(this T[] t, in int N = 1)
        {
            if(t.Length == 0) return;
            if(t.Length == 1)
            {
                t[0] = (dynamic)null;
                return;
            }
            for(int ne = 0; ne < N; ++ne)
            {
                for(int x = 1; x < t.Length; ++x)
                {
                    t[x - 1] = t[x];
                    t[x] = (dynamic)null;
                }
            }
        }

        public static void ShiftRight<T>(this T[] t, in int N = 1)
        {
            if(t.Length == 0) return;
            if(t.Length == 1)
            {
                t[0] = (dynamic)null;
                return;
            }

            for(int many = 0; many < N; ++many)
            {
                for(int x = (t.Length - 1); x > 0; --x)
                {
                    t[x] = t[x - 1];
                    t[x - 1] = (dynamic)null;
                }
            }
        }

        public static bool IsFull<T>(this T[] t)
        {
            foreach(T xt in t)
            {
                if(xt == null) return false;
            }
            return true;
        }


    }
}
