using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;

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

        [TestMethod]
        public void DeepClonePerformance()
        {
            var item = new PlantGrowingEntity();
            
            EntityFactory.InitializeProtobufInheritanceHierarchy();

            var cloned = item.Clone();
            var protoCloned = Serializer.DeepClone(item);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                cloned = item.Clone();
            }
            sw.Stop();
            Trace.WriteLine("Default clone " + sw.ElapsedMilliseconds);

            sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                cloned = Serializer.DeepClone(item);
            }
            sw.Stop();
            Trace.WriteLine("Protobuf clone " + sw.ElapsedMilliseconds);
        }
    }
}
