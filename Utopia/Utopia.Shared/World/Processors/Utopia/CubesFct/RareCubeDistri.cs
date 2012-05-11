using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;
using S33M3CoreComponents.Noise.Fractal;
using Ninject.Activation.Caching;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Noise.Various;
using S33M3CoreComponents.Noise.DomainModifier;
using S33M3CoreComponents.Noise.ResultModifier;
using S33M3CoreComponents.Noise.NoiseResultCombiner;
using S33M3CoreComponents.Noise.ResultCombiner;
using Utopia.Shared.Cubes;
using S33M3CoreComponents.Maths;

namespace Utopia.Shared.World.Processors.Utopia.LandformFct
{
    public class RareCubeDistri : ITerrainGenerator
    {
        #region Private Variables
        private int _seed;
        private INoise _mainLandscape;
        private static byte[] CubesType = new byte[100];
        #endregion

        #region Public Properties
        #endregion

        static RareCubeDistri()
        {
            //Rare type
            for (int i = 0; i < 20; i++) CubesType[i] = CubeId.Minerai1;    //Gold
            for (int i = 20; i < 30; i++) CubesType[i] = CubeId.Minerai4;   //Diamond
            for (int i = 30; i < 50; i++) CubesType[i] = CubeId.Minerai5;   //RedStone
            for (int i = 50; i < 80; i++) CubesType[i] = CubeId.Minerai6;   //MoonStone
            for (int i = 80; i < 100; i++) CubesType[i] = CubeId.Stone;
        }

        public RareCubeDistri(int seed, INoise mainLandscape)
        {
            _seed = seed;
            _mainLandscape = mainLandscape;
        }

        #region Public Methods
        public static byte GetCube(double noiseAmount, double minAmount, double maxAmount)
        {
            int index = MathHelper.Fastfloor(MathHelper.FullLerp(0, 99, minAmount, maxAmount, noiseAmount, true));
            return CubesType[index];
        }

        public INoise GetLandFormFct()
        {
            //Distribution of Semi-Rare VS Common ( = stone)
            INoise RareCube = new FractalHybridMulti(new Simplex(_seed), 4, 6, enuBaseNoiseRange.ZeroToOne);

            INoise biaslandscape = new Bias(_mainLandscape,0.80);
            Combiner RareCubeDistribution = new Combiner(Combiner.CombinerType.Multiply);
            RareCubeDistribution.Noises.Add(RareCube);
            RareCubeDistribution.Noises.Add(biaslandscape);

            return RareCubeDistribution;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
