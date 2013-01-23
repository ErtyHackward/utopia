using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.LandscapeEntities.Trees
{
    public struct TreeTemplate
    {
        public string Name { get; set; }
        public string Axiom { get; set; }
        public LSystemRule Rules_a { get; set; }
        public LSystemRule Rules_b { get; set; }
        public LSystemRule Rules_c { get; set; }
        public LSystemRule Rules_d { get; set; }
        public byte TrunkBlock { get; set; }
        public byte FoliageBlock { get; set; }
        public double Angle { get; set; }
        public int Iteration { get; set; }
        public int IterationRndLevel { get; set; }
        public int RandomeLevel { get; set; }
        public TrunkType TrunkType { get; set; }
        public bool SmallBranches { get; set; }
        public int FoliageGenerationStart { get; set; }
    }

    public struct LSystemRule
    {
        public string Rule { get; set; }
        public float Prob { get; set; }
    }

    public enum TrunkType
    {
        Single,
        Double,
        Crossed
    }
}
