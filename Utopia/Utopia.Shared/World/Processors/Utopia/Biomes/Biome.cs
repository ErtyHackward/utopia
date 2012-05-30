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
        public virtual void GenerateChunkResources(byte[] chunkData, FastRandom rnd)
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

        public virtual void GenerateChunkLiquidSources(byte[] chunkData, FastRandom rnd)
        {
            ByteChunkCursor cursor = new ByteChunkCursor(chunkData);

            //Generate WaterSource
            for (int i = 0; i < WaterSource.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(1, 15);
                int y = rnd.Next(WaterSource.SpawningHeight.Min, WaterSource.SpawningHeight.Max);
                int z = rnd.Next(1, 15);
                PopulateChunkLiquidSources(WaterSource.CubeId, cursor, x, y, z);
            }

            //Generate LavaSources
            for (int i = 0; i < LavaSource.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(1, 15);
                int y = rnd.Next(LavaSource.SpawningHeight.Min, LavaSource.SpawningHeight.Max);
                int z = rnd.Next(1, 15);
                PopulateChunkLiquidSources(LavaSource.CubeId, cursor, x, y, z);
            }
        }

        public virtual void GenerateChunkLakes(byte[] chunkData, FastRandom rnd)
        {
            //Generate Still Water Lakes

            //Generate Still Lava Lakes
        }

        public virtual void GenerateChunkTrees(byte[] chunkData, ChunkColumnInfo[] columndInfo, FastRandom rnd)
        {
            ByteChunkCursor cursor = new ByteChunkCursor(chunkData);
            int nbrTree = rnd.Next(TreePerChunk.Min, TreePerChunk.Max + 1);
            for (int i = 0; i < nbrTree; i++)
            {
                PopulateChunkWithTree(cursor, columndInfo, rnd);
            }
        }

        public virtual void GenerateChunkItems(byte[] chunkData, FastRandom rnd)
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
        protected void PopulateChunkWithResource(byte cubeId, ByteChunkCursor cursor, int x, int y, int z, int qt, FastRandom rnd)
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
        protected void PopulateChunkLiquidSources(byte cubeId, ByteChunkCursor cursor, int x, int y, int z)
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


        protected virtual void PopulateChunkWithTree(ByteChunkCursor cursor, ChunkColumnInfo[] columndInfo, FastRandom rnd)
        {
            var treeTemplate = TreeTemplates.Templates[rnd.Next(TreeTypeRange.Min, TreeTypeRange.Max + 1)];

            //Get Rnd chunk Location.
            int x = rnd.Next(treeTemplate.Radius - 1, 16 - treeTemplate.Radius + 1);
            int z = rnd.Next(treeTemplate.Radius - 1, 16 - treeTemplate.Radius + 1);
            int y = columndInfo[z * AbstractChunk.ChunkSize.X + x].MaxHeight;

            cursor.SetInternalPosition(x, y, z);
            //No other tree around me ?
            byte trunkRootCube = cursor.Read();

            Vector3I radiusRange = new Vector3I(treeTemplate.Radius / 2.0, 1, treeTemplate.Radius / 2.0);

            if ((trunkRootCube == CubeId.Grass || trunkRootCube == CubeId.Dirt || trunkRootCube == CubeId.Snow || trunkRootCube == CubeId.Sand) &&
                cursor.IsCubePresent(treeTemplate.TrunkCubeId, radiusRange) == false &&
                cursor.IsCubePresent(CubeId.StillWater, radiusRange) == false)
            {
                //Generate the Trunk first
                int trunkSize = rnd.Next(treeTemplate.TrunkSize.Min, treeTemplate.TrunkSize.Max + 1);
                for (int trunkBlock = 0; trunkBlock < trunkSize; trunkBlock++)
                {
                    cursor.Write(treeTemplate.TrunkCubeId);
                    cursor.Move(3);
                }
                //Move Down to the last trunk block
                cursor.Move(4);
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
