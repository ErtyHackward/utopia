using System.ComponentModel;
using ProtoBuf;
using S33M3Resources.Structs;
using S33M3CoreComponents.Maths;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    [TypeConverter(typeof(BiomeTreesTypeConverter))]
    [ProtoContract]
    public partial class BiomeTrees
    {
        private int _small;
        private int _medium;
        private int _big;
        private int _cactus;
        private TreeTemplates.TreeType[] _treeTypeDistribution = new TreeTemplates.TreeType[100];

        #region Public properties
        [ProtoMember(1)]
        public int Small
        {
            get { return _small; }
            set
            {
                _small = value;
                RefreshTreeTypeDistribution();
            }
        }

        [ProtoMember(2)]
        public int Medium
        {
            get { return _medium; }
            set
            {
                _medium = value;
                RefreshTreeTypeDistribution();
            }
        }

        [ProtoMember(3)]
        public int Big
        {
            get { return _big; }
            set
            {
                _big = value;
                RefreshTreeTypeDistribution();
            }
        }

        [ProtoMember(4)]
        public int Cactus
        {
            get { return _cactus; }
            set
            {
                _cactus = value;
                RefreshTreeTypeDistribution();
            }
        }

        [ProtoMember(5)]
        public RangeI TreePerChunks { get; set; }
        #endregion


        public BiomeTrees()
        {
            _small = 50;
            _medium = 35;
            _big = 15;
            _cactus = 0;
            TreePerChunks = new RangeI(0, 0);
        }

        private void RefreshTreeTypeDistribution()
        {
            //Default tree distribution
            int lowLimit = 0;
            int highLimit = 0;

            GetNextDistributionRange(_small, highLimit, out lowLimit, out highLimit);
            for (int i = lowLimit; i < highLimit; i++) _treeTypeDistribution[i] = TreeTemplates.TreeType.Small;
            
            GetNextDistributionRange(_medium, highLimit, out lowLimit, out highLimit);
            for (int i = lowLimit; i < highLimit; i++) _treeTypeDistribution[i] = TreeTemplates.TreeType.Medium;

            GetNextDistributionRange(_big, highLimit, out lowLimit, out highLimit);
            for (int i = lowLimit; i < highLimit; i++) _treeTypeDistribution[i] = TreeTemplates.TreeType.Big;

            GetNextDistributionRange(_cactus, highLimit, out lowLimit, out highLimit);
            for (int i = lowLimit; i < highLimit; i++) _treeTypeDistribution[i] = TreeTemplates.TreeType.Cactus;
        }

        private void GetNextDistributionRange(int percent, int previousHight, out int lowLimit, out int highLimit)
        {
            lowLimit = previousHight;
            highLimit = previousHight + percent;
            if (highLimit >= 100) highLimit = 99;
        }

        public TreeTemplates.TreeType GetNextTreeType(FastRandom rnd)
        {
            return _treeTypeDistribution[rnd.Next(0, 100)];
        }
    }
}
