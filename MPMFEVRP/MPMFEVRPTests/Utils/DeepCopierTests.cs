using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Utils.Tests
{
    [TestClass()]
    public class DeepCopierTests
    {
        [TestMethod()]
        public void CopyTest()
        {
            int a = 3;
            Assert.AreEqual(2, a,"a degiskenine baktim");
        }

        [TestMethod()]
        [ExpectedException(typeof(DivideByZeroException),"sifira bolduk hata oldu")]
        public void DivideByZeroTest()
        {
            int b = 0;
            int a = 1 / b;
        }
    }

}