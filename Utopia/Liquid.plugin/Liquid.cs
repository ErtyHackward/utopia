using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using Utopia.Planets.Terran.Cube;
using Utopia.Planets.Terran;
using Utopia.Planets.Terran.Flooding;
using Utopia.Planets.Terran.World;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Maths;
using SharpDX;
using Liquid.plugin.LiquidsContent.Effects;
using Utopia.Planets.Terran.Chunk;
using S33M3Engines.StatesManager;
using S33M3Engines.D3D;
using S33M3Engines.TypeExtension;
using SharpDX.Direct3D11;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;
using S33M3Engines.Shared.Math.Noises;
using S33M3Engines.Shared.Math;
using Utopia.Worlds.Chunks;

namespace Liquid.plugin
{
    public class Liquid
    {
        private SimplexNoise _noise;

        public SimplexNoise Noise { get { return _noise; } }
        public int InfiniteLiquidStates;

        //Default texture mapping
        private HalfVector2 textureTopLeft = new HalfVector2(0, 0);
        private HalfVector2 textureTopRight = new HalfVector2(1, 0);
        private HalfVector2 textureBottomLeft = new HalfVector2(0, 1);
        private HalfVector2 textureBottomRight = new HalfVector2(1, 1);

        private LiquidManager _liquids;
        private Terra _terra;

        public Liquid(LiquidManager liquid, Terra terra)
        {
            _liquids = liquid;
            _terra = terra;
            //Register the faceGenerationCheck to the delegate ! - Will overide the default behaviours
            RenderCubeProfile.CubesProfile[CubeId.WaterSource].CanGenerateCubeFace = FaceGenerationCheck;
            RenderCubeProfile.CubesProfile[CubeId.WaterSource].CreateLiquidCubeMesh += GenCubeFace;
            RenderCubeProfile.CubesProfile[CubeId.Water].CanGenerateCubeFace = FaceGenerationCheck;
            RenderCubeProfile.CubesProfile[CubeId.Water].CreateLiquidCubeMesh += GenCubeFace;

            _noise = new SimplexNoise(new Random());
            _noise.SetParameters(0.1, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);
        }

        public void LoadContent()
        {
        }

        public void UnloadContent()
        {
        }

        public bool FaceGenerationCheck(ref TerraCube cube, ref Location3<int> cubePosiInWorld, CubeFace cubeFace, ref TerraCube neightboorFaceCube)
        {
            if (cubeFace != CubeFace.Bottom && cubeFace != CubeFace.Top) //Never display a bottom Water face !
            {
                //if (cubePosiInWorld.Y == TerraWorld.SeaLevel && cube.Type == CubeType.WaterSource)
                //{
                //    return true;
                //}
                if ((!RenderCubeProfile.CubesProfile[neightboorFaceCube.Id].IsBlockingLight && !RenderCubeProfile.CubesProfile[neightboorFaceCube.Id].IsFlooding))
                {
                    return true;
                }
                if (cube.Id == CubeId.Water && neightboorFaceCube.Id == CubeId.WaterSource && cubePosiInWorld.Y == LandscapeBuilder.SeaLevel)
                {
                    return true;
                }
            }
            if (cubeFace == CubeFace.Top)
            {
                if (cubePosiInWorld.Y == LandscapeBuilder.SeaLevel || neightboorFaceCube.Id == CubeId.Air)
                {
                    return true;
                }
            }
            return false;
        }

        //Action to take when a block surrounding a infinite liquid block is change
        public void Activate(ref Location3<int> cubeCoordinates, ref TerraCube newCube, ref WaterPool wp)
        {
            if (wp == null) wp = new WaterPool();
            FloodingData floodData;
            TerraCube SurroundingCube, replacedCube;
            RenderCubeProfile profile = RenderCubeProfile.CubesProfile[newCube.Id];
            RenderCubeProfile surroundingProfiles;

            int oldCubeIndex = _terra.World.Landscape.Index(cubeCoordinates.X, cubeCoordinates.Y, cubeCoordinates.Z);
            replacedCube = _terra.World.Landscape.Cubes[oldCubeIndex];
            if (RenderCubeProfile.CubesProfile[replacedCube.Id].IsFlooding)
            {
                wp.DryingPool = true;
            }

            //If my new block is not blocking water ==> check the surrounding block for water type, and active them if they are water !
            SurroundingIndex[] indexes = _terra.World.Landscape.GetSurroundingBlocksIndex(ref cubeCoordinates);

            for (int idx = 0; idx < 6; idx++)
            {

                floodData = new FloodingData();
                SurroundingCube = _terra.World.Landscape.Cubes[indexes[idx].Index];
                surroundingProfiles = RenderCubeProfile.CubesProfile[SurroundingCube.Id];
                if (surroundingProfiles.IsFlooding)
                {
                    //Don't process in dry up mode a cube if it flow "IN"
                    if (wp.DryingPool && replacedCube.Id != CubeId.WaterSource)
                    {
                        if (isFloodingAwayForActivate(indexes[idx].IndexRelativePosition, (TerraFlooding.FloodDirection)replacedCube.MetaData3) == false) continue;
                    }


                    if (SurroundingCube.Id == CubeId.WaterSource)
                    {
                        floodData.FloodingPower = surroundingProfiles.FloodingPropagationPower;
                    }
                    else
                    {
                        floodData.FloodingPower = SurroundingCube.MetaData2;
                        if (indexes[idx].IndexRelativePosition == IdxRelativeMove.Y_Plus1 && floodData.FloodingPower == 0) floodData.FloodingPower++;
                    }

                    floodData.CubeLocation = indexes[idx].Position;
                    wp.FloodData.Enqueue(floodData);
                }
            }
        }

        //Handle the DryUp Algo !
        public void DryUp(ref TerraCube workingCube, FloodingData cubeFloodData, WaterPool wp)
        {
            FloodingData dryingUpDestination;
            TerraCube cube = new TerraCube();
            //Get surrounding cubes
            SurroundingIndex[] indexes = _terra.World.Landscape.GetSurroundingBlocksIndex(ref cubeFloodData.CubeLocation);

            cube.Id = CubeId.Air;
            cube.MetaData1 = 0;
            cube.MetaData2 = 0;
            cube.MetaData3 = 0;
            _terra.World.Landscape.SetCubeWithNotification(ref cubeFloodData.CubeLocation, ref cube, false);

            //Find the cubes flooding away from me ! ==> They won't anymore !
            for (int idx = 0; idx < 6; idx++)
            {
                cube = _terra.World.Landscape.Cubes[indexes[idx].Index];

                if (cube.Id == CubeId.Water)
                {
                    if (isFloodingAway(indexes[idx].IndexRelativePosition, (TerraFlooding.FloodDirection)workingCube.MetaData3))
                    {
                        dryingUpDestination = new FloodingData();
                        dryingUpDestination.CubeLocation = indexes[idx].Position;
                        dryingUpDestination.FloodingPower = 0;
                        wp.FloodData.EnqueueDistinct(dryingUpDestination);
                    }
                    else
                    {
                        wp.WaterSources.Add(indexes[idx]);
                    }
                }
                else
                {
                    if (cube.Id == CubeId.WaterSource)
                    {
                        wp.WaterSources.Add(indexes[idx]);
                    }
                }
            }
        }

        private bool isFloodingAwayForActivate(IdxRelativeMove blockLookUp, TerraFlooding.FloodDirection replacedCube)
        {
            switch (replacedCube)
            {
                case TerraFlooding.FloodDirection.None:
                case TerraFlooding.FloodDirection.Undefined:
                    return true;
                case TerraFlooding.FloodDirection.Right:
                    if (blockLookUp == IdxRelativeMove.X_Plus1) return true;
                    break;
                case TerraFlooding.FloodDirection.Left:
                    if (blockLookUp == IdxRelativeMove.X_Minus1) return true;
                    break;
                case TerraFlooding.FloodDirection.Front:
                    if (blockLookUp == IdxRelativeMove.Z_Plus1) return true;
                    break;
                case TerraFlooding.FloodDirection.Back:
                    if (blockLookUp == IdxRelativeMove.Z_Minus1) return true;
                    break;
                case TerraFlooding.FloodDirection.FrontRight:
                    if (blockLookUp == IdxRelativeMove.Z_Plus1 || blockLookUp == IdxRelativeMove.X_Plus1) return true;
                    break;
                case TerraFlooding.FloodDirection.FrontLeft:
                    if (blockLookUp == IdxRelativeMove.Z_Plus1 || blockLookUp == IdxRelativeMove.X_Minus1) return true;
                    break;
                case TerraFlooding.FloodDirection.BackRight:
                    if (blockLookUp == IdxRelativeMove.Z_Minus1 || blockLookUp == IdxRelativeMove.X_Plus1) return true;
                    break;
                case TerraFlooding.FloodDirection.BackLeft:
                    if (blockLookUp == IdxRelativeMove.Z_Minus1 || blockLookUp == IdxRelativeMove.X_Minus1) return true;
                    break;
                case TerraFlooding.FloodDirection.Fall:
                    if (blockLookUp == IdxRelativeMove.Y_Minus1) return true;
                    break;
                default:
                    break;
            }

            return false;
        }

        private bool isFloodingAway(IdxRelativeMove blockLookUp, TerraFlooding.FloodDirection NeightboorCubeFloodData)
        {
            switch (blockLookUp)
            {
                case IdxRelativeMove.X_Minus1:
                    switch (NeightboorCubeFloodData)
	                {
                        case TerraFlooding.FloodDirection.Fall:
                            return true;
                        case TerraFlooding.FloodDirection.Left:
                            return true;
                        case TerraFlooding.FloodDirection.Front:
                            return true;
                        case TerraFlooding.FloodDirection.Back:
                            return true;
                        case TerraFlooding.FloodDirection.FrontLeft:
                            return true;
                        case TerraFlooding.FloodDirection.BackLeft:
                            return true;
                        case TerraFlooding.FloodDirection.None:
                            return true;
                        default:
                            break;
	                }
                    break;
                case IdxRelativeMove.X_Plus1:
                    switch (NeightboorCubeFloodData)
                    {
                        case TerraFlooding.FloodDirection.Fall:
                            return true;
                        case TerraFlooding.FloodDirection.Right:
                            return true;
                        case TerraFlooding.FloodDirection.Front:
                            return true;
                        case TerraFlooding.FloodDirection.Back:
                            return true;
                        case TerraFlooding.FloodDirection.FrontRight:
                            return true;
                        case TerraFlooding.FloodDirection.BackRight:
                            return true;
                        case TerraFlooding.FloodDirection.None:
                            return true;
                        default:
                            break;
                    }
                    break;
                case IdxRelativeMove.Z_Minus1:
                    switch (NeightboorCubeFloodData)
                    {
                        case TerraFlooding.FloodDirection.Fall:
                            return true;
                        case TerraFlooding.FloodDirection.Left:
                            return true;
                        case TerraFlooding.FloodDirection.Right:
                            return true;
                        case TerraFlooding.FloodDirection.Back:
                            return true;
                        case TerraFlooding.FloodDirection.BackRight:
                            return true;
                        case TerraFlooding.FloodDirection.BackLeft:
                            return true;
                        case TerraFlooding.FloodDirection.None:
                            return true;
                        default:
                            break;
                    }
                    break;
                case IdxRelativeMove.Z_Plus1:
                    switch (NeightboorCubeFloodData)
                    {
                        case TerraFlooding.FloodDirection.Fall:
                            return true;
                        case TerraFlooding.FloodDirection.Left:
                            return true;
                        case TerraFlooding.FloodDirection.Right:
                            return true;
                        case TerraFlooding.FloodDirection.Front:
                            return true;
                        case TerraFlooding.FloodDirection.FrontRight:
                            return true;
                        case TerraFlooding.FloodDirection.FrontLeft:
                            return true;
                        case TerraFlooding.FloodDirection.None:
                            return true;
                        default:
                            break;
                    }
                    break;

                case IdxRelativeMove.Y_Minus1:
                    return true;
                case IdxRelativeMove.Y_Plus1:
                    return true;
            }

            return false;
        }

        //Handle the water flooding algo. for Infinite Water
        public void Propagate(ref TerraCube workingCube, FloodingData cubeFloodData, WaterPool wp)
        {
            TerraCube cube;
            RenderCubeProfile groundCubeProfile;
            TerraFlooding.FloodDirection floodDirectionSource, floodDirectionTarget;
            FloodingData floodingDataDestination = new FloodingData();
            bool fallOnly = false;
            int groundCubeIndex;

            SurroundingIndex[] indexes = _terra.World.Landscape.GetSurroundingBlocksIndex(ref cubeFloodData.CubeLocation);

            //Priority to fall first
            if (!RenderCubeProfile.CubesProfile[_terra.World.Landscape.Cubes[indexes[4].Index].Id].IsBlockingWater)
            {
                fallOnly = true;
            }

            if ((TerraFlooding.FloodDirection)workingCube.MetaData3 == TerraFlooding.FloodDirection.Fall && RenderCubeProfile.CubesProfile[_terra.World.Landscape.Cubes[indexes[4].Index].Id].IsFlooding)
            {
                if (_terra.World.Landscape.Cubes[indexes[4].Index].Id == CubeId.WaterSource)
                {
                    cube = new TerraCube(CubeId.Water);
                    _terra.World.Landscape.SetCubeWithNotification(ref indexes[4].Position, ref cube, false);
                }
                return;
            }

            for (int idx = 0; idx < 6; idx++)
            {
                if (fallOnly && indexes[idx].IndexRelativePosition != IdxRelativeMove.Y_Minus1) continue;
                if (indexes[idx].IndexRelativePosition == IdxRelativeMove.Y_Plus1)
                {
                    if (canPropagateUp(indexes[idx]) == false) continue;  //Never propagate UP !
                }

                cube = _terra.World.Landscape.Cubes[indexes[idx].Index];

                if (RenderCubeProfile.CubesProfile[cube.Id].IsBlockingWater == false)
                {
                    floodingDataDestination = new FloodingData();
                    floodingDataDestination.CubeLocation = indexes[idx].Position;
                    if(indexes[idx].IndexRelativePosition != IdxRelativeMove.Y_Minus1) floodingDataDestination.FloodingPower = cubeFloodData.FloodingPower - 1;
                    else floodingDataDestination.FloodingPower = cubeFloodData.FloodingPower;

                    groundCubeIndex = _terra.World.Landscape.FastIndex(indexes[idx].Index, floodingDataDestination.CubeLocation.Y, IdxRelativeMove.Y_Minus1);
                    groundCubeProfile = RenderCubeProfile.CubesProfile[_terra.World.Landscape.Cubes[groundCubeIndex].Id];

                    //If the block below the destination make a liquid slide ...
                    //OR
                    //If the below ground block is permeable to water, and there are no more power in the cube, put one more power ! in it ! (to avoid to have bloc of water
                    //in the air !
                    if (groundCubeProfile.IsFloodPropagation ||
                        (floodingDataDestination.FloodingPower == 0 && groundCubeProfile.IsBlockingWater == false))
                    {
                        floodingDataDestination.FloodingPower++;
                    }

                    wp.FloodData.Enqueue(floodingDataDestination);
                    cube.Id = CubeId.Water;
                    cube.MetaData1 = workingCube.MetaData1;                       //Hauteur MAX
                    cube.MetaData2 = (byte)floodingDataDestination.FloodingPower; //Flood Power dans MetaData2
                    GetFloodDirection((TerraFlooding.FloodDirection)workingCube.MetaData3, indexes[idx].IndexRelativePosition, out floodDirectionTarget, out floodDirectionSource);
                    if (groundCubeProfile.IsFlooding && floodDirectionSource != TerraFlooding.FloodDirection.Fall)
                    {
                        floodDirectionSource = TerraFlooding.FloodDirection.None;
                        floodDirectionTarget = TerraFlooding.FloodDirection.None;
                    }
                    cube.MetaData3 = (byte)floodDirectionTarget;
                    workingCube.MetaData3 = (byte)floodDirectionSource;
                    _terra.World.Landscape.SetCubeWithNotification(ref cubeFloodData.CubeLocation, ref workingCube, false);
                    _terra.World.Landscape.SetCubeWithNotification(ref floodingDataDestination.CubeLocation, ref cube, false);
                }
            }
        }

        private bool canPropagateUp(SurroundingIndex index)
        {
            if (RenderCubeProfile.CubesProfile[_terra.World.Landscape.Cubes[_terra.World.Landscape.FastIndex(index.Index, index.Position.X, IdxRelativeMove.X_Plus1)].Id].IsFloodPropagation) return true;
            if (RenderCubeProfile.CubesProfile[_terra.World.Landscape.Cubes[_terra.World.Landscape.FastIndex(index.Index, index.Position.X, IdxRelativeMove.X_Minus1)].Id].IsFloodPropagation) return true;
            if (RenderCubeProfile.CubesProfile[_terra.World.Landscape.Cubes[_terra.World.Landscape.FastIndex(index.Index, index.Position.Z, IdxRelativeMove.Z_Plus1)].Id].IsFloodPropagation) return true;
            if (RenderCubeProfile.CubesProfile[_terra.World.Landscape.Cubes[_terra.World.Landscape.FastIndex(index.Index, index.Position.Z, IdxRelativeMove.Z_Minus1)].Id].IsFloodPropagation) return true;
            return false;
        }

        private void GetFloodDirection(TerraFlooding.FloodDirection fromDirection, IdxRelativeMove floodDestinationMove, out TerraFlooding.FloodDirection floodDestination, out TerraFlooding.FloodDirection floodSource)
        {
            TerraFlooding.FloodDirection destinationMove = TerraFlooding.FloodDirection.None;
            floodSource = fromDirection;

            switch (floodDestinationMove)
            {
                case IdxRelativeMove.X_Minus1:
                    destinationMove = TerraFlooding.FloodDirection.Left;
                    break;
                case IdxRelativeMove.X_Plus1:
                    destinationMove = TerraFlooding.FloodDirection.Right;
                    break;
                case IdxRelativeMove.Z_Minus1:
                    destinationMove = TerraFlooding.FloodDirection.Back;
                    break;
                case IdxRelativeMove.Z_Plus1:
                    destinationMove = TerraFlooding.FloodDirection.Front;
                    break;
                case IdxRelativeMove.Y_Minus1:
                    destinationMove = TerraFlooding.FloodDirection.Fall;
                    break;
                case IdxRelativeMove.Y_Plus1:
                    destinationMove = TerraFlooding.FloodDirection.None;
                    break;
            }

            floodDestination = destinationMove;


            switch (fromDirection)
            {
                case TerraFlooding.FloodDirection.None:
                    break;
                case TerraFlooding.FloodDirection.Right:
                    switch (destinationMove)
	                    {
                            case TerraFlooding.FloodDirection.Right:
                                break;
                            case TerraFlooding.FloodDirection.Left:
                                break;
                            case TerraFlooding.FloodDirection.Front:
                                floodSource = TerraFlooding.FloodDirection.FrontRight;
                                break;
                            case TerraFlooding.FloodDirection.Back:
                                floodSource = TerraFlooding.FloodDirection.BackRight;
                                break;
	                    }
                    break;
                case TerraFlooding.FloodDirection.Left:
                    switch (destinationMove)
                    {
                        case TerraFlooding.FloodDirection.Right:
                            break;
                        case TerraFlooding.FloodDirection.Left:
                            break;
                        case TerraFlooding.FloodDirection.Front:
                            floodSource = TerraFlooding.FloodDirection.FrontLeft;
                            break;
                        case TerraFlooding.FloodDirection.Back:
                            floodSource = TerraFlooding.FloodDirection.BackLeft;
                            break;
                    }
                    break;
                case TerraFlooding.FloodDirection.Front:
                    switch (destinationMove)
                    {
                        case TerraFlooding.FloodDirection.Right:
                            floodSource = TerraFlooding.FloodDirection.FrontRight;
                            break;
                        case TerraFlooding.FloodDirection.Left:
                            floodSource = TerraFlooding.FloodDirection.FrontLeft;
                            break;
                        case TerraFlooding.FloodDirection.Front:
                            break;
                        case TerraFlooding.FloodDirection.Back:
                            break;
                    }
                    break;
                case TerraFlooding.FloodDirection.Back:
                    switch (destinationMove)
                    {
                        case TerraFlooding.FloodDirection.Right:
                            floodSource = TerraFlooding.FloodDirection.BackRight;
                            break;
                        case TerraFlooding.FloodDirection.Left:
                            floodSource = TerraFlooding.FloodDirection.BackLeft;
                            break;
                        case TerraFlooding.FloodDirection.Front:
                            break;
                        case TerraFlooding.FloodDirection.Back:
                            break;
                    }
                    break;
                case TerraFlooding.FloodDirection.FrontRight:
                    break;
                case TerraFlooding.FloodDirection.FrontLeft:
                    break;
                case TerraFlooding.FloodDirection.BackRight:
                    break;
                case TerraFlooding.FloodDirection.BackLeft:
                    break;
                case TerraFlooding.FloodDirection.Fall:
                    break;
                case TerraFlooding.FloodDirection.Undefined:
                    break;
                default:
                    break;
            }

        }

        public void GenCubeFaceBlocky(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, ref List<VertexCubeLiquid> cubeVertices, ref List<ushort> cubeIndices)
        {
            if (cubePosition.Y != LandscapeBuilder.SeaLevel) return;

            int VerticesIndex = cubeVertices.Count - 4;

            VertexCubeLiquid vertex;

            float noiseOffset = (float)MathHelper.FullLerp(0f, MathHelper.Pi, _noise.GetNoise2DValue(cubePosiInWorld.X, cubePosiInWorld.Z, 1, 1));

            switch (cubeFace)
            {
                case CubeFace.Back:
                    vertex = cubeVertices[VerticesIndex + 1];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex + 1] = vertex;

                    vertex = cubeVertices[VerticesIndex];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex] = vertex;
                    break;
                case CubeFace.Front:
                    vertex = cubeVertices[VerticesIndex];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex] = vertex;

                    vertex = cubeVertices[VerticesIndex + 1];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex + 1] = vertex;
                    break;
                case CubeFace.Bottom:
                    break;
                case CubeFace.Top:
                    vertex = cubeVertices[VerticesIndex];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex] = vertex;

                    VerticesIndex++;

                    vertex = cubeVertices[VerticesIndex];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex] = vertex;

                    VerticesIndex++;

                    vertex = cubeVertices[VerticesIndex];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex] = vertex;

                    VerticesIndex++;

                    vertex = cubeVertices[VerticesIndex];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex] = vertex;
                    break;
                case CubeFace.Left:
                    vertex = cubeVertices[VerticesIndex];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex] = vertex;

                    vertex = cubeVertices[VerticesIndex + 1];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex + 1] = vertex;
                    break;
                case CubeFace.Right:
                    vertex = cubeVertices[VerticesIndex + 1];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex + 1] = vertex;

                    vertex = cubeVertices[VerticesIndex];
                    vertex.VertexInfo2 = new Vector4(noiseOffset, 1.0f, 0, 0);
                    cubeVertices[VerticesIndex] = vertex;
                    break;
                default:
                    break;
            }
        }

        public void GenCubeFace(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, ref List<VertexCubeLiquid> cubeVertices, ref List<ushort> cubeIndices, ref Dictionary<string, int> cubeVerticeDico)
        {
            ////Check for WaterSources above sea level !
            if (cubePosition.Y > LandscapeBuilder.SeaLevel && cube.MetaData1 == 0 && cube.Id == CubeId.WaterSource)
            {
                var chunk = ChunkFinder.GetChunk(cubePosiInWorld.X, cubePosiInWorld.Z);
                ChunkState cs = chunk.State;

                FloodingData floodingData = new FloodingData();
                WaterPool wp = new WaterPool(WaterPoolState.WithoutTimer);
                cube.MetaData1 = (byte)cubePosiInWorld.Y;
                floodingData.FloodingPower = RenderCubeProfile.CubesProfile[cube.Id].FloodingPropagationPower;
                floodingData.CubeLocation = cubePosiInWorld;
                cube.MetaData2 = (byte)floodingData.FloodingPower;
                wp.FloodData.Enqueue(floodingData);
                _liquids.WaterPools.Add(wp);
                _liquids.LiquidFloodingUpdate(true);
                //Console.WriteLine("One More : " + cubePosiInWorld);
                
                chunk.State = cs;
            }

            if (cubePosition.Y != LandscapeBuilder.SeaLevel) return;

            int indiceFace = cubeIndices.Count - 6;

            VertexCubeLiquid vertex;

            float noiseOffsetX0Z0 = (float)MathHelper.FullLerp(0f, MathHelper.Pi, _noise.GetNoise2DValue(cubePosiInWorld.X, cubePosiInWorld.Z, 1, 1));
            float noiseOffsetX1Z0 = (float)MathHelper.FullLerp(0f, MathHelper.Pi, _noise.GetNoise2DValue(cubePosiInWorld.X + 1, cubePosiInWorld.Z, 1, 1));
            float noiseOffsetX0Z1 = (float)MathHelper.FullLerp(0f, MathHelper.Pi, _noise.GetNoise2DValue(cubePosiInWorld.X, cubePosiInWorld.Z + 1, 1, 1));
            float noiseOffsetX1Z1 = (float)MathHelper.FullLerp(0f, MathHelper.Pi, _noise.GetNoise2DValue(cubePosiInWorld.X + 1, cubePosiInWorld.Z + 1, 1, 1));

            switch (cubeFace)
            {
                case CubeFace.Back:
                    vertex = cubeVertices[cubeIndices[indiceFace + 3]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX1Z0, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace + 3]] = vertex;

                    vertex = cubeVertices[cubeIndices[indiceFace]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX0Z0, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace]] = vertex;
                    break;
                case CubeFace.Front:
                    vertex = cubeVertices[cubeIndices[indiceFace]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX0Z1, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace]] = vertex;

                    vertex = cubeVertices[cubeIndices[indiceFace + 3]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX1Z1, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace + 3]] = vertex;
                    break;
                case CubeFace.Bottom:
                    break;
                case CubeFace.Top:
                    vertex = cubeVertices[cubeIndices[indiceFace]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX0Z0, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace]] = vertex;

                    vertex = cubeVertices[cubeIndices[indiceFace + 1]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX1Z1, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace + 1]] = vertex;

                    vertex = cubeVertices[cubeIndices[indiceFace + 2]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX0Z1, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace + 2]] = vertex;

                    vertex = cubeVertices[cubeIndices[indiceFace + 3]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX1Z0, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace + 3]] = vertex;
                    break;
                case CubeFace.Left:
                    vertex = cubeVertices[cubeIndices[indiceFace]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX0Z0, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace]] = vertex;

                    vertex = cubeVertices[cubeIndices[indiceFace + 3]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX0Z1, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace + 3]] = vertex;
                    break;
                case CubeFace.Right:
                    vertex = cubeVertices[cubeIndices[indiceFace + 3]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX1Z1, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace + 3]] = vertex;

                    vertex = cubeVertices[cubeIndices[indiceFace]];
                    vertex.VertexInfo2 = new Vector4(noiseOffsetX1Z0, 1.0f, 0, 0);
                    cubeVertices[cubeIndices[indiceFace]] = vertex;
                    break;
                default:
                    break;
            }
        }

        public void Update(ref GameTime TimeSpend)
        {
            RefreshWaveGlobalOffset(ref TimeSpend);
        }

        FTSValue<float> _waveGlobalOffset = new FTSValue<float>();

        public FTSValue<float> WaveGlobalOffset { get { return _waveGlobalOffset; } set { _waveGlobalOffset = value; } }

        float _stepWave = MathHelper.Pi / 256;
        long _waveFlagTempoInSec = (long)(System.Diagnostics.Stopwatch.Frequency / 1000);
        long timeAccumulator, previousTime;
        private void RefreshWaveGlobalOffset(ref GameTime TimeSpend)
        {
            //Start Tempo
            long currentTime = System.Diagnostics.Stopwatch.GetTimestamp();
            timeAccumulator += currentTime - previousTime;
            previousTime = currentTime;

            if (timeAccumulator < _waveFlagTempoInSec)return;
            timeAccumulator = 0;

            _waveGlobalOffset.BackUpValue();

            //TimeElapsed !
            _waveGlobalOffset.Value += _stepWave;
            if (_waveGlobalOffset.Value >= MathHelper.TwoPi) _waveGlobalOffset.Value = 0;
        }

        public void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            if (_waveGlobalOffset.ValuePrev > _waveGlobalOffset.Value) _waveGlobalOffset.ValuePrev = _waveGlobalOffset.Value;
            _waveGlobalOffset.ValueInterp = MathHelper.Lerp(_waveGlobalOffset.ValuePrev, _waveGlobalOffset.Value, interpolation_ld);
            if (_waveGlobalOffset.ValueInterp >= MathHelper.TwoPi) _waveGlobalOffset.ValueInterp = 0;
        }

    }
}
