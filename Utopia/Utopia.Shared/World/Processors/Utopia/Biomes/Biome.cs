﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using Utopia.Shared.World.Processors.Utopia.LandformFct;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Chunks;
using Utopia.Shared.Settings;
using S33M3Resources.Structs;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using System.IO;
using System.ComponentModel;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class Biome : IBinaryStorable
    {
        #region Satic
        public static byte GetBiome(double landFormType, double temperature, double moisture)
        {
            enuLandFormType landformtype = (enuLandFormType)landFormType;
            for (byte biomeId = 0; biomeId <= RealmConfiguration.Biomes.Count - 1; biomeId++)
            {
                Biome biome = RealmConfiguration.Biomes[biomeId];
                //Does this biome support this land form type ?
                if (biome.LandFormFilters.Contains(landformtype))
                {
                    //Check the temp range
                    if (temperature >= biome.TemperatureFilter.Min &&
                        temperature <= biome.TemperatureFilter.Max)
                    {
                        if (moisture >= biome.MoistureFilter.Min &&
                            moisture <= biome.MoistureFilter.Max)
                        {
                            return biomeId;
                        }
                    }
                }
            }

            //By default return the first Biome from the list
            return (byte)0;
        }
        #endregion

        #region Private Variables
        //Chunk Population elements
        private List<CubeVein> _cubeVeins = new List<CubeVein>();
        private List<BiomeEntity> _biomeEntities = new List<BiomeEntity>();
        private List<Cavern> _caverns = new List<Cavern>();
        private BiomeTrees _biomeTrees = new BiomeTrees();

        //Chunk Composition elements
        private RangeI _underSurfaceLayers = new RangeI(1, 3);

        //Chunk filters needed to have this biome to spawn at a specific World Grid column (for a given X - Z cube world coord)
        //A landFormType
        //A temperature
        //A moisture
        private RangeD _temperatureFilter = new RangeD(0.0, 1.0);
        private RangeD _moistureFilter = new RangeD(0.0, 1.0);
        private List<enuLandFormType> _landFormFilters = new List<enuLandFormType>() { enuLandFormType.Plain };

        #endregion

        #region Public Properties
        [Browsable(false)]
        public byte SurfaceCube { get; set; }

        [Browsable(false)]
        public byte UnderSurfaceCube { get; set; }

        [Browsable(false)]
        public byte GroundCube { get; set; }

        [Category("General")]
        public string Name { get; set; }

        [Description("Under surface layer size"), Category("Composition")]
        public RangeI UnderSurfaceLayers
        {
            get { return _underSurfaceLayers; }
            set { _underSurfaceLayers = value; }
        }

        [Description("Mineral veins spawning configuration"), Category("Population")]
        public List<CubeVein> CubeVeins
        {
            get { return _cubeVeins; }
            set { _cubeVeins = value; }
        }

        [Description("Entities spawning configuration"), Category("Population")]
        public List<BiomeEntity> BiomeEntities
        {
            get { return _biomeEntities; }
            set { _biomeEntities = value; }
        }

        [Description("Cavern spawning configuration"), Category("Population")]
        public List<Cavern> Caverns
        {
            get { return _caverns; }
            set { _caverns = value; }
        }

        [Description("Tree spawning configuration"), Category("Population")]
        public BiomeTrees BiomeTrees
        {
            get { return _biomeTrees; }
            set { _biomeTrees = value; }
        }

        [Description("LandForm falling inside this biome"), Category("Filter")]
        public List<enuLandFormType> LandFormFilters
        {
            get { return _landFormFilters; }
            set { _landFormFilters = value; }
        }

        [Description("Temperature range [0.00 to 1.00] inside this biome"), Category("Filter")]
        public RangeD TemperatureFilter
        {
            get { return _temperatureFilter; }
            set { _temperatureFilter = value; }
        }

        [Description("Moisture range [0.00 to 1.00] inside this biome"), Category("Filter")]
        public RangeD MoistureFilter
        {
            get { return _moistureFilter; }
            set { _moistureFilter = value; }
        }

        #endregion

        public Biome()
        {
        }

        #region Public Methods

        public void GenerateChunkResources(ByteChunkCursor cursor, FastRandom rnd)
        {
            //Create the various Veins in the chunk
            foreach (CubeVein vein in CubeVeins)
            {
                //Generate the vein X time
                for (int i = 0; i < vein.VeinPerChunk; i++)
                {
                    //Get Rnd chunk Location.
                    int x = rnd.Next(0, 16);
                    int y = rnd.Next(vein.SpawningHeight.Min, vein.SpawningHeight.Max);
                    int z = rnd.Next(0, 16);

                    if (vein.CubeProfile.CubeFamilly == Enums.enuCubeFamilly.Liquid)
                    {
                        PopulateChunkWithResource(vein.CubeId, cursor, x, y, z, vein.VeinSize, rnd);
                    }
                    else
                    {
                        //PopulateChunkWithLiquidSources(vein.CubeId, cursor, x, y, z, vein.VeinSize);
                    }
                }
            }
        }

        public void GenerateChunkCaverns(ByteChunkCursor cursor, FastRandom rnd)
        {
            //Create the various Cavern in the chunk
            foreach (Cavern cavern in Caverns)
            {
                //Nbr of cavern to generate
                for (int i = 0; i < cavern.CavernPerChunk; i++)
                {
                    if (rnd.NextDouble() <= cavern.ChanceOfSpawning)
                    {
                        //Get Rnd chunk Location.
                        int x = rnd.Next(7, 9);
                        int y = rnd.Next(cavern.SpawningHeight.Min, cavern.SpawningHeight.Max);
                        int z = rnd.Next(7, 9);
                        int layer = rnd.Next(cavern.CavernHeightSize.Min, cavern.CavernHeightSize.Max + 1);
                        PopulateChunkWithCave(cursor, x, y, z, layer, cavern.CubeId, rnd);
                    }
                }
            }
        }

        public void GenerateChunkTrees(ByteChunkCursor cursor, GeneratedChunk chunk, ref Vector3D chunkWorldPosition, ChunkColumnInfo[] columndInfo, Biome biome, FastRandom rnd, EntityFactory entityFactory)
        {
            int nbrTree = rnd.Next(BiomeTrees.TreePerChunks.Min, BiomeTrees.TreePerChunks.Max + 1);
            for (int i = 0; i < nbrTree; i++)
            {
                PopulateChunkWithTree(cursor, chunk, ref chunkWorldPosition, entityFactory, columndInfo, biome, rnd);
            }
        }

        public void GenerateChunkItems(ByteChunkCursor cursor, GeneratedChunk chunk, ref Vector3D chunkWorldPosition, ChunkColumnInfo[] columndInfo, Biome biome, FastRandom rnd, EntityFactory entityFactory)
        {
            foreach (BiomeEntity entity in BiomeEntities)
            {
                //Entity population
                for (int i = 0; i < entity.EntityPerChunk; i++)
                {
                    if (rnd.NextDouble() <= entity.ChanceOfSpawning)
                    {
                        //Get Rnd chunk Location.
                        int x = rnd.Next(0, 16);
                        int z = rnd.Next(0, 16);
                        int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxHeight;

                        PopulateChunkWithItems(cursor, chunk, ref chunkWorldPosition, entity.EntityId, x, y, z, rnd, entityFactory, false);
                    }
                }
            }
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
        private void PopulateChunkWithResource(byte cubeId, ByteChunkCursor cursor, int x, int y, int z, int qt, FastRandom rnd)
        {
            cursor.SetInternalPosition(x, y, z);
            int nbrCubePlaced;
            if (cursor.Read() == RealmConfiguration.CubeId.Stone)
            {
                cursor.Write(cubeId);
                nbrCubePlaced = 1;
                for (int i = 0; i < qt + 10 && nbrCubePlaced < qt; i++)
                {
                    int relativeMove = rnd.Next(1, 7);
                    cursor.Move(relativeMove);
                    if (cursor.Read() == RealmConfiguration.CubeId.Stone)
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
        private void PopulateChunkWithLiquidSources(byte cubeId, ByteChunkCursor cursor, int x, int y, int z, int liquidPower)
        {
            cursor.SetInternalPosition(x, y, z);

            //Check if this source is candidate as valid source = Must be surrended by 5 solid blocks and ahave one side block going to Air
            if (cursor.Read() != RealmConfiguration.CubeId.Air)
            {
                //Looking Up for Air
                if (RealmConfiguration.CubeProfiles[cursor.Peek(CursorRelativeMovement.Up)].IsBlockingWater == false || cursor.Peek(CursorRelativeMovement.Up) == RealmConfiguration.CubeId.Snow) return;
                if (RealmConfiguration.CubeProfiles[cursor.Peek(CursorRelativeMovement.Down)].IsBlockingWater == false) return;
                int cpt = 0;
                //Counting the number of holes arround the source
                if (RealmConfiguration.CubeProfiles[cursor.Peek(CursorRelativeMovement.East)].IsBlockingWater == false) cpt++;
                if (RealmConfiguration.CubeProfiles[cursor.Peek(CursorRelativeMovement.West)].IsBlockingWater == false) cpt++;
                if (RealmConfiguration.CubeProfiles[cursor.Peek(CursorRelativeMovement.North)].IsBlockingWater == false) cpt++;
                if (RealmConfiguration.CubeProfiles[cursor.Peek(CursorRelativeMovement.South)].IsBlockingWater == false) cpt++;

                //Only one face touching air ==> Createing the Liquid Source !
                if (cpt != 1) return;

                cursor.Write(cubeId);
                Queue<Tuple<ByteChunkCursor, int>> sourcesWithPower = new Queue<Tuple<ByteChunkCursor, int>>();
                sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(cursor, liquidPower));
                PropagateLiquidSources(sourcesWithPower, cubeId);
            }
        }
        private void PropagateLiquidSources(Queue<Tuple<ByteChunkCursor, int>> sourcesWithPower, byte cubeId)
        {
            Tuple<ByteChunkCursor, int> liquidSource = sourcesWithPower.Dequeue();

            //Can Fall, falling doesn't remove Power at propagation
            bool isFalling = false;
            if (RealmConfiguration.CubeProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.Down)].CubeFamilly == Enums.enuCubeFamilly.Liquid) return;
            while (RealmConfiguration.CubeProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.Down)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == RealmConfiguration.CubeId.Snow)
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
                    if (RealmConfiguration.CubeProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.East)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == RealmConfiguration.CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.East);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                        liquidSource.Item1.Move(CursorRelativeMovement.West);
                    }

                    if (RealmConfiguration.CubeProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.West)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == RealmConfiguration.CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.West);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                        liquidSource.Item1.Move(CursorRelativeMovement.East);
                    }

                    if (RealmConfiguration.CubeProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.North)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == RealmConfiguration.CubeId.Snow)
                    {
                        liquidSource.Item1.Move(CursorRelativeMovement.North);
                        liquidSource.Item1.Write(cubeId);
                        sourcesWithPower.Enqueue(new Tuple<ByteChunkCursor, int>(liquidSource.Item1.Clone(), power));
                        liquidSource.Item1.Move(CursorRelativeMovement.South);
                    }

                    if (RealmConfiguration.CubeProfiles[liquidSource.Item1.Peek(CursorRelativeMovement.South)].IsBlockingWater == false || liquidSource.Item1.Peek(CursorRelativeMovement.Down) == RealmConfiguration.CubeId.Snow)
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

        private void PopulateChunkWithCave(ByteChunkCursor cursor, int x, int y, int z, int layers, byte cubeId, FastRandom rnd)
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
                                    if (cursor.Read() == RealmConfiguration.CubeId.Stone) cursor.Write(RealmConfiguration.CubeId.LightWhite);
                                }
                                else cursor.Write(RealmConfiguration.CubeId.Air);

                            }
                        }
                    }
                }
                if (layerRadiusModifier < caveRadius) layerRadiusModifier++;
            }
        }

        private void PopulateChunkWithTree(ByteChunkCursor cursor, GeneratedChunk chunk, ref Vector3D chunkWorldPosition, EntityFactory entityFactory, ChunkColumnInfo[] columndInfo, Biome biome, FastRandom rnd)
        {

            var treeTemplate = TreeTemplates.Templates[(int)BiomeTrees.GetNextTreeType(rnd)];

            //Get Rnd chunk Location.
            int x = rnd.Next(treeTemplate.Radius - 1, 16 - treeTemplate.Radius + 1);
            int z = rnd.Next(treeTemplate.Radius - 1, 16 - treeTemplate.Radius + 1);
            int y = columndInfo[x * AbstractChunk.ChunkSize.Z + z].MaxHeight;

            cursor.SetInternalPosition(x, y, z);
            //No other tree around me ?
            byte trunkRootCube = cursor.Read();

            Vector3I radiusRange = new Vector3I(treeTemplate.Radius - 1, 1, treeTemplate.Radius - 1);

            if ((trunkRootCube == RealmConfiguration.CubeId.Grass || trunkRootCube == RealmConfiguration.CubeId.Dirt || trunkRootCube == RealmConfiguration.CubeId.Snow || trunkRootCube == RealmConfiguration.CubeId.Sand) &&
                cursor.IsCubePresent(treeTemplate.TrunkCubeId, radiusRange) == false &&
                cursor.IsCubePresent(RealmConfiguration.CubeId.StillWater, radiusRange) == false)
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
                        if (foliageMove >= 0 && cursor.Read() == RealmConfiguration.CubeId.Air)
                        {
                            cursor.Write(treeTemplate.FoliageCubeId);
                        }
                    }

                    //In case of cactus add a flower on top of fit
                    if (treeTemplate.TreeType == TreeTemplates.TreeType.Cactus)
                    {
                        Vector3I posi = cursor.InternalPosition;
                        PopulateChunkWithItems(cursor, chunk, ref chunkWorldPosition, RealmConfiguration.EntityId.CactusFlower , posi.X, posi.Y, posi.Z, rnd, entityFactory, true, true);
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

        private void PopulateChunkWithItems(ByteChunkCursor cursor, GeneratedChunk chunk, ref Vector3D chunkWorldPosition, ushort entityId, int x, int y, int z, FastRandom rnd, EntityFactory entityFactory, bool isBlockCentered = true, bool test = false)
        {
            cursor.SetInternalPosition(x, y, z);

            //Check that the block above is "Air"
            if (cursor.Peek(CursorRelativeMovement.Up) != RealmConfiguration.CubeId.Air) return;
            //Check that the block below is "solid"
            byte blockBelow = cursor.Read();
            CubeProfile blockBelowProfile = RealmConfiguration.CubeProfiles[blockBelow];
            if (blockBelowProfile.IsSolidToEntity)
            {
                //Cloning the Entity Blue Print !
                var entity = (IEntity)RealmConfiguration.Entities[entityId].Clone();

                if (entity is IBlockLinkedEntity)
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

                entity.Position = new Vector3D(chunkWorldPosition.X + x + XOffset, y + 1, chunkWorldPosition.Z + z + ZOffset);

                chunk.Entities.Add((IStaticEntity)entity);
            }
        }

        #endregion
    }
}
