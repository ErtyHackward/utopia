using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utopia.Shared.Structs;

namespace Utopia.Tests
{
    [TestClass]
    public class Md5HashTest
    {
        [TestMethod]
        public void Md5HashCodeTest()
        {
            var set = new HashSet<Md5Hash>();

            for (int i = 0; i < int.MaxValue; i++)
            {
                Assert.IsTrue(set.Add(Md5Hash.Calculate(i.ToString())));
            }
            
        }

    }
}
