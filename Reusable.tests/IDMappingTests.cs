using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using reusable.tests.data;

using reusable.data;

namespace reusable.tests
{
    /// <summary>
    /// Tests for the IDMapping Data Structure -- including file io, etc.
    /// </summary>
    [TestClass]
    public class IDMappingTests
    {
        [TestMethod]
        public void test_read_write()
        {
            Encoding encoding = Encoding.UTF8;
            id_mapping<string> dirmapping = RandomTestData.random_directory_mapping(),
                read_data;

            read_data = TestAlgo.read_write(dirmapping, encoding);

            Assert.IsTrue(read_data == dirmapping);
        }

        [TestMethod]
        public void keys_test()
        {
            var mapping = RandomTestData.random_directory_mapping();
            mapping.update(RandomTestData.random_folders("/", 10));
        }




    }
}
