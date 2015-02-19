using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Chunks;
using SharpDX;
using Utopia.Worlds.Liquid;
using Utopia.Shared.Enums;
using Utopia.Shared.Settings;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using Utopia.Shared.Chunks.Tags;
using Utopia.Resources.VertexFormats;
using Utopia.Shared.Configuration;
using Utopia.Shared.World;

namespace Utopia.Worlds.Cubes
{
    public class LiquidCubeMeshFactory : ICubeMeshFactory
    {
        private SingleArrayChunkContainer _cubesHolder;
        private VisualWorldParameters _wp;

        public LiquidCubeMeshFactory(SingleArrayChunkContainer cubesHolder, VisualWorldParameters wp)
        {
            _cubesHolder = cubesHolder;
            _wp = wp;
        }

         //Default Face Generation Checks !
        public bool FaceGenerationCheck(ref TerraCube cube, ref Vector3I cubePosiInWorld, CubeFaces cubeFace, ref TerraCube NeightBorFaceCube)
        {
            //if (cubeFace != CubeFaces.Top)
            //{
            //    blockProfile NeightBorProfile = _wp.WorldParameters.Configuration.CubeProfiles[NeightBorFaceCube.Id];

            //    if ((!NeightBorProfile.IsBlockingLight && NeightBorProfile.CubeFamilly != enuCubeFamilly.Liquid))
            //    {
            //        return true;
            //    }
            //}else{
            //    if (cubePosiInWorld.Y == seaLevel || NeightBorFaceCube.Id == WorldConfiguration.CubeId.Air)
            //    {
            //        return true;
            //    }
            //}

            BlockProfile NeightBorProfile = _wp.WorldParameters.Configuration.BlockProfiles[NeightBorFaceCube.Id];

            if ((!NeightBorProfile.IsBlockingLight && NeightBorProfile.CubeFamilly != enuCubeFamilly.Liquid))
            {
                return true;
            }

            return false;
        }

        public void GenCubeFace(ref TerraCube cube, CubeFaces cubeFace, ref Vector4B cubePosition, ref Vector3I cubePosiInWorld, VisualChunk2D chunk, ref TerraCube topCube, Dictionary<long, int> verticeDico)
        {
            int yBlockOffsetAsInt = 0;
            float yBlockOffset = 0;
            int verticeCubeOffset = chunk.Graphics.LiquidCubeVertices.Count;
            int indiceCubeOffset = chunk.Graphics.LiquidCubeIndices.Count;
            ByteColor newColor = cube.EmissiveColor;
            BlockTag tag = null;

            BlockProfile blockProfile = _wp.WorldParameters.Configuration.BlockProfiles[cube.Id];
            //Get the Cube Tag Informations
            if (blockProfile.IsTaggable)
            {
                tag = chunk.BlockData.GetTag(new Vector3I(cubePosition.X, cubePosition.Y, cubePosition.Z));
            }

            bool IsEmissiveColor = blockProfile.IsEmissiveColorLightSource;
            bool vertexInDico;

            //Les 4 vertex de ma face.... en fct de leur position dans le cube leur valeur en Z va changer ! (Face Top, Bottom, ...
            Vector4B topLeft;
            Vector4B topRight;
            Vector4B bottomLeft;
            Vector4B bottomRight;

            //GetBlock Offset
            if (blockProfile.IsTaggable && tag is ICubeYOffsetModifier)
            {
                yBlockOffset = ((ICubeYOffsetModifier)tag).YOffset;
            }
            else
            {
                //Add a naturel Offset to StillWater when touching water at the surface !
                if(topCube.Id != cube.Id) yBlockOffset = (float)blockProfile.YBlockOffset;
            }

            yBlockOffsetAsInt = (int)(yBlockOffset * 255);

            ChunkColumnInfo chunkInfo = chunk.BlockData.GetColumnInfo(new Vector2I(cubePosition.X, cubePosition.Z));

            Vector4B vertexInfo2 = new Vector4B(chunkInfo.Moisture, chunkInfo.Temperature, (byte)0, (byte)0);
            Vector4B vertexInfo1 = new Vector4B((byte)cubeFace,
                                                      (byte)0,          //Is "UP" vertex
                                                      blockProfile.BiomeColorArrayTexture,
                                                      (byte)0);

            long hashVertex;
            int generatedVertex = 0;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;

            int[] ind = new int[9];

            //Get the index of the current cube.
            int baseIndex = _cubesHolder.Index(ref cubePosiInWorld);

            switch (cubeFace)
            {
                case CubeFaces.Front:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.Z, ind, true);

                    ByteColor Back_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor;
                    ByteColor BackLeft_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor;
                    ByteColor BackRight_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor;
                    ByteColor BackTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor;
                    ByteColor BackBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor;
                    ByteColor BackLeftTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor;
                    ByteColor BackRightTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor;
                    ByteColor BackLeftBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor;
                    ByteColor BackRightBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor;

                    topLeft = cubePosition + new Vector4B(0, 1, 1, yBlockOffsetAsInt);
                    topRight = cubePosition + new Vector4B(1, 1, 1, yBlockOffsetAsInt);
                    bottomLeft = cubePosition + new Vector4B(0, 0, 1, yBlockOffsetAsInt);
                    bottomRight = cubePosition + new Vector4B(1, 0, 1, yBlockOffsetAsInt);

                    vertexInfo2.Z = blockProfile.Tex_Front.AnimationSpeed;
                    vertexInfo2.W = blockProfile.Tex_Front.Texture.AnimationFrames;

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset0);
                    if (vertexInDico == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackTop_Cube, BackLeftTop_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, blockProfile.Tex_Front.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset1);
                    if (vertexInDico == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackTop_Cube, BackRightTop_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, blockProfile.Tex_Front.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset2);
                    if (vertexInDico == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackBottom_Cube, BackLeftBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, blockProfile.Tex_Front.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset3);
                    if (vertexInDico == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackBottom_Cube, BackRightBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, blockProfile.Tex_Front.TextureArrayId, ref  newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));

                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));

                    break;
                case CubeFaces.Back:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.Z, ind, true);
                    ByteColor Front_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor;
                    ByteColor FrontLeft_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor;
                    ByteColor FrontRight_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor;
                    ByteColor FrontTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor;
                    ByteColor FrontBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor;
                    ByteColor FrontLeftTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor;
                    ByteColor FrontRightTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor;
                    ByteColor FrontLeftBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor;
                    ByteColor FrontRightBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor;

                    topLeft = cubePosition + new Vector4B(1, 1, 0, yBlockOffsetAsInt);
                    topRight = cubePosition + new Vector4B(0, 1, 0, yBlockOffsetAsInt);
                    bottomLeft = cubePosition + new Vector4B(1, 0, 0, yBlockOffsetAsInt);
                    bottomRight = cubePosition + new Vector4B(0, 0, 0, yBlockOffsetAsInt);

                    vertexInfo2.Z = blockProfile.Tex_Back.AnimationSpeed;
                    vertexInfo2.W = blockProfile.Tex_Back.Texture.AnimationFrames;

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset0);
                    if (vertexInDico == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontLeftTop_Cube, FrontLeft_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, blockProfile.Tex_Back.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset1);
                    if (vertexInDico == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontRight_Cube, FrontRightTop_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, blockProfile.Tex_Back.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset2);
                    if (vertexInDico == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontLeft_Cube, FrontLeftBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, blockProfile.Tex_Back.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset3);
                    if (vertexInDico == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontRight_Cube, FrontRightBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, blockProfile.Tex_Back.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));

                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset3));

                    break;
                case CubeFaces.Top:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.Y, ind, true);

                    ByteColor Bottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor;
                    ByteColor BottomLeft_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor;
                    ByteColor BottomRight_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor;
                    ByteColor BottomTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor;
                    ByteColor BottomBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor;
                    ByteColor BottomLeftTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor;
                    ByteColor BottomRightTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor;
                    ByteColor BottomLeftBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor;
                    ByteColor BottomRightBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor;

                    topLeft = cubePosition + new Vector4B(0, 1, 0, yBlockOffsetAsInt);
                    topRight = cubePosition + new Vector4B(1, 1, 0, yBlockOffsetAsInt);
                    bottomLeft = cubePosition + new Vector4B(0, 1, 1, yBlockOffsetAsInt);
                    bottomRight = cubePosition + new Vector4B(1, 1, 1, yBlockOffsetAsInt);

                    vertexInfo2.Z = blockProfile.Tex_Top.AnimationSpeed;
                    vertexInfo2.W = blockProfile.Tex_Top.Texture.AnimationFrames;

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset0);
                    if (vertexInDico == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomLeft_Cube, BottomLeftTop_Cube, BottomTop_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, blockProfile.Tex_Top.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset1);
                    if (vertexInDico == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomRight_Cube, BottomBottom_Cube, BottomRightBottom_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, blockProfile.Tex_Top.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset2);
                    if (vertexInDico == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomBottom_Cube, BottomLeft_Cube, BottomLeftBottom_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, blockProfile.Tex_Top.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset3);
                    if (vertexInDico == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomTop_Cube, BottomRight_Cube, BottomRightTop_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, blockProfile.Tex_Top.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));

                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    break;

                case CubeFaces.Bottom:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.Y, ind, true);

                    ByteColor Top_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor;
                    ByteColor TopLeft_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor;
                    ByteColor TopRight_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor;
                    ByteColor TopTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor;
                    ByteColor TopBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor;
                    ByteColor TopLeftTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor;
                    ByteColor TopRightTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor;
                    ByteColor TopLeftBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor;
                    ByteColor TopRightBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor;

                    topLeft = cubePosition + new Vector4B(0, 0, 1, yBlockOffsetAsInt);
                    topRight = cubePosition + new Vector4B(1, 0, 1, yBlockOffsetAsInt);
                    bottomLeft = cubePosition + new Vector4B(0, 0, 0, yBlockOffsetAsInt);
                    bottomRight = cubePosition + new Vector4B(1, 0, 0, yBlockOffsetAsInt);

                    vertexInfo2.Z = blockProfile.Tex_Bottom.AnimationSpeed;
                    vertexInfo2.W = blockProfile.Tex_Bottom.Texture.AnimationFrames;

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset0);
                    if (vertexInDico == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopLeft_Cube, TopLeftBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, blockProfile.Tex_Bottom.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset1);
                    if (vertexInDico == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopLeft_Cube, TopLeftTop_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, blockProfile.Tex_Bottom.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset2);
                    if (vertexInDico == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopRight_Cube, TopRightBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, blockProfile.Tex_Bottom.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset3);
                    if (vertexInDico == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopRight_Cube, TopRightTop_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, blockProfile.Tex_Bottom.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));

                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    break;

                case CubeFaces.Left:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.X, ind, true);

                    ByteColor Right_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor;
                    ByteColor RightLeft_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor;
                    ByteColor RightRight_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor;
                    ByteColor RightTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor;
                    ByteColor RightBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor;
                    ByteColor RightLeftTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor;
                    ByteColor RightRightTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor;
                    ByteColor RightLeftBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor;
                    ByteColor RightRightBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor;

                    topLeft = cubePosition + new Vector4B(0, 1, 0, yBlockOffsetAsInt);
                    bottomRight = cubePosition + new Vector4B(0, 0, 1, yBlockOffsetAsInt);
                    bottomLeft = cubePosition + new Vector4B(0, 0, 0, yBlockOffsetAsInt);
                    topRight = cubePosition + new Vector4B(0, 1, 1, yBlockOffsetAsInt);

                    vertexInfo2.Z = blockProfile.Tex_Left.AnimationSpeed;
                    vertexInfo2.W = blockProfile.Tex_Left.Texture.AnimationFrames;

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset0);
                    if (vertexInDico == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightRight_Cube, RightRightTop_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, blockProfile.Tex_Left.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset1);
                    if (vertexInDico == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightLeft_Cube, RightLeftTop_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, blockProfile.Tex_Left.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset2);
                    if (vertexInDico == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightRight_Cube, RightRightBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, blockProfile.Tex_Left.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset3);
                    if (vertexInDico == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightLeft_Cube, RightLeftBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, blockProfile.Tex_Left.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset3));

                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    break;
                case CubeFaces.Right:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.X, ind, true);

                    ByteColor Left_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor;
                    ByteColor LeftLeft_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor;
                    ByteColor LefttRight_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor;
                    ByteColor LeftTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor;
                    ByteColor LeftBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor;
                    ByteColor LeftLeftTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor;
                    ByteColor LeftRightTop_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor;
                    ByteColor LeftLeftBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor;
                    ByteColor LeftRightBottom_Cube = _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor;

                    topLeft = cubePosition + new Vector4B(1, 1, 1, yBlockOffsetAsInt);
                    topRight = cubePosition + new Vector4B(1, 1, 0, yBlockOffsetAsInt);
                    bottomLeft = cubePosition + new Vector4B(1, 0, 1, yBlockOffsetAsInt);
                    bottomRight = cubePosition + new Vector4B(1, 0, 0, yBlockOffsetAsInt);

                    vertexInfo2.Z = blockProfile.Tex_Right.AnimationSpeed;
                    vertexInfo2.W = blockProfile.Tex_Right.Texture.AnimationFrames;

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset0);
                    if (vertexInDico == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LefttRight_Cube, LeftRightTop_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, blockProfile.Tex_Right.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffsetAsInt << 48);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset1);
                    if (vertexInDico == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LeftLeft_Cube, LeftLeftTop_Cube);
                        vertexInfo1.Y = 1;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, blockProfile.Tex_Right.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset2);
                    if (vertexInDico == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LeftLeft_Cube, LeftLeftBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, blockProfile.Tex_Right.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    vertexInDico = verticeDico.TryGetValue(hashVertex, out vertexOffset3);
                    if (vertexInDico == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        verticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LefttRight_Cube, LeftRightBottom_Cube);
                        vertexInfo1.Y = 0;
                        chunk.Graphics.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, blockProfile.Tex_Right.TextureArrayId, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset3));

                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.Graphics.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    break;
            }
        }
    }
}
