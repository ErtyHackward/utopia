using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Worlds.Cubes;
using Utopia.Entities;
using System.Collections.Generic;
using SharpDX;
using Ninject;
using Utopia.Shared.Enums;
using Utopia.Shared.Settings;
using S33M3DXEngine.Threading;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using Utopia.Shared.Chunks.Tags;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Configuration;

namespace Utopia.Worlds.Chunks.ChunkMesh
{
    public class ChunkMeshManager : IChunkMeshManager
    {
        #region private variables
        private readonly VisualWorldParameters _visualWorldParameters;
        private readonly SingleArrayChunkContainer _cubesHolder;
        private ICubeMeshFactory _solidCubeMeshFactory;
        private ICubeMeshFactory _liquidCubeMeshFactory;
        #endregion

        #region public variables/properties
        public IWorldChunks2D WorldChunks { get; set; }
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
        public void CreateChunkMesh(VisualChunk chunk)
        {
            CreateCubeMeshes(chunk);
            chunk.State = ChunkState.MeshesChanged;
        }
        
        #endregion

        #region Private methods
        private void Intialize()
        {
        }

        //Creation of the cubes meshes, Face type by face type
        private void CreateCubeMeshes(VisualChunk chunk)
        {
            //Instanciate the various collection objects that wil be used during the cube mesh creations
            chunk.Graphics.InitializeChunkBuffers();

            GenerateCubesFace(CubeFaces.Front, chunk);
            GenerateCubesFace(CubeFaces.Back, chunk);
            GenerateCubesFace(CubeFaces.Bottom, chunk);
            GenerateCubesFace(CubeFaces.Top, chunk);
            GenerateCubesFace(CubeFaces.Left, chunk);
            GenerateCubesFace(CubeFaces.Right, chunk);

            chunk.OnChunkMeshUpdated();
        }

        private void GenerateCubesFace(CubeFaces cubeFace, VisualChunk chunk)
        {
            TerraCube currentCube, neightborCube, topCube;
            BlockProfile blockProfile, neightborCubeProfile;

            Vector4B cubePosiInChunk = new Vector4B(0, 0, 0, 1);
            Vector3I cubePosiInWorld;
            int XWorld, YWorld, ZWorld;
            int neightborCubeIndex;

            int baseCubeIndex = _cubesHolder.Index(chunk.CubeRange.Position.X, chunk.CubeRange.Position.Y, chunk.CubeRange.Position.Z);
            int cubeIndexX = baseCubeIndex;
            int cubeIndexZ = baseCubeIndex;
            int cubeIndex = baseCubeIndex;

            var worldRangeMaxX = _visualWorldParameters.WorldRange.Max.X;
            var worldRangeMaxY = _visualWorldParameters.WorldRange.Max.Y;
            var worldRangeMaxZ = _visualWorldParameters.WorldRange.Max.Z;
            int xNeight, yNeight, zNeight;

            byte yMin = (byte)(chunk.Graphics.SliceValue == -1 ? 0 : Math.Max(0, chunk.Graphics.SliceValue - 5));
            byte yMax = (byte)(chunk.Graphics.SliceValue == -1 ? chunk.CubeRange.Size.Y : chunk.Graphics.SliceValue);

            Dictionary<long, int> verticeDico = new Dictionary<long, int>();

            for (byte x = 0; x < AbstractChunk.ChunkSize.X; x++)
            {
                XWorld = (x + chunk.CubeRange.Position.X);
                if (x != 0)
                {
                    cubeIndexX += _cubesHolder.MoveX;
                    cubeIndexZ = cubeIndexX;
                    cubeIndex = cubeIndexX;
                }

                for (byte z = 0; z < AbstractChunk.ChunkSize.Z; z++)
                {
                    ZWorld = (z + chunk.CubeRange.Position.Z);

                    if (z != 0)
                    {
                        cubeIndexZ += _cubesHolder.MoveZ;
                        cubeIndex = cubeIndexZ;
                    }

                    for (byte y = yMin; y < yMax; y++)
                    {

                        //_cubeRange in fact identify the chunk, the chunk position in the world being _cubeRange.Min
                        YWorld = (y + chunk.CubeRange.Position.Y);

                        if (y != yMin)
                        {
                            cubeIndex += _cubesHolder.MoveY;
                        }
                        else
                        {
                            cubeIndex += _cubesHolder.MoveY * yMin;
                        }

                        //_cubesHolder.Cubes[] is the BIG table containing all terraCube in the visible world.
                        //For speed access, I use an array with only one dimension, thus the table index must be computed from the X, Y, Z position of the terracube.
                        //Computing myself this index, is faster than using an array defined as [x,y,z]
                       
                        //Terra Cube contain only the data that are variables, and could be different between 2 cube.
                        currentCube = _cubesHolder.Cubes[cubeIndex];

                        // ? Am I an Air Cube ? ==> Default Value, not needed to render !
                        if (currentCube.Id == WorldConfiguration.CubeId.Air) continue;

                        //The Cube profile contain the value that are fixed for a block type.
                        blockProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[currentCube.Id];

                        cubePosiInWorld = new Vector3I(XWorld, YWorld, ZWorld);
                        cubePosiInChunk.X = x; //cubePosiInChunk = new Vector4B(x, y, z);
                        cubePosiInChunk.Y = y;
                        cubePosiInChunk.Z = z;
                        //Check to see if the face needs to be generated or not !
                        //Border Chunk test ! ==> Don't generate faces that are "border" chunks
                        //BorderChunk value is true if the chunk is at the border of the visible world.
                        int topCubeIndex = cubeIndex + _cubesHolder.MoveY;

                        xNeight = x;
                        yNeight = y;
                        zNeight = z;

                        switch (cubeFace)
                        {
                            case CubeFaces.Back:
                                if (ZWorld - 1 < _visualWorldParameters.WorldRange.Position.Z) continue;
                                neightborCubeIndex = _cubesHolder.FastIndex(cubeIndex, ZWorld, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1);
                                zNeight--;
                                break;
                            case CubeFaces.Front:
                                if (ZWorld + 1 >= worldRangeMaxZ) continue;
                                neightborCubeIndex = _cubesHolder.FastIndex(cubeIndex, ZWorld, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1);
                                zNeight++;
                                break;
                            case CubeFaces.Bottom:
                                if (YWorld - 1 < yMin) continue;
                                neightborCubeIndex = cubeIndex - _cubesHolder.MoveY;
                                yNeight--;
                                break;
                            case CubeFaces.Top:
                                if (YWorld + 1 > worldRangeMaxY) continue;
                                neightborCubeIndex = topCubeIndex;
                                yNeight++;
                                break;
                            case CubeFaces.Left:
                                if (XWorld - 1 < _visualWorldParameters.WorldRange.Position.X) continue;
                                neightborCubeIndex = _cubesHolder.FastIndex(cubeIndex, XWorld, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1);
                                xNeight--;
                                break;
                            case CubeFaces.Right:
                                if (XWorld + 1 >= worldRangeMaxX) continue;
                                neightborCubeIndex = _cubesHolder.FastIndex(cubeIndex, XWorld, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1);
                                xNeight++;
                                break;
                            default:
                                throw new NullReferenceException();
                        }

                        if (YWorld + 1 < yMax)
                        {
                            neightborCube = _cubesHolder.Cubes[neightborCubeIndex];
                        }
                        else
                        {
                            neightborCube = new TerraCube(WorldConfiguration.CubeId.Air);
                        }

                        neightborCubeProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[neightborCube.Id];

                        //Check if a tag is present and ICubeYOffsetModifier is implementad by the tag;
                        float cubeYOffset = (float)blockProfile.YBlockOffset;    //Natural YOffset of the Cube
                        if (blockProfile.IsTaggable)                             
                        {
                            BlockTag tag = chunk.BlockData.GetTag(new Vector3I(x, y, z));
                            ICubeYOffsetModifier tagOffset = tag as ICubeYOffsetModifier;   //If block is taggle then it will overide the natural UOffset
                            if (tagOffset != null)
                            {
                                cubeYOffset = tagOffset.YOffset;
                            }
                        }

                        float neightborcubeYOffset = (float)neightborCubeProfile.YBlockOffset;
                        if (neightborCubeProfile.IsTaggable)
                        {
                            //Find the chunk where this neightboor is located !! (Could be a chunk next to this one !)
                            Vector3I NeightCubeWorldPosition = new Vector3I(xNeight + chunk.CubeRange.Position.X, yNeight, zNeight + chunk.CubeRange.Position.Z);
                            VisualChunk neighbChunk = WorldChunks.GetChunk(NeightCubeWorldPosition);

                            BlockTag tag = neighbChunk.BlockData.GetTag(new Vector3I(NeightCubeWorldPosition.X - neighbChunk.CubeRange.Position.X, yNeight, NeightCubeWorldPosition.Z - neighbChunk.CubeRange.Position.Z));
                            ICubeYOffsetModifier tagOffset = tag as ICubeYOffsetModifier;
                            if (tagOffset != null)
                            {
                                neightborcubeYOffset = tagOffset.YOffset;
                            }
                        }

                        bool yOffsetDiff = (cubeYOffset < neightborcubeYOffset && cubeFace != CubeFaces.Top) || (cubeYOffset > 0 && cubeFace == CubeFaces.Top); // && neightborCube.Id != currentCube.Id);

                        switch (blockProfile.CubeFamilly)
                        {
                            case enuCubeFamilly.Solid:
                                //Default linked to : CubeMeshFactory.GenSolidCubeFace;
                                if (!yOffsetDiff && !_solidCubeMeshFactory.FaceGenerationCheck(ref currentCube, ref cubePosiInWorld, cubeFace, ref neightborCube)) continue;
                                topCube = _cubesHolder.Cubes[topCubeIndex];
                                _solidCubeMeshFactory.GenCubeFace(ref currentCube, cubeFace, ref cubePosiInChunk, ref cubePosiInWorld, chunk, ref topCube, verticeDico);
                                break;
                            case enuCubeFamilly.Liquid:
                                //Default linked to : CubeMeshFactory.GenLiquidCubeFace;

                                //In case of Water block, never display the bottom or up face if both cube are of the same type !
                                if (cubeFace == CubeFaces.Top || cubeFace == CubeFaces.Bottom)
                                {
                                    if (neightborCube.Id == currentCube.Id) continue;
                                }

                                if (!yOffsetDiff && !_liquidCubeMeshFactory.FaceGenerationCheck(ref currentCube, ref cubePosiInWorld, cubeFace, ref neightborCube)) continue;
                                topCube = _cubesHolder.Cubes[topCubeIndex];
                                _liquidCubeMeshFactory.GenCubeFace(ref currentCube, cubeFace, ref cubePosiInChunk, ref cubePosiInWorld, chunk, ref topCube, verticeDico);
                                break;
                            case enuCubeFamilly.Other:
                                break;
                        }

                    }
                }
            }
            verticeDico.Clear();
        }

        //private void GenerateStaticEntitiesMesh(VisualChunk chunk)
        //{
        //    chunk.StaticSpritesVertices.Clear();
        //    chunk.StaticSpritesIndices.Clear();
        //    //Loop trhough all Visual Entity and create meshes from them.
        //    foreach (VisualSpriteEntity SpriteEntities in chunk.VisualSpriteEntities)
        //    {
        //        GenerateEntitySprite(chunk, chunk.StaticSpritesVertices, chunk.StaticSpritesIndices, SpriteEntities.SpriteEntity.Format, SpriteEntities);
        //    }
        //}

        //private void GenerateEntitySprite(VisualChunk chunk, List<VertexSprite3D> vertices, List<ushort> indices, SpriteFormat spriteFormat, VisualSpriteEntity sprite)
        //{
        //    Vector3 spriteLocation = sprite.SpriteEntity.Position.AsVector3();
        //    int baseIndex = vertices.Count;

        //   Vector3 normalSize = sprite.SpriteEntity.Size;
        //   Vector3 normalHalfSize = new Vector3(normalSize.X / 2, normalSize.Y, normalSize.Z / 2);
        //   Vector3 normalQuartSize = new Vector3(normalSize.X / 4, normalSize.Y, normalSize.Z / 4);
        //   Vector4B biomeInfo = new Vector4B();
        //   if (sprite.SpriteEntity is IBlockLinkedEntity)
        //   {
        //       //This is working only if the linked cube is inside the same chunk as the entity. 
        //       Vector3I linkedCubeWorldPosition = ((IBlockLinkedEntity)sprite.SpriteEntity).LinkedCube;
        //       Vector2I InChunkPosition = new Vector2I(linkedCubeWorldPosition.X - chunk.ChunkPositionBlockUnit.X, linkedCubeWorldPosition.Z - chunk.ChunkPositionBlockUnit.Y);
        //       var columnInfo = chunk.BlockData.GetColumnInfo(InChunkPosition);
        //       biomeInfo.X = columnInfo.Moisture;
        //       biomeInfo.Y = columnInfo.Temperature; 
        //   }
        //    switch (spriteFormat)
        //    {
        //        case SpriteFormat.Single:
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));
        //            break;
        //        case SpriteFormat.Billboard:
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X, spriteLocation.Y, spriteLocation.Z, 4), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), normalSize, biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X, spriteLocation.Y, spriteLocation.Z, 1), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), normalSize, biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X, spriteLocation.Y, spriteLocation.Z, 3), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), normalSize, biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X, spriteLocation.Y, spriteLocation.Z, 2), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), normalSize, biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));
        //            break;
        //        case SpriteFormat.Cross:
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
                    
        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));

        //            baseIndex += 4;

        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));
        //            break;
        //        case SpriteFormat.Triangle:

        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));

        //            baseIndex += 4;

        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));

        //            baseIndex += 4;

        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));

        //            break;
        //        case SpriteFormat.Quad:

        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z + normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z + normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));

        //            baseIndex += 4;

        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalHalfSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalQuartSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));

        //            baseIndex += 4;

        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X + normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));

        //            baseIndex += 4;

        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z - normalHalfSize.Z, 0), sprite.Color, new Vector3(0.0f, 0.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 1.0f, sprite.spriteTextureId), biomeInfo));
        //            vertices.Add(new VertexSprite3D(new Vector4(spriteLocation.X - normalQuartSize.X, spriteLocation.Y + normalSize.Y, spriteLocation.Z + normalHalfSize.Z, 0), sprite.Color, new Vector3(1.0f, 0.0f, sprite.spriteTextureId), biomeInfo));

        //            indices.Add((ushort)(baseIndex + 0));
        //            indices.Add((ushort)(baseIndex + 1));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 3));
        //            indices.Add((ushort)(baseIndex + 2));
        //            indices.Add((ushort)(baseIndex + 1));
        //            break;
        //        default:
        //            break;
        //    }
        //}

        #endregion
    }
}
