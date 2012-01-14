using System.Collections.Generic;
using S33M3Engines;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Shared.Enums;

namespace Utopia.Entities.Voxel
{
    public class VoxelMeshFactory
    {
        private readonly D3DEngine _d3DEngine;

        public D3DEngine Engine
        {
            get { return _d3DEngine; }
        }
    
        public VoxelMeshFactory(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
        }

        public void GenerateVoxelFaces(InsideDataProvider blockData, out List<VertexVoxel> vertices, out List<ushort> indices)
        {
            var size = blockData.ChunkSize;
            vertices = new List<VertexVoxel>();
            indices = new List<ushort>();
            var dico = new Dictionary<int, int>();


            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        byte blockType = blockData.GetBlock(x, y, z);
                        if (blockType == 0) continue;
                        var vec = new ByteVector4(x, y, z, blockType);
                        if (IsEmpty(ref blockData, ref size, x, y, z - 1))
                            GenerateFaces(ref blockData, CubeFaces.Back, ref dico, vec, ref vertices, ref indices);
                        
                        if (IsEmpty(ref blockData, ref size, x, y - 1, z))
                            GenerateFaces(ref blockData, CubeFaces.Bottom, ref dico, vec, ref vertices, ref indices);

                        if (IsEmpty(ref blockData, ref size, x, y, z + 1))
                            GenerateFaces(ref blockData, CubeFaces.Front, ref dico, vec, ref vertices, ref indices);
                        
                        if (IsEmpty(ref blockData, ref size, x - 1, y, z))
                            GenerateFaces(ref blockData, CubeFaces.Left, ref dico, vec, ref vertices, ref indices);

                        if (IsEmpty(ref blockData, ref size, x + 1, y, z))
                            GenerateFaces(ref blockData, CubeFaces.Right, ref dico, vec, ref vertices, ref indices);

                        if (IsEmpty(ref blockData, ref size, x, y + 1, z))
                            GenerateFaces(ref blockData, CubeFaces.Top, ref dico, vec, ref vertices, ref indices);
                    }
                }
            }

        }

        private bool IsEmpty(ref InsideDataProvider blockData, ref Vector3I size, int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x == size.X || y == size.Y || z == size.Z)
                return true;
            return blockData.GetBlock(x, y, z) == 0;
        }

        private byte Avg(int b1, int b2, int b3, int b4)
        {
            return (byte)((b1 + b2 + b3 + b4)/4);
        }

        private void GenerateFaces(ref InsideDataProvider blockData, CubeFaces cubeFace, ref Dictionary<int, int> dico, ByteVector4 cubePosition, ref List<VertexVoxel> vertices, ref List<ushort> indices)
        {
            // hash and index

            ByteVector4 topLeft;
            ByteVector4 topRight;
            ByteVector4 bottomLeft;
            ByteVector4 bottomRight;

            var chunkSize = blockData.ChunkSize;
            var cubeId = cubePosition.W;
            var cubeFaceType = (int)cubeFace;
            var faceTypeByte = (byte)cubeFace;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;
            int hashVertex;
            bool vertexInDico;
            int generatedVertex = 0;
            var verticeCubeOffset = vertices.Count;
            
            switch (cubeFace)
            {
                #region Front
                case CubeFaces.Front:
                    {
                        var lfront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y, cubePosition.Z + 1) ? 255 : 0;
                        var ltopFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y + 1, cubePosition.Z + 1) ? 255 : 0;
                        var lbottomFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y - 1, cubePosition.Z + 1) ? 255 : 0;
                        var lrightFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y, cubePosition.Z + 1) ? 255 : 0;
                        var lfrontLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y, cubePosition.Z + 1) ? 255 : 0;
                        var ltopLeftFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z + 1) ? 255 : 0;
                        var ltopFrontRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z + 1) ? 255 : 0;
                        var lbottomLeftFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z + 1) ? 255 : 0;
                        var lbottomFrontRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z + 1) ? 255 : 0;

                        topLeft = cubePosition + new ByteVector4(0, 1, 1, 0); // topLeftFront
                        topRight = cubePosition + new ByteVector4(1, 1, 1, 0); // topRightFront
                        bottomLeft = cubePosition + new ByteVector4(0, 0, 1, 0); // bottomLeftFront
                        bottomRight = cubePosition + new ByteVector4(1, 0, 1, 0); // bottomRightFront

                        hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // topLeftFront
                            var light = Avg(lfront, lfrontLeft, ltopFront, ltopLeftFront);

                            vertices.Add(new VertexVoxel(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topRightFront
                            var light = Avg(lfront, ltopFront, lrightFront, ltopFrontRight);

                            vertices.Add(new VertexVoxel(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomLeftFront
                            var light = Avg(lfront, lfrontLeft, lbottomFront, lbottomLeftFront);

                            vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottomRightFront
                            var light = Avg(lfront, lrightFront, lbottomFront, lbottomFrontRight);

                            vertices.Add(new VertexVoxel(bottomRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        //Create Vertices
                        indices.Add((ushort)(vertexOffset0));
                        indices.Add((ushort)(vertexOffset2));
                        indices.Add((ushort)(vertexOffset1));

                        indices.Add((ushort)(vertexOffset2));
                        indices.Add((ushort)(vertexOffset3));
                        indices.Add((ushort)(vertexOffset1));
                    }
                    break;
                #endregion
                #region Back
                case CubeFaces.Back:
                    {
                        var lback = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y, cubePosition.Z - 1) ? 255 : 0;
                        var ltopBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y + 1, cubePosition.Z - 1) ? 255 : 0;
                        var lbottomBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y - 1, cubePosition.Z - 1) ? 255 : 0;
                        var lbackRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y, cubePosition.Z - 1) ? 255 : 0;
                        var lleftback = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y, cubePosition.Z - 1) ? 255 : 0;
                        var ltopRightBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z - 1) ? 255 : 0;
                        var ltopBackLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z - 1) ? 255 : 0;
                        var lbottomRightBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z - 1) ? 255 : 0;
                        var lbottomBackLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z - 1) ? 255 : 0;

                        topLeft = cubePosition + new ByteVector4(1, 1, 0, 0); // topRightBack
                        topRight = cubePosition + new ByteVector4(0, 1, 0, 0); // topLeftBack
                        bottomLeft = cubePosition + new ByteVector4(1, 0, 0, 0); // bottomRightBack
                        bottomRight = cubePosition + new ByteVector4(0, 0, 0, 0); // bottomLeftBack

                        hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // topLeftBack
                            var light = Avg(lback, ltopBack, lleftback, ltopBackLeft);

                            vertices.Add(new VertexVoxel(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topRightBack
                            var light = Avg(lback, lbackRight, ltopBack, ltopRightBack);
                            
                            vertices.Add(new VertexVoxel(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomLeftBack
                            var light = Avg(lback, lbottomBack, lleftback, lbottomBackLeft);

                            vertices.Add(new VertexVoxel(bottomRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottomRightBack
                            var light = Avg(lback, lbackRight, lbottomBack, lbottomRightBack);
                            
                            vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        //Create Vertices
                        indices.Add((ushort)(vertexOffset0));
                        indices.Add((ushort)(vertexOffset1));
                        indices.Add((ushort)(vertexOffset2));

                        indices.Add((ushort)(vertexOffset2));
                        indices.Add((ushort)(vertexOffset1));
                        indices.Add((ushort)(vertexOffset3));
                    }
                    break;
                #endregion
                #region Top
                case CubeFaces.Top:
                    {
                        var ltop = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y + 1, cubePosition.Z) ? 255 : 0;
                        var ltopLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z) ? 255 : 0;
                        var ltopBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y + 1, cubePosition.Z - 1) ? 255 : 0;
                        var ltopRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z) ? 255 : 0;
                        var ltopFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y + 1, cubePosition.Z + 1) ? 255 : 0;
                        var ltopLeftFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z + 1) ? 255 : 0;
                        var ltopFrontRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z + 1) ? 255 : 0;
                        var ltopRightBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z - 1) ? 255 : 0;
                        var ltopBackLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z - 1) ? 255 : 0;

                        topLeft = cubePosition + new ByteVector4(0, 1, 0, 0); // topLeftBack
                        topRight = cubePosition + new ByteVector4(1, 1, 0, 0); // topRightBack
                        bottomLeft = cubePosition + new ByteVector4(0, 1, 1, 0); // topLeftFront
                        bottomRight = cubePosition + new ByteVector4(1, 1, 1, 0); // topRightFront

                        hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);
                            
                            // topLeftBack
                            var light = Avg(ltop, ltopLeft, ltopBack, ltopBackLeft);

                            vertices.Add(new VertexVoxel(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topRightFront
                            var light = Avg(ltop, ltopRight, ltopFront, ltopFrontRight);

                            vertices.Add(new VertexVoxel(bottomRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // topLeftFront
                            var light = Avg(ltop, ltopLeft, ltopFront, ltopLeftFront);

                            vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // topRightBack
                            var light = Avg(ltop, ltopRight, ltopBack, ltopRightBack);

                            vertices.Add(new VertexVoxel(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        //Create Vertices
                        indices.Add((ushort)(vertexOffset0));
                        indices.Add((ushort)(vertexOffset2));
                        indices.Add((ushort)(vertexOffset1));

                        indices.Add((ushort)(vertexOffset0));
                        indices.Add((ushort)(vertexOffset1));
                        indices.Add((ushort)(vertexOffset3));
                    }
                    break;
                #endregion
                #region Bottom
                case CubeFaces.Bottom:
                    {
                        var lbottom = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y - 1, cubePosition.Z) ? 255 : 0;
                        var lbottomLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z) ? 255 : 0;
                        var lbottomBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y - 1, cubePosition.Z - 1) ? 255 : 0;
                        var lbottomright = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z) ? 255 : 0;
                        var lbottomFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X, cubePosition.Y - 1, cubePosition.Z + 1) ? 255 : 0;
                        var lbottomLeftFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z + 1) ? 255 : 0;
                        var lbottomFrontRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z + 1) ? 255 : 0;
                        var lbottomRightBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z - 1) ? 255 : 0;
                        var lbottomBackLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z - 1) ? 255 : 0;

                        topLeft = cubePosition + new ByteVector4(0, 0, 1, 0); // bottomLeftFront
                        topRight = cubePosition + new ByteVector4(1, 0, 1, 0); // bottomRightFront
                        bottomLeft = cubePosition + new ByteVector4(0, 0, 0, 0); // bottomLeftBack
                        bottomRight = cubePosition + new ByteVector4(1, 0, 0, 0); // bottomRightBack

                        hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // bottomLeftFront
                            var light = Avg(lbottom, lbottomLeft, lbottomFront, lbottomLeftFront);

                            vertices.Add(new VertexVoxel(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // bottomLeftBack
                            var light = Avg(lbottom, lbottomLeft, lbottomBack, lbottomBackLeft);

                            vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomRightFront
                            var light = Avg(lbottom, lbottomright, lbottomFront, lbottomFrontRight);

                            vertices.Add(new VertexVoxel(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottomRightBack
                            var light = Avg(lbottom, lbottomright, lbottomBack, lbottomRightBack);

                            vertices.Add(new VertexVoxel(bottomRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        //Create Vertices
                        indices.Add((ushort)(vertexOffset0));
                        indices.Add((ushort)(vertexOffset1));
                        indices.Add((ushort)(vertexOffset2));

                        indices.Add((ushort)(vertexOffset1));
                        indices.Add((ushort)(vertexOffset3));
                        indices.Add((ushort)(vertexOffset2));
                    }
                    break;
                #endregion
                #region Left
                case CubeFaces.Left:
                    {
                        var lleft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y, cubePosition.Z) ? 255 : 0;
                        var ltopLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z) ? 255 : 0;
                        var lbottomLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z) ? 255 : 0;
                        var lfrontLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y, cubePosition.Z + 1) ? 255 : 0;
                        var lleftback = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y, cubePosition.Z - 1) ? 255 : 0;
                        var ltopLeftFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z + 1) ? 255 : 0;
                        var ltopBackLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z - 1) ? 255 : 0;
                        var lbottomLeftFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z + 1) ? 255 : 0;
                        var lbottomBackLeft = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z - 1) ? 255 : 0;

                        topLeft = cubePosition + new ByteVector4(0, 1, 0, 0); // topLeftBack
                        bottomRight = cubePosition + new ByteVector4(0, 0, 1, 0); // bottomLeftFront
                        bottomLeft = cubePosition + new ByteVector4(0, 0, 0, 0); // bottomLeftBack
                        topRight = cubePosition + new ByteVector4(0, 1, 1, 0); // topLeftFront

                        hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // topLeftBack
                            var light = Avg(lleft, ltopLeft, lleftback, ltopBackLeft);

                            vertices.Add(new VertexVoxel(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topLeftFront
                            var light = Avg(lleft, ltopLeft, lfrontLeft, ltopLeftFront);

                            vertices.Add(new VertexVoxel(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomLeftBack
                            var light = Avg(lleft, lbottomLeft, lleftback, lbottomBackLeft);

                            vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottomLeftFront
                            var light = Avg(lleft, lbottomLeft, lfrontLeft, lbottomLeftFront);

                            vertices.Add(new VertexVoxel(bottomRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        //Create Vertices
                        indices.Add((ushort)(vertexOffset0));
                        indices.Add((ushort)(vertexOffset2));
                        indices.Add((ushort)(vertexOffset3));

                        indices.Add((ushort)(vertexOffset1));
                        indices.Add((ushort)(vertexOffset0));
                        indices.Add((ushort)(vertexOffset3));
                    }
                    break;
                #endregion
                #region Right
                case CubeFaces.Right:
                    {
                        var lright = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y, cubePosition.Z) ? 255 : 0;
                        var ltopRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z) ? 255 : 0;
                        var lbottomright = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z) ? 255 : 0;
                        var lbackRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y, cubePosition.Z - 1) ? 255 : 0;
                        var lrightFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y, cubePosition.Z + 1) ? 255 : 0;
                        var ltopFrontRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z + 1) ? 255 : 0;
                        var ltopRightBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z - 1) ? 255 : 0;
                        var lbottomFrontRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z + 1) ? 255 : 0;
                        var lbottomRightBack = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z - 1) ? 255 : 0;

                        topLeft = cubePosition + new ByteVector4(1, 1, 1, 0); // topRightFront
                        topRight = cubePosition + new ByteVector4(1, 1, 0, 0); // topRightBack
                        bottomLeft = cubePosition + new ByteVector4(1, 0, 1, 0); // bottomRightFront
                        bottomRight = cubePosition + new ByteVector4(1, 0, 0, 0); // bottonRightBack

                        hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // topRightBack
                            var light = Avg(lright, ltopRight, lbackRight, ltopRightBack);

                            vertices.Add(new VertexVoxel(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topRightFront
                            var light = Avg(lright, ltopRight, lrightFront, ltopFrontRight);

                            vertices.Add(new VertexVoxel(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomRightFront
                            var light = Avg(lright, lbottomright, lrightFront, lbottomFrontRight);

                            vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottonRightBack
                            var light = Avg(lright, lbottomright, lbackRight, lbottomRightBack);

                            vertices.Add(new VertexVoxel(bottomRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        //Create Vertices
                        indices.Add((ushort)(vertexOffset0));
                        indices.Add((ushort)(vertexOffset2));
                        indices.Add((ushort)(vertexOffset3));

                        indices.Add((ushort)(vertexOffset1));
                        indices.Add((ushort)(vertexOffset2));
                        indices.Add((ushort)(vertexOffset0));
                    }
                    break;
                #endregion
            }
        }
        
        public VertexBuffer<VertexVoxel> InitBuffer(List<VertexVoxel> vertice)
        {
            var vb = new VertexBuffer<VertexVoxel>(_d3DEngine, vertice.Count,
                                                     VertexVoxel.VertexDeclaration,
                                                     PrimitiveTopology.TriangleList,
                                                     "VoxelMeshFactory_VB",
                                                     ResourceUsage.Default,
                                                     10);
            if(vertice.Count > 0)
                vb.SetData(vertice.ToArray());
            return vb;
        }

        public IndexBuffer<ushort> InitBuffer(List<ushort> indices)
        {
            var ib = new IndexBuffer<ushort>(_d3DEngine, indices.Count, SharpDX.DXGI.Format.R16_UInt, "VoxelMeshFactory_IB");

            if(indices.Count > 0)
                ib.SetData(indices.ToArray());

            return ib;
        }
    }
}