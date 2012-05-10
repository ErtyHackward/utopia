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
    public class UncommonCubeDistri : ITerrainGenerator
    {
        #region Private Variables
        private int _seed;
        private INoise _mainLandscape;
        private static byte[] CubesType = new byte[100];
        #endregion

        #region Public Properties
        #endregion

        static UncommonCubeDistri()
        {
            //Uncommon type
            for (int i = 0; i < 40; i++) CubesType[i] = CubeId.Gravel;
            for (int i = 40; i < 20; i++) CubesType[i] = CubeId.Minerai2; //Copper
            for (int i = 60; i < 85; i++) CubesType[i] = CubeId.Minerai3; //Coal
            for (int i = 85; i < 100; i++) CubesType[i] = CubeId.Stone;
        }

        public UncommonCubeDistri(int seed, INoise mainLandscape)
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
            INoise UncommonCube = new FractalHybridMulti(new Simplex(_seed), 4, 6, enuBaseNoiseRange.ZeroToOne);
            //INoise semiRareSelect = new Select(0.0, semiRareCube, semiRareCube, 0.55);

            //Combiner cubeDistribution = new Combiner(Combiner.CombinerType.Multiply);
            //cubeDistribution.Noises.Add(semiRareCube);
            //cubeDistribution.Noises.Add(_mainLandscape);

            return UncommonCube;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
