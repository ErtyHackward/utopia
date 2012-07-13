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
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Entities.Interfaces;

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

        //Default mineral vein resources configurations
        private CubeVein _sandVein = new CubeVein() { CubeId = CubeId.Sand, VeinSize = 12, VeinPerChunk = 8, SpawningHeight = new RangeB(40, 128) };
        private CubeVein _rockVein = new CubeVein() { CubeId = CubeId.Rock, VeinSize = 8, VeinPerChunk = 8, SpawningHeight = new RangeB(1, 50)};
        private CubeVein _dirtVein = new CubeVein() { CubeId = CubeId.Dirt, VeinSize = 12, VeinPerChunk = 16, SpawningHeight = new RangeB(1, 128) };
        private CubeVein _gravelVein = new CubeVein() { CubeId = CubeId.Gravel, VeinSize = 16, VeinPerChunk = 5, SpawningHeight = new RangeB(40, 128) };
        private CubeVein _goldVein = new CubeVein() { CubeId = CubeId.GoldOre, VeinSize = 8, VeinPerChunk = 5, SpawningHeight = new RangeB(1, 40)};
        private CubeVein _coalVein = new CubeVein() { CubeId = CubeId.CoalOre, VeinSize = 16, VeinPerChunk = 16, SpawningHeight = new RangeB(1, 80) };
        private CubeVein _moonStoneVein = new CubeVein() { CubeId = CubeId.MoonStone, VeinSize = 4, VeinPerChunk = 3, SpawningHeight = new RangeB(1, 20) };
        //Default Liquid block spawning resource
        private CubeVein _waterSource = new CubeVein() { CubeId = CubeId.DynamicWater,  VeinPerChunk = 20, SpawningHeight = new RangeB(60, 120) };
        private CubeVein _lavaSource = new CubeVein() { CubeId = CubeId.DynamicLava,  VeinPerChunk = 40, SpawningHeight = new RangeB(2, 60) };
        //Default Spawning lake
        private Cavern _moonStoneCavern = new Cavern() { CubeId = CubeId.MoonStone, CavernHeightSize = new RangeB(4, 7) ,CavernPerChunk = 1, SpawningHeight = new RangeB(20, 60), ChanceOfSpawning = 0.01 };

        private RangeI _treePerChunk = new RangeI(0, 0);
        protected int[] _treeTypeDistribution = new int[100];
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

        protected virtual Cavern MoonStoneCavern { get { return _moonStoneCavern; } }

        protected virtual BiomeEntity GrassEntities { get { return BiomeEntity.None; } }
        protected virtual BiomeEntity Flower1Entities { get { return BiomeEntity.None; } }
        protected virtual BiomeEntity Flower2Entities { get { return BiomeEntity.None; } }
        protected virtual BiomeEntity Flower3Entities { get { return BiomeEntity.None; } }

        protected virtual RangeI TreePerChunk { get { return _treePerChunk; } }
        protected int[] TreeTypeDistribution { get { return _treeTypeDistribution; } }
        #endregion

        #region Public Properties
        public abstract byte SurfaceCube { get; }
        public abstract byte UnderSurfaceCube { get; }
        public abstract RangeI UnderSurfaceLayers { get; }
        public abstract byte GroundCube { get; }
        #endregion

        public Biome()
        {
            CreateTreeDistribution();
        }

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
            int liquidPower = 5;
            //Generate WaterSource
            //for (int i = 0; i < biome.WaterSource.VeinPerChunk; i++)
            //{
            //    //Get Rnd chunk Location.
            //    int x = rnd.Next(liquidPower, 16 - liquidPower);
            //    int y = rnd.Next(biome.WaterSource.SpawningHeight.Min, biome.WaterSource.SpawningHeight.Max);
            //    int z = rnd.Next(liquidPower, 16 - liquidPower);
            //    PopulateChunkLiquidSources(biome.WaterSource.CubeId, cursor, x, y, z, liquidPower);
            //}

            liquidPower = 5;
            //Generate LavaSources
            for (int i = 0; i < biome.LavaSource.VeinPerChunk; i++)
            {
                //Get Rnd chunk Location.
                int x = rnd.Next(liquidPower, 16 - liquidPower);
                int y = rnd.Next(biome.LavaSource.SpawningHeight.Min, biome.LavaSource.SpawningHeight.Max);
                int z = rnd.Next(liquidPower, 16 - liquidPower);
                PopulateChunkLiquidSources(biome.LavaSource.CubeId, cursor, x, y, z, liquidPower);
            }
        }

        public static void GenerateMoonStoneCavern(ByteChunkCursor cursor, Biome biome, FastRandom rnd)
        {
            //Generate MoonStoneCavern
            for (int i = 0; i < biome.MoonStoneCavern.CavernPerChunk; i++)
            {
                if (rnd.NextDouble() <= biome.MoonStoneCavern.ChanceOfSpawning)
                {
                    //Get Rnd chunk Location.
                    int x = rnd.Next(7, 9);
                    int y = rnd.Next(biome.MoonStoneCavern.SpawningHeight.Min, biome.MoonStoneCavern.SpawningHeight.Max);
                    int z = rnd.Next(7, 9);
                    int layer = rnd.Next(biome.MoonStoneCavern.CavernHeightSize.Min, biome.MoonStoneCavern.CavernHeightSize.Max + 1);
                    PopulateChunkWithCave(cursor, x, y, z, layer, biome, biome.MoonStoneCavern.CubeId, rnd);
                }
            }
        }

        public static void GenerateChunkTrees(ByteChunkCursor cursor, ChunkColumnInfo[] columndInfo, Biome biome, FastRandom rnd)
        {
            int nbrTree = rnd.Next(biome.TreePerChunk.Min, biome.TreePerChunk.Max + 1);
            for (int i = 0; i < nbrTree; i++)
            {
                PopulateChunkWithTree(cursor, columndInfo, biome, rnd);
            }
        }

        public static void GenerateChunkItems(ByteChunkCursor cursor, GeneratedChunk chunk, ref Vector3D chunkWorldPosition, ChunkColumnInfo[] columndInfo, Biome biome, FastRandom rnd, EntityFactory entityFactory)
        {
            //Grass population
            for (int i = 0; i < biome.GrassEntities.EntityPerChunk; i++)
            {
                if (rnd.NextDouble() <= biome.GrassEntities.ChanceOfSpawning)
                {
                    //Get Rnd chunk Location.
                    int x = rnd.Next(0, 16);
                    int z = rnd.Next(0, 16);
                    int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxHeight;

                    PopulateChunkWithItems(cursor, chunk, ref chunkWorldPosition, biome.GrassEntities.EntityId, x, y, z, rnd, entityFactory, false, false);
                }
            }

            ////Flower 1 population
            //for (int i = 0; i < biome.Flower1Entities.EntityPerChunk; i++)
            //{
            //    if (rnd.NextDouble() <= biome.Flower1Entities.ChanceOfSpawning)
            //    {
            //        //Get Rnd chunk Location.
            //        int x = rnd.Next(0, 16);
            //        int z = rnd.Next(0, 16);
            //        int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxHeight;

            //        PopulateChunkWithItems(cursor, chunk, ref chunkWorldPosition, biome.Flower1Entities.EntityId, x, y, z, rnd, entityFactory, false, true);
            //    }
            //}

            ////Flower 2 population
            //for (int i = 0; i < biome.Flower2Entities.EntityPerChunk; i++)
            //{
            //    if (rnd.NextDouble() <= biome.Flower2Entities.ChanceOfSpawning)
            //    {
            //        //Get Rnd chunk Location.
            //        int x = rnd.Next(0, 16);
            //        int z = rnd.Next(0, 16);
            //        int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxHeight;

            //        PopulateChunkWithItems(cursor, chunk, ref chunkWorldPosition, biome.Flower2Entities.EntityId, x, y, z, rnd, entityFactory, false, true);
            //    }
            //}

            ////Flower 3 population
            //for (int i = 0; i < biome.Flower3Entities.EntityPerChunk; i++)
            //{
            //    if (rnd.NextDouble() <= biome.Flower3Entities.ChanceOfSpawning)
            //    {
            //        //Get Rnd chunk Location.
            //        int x = rnd.Next(0, 16);
            //        int z = rnd.Next(0, 16);
            //        int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxHeight;

            //        PopulateChunkWithItems(cursor, chunk, ref chunkWorldPosition, biome.Flower3Entities.EntityId, x, y, z, rnd, entityFactory, false, true);
            //    }
            //}
        }
        #endregion

        #region Private Methods

        private void CreateTreeDistribution()
        {
            //Default tree distribution
            for (int i = 0; i < 50; i++) TreeTypeDistribution[i] = (int)TreeTemplates.TreeType.Small;
            for (int i = 50; i < 85; i++) TreeTypeDistribution[i] = (int)TreeTemplates.TreeType.Medium;
            for (int i = 85; i < 100; i++) TreeTypeDistribution[i] = (int)TreeTemplates.TreeType.Big;  
        }

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
        protected static void PopulateChunkLiquidSources(byte cubeId, ByteChunkCursor cursor, int x, int y, int z, int liquidPower)
        {

            cursor.SetInternalPosition(x, y, z);

            //Check if this source is candidate as valid source = Must be surrended by 5 solid blocks and ahave one side block going to Air
            if (cursor.Read() != CubeId.Air)
            {
                //Looking Up for Air
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(CursorRelativeMovement.Up)].IsBlockingWater == false || cursor.Peek(CursorRelativeMovement.Up) == CubeId.Snow) return;
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(CursorRelativeMovement.Down)].IsBlockingWater == false ) return;
                int cpt = 0;
                //Counting the number of holes arround the source
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(CursorRelativeMovement.East)].IsBlockingWater == false) cpt++;
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(CursorRelativeMovement.West)].IsBlockingWater == false) cpt++;
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(CursorRelativeMovement.North)].IsBlockingWater == false) cpt++;
                if (GameSystemSettings.Current.Settings.CubesProfile[cursor.Peek(CursorRelativeMovement.South)].IsBlockingWater == false) cpt++;

                //Only one face touching air ==> Createing the Liquid Source !
                if (cpt != 1) return;

                cursor.Write(cubeId);
                Queue<Tuple<ByteChunkCursor, int>> sourcesWithPower = new Queue<Tuple<ByteChunkCursor, int>>();
                sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(cursor, liquidPower));
                PropagateLiquidSources(sourcesWithPower, cubeId);
            }
        }

        protected static void PropagateLiquidSources(Queue<Tuple<ByteChunkCursor, int>> sourcesWithPower, byte cubeId)
        {
            Tuple<ByteChunkCursor, int> liquidSource = sourcesWithPower.Dequeue();

            //Can Fall, falling doesn't remove Power at propagation
            bool isFalling = false;
            if (GameSystemSettings.Current.Settings.CubesProfile[liquidSource.Item1.Peek(CursorRelativeMovement.Down)].CubeFamilly == Enums.enuCubeFamilly.Liquid) return;
            while (GameSystemSettings.Current.Settings.CubesProfile[liquidSource.Item1.Peek(CursorRelativeMovement.Down)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == CubeId.Snow)
            {
                liquidSource.Item1.Move(CursorRelativeMovement.Down);
                liquidSource.Item1.Write(cubeId);
                isFalling = true;
            }
            if (isFalling)
            {
                sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), liquidSource.Item2));
            }
            else
            {
                int power = liquidSource.Item2 - 1;
                if (power >= 0)
                {
                    if (GameSystemSettings.Current.Settings.CubesProfile[liquidSource.Item1.Peek(CursorRelativeMovement.East)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.East);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                        liquidSource.Item1.Move(CursorRelativeMovement.West);
                    }

                    if (GameSystemSettings.Current.Settings.CubesProfile[liquidSource.Item1.Peek(CursorRelativeMovement.West)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.West);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                        liquidSource.Item1.Move(CursorRelativeMovement.East);
                    }

                    if (GameSystemSettings.Current.Settings.CubesProfile[liquidSource.Item1.Peek(CursorRelativeMovement.North)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.North);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                        liquidSource.Item1.Move(CursorRelativeMovement.South);
                    }

                    if (GameSystemSettings.Current.Settings.CubesProfile[liquidSource.Item1.Peek(CursorRelativeMovement.South)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.South);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                    }
                }
            }

            while (sourcesWithPower.Count > 0)
            {
                PropagateLiquidSources(sourcesWithPower, cubeId);
            }
        }

        protected static void PopulateChunkWithTree(ByteChunkCursor cursor, ChunkColumnInfo[] columndInfo, Biome biome, FastRandom rnd)
        {
            var treeTemplate = TreeTemplates.Templates[biome.TreeTypeDistribution[rnd.Next(0, 100)]];

            //Get Rnd chunk Location.
            int x = rnd.Next(treeTemplate.Radius - 1, 16 - treeTemplate.Radius + 1);
            int z = rnd.Next(treeTemplate.Radius - 1, 16 - treeTemplate.Radius + 1);
            int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxHeight;

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
                foreach (List<int> treeStructBlock in treeTemplate.FoliageStructure)
                {
                    int foliageStructOffset1 = 0; int foliageStructOffset2 = 0;
                    //Random "move" between each block if blocks nbr is > 1
                    if (treeTemplate.FoliageStructure.Count > 1)
                    {
                        foliageStructOffset1 = rnd.Next(1, 5);
                        cursor.Move(foliageStructOffset1);
                        foliageStructOffset2 = rnd.Next(1, 5);
                        cursor.Move(foliageStructOffset2);
                    }
                    foreach (int foliageMove in treeStructBlock)
                    {
                        cursor.Move(foliageMove);
                        if (foliageMove >= 0 && cursor.Read() == CubeId.Air) cursor.Write(treeTemplate.FoliageCubeId);
                    }
                    //Remove OFfset
                    if (foliageStructOffset1 != 0)
                    {
                        switch (foliageStructOffset1)
                        {
                            case 1:
                                cursor.Move(CursorRelativeMovement.West);
                                break;
                            case 2:
                                cursor.Move(CursorRelativeMovement.East);
                                break;
                            case 3:
                                cursor.Move(CursorRelativeMovement.South);
                                break;
                            case 4:
                                cursor.Move(CursorRelativeMovement.North);
                                break;
                            default:
                                break;
                        }
                        switch (foliageStructOffset2)
                        {
                            case 1:
                                cursor.Move(CursorRelativeMovement.West);
                                break;
                            case 2:
                                cursor.Move(CursorRelativeMovement.East);
                                break;
                            case 3:
                                cursor.Move(CursorRelativeMovement.South);
                                break;
                            case 4:
                                cursor.Move(CursorRelativeMovement.North);
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
        }

        protected static void PopulateChunkWithCave(ByteChunkCursor cursor, int x, int y, int z, int layers ,Biome biome, byte cubeId, FastRandom rnd)
        {
            cursor.SetInternalPosition(x, y, z);

                int caveRadius = rnd.Next(3, 8);

                int layerRadiusModifier = 0;

                for (int l = 0; l < layers; l++)
                {
                    //Generate Lake Layers
                    for (int X = x - (caveRadius - layerRadiusModifier); X <= x + (caveRadius - layerRadiusModifier); X++)
                    {
                        for (int Z = z - (caveRadius - layerRadiusModifier); Z <= z + (caveRadius - layerRadiusModifier); Z++)
                        {
                            //Create "Noise" at Cave border
                            if ((X == x - (caveRadius - layerRadiusModifier) ||
                                 X == x + (caveRadius - layerRadiusModifier) ||
                                 Z == z - (caveRadius - layerRadiusModifier) ||
                                 Z == z + (caveRadius - layerRadiusModifier)) 
                                 && rnd.NextDouble() < 0.2)
                            {
                                continue;
                            }

                            cursor.SetInternalPosition(X, y + l, Z);
                            if (l <= 1 && rnd.NextDouble() < 0.3)
                            {
                                cursor.Write(cubeId);
                            }
                            else
                            {
                                if (l != 0)
                                {
                                    if (l == layers - 1)
                                    {
                                        if (cursor.Read() == CubeId.Stone) cursor.Write(CubeId.LightWhite);
                                    }
                                    else cursor.Write(CubeId.Air);

                                }
                            }
                        }
                    }
                    if (layerRadiusModifier < caveRadius) layerRadiusModifier++;
                }
        }

        protected static void PopulateChunkWithItems(ByteChunkCursor cursor, GeneratedChunk chunk, ref Vector3D chunkWorldPosition, ushort entityId, int x, int y, int z, FastRandom rnd, EntityFactory entityFactory, bool isBlockCentered = true, bool isSpriteVariableSize = false)
        {
            cursor.SetInternalPosition(x, y, z);
            //Check that the block above is "Air"
            if (cursor.Peek(CursorRelativeMovement.Up) != CubeId.Air) return;
            //Check that the block below is "solid"
            byte blockBelow = cursor.Read();
            CubeProfile blockBelowProfile = GameSystemSettings.Current.Settings.CubesProfile[blockBelow];
            if (blockBelowProfile.IsSolidToEntity)
            {
                var entity = entityFactory.CreateEntity(entityId);
                if(entity is IBlockLinkedEntity)
                {
                    Vector3I linkedCubePosition = new Vector3I(chunkWorldPosition.X + x, y, chunkWorldPosition.Z + z);
                    ((IBlockLinkedEntity)entity).LinkedCube = linkedCubePosition;
                }

                double XOffset = 0.5;
                double ZOffset = 0.5;
                if (isBlockCentered == false)
                {
                    XOffset = rnd.NextDouble(0.2, 0.8);
                    ZOffset = rnd.NextDouble(0.2, 0.8);
                }
                if (isSpriteVariableSize)
                {
                    float newSize = (float)rnd.NextDouble(0.4, 0.8);
                    entity.Size = new SharpDX.Vector3(newSize, newSize, newSize);
                }

                entity.Position = new Vector3D(chunkWorldPosition.X + x + XOffset, y + 1, chunkWorldPosition.Z + z + ZOffset);
                chunk.Entities.Add((IStaticEntity)entity);
            }
        }

        #endregion
    }
}
