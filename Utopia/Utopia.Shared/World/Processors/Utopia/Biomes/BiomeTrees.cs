using System.ComponentModel;
using ProtoBuf;
using S33M3Resources.Structs;
using S33M3CoreComponents.Maths;
using System.Collections.Generic;
using Utopia.Shared.LandscapeEntities.Trees;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [TypeConverter(typeof(BiomeTreesTypeConverter))]
    [ProtoContract]
    public partial class BiomeTrees
    {
        #region Public properties
        [ProtoMember(6, OverwriteList=true)]
        public List<BiomeTree> Trees { get; set; }

        [ProtoMember(7)]
        public double ChanceOfSpawning { get; set; }

        [ProtoMember(5)]
        public RangeI TreePerChunks { get; set; }
        #endregion


        public BiomeTrees()
        {
            TreePerChunks = new RangeI(0, 0);
            Trees = new List<BiomeTree>();
        }

    }
}
