using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Cubes;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;
using Utopia.Shared.World.Processors.Utopia.LandformFct;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public abstract class Biome
    {
        #region Static Attributes
        //Static Class components
        public static readonly Biome[] BiomeList;

        static Biome()
        {
            BiomeList = new Biome[BiomeType.BiomeTypesCollection.Values.Count];
            //Init Biomes Type
            BiomeList[BiomeType.Snow] = new SnowBiome();
            BiomeList[BiomeType.Grassland] = new GrasslandBiome();
            BiomeList[BiomeType.Desert] = new DesertBiome();
            BiomeList[BiomeType.Forest] = new ForestBiome();
            BiomeList[BiomeType.Ocean] = new OceanBiome();
            BiomeList[BiomeType.Montain] = new MontainBiome();
        }

        public static byte GetBiome(double landFormType, double temperature, double humidity)
        {
            enuLandFormType landForm = (enuLandFormType)landFormType;

            switch (landForm)
            {
                case enuLandFormType.Flat:
                    return BiomeType.Desert;
                case enuLandFormType.Hill:
                    return BiomeType.Forest;
                case enuLandFormType.Plain:
                    return BiomeType.Grassland;
                case enuLandFormType.Montain:
                    return BiomeType.Montain;
                case enuLandFormType.Midland:
                    return BiomeType.Snow;
                case enuLandFormType.Ocean:
                    return BiomeType.Grassland;
                default:
                    return BiomeType.Grassland;
            }
        }

        #endregion

        #region Private Variables
        protected RangeI _underSurfaceLayers = new RangeI(1, 3);
        #endregion

        #region Public Properties
        public abstract byte SurfaceCube { get; }
        public abstract byte UnderSurfaceCube { get; }
        public abstract RangeI UnderSurfaceLayers { get; }
        public abstract byte GroundCube { get; }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
