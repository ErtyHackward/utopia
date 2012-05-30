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
using Utopia.Shared.Settings;
using S33M3Resources.Structs;

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
            BiomeList[BiomeType.Plain] = new PlainBiome();
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
                    if (temperature > 0.7 && moisture < 0.6) 
                        return BiomeType.Desert;
                    if (moisture < 0.5)
                    {
                        return BiomeType.Grassland;
                    }
                    else
                    {
                        if (temperature > 0.5)
                        {
                            return BiomeType.Forest;
                        }
                        else
                        {
                            return BiomeType.Plain;
                        }
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

        private CubeVein _waterSource = new CubeVein() { CubeId = CubeId.DynamicWater,  VeinPerChunk = 20, SpawningHeight = new RangeB(60, 120) };
        private CubeVein _lavaSource = new CubeVein() { CubeId = CubeId.DynamicLava,  VeinPerChunk = 40, SpawningHeight = new RangeB(2, 70) };

        private RangeI _treePerChunk = new RangeI(0, 0);
        private RangeI _DefaultTreeTypeRange = new RangeI(0,0);
        //Default chunk Vein values
        protected virtual CubeVein SandVein { get { return _sandVein; } }
        protected virtual CubeVein RockVein { get { return _rockVein; } }
        protected virtual CubeVein DirtVein { get { return _dirtVein; } }
        protected virtual CubeVein GravelVein { get { return _gravelVein; } }
        protected virtual CubeVein GoldVein { get { return _goldVein; } }
        protected virtual CubeVein CoalVein { get { return _coalVein; } }
        protected virtual CubeVein MoonStoneVein { get { return _moonStoneVein; } }

        protected virtual CubeVein WaterSource { get { return _waterSource; } }
        protected virtual CubeVein LavaSource { get { return _lavaSource; } }

        protected virtual RangeI TreePerChunk { get { return _treePerChunk; } }
        protected virtual RangeI TreeTypeRange { get { return _DefaultTreeTypeRange; } }
        #endregion

        #region Public Properties
        public abstract byte SurfaceCube { get; }
        public abstract byte UnderSurfaceCube { get; }
        public abstract RangeI UnderSurfaceLayers { get; }
        public abstract byte GroundCube { get; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Will populate the chunk with various resources
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="rnd"></param>
        public static void GenerateChunkResources(ByteChunkCursor cursor, Biome biome, FastRandom rnd)
        {
            //Generate Sand vein
            for (int i = 0; i < biome.SandVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0,16);
                int y = rnd.Next(biome.SandVein.SpawningHeight.Min, biome.SandVein.SpawningHeight.Max);
                int z = rnd.Next(0,16);
                PopulateChunkWithResource(biome.SandVein.CubeId, cursor, x, y, z, biome.SandVein.VeinSize, rnd);
            }

            //Generate RockVein vein
            for (int i = 0; i < biome.RockVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(biome.RockVein.SpawningHeight.Min, biome.RockVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(biome.RockVein.CubeId, cursor, x, y, z, biome.RockVein.VeinSize, rnd);
            }

            //Generate DirtVein vein
            for (int i = 0; i < biome.DirtVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(biome.DirtVein.SpawningHeight.Min, biome.DirtVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(biome.DirtVein.CubeId, cursor, x, y, z, biome.DirtVein.VeinSize, rnd);
            }

            //Generate GravelVein vein
            for (int i = 0; i < biome.GravelVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(biome.GravelVein.SpawningHeight.Min, biome.GravelVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(biome.GravelVein.CubeId, cursor, x, y, z, biome.GravelVein.VeinSize, rnd);
            }

            //Generate GoldVein vein
            for (int i = 0; i < biome.GoldVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(biome.GoldVein.SpawningHeight.Min, biome.GoldVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(biome.GoldVein.CubeId, cursor, x, y, z, biome.GoldVein.VeinSize, rnd);
            }

            //Generate CoalVein vein
            for (int i = 0; i < biome.CoalVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(biome.CoalVein.SpawningHeight.Min, biome.CoalVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(biome.CoalVein.CubeId, cursor, x, y, z, biome.CoalVein.VeinSize, rnd);
            }

            //Generate MoonStoneVein vein
            for (int i = 0; i < biome.MoonStoneVein.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(0, 16);
                int y = rnd.Next(biome.MoonStoneVein.SpawningHeight.Min, biome.MoonStoneVein.SpawningHeight.Max);
                int z = rnd.Next(0, 16);
                PopulateChunkWithResource(biome.MoonStoneVein.CubeId, cursor, x, y, z, biome.MoonStoneVein.VeinSize, rnd);
            }
        }

        public static void GenerateChunkLiquidSources(ByteChunkCursor cursor, Biome biome, FastRandom rnd)
        {
            //Generate WaterSource
            for (int i = 0; i < biome.WaterSource.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(1, 15);
                int y = rnd.Next(biome.WaterSource.SpawningHeight.Min, biome.WaterSource.SpawningHeight.Max);
                int z = rnd.Next(1, 15);
                PopulateChunkLiquidSources(biome.WaterSource.CubeId, cursor, x, y, z);
            }

            //Generate LavaSources
            for (int i = 0; i < biome.LavaSource.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(1, 15);
                int y = rnd.Next(biome.LavaSource.SpawningHeight.Min, biome.LavaSource.SpawningHeight.Max);
                int z = rnd.Next(1, 15);
                PopulateChunkLiquidSources(biome.LavaSource.CubeId, cursor, x, y, z);
            }
        }

        public static void GenerateChunkLakes(ByteChunkCursor cursor, Biome biome, FastRandom rnd)
        {
            //Generate Still Water Lakes

            //Generate Still Lava Lakes
        }

        public static void GenerateChunkTrees(ByteChunkCursor cursor, ChunkColumnInfo[] columndInfo, Biome biome, FastRandom rnd)
        {
            int nbrTree = rnd.Next(biome.TreePerChunk.Min, biome.TreePerChunk.Max + 1);
            for (int i = 0; i < nbrTree; i++)
            {
                PopulateChunkWithTree(cursor, columndInfo, biome, rnd);
            }
        }

        public static void GenerateChunkItems(ByteChunkCursor cursor, Biome biome, FastRandom rnd)
        {
            //Generate grass, ...
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Will create a resource vein
        /// </summary>
        /// <param name="cubeId">The resource to be created</param>
        /// <param name="cursor">Class helper to move inside the Chunk cube data</param>
        /// <param name="x">InsideChunk X starting position</param>
        /// <param name="y">InsideChunk Y starting position</param>
        /// <param name="z">InsideChunk Z starting position</param>
        /// <param name="qt">Vein size</param>
        /// <param name="rnd">Random generator for vein creation</param>
        protected static void PopulateChunkWithResource(byte cubeId, ByteChunkCursor cursor, int x, int y, int z, int qt, FastRandom rnd)
        {
            cursor.SetInternalPosition(x, y, z);
            int nbrCubePlaced;
            if (cursor.Read() == CubeId.Stone)
            {
                cursor.Write(cubeId);
                nbrCubePlaced = 1;
                for (int i = 0; i < qt + 10 && nbrCubePlaced < qt; i++)
                {
                    int relativeMove = rnd.Next(1, 7);
                    cursor.Move(relativeMove);
                    if (cursor.Read() == CubeId.Stone)
                    {
                        cursor.Write(cubeId);
                        nbrCubePlaced++;
                    }
                }
            }
        }

        /// <summary>
        /// Will create a single Liquid "Source"
        /// </summary>
        /// <param name="cubeId">the liquid CubeId</param>
        /// <param name="cursor">Class helper to move inside the chunk</param>
        /// <param name="x">InsideChunk X starting position</param>
        /// <param name="y">InsideChunk Y starting position</param>
        /// <param name="z">InsideChunk Z starting position</param>
        protected static void PopulateChunkLiquidSources(byte cubeId, ByteChunkCursor cursor, int x, int y, int z)
        {
            cursor.SetInternalPosition(x, y, z);

            //Check if this source is candidate as valid source = Must be surrended by 5 solid blocks and ahave one side block going to Air
            if (cursor.Read() != CubeId.Air)
            {
                //Looking Up for Air
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(4)].IsSolidToEntity == false) return;
                //Looking Down for Air
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(3)].IsSolidToEntity == false) return;
                int cpt = 0;
                //Counting the number of holes arround the source
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(1)].IsSolidToEntity == false) cpt++;
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(2)].IsSolidToEntity == false) cpt++;
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(5)].IsSolidToEntity == false) cpt++;
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(6)].IsSolidToEntity == false) cpt++;

                //Only one face touching air ==> Createing the Liquid Source !
                if (cpt == 1)
                {
                    cursor.Write(cubeId);
                }
            }
        }


        protected static void PopulateChunkWithTree(ByteChunkCursor cursor, ChunkColumnInfo[] columndInfo, Biome biome, FastRandom rnd)
        {
            var treeTemplate = TreeTemplates.Templates[rnd.Next(biome.TreeTypeRange.Min, biome.TreeTypeRange.Max + 1)];

            //Get Rnd chunk Location.
            int x = rnd.Next(treeTemplate.Radius - 1, 16 - treeTemplate.Radius + 1);
            int z = rnd.Next(treeTemplate.Radius - 1, 16 - treeTemplate.Radius + 1);
            int y = columndInfo[z * AbstractChunk.ChunkSize.X + x].MaxHeight;

            cursor.SetInternalPosition(x, y, z);
            //No other tree around me ?
            byte trunkRootCube = cursor.Read();

            Vector3I radiusRange = new Vector3I(treeTemplate.Radius - 1, 1, treeTemplate.Radius - 1);

            if ((trunkRootCube == CubeId.Grass || trunkRootCube == CubeId.Dirt || trunkRootCube == CubeId.Snow || trunkRootCube == CubeId.Sand) &&
                cursor.IsCubePresent(treeTemplate.TrunkCubeId, radiusRange) == false &&
                cursor.IsCubePresent(CubeId.StillWater, radiusRange) == false)
            {
                //Generate the Trunk first
                int trunkSize = rnd.Next(treeTemplate.TrunkSize.Min, treeTemplate.TrunkSize.Max + 1);
                for (int trunkBlock = 0; trunkBlock < trunkSize; trunkBlock++)
                {
                    cursor.Write(treeTemplate.TrunkCubeId);
                    cursor.Move(CursorRelativeMovement.Up);
                }
                //Move Down to the last trunk block
                cursor.Move(CursorRelativeMovement.Down);
                //Add Foliage
                foreach (int foliageMove in treeTemplate.FoliageStructure)
                {
                    cursor.Move(foliageMove);
                    if (foliageMove >= 0 && cursor.Read() == CubeId.Air) cursor.Write(treeTemplate.FoliageCubeId);
                }
            }
        }
        #endregion
    }
}
