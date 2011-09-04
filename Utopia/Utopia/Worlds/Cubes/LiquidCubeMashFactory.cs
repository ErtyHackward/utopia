using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Worlds.Chunks;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Chunks;
using SharpDX;
using Utopia.Worlds.Liquid;

namespace Utopia.Worlds.Cubes
{
    public class LiquidCubeMashFactory : ICubeMeshFactory
    {
        private SingleArrayChunkContainer _cubesHolder;

        public LiquidCubeMashFactory(SingleArrayChunkContainer cubesHolder)
        {
            _cubesHolder = cubesHolder;
        }

        public void GenCubeFace(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, VisualChunk chunk)
        {
            int verticeCubeOffset = chunk.LiquidCubeVertices.Count;
            int indiceCubeOffset = chunk.LiquidCubeIndices.Count;
            ByteColor newColor = cube.EmissiveColor;

            VisualCubeProfile cubeProfile = VisualCubeProfile.CubesProfile[cube.Id];
            bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;

            //Les 4 vertex de ma face.... en fct de leur position dans le cube leur valeur en Z va changer ! (Face Top, Bottom, ...
            ByteVector4 topLeft;
            ByteVector4 topRight;
            ByteVector4 bottomLeft;
            ByteVector4 bottomRight;

            int cubeFaceType = (int)cubeFace;

            Vector4 vertexInfo2 = new Vector4(0, 0, 0, 0);
            ByteVector4 vertexInfo1 = new ByteVector4((byte)cubeFace,
                                                      (byte)cubeProfile.LiquidType,
                //cubeFace == CubeFace.Top || (cube.FloodingData == (byte)TerraFlooding.FloodDirection.Fall && cubeFace != CubeFace.Top && cubeFace != CubeFace.Bottom) ? (byte)cube.FloodingData : (byte)0,
                                                      cubeFace == CubeFace.Top ? (byte)cube.MetaData3 : (byte)FloodDirection.Fall,
                                                      (byte)0);

            long hashVertex;
            int generatedVertex = 0;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;

            int[] ind = new int[9];

            //Get the index of the current cube.
            int baseIndex = _cubesHolder.Index(ref cubePosiInWorld);

            switch (cubeFace)
            {
                case CubeFace.Front:

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

                    topLeft = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackTop_Cube, BackLeftTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackTop_Cube, BackRightTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackBottom_Cube, BackLeftBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackBottom_Cube, BackRightBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref  newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset3));

                    break;
                case CubeFace.Back:

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

                    topLeft = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontLeftTop_Cube, FrontLeft_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontRight_Cube, FrontRightTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontLeft_Cube, FrontLeftBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontRight_Cube, FrontRightBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));

                    break;
                case CubeFace.Top:

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

                    topLeft = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomLeft_Cube, BottomLeftTop_Cube, BottomTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomRight_Cube, BottomBottom_Cube, BottomRightBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomBottom_Cube, BottomLeft_Cube, BottomLeftBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomTop_Cube, BottomRight_Cube, BottomRightTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    break;

                case CubeFace.Bottom:

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

                    topLeft = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopLeft_Cube, TopLeftBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopLeft_Cube, TopLeftTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopRight_Cube, TopRightBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopRight_Cube, TopRightTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    break;

                case CubeFace.Left:

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

                    topLeft = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightRight_Cube, RightRightTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightLeft_Cube, RightLeftTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightRight_Cube, RightRightBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightLeft_Cube, RightLeftBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    break;
                case CubeFace.Right:

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

                    topLeft = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LefttRight_Cube, LeftRightTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LeftLeft_Cube, LeftLeftTop_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LeftLeft_Cube, LeftLeftBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LefttRight_Cube, LeftRightBottom_Cube);
                        chunk.LiquidCubeVertices.Add(new VertexCubeLiquid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.LiquidCubeIndices.Add((ushort)(vertexOffset2));
                    break;
            }
        }
    }
}
