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

namespace Utopia.Worlds.Cubes
{
    public class SolidCubeMeshFactory : ICubeMeshFactory
    {
        private SingleArrayChunkContainer _cubesHolder;

        public SolidCubeMeshFactory(SingleArrayChunkContainer cubesHolder)
        {
            _cubesHolder = cubesHolder;
        }

        //public void GenCubeFace_WihtoutLighting(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, VisualChunk chunk)
        //{
        //    int verticeCubeOffset = chunk.SolidCubeVertices.Count;
        //    int indiceCubeOffset = chunk.SolidCubeIndices.Count;

        //    VisualCubeProfile cubeProfile = VisualCubeProfile.CubesProfile[cube.Id];
        //    bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;

        //    ByteVector4 topLeft;
        //    ByteVector4 topRight;
        //    ByteVector4 bottomLeft;
        //    ByteVector4 bottomRight;

        //    int cubeFaceType = (int)cubeFace;

        //    ByteVector4 vertexInfo = new ByteVector4((byte)0, (byte)cubeFace, (byte)0, (byte)0);

        //    string hashVertex;
        //    int generatedVertex = 0;
        //    int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;
        //    ByteColor newColor = new ByteColor(255, 255, 255, 255);

        //    switch (cubeFace)
        //    {
        //        case CubeFace.Front:

        //            topLeft = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref  newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));

        //            break;
        //        case CubeFace.Back:

        //            topLeft = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));

        //            break;
        //        case CubeFace.Top:

        //            topLeft = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            break;

        //        case CubeFace.Bottom:

        //            topLeft = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            break;

        //        case CubeFace.Left:
        //            topLeft = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            break;
        //        case CubeFace.Right:

        //            topLeft = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            break;
        //    }
        //}

        public void GenCubeFaceOLD(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, VisualChunk chunk)
        {
            int verticeCubeOffset = chunk.SolidCubeVertices.Count;
            int indiceCubeOffset = chunk.SolidCubeIndices.Count;
            ByteColor newColor = cube.EmissiveColor;

            VisualCubeProfile cubeProfile = VisualCubeProfile.CubesProfile[cube.Id];
            bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;

            //Les 4 vertex de ma face.... en fct de leur position dans le cube leur valeur en Z va changer ! (Face Top, Bottom, ...
            ByteVector4 topLeft;
            ByteVector4 topRight;
            ByteVector4 bottomLeft;
            ByteVector4 bottomRight;

            int cubeFaceType = (int)cubeFace;

            ByteVector4 vertexInfo = new ByteVector4((byte)0, (byte)cubeFace, (byte)0, (byte)0);

            long hashVertex;
            int generatedVertex = 0;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;

            //Get the index of the current cube.
            int baseIndex = _cubesHolder.Index(ref cubePosiInWorld);

            int[] ind = new int[9];

            int baseindex, baseIndexP1, baseIndexM1;

            switch (cubeFace)
            {
                case CubeFace.Front:

                    ByteColor Back_Cube, BackLeft_Cube, BackRight_Cube, BackTop_Cube, BackBottom_Cube, BackLeftBottom_Cube, BackRightBottom_Cube, BackLeftTop_Cube, BackRightTop_Cube;
                    try
                    {
                        baseindex = _cubesHolder.Index(cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z + 1);
                        baseIndexP1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1);
                        baseIndexM1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1);

                        //Get the 9 Facing cubes to the face
                        Back_Cube = _cubesHolder.Cubes[baseindex].EmissiveColor;
                        BackLeft_Cube = _cubesHolder.Cubes[baseIndexM1].EmissiveColor;
                        BackRight_Cube = _cubesHolder.Cubes[baseIndexP1].EmissiveColor;
                        BackTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        if (cubePosiInWorld.Y > 0)
                        {
                            BackBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            BackLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            BackRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                        }
                        else
                        {
                            BackBottom_Cube = new ByteColor();
                            BackLeftBottom_Cube = new ByteColor();
                            BackRightBottom_Cube = new ByteColor();
                        }
                        BackLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        BackRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;

                    }
                    catch (Exception)
                    {
                        Back_Cube = new ByteColor();
                        BackLeft_Cube = new ByteColor();
                        BackRight_Cube = new ByteColor();
                        BackTop_Cube = new ByteColor();
                        BackBottom_Cube = new ByteColor();
                        BackLeftBottom_Cube = new ByteColor();
                        BackRightBottom_Cube = new ByteColor();
                        BackLeftTop_Cube = new ByteColor();
                        BackRightTop_Cube = new ByteColor();
                    }
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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackTop_Cube, BackRightTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackBottom_Cube, BackLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackBottom_Cube, BackRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref  newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));

                    break;
                case CubeFace.Back:

                    ByteColor Front_Cube, FrontLeft_Cube, FrontRight_Cube, FrontTop_Cube, FrontBottom_Cube, FrontLeftBottom_Cube, FrontRightBottom_Cube, FrontLeftTop_Cube, FrontRightTop_Cube;
                    try
                    {
                        baseindex = _cubesHolder.Index(cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z - 1);
                        baseIndexP1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1);
                        baseIndexM1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1);

                        //Get the 9 Facing cubes to the face
                        Front_Cube = _cubesHolder.Cubes[baseindex].EmissiveColor;
                        FrontLeft_Cube = _cubesHolder.Cubes[baseIndexM1].EmissiveColor;
                        FrontRight_Cube = _cubesHolder.Cubes[baseIndexM1].EmissiveColor;
                        FrontTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;

                        if (cubePosiInWorld.Y > 0)
                        {
                            FrontBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            FrontLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            FrontRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                        }
                        else
                        {
                            FrontBottom_Cube = new ByteColor();
                            FrontLeftBottom_Cube = new ByteColor();
                            FrontRightBottom_Cube = new ByteColor();
                        }

                        FrontLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        FrontRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;

                    }
                    catch (Exception)
                    {
                        Front_Cube = new ByteColor();
                        FrontLeft_Cube = new ByteColor();
                        FrontRight_Cube = new ByteColor();
                        FrontTop_Cube = new ByteColor();
                        FrontBottom_Cube = new ByteColor();
                        FrontLeftBottom_Cube = new ByteColor();
                        FrontRightBottom_Cube = new ByteColor();
                        FrontLeftTop_Cube = new ByteColor();
                        FrontRightTop_Cube = new ByteColor();
                    }

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontRight_Cube, FrontRightTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontLeft_Cube, FrontLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontRight_Cube, FrontRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));

                    break;
                case CubeFace.Top:

                    ByteColor Bottom_Cube, BottomLeft_Cube, BottomRight_Cube, BottomTop_Cube, BottomBottom_Cube, BottomLeftTop_Cube, BottomRightTop_Cube, BottomLeftBottom_Cube, BottomRightBottom_Cube;
                    try
                    {
                        baseindex = _cubesHolder.Index(cubePosiInWorld.X, cubePosiInWorld.Y + 1, cubePosiInWorld.Z);
                        baseIndexP1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1);
                        baseIndexM1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1);

                        //Get the 9 Facing cubes to the face
                        Bottom_Cube = _cubesHolder.Cubes[baseindex].EmissiveColor;
                        BottomLeft_Cube = _cubesHolder.Cubes[baseIndexM1].EmissiveColor;
                        BottomRight_Cube = _cubesHolder.Cubes[baseIndexP1].EmissiveColor;
                        BottomTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        BottomBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1)].EmissiveColor;
                        BottomLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        BottomRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        BottomLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1)].EmissiveColor;
                        BottomRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1)].EmissiveColor;
                    }
                    catch (Exception)
                    {
                        Bottom_Cube = new ByteColor();
                        BottomLeft_Cube = new ByteColor();
                        BottomRight_Cube = new ByteColor();
                        BottomTop_Cube = new ByteColor();
                        BottomBottom_Cube = new ByteColor();
                        BottomLeftTop_Cube = new ByteColor();
                        BottomRightTop_Cube = new ByteColor();
                        BottomLeftBottom_Cube = new ByteColor();
                        BottomRightBottom_Cube = new ByteColor();
                    }

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomRight_Cube, BottomBottom_Cube, BottomRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomBottom_Cube, BottomLeft_Cube, BottomLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomTop_Cube, BottomRight_Cube, BottomRightTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    break;

                case CubeFace.Bottom:

                    ByteColor Top_Cube, TopLeft_Cube, TopRight_Cube, TopTop_Cube, TopBottom_Cube, TopLeftTop_Cube, TopRightTop_Cube, TopLeftBottom_Cube, TopRightBottom_Cube;
                    try
                    {
                        baseindex = _cubesHolder.Index(cubePosiInWorld.X, cubePosiInWorld.Y - 1, cubePosiInWorld.Z);
                        baseIndexP1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1);
                        baseIndexM1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1);

                        //Get the 9 Facing cubes to the face
                        Top_Cube = _cubesHolder.Cubes[baseindex].EmissiveColor;
                        TopLeft_Cube = _cubesHolder.Cubes[baseIndexM1].EmissiveColor;
                        TopRight_Cube = _cubesHolder.Cubes[baseIndexP1].EmissiveColor;
                        TopTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        TopBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1)].EmissiveColor;
                        TopLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        TopRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        TopLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1)].EmissiveColor;
                        TopRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1)].EmissiveColor;

                    }
                    catch (Exception)
                    {
                        Top_Cube = new ByteColor();
                        TopLeft_Cube = new ByteColor();
                        TopRight_Cube = new ByteColor();
                        TopTop_Cube = new ByteColor();
                        TopBottom_Cube = new ByteColor();
                        TopLeftTop_Cube = new ByteColor();
                        TopRightTop_Cube = new ByteColor();
                        TopLeftBottom_Cube = new ByteColor();
                        TopRightBottom_Cube = new ByteColor();
                    }

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopLeft_Cube, TopLeftTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopRight_Cube, TopRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopRight_Cube, TopRightTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    break;

                case CubeFace.Left:

                    if (cubePosiInWorld.X == 275 && cubePosiInWorld.Y == 76 && cubePosiInWorld.Z == 159)
                    {
                        Console.WriteLine("");
                    }

                    ByteColor Right_Cube, RightLeft_Cube, RightRight_Cube, RightTop_Cube, RightBottom_Cube, RightLeftBottom_Cube, RightRightBottom_Cube, RightLeftTop_Cube, RightRightTop_Cube;
                    try
                    {
                        baseindex = _cubesHolder.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y, cubePosiInWorld.Z);
                        baseIndexP1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1);
                        baseIndexM1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1);

                        //Get the 9 Facing cubes to the face
                        Right_Cube = _cubesHolder.Cubes[baseindex].EmissiveColor;
                        RightLeft_Cube = _cubesHolder.Cubes[baseIndexP1].EmissiveColor;
                        RightRight_Cube = _cubesHolder.Cubes[baseIndexM1].EmissiveColor;
                        RightTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;

                        if (cubePosiInWorld.Y > 0)
                        {
                            RightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            RightLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            RightRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                        }
                        else
                        {
                            RightBottom_Cube = new ByteColor();
                            RightLeftBottom_Cube = new ByteColor();
                            RightRightBottom_Cube = new ByteColor();
                        }
                        RightLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        RightRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;

                    }
                    catch (Exception)
                    {
                        Right_Cube = new ByteColor();
                        RightLeft_Cube = new ByteColor();
                        RightRight_Cube = new ByteColor();
                        RightTop_Cube = new ByteColor();
                        RightBottom_Cube = new ByteColor();
                        RightLeftBottom_Cube = new ByteColor();
                        RightRightBottom_Cube = new ByteColor();
                        RightLeftTop_Cube = new ByteColor();
                        RightRightTop_Cube = new ByteColor();
                    }

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightLeft_Cube, RightLeftTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightRight_Cube, RightRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightLeft_Cube, RightLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    break;
                case CubeFace.Right:

                    ByteColor Left_Cube, LeftLeft_Cube, LefttRight_Cube, LeftTop_Cube, LeftBottom_Cube, LeftLeftBottom_Cube, LeftRightBottom_Cube, LeftLeftTop_Cube, LeftRightTop_Cube;
                    try
                    {
                        baseindex = _cubesHolder.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y, cubePosiInWorld.Z);
                        baseIndexP1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1);
                        baseIndexM1 = _cubesHolder.FastIndex(baseindex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1);

                        //Get the 9 Facing cubes to the face
                        Left_Cube = _cubesHolder.Cubes[baseindex].EmissiveColor;
                        LeftLeft_Cube = _cubesHolder.Cubes[baseIndexP1].EmissiveColor;
                        LefttRight_Cube = _cubesHolder.Cubes[baseIndexM1].EmissiveColor;
                        LeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;

                        if (cubePosiInWorld.Y > 0)
                        {
                            LeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseindex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            LeftLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            LeftRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)].EmissiveColor;
                        }
                        else
                        {
                            LeftBottom_Cube = new ByteColor();
                            LeftLeftBottom_Cube = new ByteColor();
                            LeftRightBottom_Cube = new ByteColor();
                        }

                        LeftLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexP1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        LeftRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.FastIndex(baseIndexM1, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1)].EmissiveColor;

                    }
                    catch (Exception)
                    {
                        Left_Cube = new ByteColor();
                        LeftLeft_Cube = new ByteColor();
                        LefttRight_Cube = new ByteColor();
                        LeftTop_Cube = new ByteColor();
                        LeftBottom_Cube = new ByteColor();
                        LeftLeftBottom_Cube = new ByteColor();
                        LeftRightBottom_Cube = new ByteColor();
                        LeftLeftTop_Cube = new ByteColor();
                        LeftRightTop_Cube = new ByteColor();
                    }

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LeftLeft_Cube, LeftLeftTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LeftLeft_Cube, LeftLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LefttRight_Cube, LeftRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    break;
            }
        }

        public void GenCubeFace(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, VisualChunk chunk)
        {
            int verticeCubeOffset = chunk.SolidCubeVertices.Count;
            int indiceCubeOffset = chunk.SolidCubeIndices.Count;
            ByteColor newColor = cube.EmissiveColor;

            VisualCubeProfile cubeProfile = VisualCubeProfile.CubesProfile[cube.Id];
            bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;

            //Les 4 vertex de ma face.... en fct de leur position dans le cube leur valeur en Z va changer ! (Face Top, Bottom, ...
            ByteVector4 topLeft;
            ByteVector4 topRight;
            ByteVector4 bottomLeft;
            ByteVector4 bottomRight;

            int cubeFaceType = (int)cubeFace;

            ByteVector4 vertexInfo = new ByteVector4((byte)0, (byte)cubeFace, (byte)0, (byte)0);

            long hashVertex;
            int generatedVertex = 0;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;

            //Get the index of the current cube.
            int baseIndex = _cubesHolder.Index(ref cubePosiInWorld);

            int[] ind = new int[9];

            switch (cubeFace)
            {
                case CubeFace.Front:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Plus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.Z, ind, false);

                    ByteColor Back_Cube = (ind[SingleArrayChunkContainer.BaseIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor : new ByteColor();
                    ByteColor BackLeft_Cube = (ind[SingleArrayChunkContainer.LeftIndex] != int.MaxValue) ?_cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor BackRight_Cube = (ind[SingleArrayChunkContainer.RightIndex] != int.MaxValue) ?_cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor : new ByteColor();
                    ByteColor BackTop_Cube = (ind[SingleArrayChunkContainer.UpIndex] != int.MaxValue) ?_cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor : new ByteColor();
                    ByteColor BackBottom_Cube = (ind[SingleArrayChunkContainer.DownIndex] != int.MaxValue) ?_cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor : new ByteColor();
                    ByteColor BackLeftTop_Cube = (ind[SingleArrayChunkContainer.UpLeftIndex] != int.MaxValue) ?_cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor BackRightTop_Cube = (ind[SingleArrayChunkContainer.UpRightIndex] != int.MaxValue) ?_cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor : new ByteColor();
                    ByteColor BackLeftBottom_Cube = (ind[SingleArrayChunkContainer.DownLeftIndex] != int.MaxValue) ?_cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor BackRightBottom_Cube = (ind[SingleArrayChunkContainer.DownRightIndex] != int.MaxValue) ?_cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor : new ByteColor();

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackTop_Cube, BackRightTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackBottom_Cube, BackLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackBottom_Cube, BackRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref  newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));

                    break;
                case CubeFace.Back:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.Z, SingleArrayChunkContainer.IdxRelativeMove.Z_Minus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.Z, ind, false);

                    ByteColor Front_Cube = (ind[SingleArrayChunkContainer.BaseIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor : new ByteColor();
                    ByteColor FrontLeft_Cube = (ind[SingleArrayChunkContainer.LeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor FrontRight_Cube = (ind[SingleArrayChunkContainer.RightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor : new ByteColor();
                    ByteColor FrontTop_Cube = (ind[SingleArrayChunkContainer.UpIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor : new ByteColor();
                    ByteColor FrontBottom_Cube = (ind[SingleArrayChunkContainer.DownIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor : new ByteColor();
                    ByteColor FrontLeftTop_Cube = (ind[SingleArrayChunkContainer.UpLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor FrontRightTop_Cube = (ind[SingleArrayChunkContainer.UpRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor : new ByteColor();
                    ByteColor FrontLeftBottom_Cube = (ind[SingleArrayChunkContainer.DownLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor FrontRightBottom_Cube = (ind[SingleArrayChunkContainer.DownRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor : new ByteColor();

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontRight_Cube, FrontRightTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontLeft_Cube, FrontLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontRight_Cube, FrontRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));

                    break;
                case CubeFace.Top:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.Y, ind, false);

                    ByteColor Bottom_Cube = (ind[SingleArrayChunkContainer.BaseIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor : new ByteColor();
                    ByteColor BottomLeft_Cube = (ind[SingleArrayChunkContainer.LeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor BottomRight_Cube = (ind[SingleArrayChunkContainer.RightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor : new ByteColor();
                    ByteColor BottomTop_Cube = (ind[SingleArrayChunkContainer.UpIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor : new ByteColor();
                    ByteColor BottomBottom_Cube = (ind[SingleArrayChunkContainer.DownIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor : new ByteColor();
                    ByteColor BottomLeftTop_Cube = (ind[SingleArrayChunkContainer.UpLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor BottomRightTop_Cube = (ind[SingleArrayChunkContainer.UpRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor : new ByteColor();
                    ByteColor BottomLeftBottom_Cube = (ind[SingleArrayChunkContainer.DownLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor BottomRightBottom_Cube = (ind[SingleArrayChunkContainer.DownRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor : new ByteColor();

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomRight_Cube, BottomBottom_Cube, BottomRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomBottom_Cube, BottomLeft_Cube, BottomLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomTop_Cube, BottomRight_Cube, BottomRightTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    break;

                case CubeFace.Bottom:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.Y, ind, false);

                    ByteColor Top_Cube = (ind[SingleArrayChunkContainer.BaseIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor : new ByteColor();
                    ByteColor TopLeft_Cube = (ind[SingleArrayChunkContainer.LeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor TopRight_Cube = (ind[SingleArrayChunkContainer.RightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor : new ByteColor();
                    ByteColor TopTop_Cube = (ind[SingleArrayChunkContainer.UpIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor : new ByteColor();
                    ByteColor TopBottom_Cube = (ind[SingleArrayChunkContainer.DownIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor : new ByteColor();
                    ByteColor TopLeftTop_Cube = (ind[SingleArrayChunkContainer.UpLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor TopRightTop_Cube = (ind[SingleArrayChunkContainer.UpRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor : new ByteColor();
                    ByteColor TopLeftBottom_Cube = (ind[SingleArrayChunkContainer.DownLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor TopRightBottom_Cube = (ind[SingleArrayChunkContainer.DownRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor : new ByteColor();

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopLeft_Cube, TopLeftTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopRight_Cube, TopRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopRight_Cube, TopRightTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    break;

                case CubeFace.Left:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Minus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.X, ind, false);

                    ByteColor Right_Cube = (ind[SingleArrayChunkContainer.BaseIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor : new ByteColor();
                    ByteColor RightLeft_Cube = (ind[SingleArrayChunkContainer.LeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor RightRight_Cube = (ind[SingleArrayChunkContainer.RightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor : new ByteColor();
                    ByteColor RightTop_Cube = (ind[SingleArrayChunkContainer.UpIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor : new ByteColor();
                    ByteColor RightBottom_Cube = (ind[SingleArrayChunkContainer.DownIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor : new ByteColor();
                    ByteColor RightLeftTop_Cube = (ind[SingleArrayChunkContainer.UpLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor RightRightTop_Cube = (ind[SingleArrayChunkContainer.UpRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor : new ByteColor();
                    ByteColor RightLeftBottom_Cube = (ind[SingleArrayChunkContainer.DownLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor RightRightBottom_Cube = (ind[SingleArrayChunkContainer.DownRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor : new ByteColor();

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightLeft_Cube, RightLeftTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightRight_Cube, RightRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightLeft_Cube, RightLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    break;
                case CubeFace.Right:

                    //Get the 9 Facing cubes to the face
                    _cubesHolder.SurroundingAxisIndex(_cubesHolder.FastIndex(baseIndex, cubePosiInWorld.X, SingleArrayChunkContainer.IdxRelativeMove.X_Plus1), cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z, SingleArrayChunkContainer.Axis.X, ind, false);

                    ByteColor Left_Cube = (ind[SingleArrayChunkContainer.BaseIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.BaseIndex]].EmissiveColor : new ByteColor();
                    ByteColor LeftLeft_Cube = (ind[SingleArrayChunkContainer.LeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.LeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor LefttRight_Cube = (ind[SingleArrayChunkContainer.RightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.RightIndex]].EmissiveColor : new ByteColor();
                    ByteColor LeftTop_Cube = (ind[SingleArrayChunkContainer.UpIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpIndex]].EmissiveColor : new ByteColor();
                    ByteColor LeftBottom_Cube = (ind[SingleArrayChunkContainer.DownIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownIndex]].EmissiveColor : new ByteColor();
                    ByteColor LeftLeftTop_Cube = (ind[SingleArrayChunkContainer.UpLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor LeftRightTop_Cube = (ind[SingleArrayChunkContainer.UpRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.UpRightIndex]].EmissiveColor : new ByteColor();
                    ByteColor LeftLeftBottom_Cube = (ind[SingleArrayChunkContainer.DownLeftIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownLeftIndex]].EmissiveColor : new ByteColor();
                    ByteColor LeftRightBottom_Cube = (ind[SingleArrayChunkContainer.DownRightIndex] != int.MaxValue) ? _cubesHolder.Cubes[ind[SingleArrayChunkContainer.DownRightIndex]].EmissiveColor : new ByteColor();

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
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LeftLeft_Cube, LeftLeftTop_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LeftLeft_Cube, LeftLeftBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LefttRight_Cube, LeftRightBottom_Cube);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    break;
            }
        }

        //public void GenCubeFaceOLD(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, VisualChunk chunk)
        //{
        //    int verticeCubeOffset = chunk.SolidCubeVertices.Count;
        //    int indiceCubeOffset = chunk.SolidCubeIndices.Count;
        //    ByteColor newColor = cube.EmissiveColor : new ByteColor();

        //    VisualCubeProfile cubeProfile = VisualCubeProfile.CubesProfile[cube.Id];
        //    bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;

        //    //Les 4 vertex de ma face.... en fct de leur position dans le cube leur valeur en Z va changer ! (Face Top, Bottom, ...
        //    ByteVector4 topLeft;
        //    ByteVector4 topRight;
        //    ByteVector4 bottomLeft;
        //    ByteVector4 bottomRight;

        //    int cubeFaceType = (int)cubeFace;

        //    ByteVector4 vertexInfo = new ByteVector4((byte)0, (byte)cubeFace, (byte)0, (byte)0);

        //    string hashVertex;
        //    int generatedVertex = 0;
        //    int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;

        //    //Get the index of the current cube.
        //    int baseIndex = _cubesHolder.Index(ref cubePosiInWorld);

        //    switch (cubeFace)
        //    {
        //        case CubeFace.Front:

        //            //Get the 9 Facing cubes to the face

        //            ByteColor Back_Cube = _cubesHolder.Cubes[_cubesHolder.ValidateIndex(baseIndex + _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BackLeft_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, -_cubesHolder.MoveX, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BackRight_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, _cubesHolder.MoveX, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BackTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, _cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BackBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, -_cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BackLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, -_cubesHolder.MoveX, _cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BackRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, _cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BackLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, -_cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BackRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, -_cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();

        //            topLeft = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackTop_Cube, BackLeftTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackTop_Cube, BackRightTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackBottom_Cube, BackLeftBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackBottom_Cube, BackRightBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Front, ref  newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));

        //            break;
        //        case CubeFace.Back:

        //            //Get the 9 Facing cubes to the face
        //            ByteColor Front_Cube = _cubesHolder.Cubes[_cubesHolder.ValidateIndex(baseIndex -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor FrontLeft_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX,  -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor FrontRight_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX,  -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor FrontTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, _cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor FrontBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, -_cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor FrontLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, _cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor FrontRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, _cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor FrontLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, -_cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor FrontRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, -_cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();

        //            topLeft = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontLeftTop_Cube, FrontLeft_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontRight_Cube, FrontRightTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontLeft_Cube, FrontLeftBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontRight_Cube, FrontRightBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));

        //            break;
        //        case CubeFace.Top:

        //            //Get the 9 Facing cubes to the face
        //            ByteColor Bottom_Cube = _cubesHolder.Cubes[_cubesHolder.ValidateIndex(baseIndex + _cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor BottomLeft_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, _cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor BottomRight_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, _cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor BottomTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, _cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BottomBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, _cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BottomLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, _cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BottomRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, _cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BottomLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, -_cubesHolder.MoveX, _cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor BottomRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, _cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();

        //            topLeft = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomLeft_Cube, BottomLeftTop_Cube, BottomTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomRight_Cube, BottomBottom_Cube, BottomRightBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomBottom_Cube, BottomLeft_Cube, BottomLeftBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomTop_Cube, BottomRight_Cube, BottomRightTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            break;

        //        case CubeFace.Bottom:

        //            //Get the 9 Facing cubes to the face
        //            ByteColor Top_Cube = _cubesHolder.Cubes[_cubesHolder.ValidateIndex(baseIndex - _cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor TopLeft_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, -_cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor TopRight_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, -_cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor TopTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, -_cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor TopBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, -_cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor TopLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, -_cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor TopRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, -_cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor TopLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, -_cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor TopRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, -_cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();

        //            topLeft = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopLeft_Cube, TopLeftBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopLeft_Cube, TopLeftTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopRight_Cube, TopRightBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopRight_Cube, TopRightTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            break;

        //        case CubeFace.Left:
        //            //Get the 9 Facing cubes to the face
        //            ByteColor Right_Cube = _cubesHolder.Cubes[_cubesHolder.ValidateIndex(baseIndex - _cubesHolder.MoveX)].EmissiveColor : new ByteColor();
        //            ByteColor RightLeft_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX,  _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor RightRight_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX,  -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor RightTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, _cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor RightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, -_cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor RightLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex, -_cubesHolder.MoveX, _cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor RightRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, _cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor RightLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, -_cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor RightRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,-_cubesHolder.MoveX, -_cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();

        //            topLeft = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightRight_Cube, RightRightTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightLeft_Cube, RightLeftTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightRight_Cube, RightRightBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightLeft_Cube, RightLeftBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            break;
        //        case CubeFace.Right:

        //            //Get the 9 Facing cubes to the face
        //            ByteColor Left_Cube = _cubesHolder.Cubes[_cubesHolder.ValidateIndex(baseIndex + _cubesHolder.MoveX)].EmissiveColor : new ByteColor();
        //            ByteColor LeftLeft_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX,  _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor LefttRight_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX,  -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor LeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, _cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor LeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, -_cubesHolder.MoveY)].EmissiveColor : new ByteColor();
        //            ByteColor LeftLeftTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, _cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor LeftRightTop_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, _cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor LeftLeftBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, -_cubesHolder.MoveY, _cubesHolder.MoveZ)].EmissiveColor : new ByteColor();
        //            ByteColor LeftRightBottom_Cube = _cubesHolder.Cubes[_cubesHolder.IndexMoves(baseIndex,_cubesHolder.MoveX, -_cubesHolder.MoveY, -_cubesHolder.MoveZ)].EmissiveColor : new ByteColor();

        //            topLeft = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);
        //            topRight = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
        //            bottomLeft = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);
        //            bottomRight = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);

        //            hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
        //            {
        //                vertexOffset0 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LefttRight_Cube, LeftRightTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
        //            {
        //                vertexOffset1 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LeftLeft_Cube, LeftLeftTop_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
        //            {
        //                vertexOffset2 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LeftLeft_Cube, LeftLeftBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
        //            if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
        //            {
        //                vertexOffset3 = generatedVertex + verticeCubeOffset;
        //                chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
        //                if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LefttRight_Cube, LeftRightBottom_Cube);
        //                chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
        //                generatedVertex++;
        //            }

        //            //Create Vertices
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
        //            chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
        //            break;
        //    }
        //}
    }
}
