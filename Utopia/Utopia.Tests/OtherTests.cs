using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utopia.Shared;

namespace Utopia.Tests
{
    [TestClass]
    public class OtherTests
    {
        [TestMethod]
        public void CheckCountAtLeast()
        {
            var list = new List<int>();

            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            Assert.IsTrue(list.CountAtLeast(0));
            Assert.IsTrue(list.CountAtLeast(10));
            Assert.IsTrue(list.CountAtLeast(100));
            Assert.IsFalse(list.CountAtLeast(110));
        }
    }
}
