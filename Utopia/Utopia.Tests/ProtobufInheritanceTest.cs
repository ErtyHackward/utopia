using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using ProtoBuf.Meta;
using SharpDX;

namespace Utopia.Tests
{
    [TestClass]
    public class ProtobufInheritanceTest
    {
        [TestMethod]
        public void ProtoInheritanceTest()
        {
            var model = TypeModel.Create();

            var baseType = model.Add(typeof(Base), true);
            var subType1 = model.Add(typeof(Level1), true);
            var subType2 = model.Add(typeof(Level1_1), true);
            var subsubType1 = model.Add(typeof(Level2), true);

            baseType.AddSubType(100, typeof(Level1));
            baseType.AddSubType(101, typeof(Level1_1));
            subType1.AddSubType(100, typeof(Level2));

            var quaternion = model.Add(typeof(Quaternion), true);
            quaternion.AddField(1, "X");
            quaternion.AddField(2, "Y");
            quaternion.AddField(3, "Z");
            quaternion.AddField(4, "W");

            var inst = new Level2() { 
                BaseString = "test11", 
                BaseInt = 42,
                Level1Int = 54, 
                Level1String = "level1str",
                Level2String = "stringlvl2",
                Time = DateTime.Now,
                Rotation = new Quaternion(1f, 2f, 3f, 4f),
            };

            inst.Bases.Add(new Base() { BaseInt = 23, BaseString = "listed base"});
            inst.Bases.Add(new Level1_1() { Level2Int = 144 });

            var ms = new MemoryStream();

            model.Serialize(ms, inst);

            ms.Position = 0;
            
            var result = model.Deserialize(ms, null, typeof(Level2));

            Trace.WriteLine(result.ToString());

        }
    }

    [ProtoContract]
    internal class Base
    {
        [ProtoMember(1)]
        public string BaseString { get; set; }

        [ProtoMember(2)]
        public int BaseInt { get; set; }
    }

    [ProtoContract]
    internal class Level1 : Base
    {
        [ProtoMember(1)]
        public int Level1Int { get; set; }

        [ProtoMember(2)]
        public string Level1String { get; set; }

        [ProtoMember(3)]
        public Quaternion Rotation { get; set; }
    }

    [ProtoContract]
    internal class Level1_1 : Base
    {
        [ProtoMember(1)]
        public int Level2Int { get; set; }
    }

    [ProtoContract]
    internal class Level2 : Level1
    {
        public Level2()
        {
            Bases = new List<Base>();
        }

        [ProtoMember(1)]
        public string Level2String { get; set; }

        [ProtoMember(2)]
        public DateTime Time { get; set; }

        [ProtoMember(3)]
        public List<Base> Bases { get; set; }
    }
}

