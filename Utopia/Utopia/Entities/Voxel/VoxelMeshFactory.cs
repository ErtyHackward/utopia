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
                            GenerateFaces(CubeFaces.Back, ref dico, vec, ref vertices, ref indices);
                        
                        if (IsEmpty(ref blockData, ref size, x, y - 1, z))
                            GenerateFaces(CubeFaces.Bottom, ref dico, vec, ref vertices, ref indices);

                        if (IsEmpty(ref blockData, ref size, x, y, z + 1))
                            GenerateFaces(CubeFaces.Front, ref dico, vec, ref vertices, ref indices);
                        
                        if (IsEmpty(ref blockData, ref size, x - 1, y, z))
                            GenerateFaces(CubeFaces.Left, ref dico, vec, ref vertices, ref indices);

                        if (IsEmpty(ref blockData, ref size, x + 1, y, z))
                            GenerateFaces(CubeFaces.Right, ref dico, vec, ref vertices, ref indices);

                        if (IsEmpty(ref blockData, ref size, x, y + 1, z))
                            GenerateFaces(CubeFaces.Top, ref dico, vec, ref vertices, ref indices);
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

        private void GenerateFaces(CubeFaces cubeFace, ref Dictionary<int, int> dico, ByteVector4 cubePosition, ref List<VertexVoxel> vertices, ref List<ushort> indices)
        {
             // hash and index

            ByteVector4 topLeft;
            ByteVector4 topRight;
            ByteVector4 bottomLeft;
            ByteVector4 bottomRight;

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
                case CubeFaces.Front:

                    topLeft = cubePosition + new ByteVector4(0, 1, 1, 0);
                    topRight = cubePosition + new ByteVector4(1, 1, 1, 0);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 1, 0);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 1, 0);

                    hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                    if ( !vertexInDico)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset0);
                        
                        
                        vertices.Add(new VertexVoxel(topLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                    if ( !vertexInDico)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset1);

                        vertices.Add(new VertexVoxel(topRight, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                    if ( !vertexInDico)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset2);

                        vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                    if ( !vertexInDico)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset3);

                        vertices.Add(new VertexVoxel(bottomRight, faceTypeByte));
                        generatedVertex++;
                    }

                    //Create Vertices
                    indices.Add((ushort)(vertexOffset0));
                    indices.Add((ushort)(vertexOffset2));
                    indices.Add((ushort)(vertexOffset1));

                    indices.Add((ushort)(vertexOffset2));
                    indices.Add((ushort)(vertexOffset3));
                    indices.Add((ushort)(vertexOffset1));

                    break;
                case CubeFaces.Back:


                    topLeft = cubePosition + new ByteVector4(1, 1, 0, 0);
                    topRight = cubePosition + new ByteVector4(0, 1, 0, 0);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 0, 0);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 0, 0);

                    hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                    if ( !vertexInDico)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset0);

                        vertices.Add(new VertexVoxel(topRight, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                    if ( !vertexInDico)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset1);

                        vertices.Add(new VertexVoxel(topLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                    if ( !vertexInDico)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset2);

                        vertices.Add(new VertexVoxel(bottomRight, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                    if ( !vertexInDico)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset3);

                        vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    //Create Vertices
                    indices.Add((ushort)(vertexOffset0));
                    indices.Add((ushort)(vertexOffset1));
                    indices.Add((ushort)(vertexOffset2));

                    indices.Add((ushort)(vertexOffset2));
                    indices.Add((ushort)(vertexOffset1));
                    indices.Add((ushort)(vertexOffset3));

                    break;
                case CubeFaces.Top:


                    topLeft = cubePosition + new ByteVector4(0, 1, 0, 0);
                    topRight = cubePosition + new ByteVector4(1, 1, 0, 0);
                    bottomLeft = cubePosition + new ByteVector4(0, 1, 1, 0);
                    bottomRight = cubePosition + new ByteVector4(1, 1, 1, 0);

                    hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                    if (!vertexInDico)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset0);

                        vertices.Add(new VertexVoxel(topLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                    if (!vertexInDico)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset1);

                        vertices.Add(new VertexVoxel(bottomRight, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                    if (!vertexInDico)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset2);

                        vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                    if (!vertexInDico)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset3);

                        vertices.Add(new VertexVoxel(topRight, faceTypeByte));
                        generatedVertex++;
                    }

                    //Create Vertices
                    indices.Add((ushort)(vertexOffset0));
                    indices.Add((ushort)(vertexOffset2));
                    indices.Add((ushort)(vertexOffset1));

                    indices.Add((ushort)(vertexOffset0));
                    indices.Add((ushort)(vertexOffset1));
                    indices.Add((ushort)(vertexOffset3));

                    break;

                case CubeFaces.Bottom:

                    topLeft = cubePosition + new ByteVector4(0, 0, 1, 0);
                    topRight = cubePosition + new ByteVector4(1, 0, 1, 0);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0, 0);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0, 0);

                    hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                    if (!vertexInDico)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset0);

                        vertices.Add(new VertexVoxel(topLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                    if (!vertexInDico)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset1);

                        vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                    if (!vertexInDico)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset2);

                        vertices.Add(new VertexVoxel(topRight, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                    if (!vertexInDico)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset3);

                        vertices.Add(new VertexVoxel(bottomRight, faceTypeByte));
                        generatedVertex++;
                    }

                    //Create Vertices
                    indices.Add((ushort)(vertexOffset0));
                    indices.Add((ushort)(vertexOffset1));
                    indices.Add((ushort)(vertexOffset2));

                    indices.Add((ushort)(vertexOffset1));
                    indices.Add((ushort)(vertexOffset3));
                    indices.Add((ushort)(vertexOffset2));

                    break;

                case CubeFaces.Left:

                    topLeft = cubePosition + new ByteVector4(0, 1, 0, 0);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 1, 0);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0, 0);
                    topRight = cubePosition + new ByteVector4(0, 1, 1, 0);

                    hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                    if ( !vertexInDico)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset0);

                        vertices.Add(new VertexVoxel(topLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                    if ( !vertexInDico)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset1);

                        vertices.Add(new VertexVoxel(topRight, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                    if ( !vertexInDico)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset2);

                        vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                    if ( !vertexInDico)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset3);

                        vertices.Add(new VertexVoxel(bottomRight, faceTypeByte));
                        generatedVertex++;
                    }

                    //Create Vertices
                    indices.Add((ushort)(vertexOffset0));
                    indices.Add((ushort)(vertexOffset2));
                    indices.Add((ushort)(vertexOffset3));

                    indices.Add((ushort)(vertexOffset1));
                    indices.Add((ushort)(vertexOffset0));
                    indices.Add((ushort)(vertexOffset3));
                    break;
                case CubeFaces.Right:

                    topLeft = cubePosition + new ByteVector4(1, 1, 1, 0);
                    topRight = cubePosition + new ByteVector4(1, 1, 0, 0);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 1, 0);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0, 0);

                    hashVertex = cubeFaceType + (topRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                    if ( !vertexInDico)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset0);

                        vertices.Add(new VertexVoxel(topRight, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (topLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                    if ( !vertexInDico)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset1);

                        vertices.Add(new VertexVoxel(topLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomLeft.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                    if ( !vertexInDico)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset2);

                        vertices.Add(new VertexVoxel(bottomLeft, faceTypeByte));
                        generatedVertex++;
                    }

                    hashVertex = cubeFaceType + (bottomRight.GetHashCode() << 4);
                    vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                    if ( !vertexInDico)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        dico.Add(hashVertex, vertexOffset3);

                        vertices.Add(new VertexVoxel(bottomRight, faceTypeByte));
                        generatedVertex++;
                    }

                    //Create Vertices
                    indices.Add((ushort)(vertexOffset0));
                    indices.Add((ushort)(vertexOffset2));
                    indices.Add((ushort)(vertexOffset3));

                    indices.Add((ushort)(vertexOffset1));
                    indices.Add((ushort)(vertexOffset2));
                    indices.Add((ushort)(vertexOffset0));

                    break;
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