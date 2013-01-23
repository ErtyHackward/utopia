using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.LandscapeEntities.Trees
{
    public struct TreeTemplate
    {
        public string Axiom { get; set; }
        public string Rules_a { get; set; }
        public string Rules_b { get; set; }
        public string Rules_c { get; set; }
        public string Rules_d { get; set; }
        public byte TrunkBlock { get; set; }
        public byte FoliageBlock { get; set; }
        public double Angle { get; set; }
        public int Iteration { get; set; }
        public int IterationRndLevel { get; set; }
        public int RandomeLevel { get; set; }
        public TrunkType TrunkType { get; set; }
        public bool SmallBranches { get; set; }
    }

    public enum TrunkType
    {
        Single,
        Double,
        Crossed
    }
}
