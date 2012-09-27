using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utopia.Tests
{
    internal class Foo : ICloneable
    {
        public string FooProp;
        
        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    internal class Bar : Foo
    {
        public int Value;
    }


    [TestClass]
    public class CloneTest
    {
        [TestMethod]
        public void Test()
        {
            var bar = new Bar();

            bar.FooProp = "test";
            bar.Value = 500;

            var barClone = bar.Clone();

            Console.Write(barClone);

        }
    }
}
