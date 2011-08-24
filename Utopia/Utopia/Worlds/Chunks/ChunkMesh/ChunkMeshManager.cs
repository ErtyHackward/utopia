﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S33M3Engines.Threading;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Shared.Structs;
using Utopia.Worlds.Cubes;
using Utopia.Shared.World;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using S33M3Engines;
using Utopia.Shared.Structs.Landscape;
using Amib.Threading;
using Utopia.Shared.Cubes;

namespace Utopia.Worlds.Chunks.ChunkMesh
{
    public class ChunkMeshManager : IChunkMeshManager
    {
        #region private variables
        private CreateChunkMeshDelegate _createChunkMeshDelegate;
        private delegate object CreateChunkMeshDelegate(object chunk);
        private VisualWorldParameters _visualWorldParameters;
        private Location3<int> _visibleWorldSize;
        private SingleArrayChunkContainer _cubesHolder;
        #endregion

        #region public variables/properties
        #endregion

        public ChunkMeshManager(VisualWorldParameters visualWorldParameters, SingleArrayChunkContainer cubesHolder)
        {
            _visualWorldParameters = visualWorldParameters;
            _cubesHolder = cubesHolder;
            Intialize();
        }

        #region Public methods
        public void CreateChunkMesh(VisualChunk chunk, bool Async)
        {
            if (Async)
            {
                WorkQueue.DoWorkInThread(new WorkItemCallback(createChunkMesh_threaded), chunk, chunk as IThreadStatus, chunk.ThreadPriority);
            }
            else
            {
                _createChunkMeshDelegate.Invoke(chunk);
            }
        }
        
        #endregion

        #region Private methods
        private void Intialize()
        {
            _createChunkMeshDelegate = new CreateChunkMeshDelegate(createChunkMesh_threaded);
            _visibleWorldSize = new Location3<int>()
            {
                X = AbstractChunk.ChunkSize.X * _visualWorldParameters.WorldParameters.WorldChunkSize.X,
                Y = AbstractChunk.ChunkSize.Y,
                Z = AbstractChunk.ChunkSize.Z * _visualWorldParameters.WorldParameters.WorldChunkSize.Z,
            };
        }

        //Create the landscape for the chunk
        private object createChunkMesh_threaded(object chunk)
        {
            VisualChunk visualChunk = (VisualChunk)chunk;
            CreateCubeMeshes(visualChunk);

            visualChunk.State = ChunkState.MeshesChanged;
            visualChunk.ThreadStatus = ThreadStatus.Idle;

            return null;
        }

        //Creation of the cubes meshes, Face type by face type
        private void CreateCubeMeshes(VisualChunk chunk)
        {
            //Instanciate the various collection objects that wil be used during the cube mesh creations
            chunk.InitializeChunkBuffers();

            GenerateCubesFace(CubeFace.Front, chunk);
            GenerateCubesFace(CubeFace.Back, chunk);
            GenerateCubesFace(CubeFace.Bottom, chunk);
            GenerateCubesFace(CubeFace.Top, chunk);
            GenerateCubesFace(CubeFace.Left, chunk);
            GenerateCubesFace(CubeFace.Right, chunk);
        }

        private void GenerateCubesFace(CubeFace cubeFace, VisualChunk chunk)
        {
            TerraCube currentCube, neightborCube;
            VisualCubeProfile cubeProfile;

            ByteVector4 cubePosiInChunk;
            Location3<int> cubePosiInWorld;
            int XWorld, YWorld, ZWorld;
            int cubeIndex, neightborCubeIndex;

            for (int X = 0; X < AbstractChunk.ChunkSize.X; X++)
            {
                XWorld = (X + chunk.CubeRange.Min.X);

                for (int Z = 0; Z < AbstractChunk.ChunkSize.Z; Z++)
                {
                    ZWorld = (Z + chunk.CubeRange.Min.Z);

                    cubeIndex = _cubesHolder.Index(XWorld, 0, ZWorld);

                    for (int Y = 0; Y < chunk.CubeRange.Max.Y; Y++)
                    {

                        //_cubeRange in fact identify the chunk, the chunk position in the world being _cubeRange.Min
                        YWorld = (Y + chunk.CubeRange.Min.Y);

                        //Inlinning Fct for speed !
                        //==> Could use this instead of the next line ==> cubeIndex = _landscape.Index(XWorld, YWorld, ZWorld);
                        //The "problem" being that everytime this fonction is called, it is creation a copy of the parameter variables, this fct is being called 
                        //A lot of time, to easy the GAC work, it's better to inline the fct here.
                        if (Y != 0) cubeIndex += _cubesHolder.MoveY;

                        //_landscape.Cubes[] is the BIG table containing all terraCube in the visible world.
                        //For speed access, I use an array with only one dimension, thus the table index must be computed from the X, Y, Z position of the terracube.
                        //Computing myself this index, is faster than using an array defined as [x,y,z]
                        // ? Am I an Air Cube ? ==> Default Value, not needed to render !
                        if (_cubesHolder.Cubes[cubeIndex].Id == CubeId.Air || _cubesHolder.Cubes[cubeIndex].Id == CubeId.Error) continue;

                        //Terra Cube contain only the data that are variables, and could be different between 2 cube.
                        currentCube = _cubesHolder.Cubes[cubeIndex];
                        //The Cube profile contain the value that are fixe for a block type.
                        cubeProfile = VisualCubeProfile.CubesProfile[currentCube.Id];

                        cubePosiInWorld = new Location3<int>(XWorld, YWorld, ZWorld);
                        cubePosiInChunk = new ByteVector4(X, Y, Z);

                        //Check to see if the face needs to be generated or not !
                        //Border Chunk test ! ==> Don't generate faces that are "border" chunks
                        //BorderChunk value is true if the chunk is at the border of the visible world.
                        int neightborCubeIndexTest;
                        switch (cubeFace)
                        {
                            case CubeFace.Back:
                                if (chunk.BorderChunk && (ZWorld - 1 < _visualWorldParameters.WorldRange.Min.Z)) continue;
                                //neightborCubeIndex = cubeIndex - _cubesHolder.MoveZ;
                                neightborCubeIndex = _cubesHolder.Index(XWorld, YWorld, ZWorld - 1);
                                neightborCubeIndexTest = _cubesHolder.FastIndex(cubeIndex, ZWorld, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1);
                                if (neightborCubeIndex != neightborCubeIndexTest)
                                {
                                    Console.WriteLine("STOP");
                                }
                                break;
                            case CubeFace.Front:
                                if (chunk.BorderChunk && (ZWorld + 1 >= _visualWorldParameters.WorldRange.Max.Z)) continue;
                                //neightborCubeIndex = cubeIndex + _cubesHolder.MoveZ;
                                neightborCubeIndex = _cubesHolder.Index(XWorld, YWorld, ZWorld + 1);
                                neightborCubeIndexTest = _cubesHolder.FastIndex(cubeIndex, ZWorld, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1);
                                if (neightborCubeIndex != neightborCubeIndexTest)
                                {
                                    Console.WriteLine("STOP");
                                }
                                break;
                            case CubeFace.Bottom:
                                if (YWorld - 1 < 0) continue;
                                //neightborCubeIndex = cubeIndex - _cubesHolder.MoveY;
                                neightborCubeIndex = _cubesHolder.Index(XWorld, YWorld - 1, ZWorld);
                                neightborCubeIndexTest = cubeIndex - _cubesHolder.MoveY;
                                if (neightborCubeIndex != neightborCubeIndexTest)
                                {
                                    Console.WriteLine("STOP");
                                }
                                
                                break;
                            case CubeFace.Top:
                                if (YWorld + 1 >= _visualWorldParameters.WorldRange.Max.Y) continue;
                                //neightborCubeIndex = cubeIndex + _cubesHolder.MoveY;
                                neightborCubeIndex = _cubesHolder.Index(XWorld, YWorld + 1, ZWorld);
                                neightborCubeIndexTest = _cubesHolder.ValidateIndex2(cubeIndex + _cubesHolder.MoveY);
                                neightborCubeIndexTest = cubeIndex + _cubesHolder.MoveY;
                                if (neightborCubeIndex != neightborCubeIndexTest)
                                {
                                    Console.WriteLine("STOP");
                                }
                                
                                break;
                            case CubeFace.Left:
                                if (chunk.BorderChunk && (XWorld - 1 < _visualWorldParameters.WorldRange.Min.X)) continue;
                                //neightborCubeIndex = cubeIndex - _cubesHolder.MoveX;
                                neightborCubeIndex = _cubesHolder.Index(XWorld - 1, YWorld, ZWorld);
                                neightborCubeIndexTest = _cubesHolder.FastIndex(cubeIndex, XWorld, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1);
                                if (neightborCubeIndex != neightborCubeIndexTest)
                                {
                                    Console.WriteLine("STOP");
                                }
                                
                                break;
                            case CubeFace.Right:
                                if (chunk.BorderChunk && (XWorld + 1 >= _visualWorldParameters.WorldRange.Max.X)) continue;
                                //neightborCubeIndex = cubeIndex + _cubesHolder.MoveX;
                                neightborCubeIndex = _cubesHolder.Index(XWorld + 1, YWorld, ZWorld);
                                neightborCubeIndexTest = _cubesHolder.FastIndex(cubeIndex, XWorld, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1);
                                if (neightborCubeIndex != neightborCubeIndexTest)
                                {
                                    Console.WriteLine("STOP");
                                }
                                break;
                            default:
                                throw new NullReferenceException();
                        }

                        //Custom test to see if the face can be generated (Between cube checks)
                        //_landscape.FastIndex is another method of computing index of the big table but faster !
                        //The constraint is that it only compute index of a cube neightbor
                        //int i = neightborCubeIndex;
                        //if (i >= _cubesHolder.Cubes.Length) i -= _cubesHolder.Cubes.Length;
                        //if (i < 0) i += _cubesHolder.Cubes.Length;
                        neightborCubeIndex = _cubesHolder.ValidateIndex(neightborCubeIndex);

                        neightborCube = _cubesHolder.Cubes[neightborCubeIndex];

                        var test = _cubesHolder.Index(XWorld + 1, YWorld, ZWorld);

                        //It is using a delegate in order to give the possibility for Plugging to replace the fonction call.
                        //Be default the fonction called here is : TerraCube.FaceGenerationCheck or TerraCube.WaterFaceGenerationCheck
                        if (!cubeProfile.CanGenerateCubeFace(ref currentCube, ref cubePosiInWorld, cubeFace, ref neightborCube, _visualWorldParameters.WorldParameters.SeaLevel)) continue;

                        switch (cubeProfile.CubeFamilly)
                        {
                            case enuCubeFamilly.Solid:
                                //Other delegate.
                                //Default linked to : CubeMeshFactory.GenSolidCubeFace;
                                cubeProfile.CreateSolidCubeMesh(ref currentCube, cubeFace, ref cubePosiInChunk, ref cubePosiInWorld, chunk);
                                break;
                            case enuCubeFamilly.Liquid:
                                //Default linked to : CubeMeshFactory.GenLiquidCubeFace;
                                cubeProfile.CreateLiquidCubeMesh(ref currentCube, cubeFace, ref cubePosiInChunk, ref cubePosiInWorld, chunk);
                                break;
                            case enuCubeFamilly.Other:
                                break;
                        }

                    }
                }
            }

        }

        #endregion
    }
}
