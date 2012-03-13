using System;
using Amib.Threading;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Worlds.Cubes;
using Utopia.Entities;
using System.Collections.Generic;
using SharpDX;
using Utopia.Entities.Sprites;
using Ninject;
using Utopia.Shared.Enums;
using Utopia.Shared.Settings;
using S33M3DXEngine.Threading;
using S33M3Resources.Structs;
using S33M3Resources.Struct.Vertex;

namespace Utopia.Worlds.Chunks.ChunkMesh
{
    public class ChunkMeshManager : IChunkMeshManager
    {
        #region private variables
        private CreateChunkMeshDelegate _createChunkMeshDelegate;
        private delegate void CreateChunkMeshDelegate(VisualChunk chunk);
        private readonly VisualWorldParameters _visualWorldParameters;
        private readonly SingleArrayChunkContainer _cubesHolder;
        private ICubeMeshFactory _solidCubeMeshFactory;
        private ICubeMeshFactory _liquidCubeMeshFactory;
        #endregion

        #region public variables/properties
        #endregion

        public ChunkMeshManager(VisualWorldParameters visualWorldParameters, SingleArrayChunkContainer cubesHolder, [Named("SolidCubeMeshFactory")] ICubeMeshFactory solidCubeMeshFactory, [Named("LiquidCubeMeshFactory")] ICubeMeshFactory liquidCubeMeshFactory)
        {
            _visualWorldParameters = visualWorldParameters;
            _cubesHolder = cubesHolder;
            _solidCubeMeshFactory = solidCubeMeshFactory;
            _liquidCubeMeshFactory = liquidCubeMeshFactory;
            Intialize();
        }

        #region Public methods
        public void CreateChunkMesh(VisualChunk chunk, bool async)
        {
            if (async)
            {
                chunk.ThreadStatus = ThreadStatus.Locked;
                SmartThread.ThreadPool.QueueWorkItem(CreateChunkMeshThreaded, chunk, chunk.ThreadPriority);
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
            _createChunkMeshDelegate = new CreateChunkMeshDelegate(CreateChunkMeshThreaded);
        }

        //Create the landscape for the chunk
        private void CreateChunkMeshThreaded(VisualChunk visualChunk)
        {
            CreateCubeMeshes(visualChunk);

            visualChunk.State = ChunkState.MeshesChanged;
            visualChunk.ThreadStatus = ThreadStatus.Idle;
        }

        //Creation of the cubes meshes, Face type by face type
        private void CreateCubeMeshes(VisualChunk chunk)
        {
            //Instanciate the various collection objects that wil be used during the cube mesh creations
            chunk.InitializeChunkBuffers();

            GenerateCubesFace(CubeFaces.Front, chunk);
            GenerateCubesFace(CubeFaces.Back, chunk);
            GenerateCubesFace(CubeFaces.Bottom, chunk);
            GenerateCubesFace(CubeFaces.Top, chunk);
            GenerateCubesFace(CubeFaces.Left, chunk);
            GenerateCubesFace(CubeFaces.Right, chunk);

            chunk.OnChunkMeshUpdated();

            GenerateStaticEntitiesMesh(chunk);
        }

        private void GenerateCubesFace(CubeFaces cubeFace, VisualChunk chunk)
        {
            TerraCube currentCube, neightborCube;
            CubeProfile cubeProfile;

            Vector4B cubePosiInChunk;
            Vector3I cubePosiInWorld;
            int XWorld, YWorld, ZWorld;
            int neightborCubeIndex;

            int baseCubeIndex = _cubesHolder.Index(chunk.CubeRange.Min.X, chunk.CubeRange.Min.Y, chunk.CubeRange.Min.Z);
            int cubeIndexX = baseCubeIndex;
            int cubeIndexZ = baseCubeIndex;
            int cubeIndex = baseCubeIndex;

            for (int x = 0; x < AbstractChunk.ChunkSize.X; x++)
            {
                XWorld = (x + chunk.CubeRange.Min.X);
                if (x != 0)
                {
                    cubeIndexX += _cubesHolder.MoveX;
                    cubeIndexZ = cubeIndexX;
                    cubeIndex = cubeIndexX;
                }

                for (int z = 0; z < AbstractChunk.ChunkSize.Z; z++)
                {
                    ZWorld = (z + chunk.CubeRange.Min.Z);

                    if (z != 0)
                    {
                        cubeIndexZ += _cubesHolder.MoveZ;
                        cubeIndex = cubeIndexZ;
                    }

                    for (int y = 0; y < chunk.CubeRange.Max.Y; y++)
                    {

                        //_cubeRange in fact identify the chunk, the chunk position in the world being _cubeRange.Min
                        YWorld = (y + chunk.CubeRange.Min.Y);

              
                        if (y != 0)
                        {
                            cubeIndex += _cubesHolder.MoveY;
                        }

                        //_cubesHolder.Cubes[] is the BIG table containing all terraCube in the visible world.
                        //For speed access, I use an array with only one dimension, thus the table index must be computed from the X, Y, Z position of the terracube.
                        //Computing myself this index, is faster than using an array defined as [x,y,z]
                       
                        //Terra Cube contain only the data that are variables, and could be different between 2 cube.
                        currentCube = _cubesHolder.Cubes[cubeIndex];

                        // ? Am I an Air Cube ? ==> Default Value, not needed to render !
                        if (currentCube.Id == CubeId.Air) continue;

                        //The Cube profile contain the value that are fixed for a block type.
                        cubeProfile = GameSystemSettings.Current.Settings.CubesProfile[currentCube.Id];

                        cubePosiInWorld = new Vector3I(XWorld, YWorld, ZWorld);
                        cubePosiInChunk = new Vector4B(x, y, z);

                        //Check to see if the face needs to be generated or not !
                        //Border Chunk test ! ==> Don't generate faces that are "border" chunks
                        //BorderChunk value is true if the chunk is at the border of the visible world.
                        switch (cubeFace)
                        {
                            case CubeFaces.Back:
                                if (ZWorld - 1 < _visualWorldParameters.WorldRange.Min.Z) continue;
                                //neightborCubeIndex = cubeIndex - _cubesHolder.MoveZ;
                                neightborCubeIndex = _cubesHolder.FastIndex(cubeIndex, ZWorld, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1);
                                break;
                            case CubeFaces.Front:
                                if (ZWorld + 1 >= _visualWorldParameters.WorldRange.Max.Z) continue;
                                neightborCubeIndex = _cubesHolder.FastIndex(cubeIndex, ZWorld, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1);
                                break;
                            case CubeFaces.Bottom:
                                if (YWorld - 1 < 0) continue;
                                neightborCubeIndex = cubeIndex - _cubesHolder.MoveY;
                                
                                break;
                            case CubeFaces.Top:
                                if (YWorld + 1 >= _visualWorldParameters.WorldRange.Max.Y) continue;
                                neightborCubeIndex = cubeIndex + _cubesHolder.MoveY;
                                
                                break;
                            case CubeFaces.Left:
                                if (XWorld - 1 < _visualWorldParameters.WorldRange.Min.X) continue;
                                neightborCubeIndex = _cubesHolder.FastIndex(cubeIndex, XWorld, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1);
                                
                                break;
                            case CubeFaces.Right:
                                if (XWorld + 1 >= _visualWorldParameters.WorldRange.Max.X) continue;
                                neightborCubeIndex = _cubesHolder.FastIndex(cubeIndex, XWorld, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1);
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
                        neightborCube = _cubesHolder.Cubes[neightborCubeIndex];

                        switch (cubeProfile.CubeFamilly)
                        {
                            case enuCubeFamilly.Solid:
                                //Other delegate.
                                //Default linked to : CubeMeshFactory.GenSolidCubeFace;
                                if (!_solidCubeMeshFactory.FaceGenerationCheck(ref currentCube, ref cubePosiInWorld, cubeFace, ref neightborCube, _visualWorldParameters.WorldParameters.SeaLevel)) continue;
                                _solidCubeMeshFactory.GenCubeFace(ref currentCube, cubeFace, ref cubePosiInChunk, ref cubePosiInWorld, chunk);
                                break;
                            case enuCubeFamilly.Liquid:
                                //Default linked to : CubeMeshFactory.GenLiquidCubeFace;
                                if (!_liquidCubeMeshFactory.FaceGenerationCheck(ref currentCube, ref cubePosiInWorld, cubeFace, ref neightborCube, _visualWorldParameters.WorldParameters.SeaLevel)) continue;
                                _liquidCubeMeshFactory.GenCubeFace(ref currentCube, cubeFace, ref cubePosiInChunk, ref cubePosiInWorld, chunk);
                                break;
                            case enuCubeFamilly.Other:
                                break;
                        }

                    }
                }
            }
        }

        private void GenerateStaticEntitiesMesh(VisualChunk chunk)
        {
            chunk.StaticSpritesVertices.Clear();
            chunk.StaticSpritesIndices.Clear();
            //Loop trhough all Visual Entity and create meshes from them.
            foreach (VisualSpriteEntity SpriteEntities in chunk.VisualSpriteEntities)
            {
                GenerateEntitySprite(chunk.StaticSpritesVertices, chunk.StaticSpritesIndices, SpriteEntities.SpriteEntity.Format, SpriteEntities);
            }
        }

        private void GenerateEntitySprite(List<VertexSprite3D> vertices, List<ushort> indices, SpriteFormat spriteFormat, VisualSpriteEntity sprite)
        {
            Vector3 spriteLocation = sprite.SpriteEntity.Position.AsVector3();
            int baseIndex = vertices.Count;

           Vector3 normalSize = sprite.SpriteEntity.Size;
           Vector3 normalHalfSize = new Vector3(normalSize.X / 2, normalSize.Y, normalSize.Z / 2);
           Vector3 normalQuartSize = new Vector3(normalSize.X / 4, normalSize.Y, normalSize.Z / 4);

            switch (spriteFormat)
            {
                case SpriteFormat.Single:
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));
                    break;
                case SpriteFormat.Billboard:
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X, spriteLocation.Y, spriteLocation.Z, 4), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), normalSize));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X, spriteLocation.Y, spriteLocation.Z, 1), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), normalSize));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X, spriteLocation.Y, spriteLocation.Z, 3), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), normalSize));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X, spriteLocation.Y, spriteLocation.Z, 2), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), normalSize));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));
                    break;
                case SpriteFormat.Cross:
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));
                    
                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));

                    baseIndex += 4;

                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));
                    break;
                case SpriteFormat.Triangle:

                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));

                    baseIndex += 4;

                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));

                    baseIndex += 4;

                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));

                    break;
                case SpriteFormat.Quad:

                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z + normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z + normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));

                    baseIndex += 4;

                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));

                    baseIndex += 4;

                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));

                    baseIndex += 4;

                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId)));
                    vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId)));

                    indices.Add((ushort)(baseIndex + 0));
                    indices.Add((ushort)(baseIndex + 1));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 3));
                    indices.Add((ushort)(baseIndex + 2));
                    indices.Add((ushort)(baseIndex + 1));
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
