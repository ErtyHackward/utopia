using System.ComponentModel;
using ProtoBuf;
using S33M3Resources.Structs;
using S33M3CoreComponents.Maths;
using System.Collections.Generic;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.RealmEditor;
using System.Drawing.Design;
using System.Linq;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [TypeConverter(typeof(BiomeTreesTypeConverter))]
    [ProtoContract]
    public partial class BiomeTrees
    {
        #region Private properties
        private bool _distributionDirty = true;
        private int _maxDistribution;
        #endregion

        #region Public properties
        [ProtoMember(6, OverwriteList=true)]
        public List<BiomeTree> Trees { get; set; }

        [ProtoMember(7)]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMaxAttribute(0, 100)]
        [DisplayName("Spanwing Chances"), Description("Chances of spawning in %")]
        public int ChanceOfSpawning { get; set; }

        [ProtoMember(5)]
        public RangeI TreePerChunks { get; set; }
        #endregion

        public BiomeTrees()
        {
            TreePerChunks = new RangeI(0, 0);
            Trees = new List<BiomeTree>();
        }

        public TreeBluePrint GetTreeTemplate(FastRandom rnd, List<TreeBluePrint> templates)
        {
            if (_distributionDirty) CreatedDistributionList();
            //Get a tree following the distribution chances

            int distriRndValue = rnd.Next(0, _maxDistribution);

            for (int i = 0; i < Trees.Count; i++)
            {
                BiomeTree tree = Trees[i];
                if (tree.SpawnDistributionThreshold >= distriRndValue)
                {
                    return templates[tree.LandscapeEntityBluePrintId];
                }
            }
            return null;
        }

        private void CreatedDistributionList()
        {
            int treeDistributionThreshold = 0;
            foreach (var tree in Trees)
            {
                treeDistributionThreshold += tree.SpawnDistribution;
                tree.SpawnDistributionThreshold = treeDistributionThreshold;
            }
            _maxDistribution = treeDistributionThreshold;
            _distributionDirty = false;
        }

    }
}
