using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using reusable.utility;

namespace reusable.tests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void test_timer()
        {
            BasicTimer t = new BasicTimer(10000);
            t.StartTimer();
            do{
                Thread.Sleep(100);
                Console.WriteLine($"Time Left: {t.TimeLeft().ToString()}");
            }while(!t.AtEnd());
        }
    }
}
