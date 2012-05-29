using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Cubes;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;
using Utopia.Shared.World.Processors.Utopia.LandformFct;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Chunks;

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
            BiomeList[BiomeType.Grassland] = new GrasslandBiome();
            BiomeList[BiomeType.Desert] = new DesertBiome();
            BiomeList[BiomeType.Forest] = new ForestBiome();
            BiomeList[BiomeType.Ocean] = new OceanBiome();
            BiomeList[BiomeType.Montain] = new MontainBiome();
        }

        /// <summary>
        /// Biomes From parameter fct
        /// </summary>
        /// <param name="landFormType">The Landscape Type</param>
        /// <param name="temperature"></param>
        /// <param name="moisture"></param>
        /// <returns></returns>
        public static byte GetBiome(double landFormType, double temperature, double moisture)
        {
            enuLandFormType landForm = (enuLandFormType)landFormType;

            switch (landForm)
            {
                case enuLandFormType.Plain:
                case enuLandFormType.Flat:
                    if (temperature > 0.7 && moisture < 0.6) return BiomeType.Desert;
                    if (moisture < 0.5)
                    {
                        return BiomeType.Grassland;
                    }
                    else
                    {
                        return BiomeType.Forest;
                    }
                case enuLandFormType.Midland:
                case enuLandFormType.Hill:
                    return BiomeType.Grassland;
                case enuLandFormType.Montain:
                    return BiomeType.Montain;
                case enuLandFormType.Ocean:
                    return BiomeType.Ocean;
                default:
                    return BiomeType.Grassland;
            }
        }

        #endregion

        #region Private Variables
        protected RangeI _underSurfaceLayers = new RangeI(1, 3);

        //Default vein resources configurations
        private CubeVein _sandVein = new CubeVein() { CubeId = CubeId.Sand, VeinSize = 12, VeinPerChunk = 8, SpawningHeight = new RangeB(40, 128) };
        private CubeVein _rockVein = new CubeVein() { CubeId = CubeId.Rock, VeinSize = 8, VeinPerChunk = 8, SpawningHeight = new RangeB(1, 50)};
        private CubeVein _dirtVein = new CubeVein() { CubeId = CubeId.Dirt, VeinSize = 12, VeinPerChunk = 16, SpawningHeight = new RangeB(1, 128) };
        private CubeVein _gravelVein = new CubeVein() { CubeId = CubeId.Gravel, VeinSize = 16, VeinPerChunk = 5, SpawningHeight = new RangeB(40, 128) };
        private CubeVein _goldVein = new CubeVein() { CubeId = CubeId.GoldOre, VeinSize = 8, VeinPerChunk = 5, SpawningHeight = new RangeB(1, 40)};
        private CubeVein _coalVein = new CubeVein() { CubeId = CubeId.CoalOre, VeinSize = 16, VeinPerChunk = 16, SpawningHeight = new RangeB(1, 80) };
        private CubeVein _moonStoneVein = new CubeVein() { CubeId = CubeId.MoonStone, VeinSize = 4, VeinPerChunk = 3, SpawningHeight = new RangeB(1, 20) };

        //Default chunk Vein values
        protected virtual CubeVein SandVein { get { return _sandVein; } }
        protected virtual CubeVein RockVein { get { return _rockVein; } }
        protected virtual CubeVein DirtVein { get { return _dirtVein; } }
        protected virtual CubeVein GravelVein { get { return _gravelVein; } }
        protected virtual CubeVein GoldVein { get { return _goldVein; } }
        protected virtual CubeVein CoalVein { get { return _coalVein; } }
        protected virtual CubeVein MoonStoneVein { get { return _moonStoneVein; } }
        #endregion

        #region Public Properties
        public abstract byte SurfaceCube { get; }
        public abstract byte UnderSurfaceCube { get; }
        public abstract RangeI UnderSurfaceLayers { get; }
        public abstract byte GroundCube { get; }
        #endregion

        #region Public Methods
        public void GenerateChunkBlockResource(byte[] chunkData, FastRandom rnd)
        {
            ByteChunkCursor cursor = new ByteChunkCursor(chunkData);

            //Generate Sand vein
            for (int i = 0; i < SandVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0,16);
                int y = rnd.Next(SandVein.SpawningHeight.Min,SandVein.SpawningHeight.Max);
                int z = rnd.Next(0,16);
                PopulateChunkWithResource(SandVein.CubeId, cursor, x, y, z, SandVein.VeinSize, rnd);
            }

            //Generate RockVein vein
            for (int i = 0; i < RockVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(RockVein.SpawningHeight.Min, RockVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(RockVein.CubeId, cursor, x, y, z, RockVein.VeinSize, rnd);
            }

            //Generate DirtVein vein
            for (int i = 0; i < DirtVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(DirtVein.SpawningHeight.Min, DirtVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(DirtVein.CubeId, cursor, x, y, z, DirtVein.VeinSize, rnd);
            }

            //Generate GravelVein vein
            for (int i = 0; i < GravelVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(GravelVein.SpawningHeight.Min, GravelVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(GravelVein.CubeId, cursor, x, y, z, GravelVein.VeinSize, rnd);
            }

            //Generate GoldVein vein
            for (int i = 0; i < GoldVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(GoldVein.SpawningHeight.Min, GoldVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(GoldVein.CubeId, cursor, x, y, z, GoldVein.VeinSize, rnd);
            }

            //Generate CoalVein vein
            for (int i = 0; i < CoalVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(CoalVein.SpawningHeight.Min, CoalVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(CoalVein.CubeId, cursor, x, y, z, CoalVein.VeinSize, rnd);
            }

            //Generate MoonStoneVein vein
            for (int i = 0; i < MoonStoneVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(MoonStoneVein.SpawningHeight.Min, MoonStoneVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(MoonStoneVein.CubeId, cursor, x, y, z, MoonStoneVein.VeinSize, rnd);
            }

        }
        #endregion

        #region Private Methods
        private void PopulateChunkWithResource(byte cubeId, ByteChunkCursor cursor, int x, int y, int z, int qt, FastRandom rnd)
        {
            cursor.SetInternalPosition(x, y, z);
            for (int i = 0; i < qt; i++)
            {
                int relativeMove = rnd.Next(0, 5);
                cursor.Move(relativeMove);
                if (cursor.Read() == CubeId.Stone) 
                    cursor.Write(cubeId);
            }
        }
        #endregion
    }
}
