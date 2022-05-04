using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

using reusable.cloud;
using reusable.utility;

namespace unit_tests
{
    /**
     * <summary>Tests the cloud api code.</summary>
     */
    [TestClass]
    public class CloudTest
    {
        [TestMethod]
        [Ignore]
        public void test_cloud_connection()
        {
            /** If you're looking at this and thinking "oh you fool!  You foolish fool!  
                Why aren't you using secrets?!?  Let me try loging in..."
                
                Think again.  You can't touch dis! */
            ICloud cloud_interface = new NextCloud(
                new CloudSettings()
                .setLogin("test", "erkbherfkbjerf")
                .setServer("https://192.168.1.200/nextcloud/"));
            cloud_interface.connect();
        }
    }
}
