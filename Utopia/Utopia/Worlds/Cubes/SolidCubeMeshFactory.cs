using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Worlds.Chunks;
using S33M3Engines.Struct.Vertex;

namespace Utopia.Worlds.Cubes
{
    public class SolidCubeMeshFactory : ICubeMeshFactory
    {
        public SolidCubeMeshFactory()
        {

        }

        public void GenCubeFace(byte cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, VisualChunk chunk)
        {
            int verticeCubeOffset = chunk.SolidCubeVertices.Count;
            int indiceCubeOffset = chunk.SolidCubeIndices.Count;

            VisualCubeProfile cubeProfile = VisualCubeProfile.CubesProfile[cube];
            bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;

            ByteVector4 topLeft;
            ByteVector4 topRight;
            ByteVector4 bottomLeft;
            ByteVector4 bottomRight;

            int cubeFaceType = (int)cubeFace;

            ByteVector4 vertexInfo = new ByteVector4((byte)0, (byte)cubeFace, (byte)0, (byte)0);

            string hashVertex;
            int generatedVertex = 0;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;
            ByteColor newColor = new ByteColor(255, 255, 255, 255);

            switch (cubeFace)
            {
                case CubeFace.Front:

                    topLeft = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube].Tex_Front, ref  newColor, ref vertexInfo));
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

                    topLeft = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube].Tex_Back, ref newColor, ref vertexInfo));
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

                    topLeft = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube].Tex_Top, ref newColor, ref vertexInfo));
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

                    topLeft = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube].Tex_Bottom, ref newColor, ref vertexInfo));
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
                    topLeft = cubePosition + new ByteVector4(0, 1, 0, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 1, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(0, 1, 1, cubeFaceType);

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube].Tex_Left, ref newColor, ref vertexInfo));
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

                    topLeft = cubePosition + new ByteVector4(1, 1, 1, cubeFaceType);
                    topRight = cubePosition + new ByteVector4(1, 1, 0, cubeFaceType);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 1, cubeFaceType);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0, cubeFaceType);

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, VisualCubeProfile.CubesProfile[cube].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, VisualCubeProfile.CubesProfile[cube].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, VisualCubeProfile.CubesProfile[cube].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.GetHashCode().ToString();
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, VisualCubeProfile.CubesProfile[cube].Tex_Right, ref newColor, ref vertexInfo));
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
    }
}
