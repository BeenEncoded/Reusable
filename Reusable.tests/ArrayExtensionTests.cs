using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using reusable;

namespace reusable.tests
{
    [TestClass]
    public class ArrayExtensionTests
    {
        [TestMethod]
        public void test_append()
        {
            ArrayExtensions.to_string_func<byte?> tostrf = (byte? b) => b?.ToString() ?? "N";
            Nullable<byte>[] b = new Nullable<byte>[10];
            for(int x = 0; x < b.Length; ++x) b[x] = null;

            b.Append((byte)5);
            Assert.IsTrue(b[0] == 5);
            for(int x = 0; x <= (b.Length - 1); ++x) b.Append((byte)7);

            Assert.AreEqual(b.Length, 10);

            for(int x = 0; x < b.Length; ++x) b[x] = null;
            Assert.IsFalse(b.IsFull());
            for(int x = 0; x < b.Length; ++x)
            {
                b.Append((byte?)x);
                Console.WriteLine(b.DisplayString(tostrf));
                Console.WriteLine(b.StringRep());
            }
            Assert.IsTrue(b.IsFull());
        }

        [TestMethod]
        public void test_shifting()
        {
            byte?[] b = new byte?[10];
            for (int x = 0; x < b.Length; ++x) b[x] = 1;
            ArrayExtensions.to_string_func<byte?> ts = (byte? bt) => {
                return bt?.ToString() ?? "N";
            };

            b[3] = 5;
            Console.WriteLine($"\"{b.DisplayString(ts)}\"");

            b.ShiftLeft();
            Console.WriteLine($"\"{b.DisplayString(ts)}\"");

            b.ShiftRight();
            Console.WriteLine($"\"{b.DisplayString(ts)}\"");
        }


    }
}
