using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utopia.Shared.Chunks.Tags;

namespace Utopia.Tests
{
    [TestClass]
    public class BlockTagEqualityTest
    {
        [TestMethod]
        public void TestEquality()
        {
            var tag1 = new DamageTag();
            var tag2 = new DamageTag();
            
            Assert.AreEqual(tag1, tag2);
            Assert.AreNotSame(tag1, tag2);

            tag1.Strength = 5;

            Assert.AreNotEqual(tag1, tag2);
            Assert.AreNotSame(tag1, tag2);

            var tag3 = tag1.Clone();

            Assert.AreNotSame(tag1, tag3);
            Assert.AreEqual(tag1, tag3);
        }

    }
}
