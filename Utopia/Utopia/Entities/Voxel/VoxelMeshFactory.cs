using System;
using System.Collections.Generic;
using S33M3Engines;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Worlds.Cubes;

namespace Utopia.Entities.Voxel
{
    public class VoxelMeshFactory
    {
        private readonly D3DEngine _d3DEngine;

        public VoxelMeshFactory(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
        }

        public List<VertexCubeSolid> GenCubesFaces(byte[,,] blocks, byte[,,] overlays = null,bool colorMode=false)
        {
            List<VertexCubeSolid> vertexList = new List<VertexCubeSolid>();

            if (blocks == null)
                return vertexList;

            for (int x = 0; x < blocks.GetLength(0); x++)
            {
                for (int y = 0; y < blocks.GetLength(1); y++)
                {
                    for (int z = 0; z < blocks.GetLength(2); z++)
                    {
                        byte blockType = blocks[x, y, z];
                     
                        byte overlay = overlays == null ? (byte) 0 : overlays[x, y, z];
                        if (blockType == 0) continue;
                        BuildBlockVertices(blocks, ref vertexList, blockType, x, y, z, overlay,colorMode);
                    }
                }
            }

            return vertexList;
        }


        private static void BuildBlockVertices(byte[,,] blocks, ref List<VertexCubeSolid> vertice, byte blockType, int x, int y, int z, byte overlay, bool colorMode)
        {
            byte blockXDecreasing = x == 0 ? (byte) 0 : blocks[x - 1, y, z];
            byte blockXIncreasing = x == blocks.GetLength(0) - 1 ? (byte) 0 : blocks[x + 1, y, z];
            byte blockYDecreasing = y == 0 ? (byte) 0 : blocks[x, y - 1, z];
            byte blockYIncreasing = y == blocks.GetLength(1) - 1 ? (byte) 0 : blocks[x, y + 1, z];
            byte blockZDecreasing = z == 0 ? (byte) 0 : blocks[x, y, z - 1];
            byte blockZIncreasing = z == blocks.GetLength(2) - 1 ? (byte) 0 : blocks[x, y, z + 1];

            if (blockXDecreasing == 0)
                BuildFaceVertices(ref vertice, x, y, z, CubeFace.Left, blockType, overlay, colorMode); //X-
            if (blockXIncreasing == 0)
                BuildFaceVertices(ref vertice, x, y, z, CubeFace.Right, blockType, overlay, colorMode); //X+

            if (blockYDecreasing == 0)
                BuildFaceVertices(ref vertice, x, y, z, CubeFace.Bottom, blockType, overlay, colorMode); //Y-
            if (blockYIncreasing == 0)
                BuildFaceVertices(ref vertice, x, y, z, CubeFace.Top, blockType, overlay, colorMode); //Y+

            if (blockZIncreasing == 0)
                BuildFaceVertices(ref vertice, x, y, z, CubeFace.Back, blockType, overlay, colorMode); //Z+
            if (blockZDecreasing == 0)
                BuildFaceVertices(ref vertice, x, y, z, CubeFace.Front, blockType, overlay, colorMode); //Z-
        }

        private static void BuildFaceVertices(ref List<VertexCubeSolid> vertice, int x, int y, int z,
                                              CubeFace faceDir,
                                              byte blockType, byte overlay,bool colorMode)
        {
            ByteColor color;
            byte textureArrayId;
            int face = (int)faceDir;
            int cubeid = blockType;

            if (colorMode)
            {
                //actually only handles 64 colors, so all blockType> 63 will have default color
                Color tmpColor = ColorLookup.Colours[blockType];
                color = new ByteColor(tmpColor.R, tmpColor.G, tmpColor.B, tmpColor.A);
                textureArrayId = 25;
            } else
            {
                color=new ByteColor(255,255,255,127);

                //textureArrayId = blockType;//coherent with palette but lacks special face case like grass /dirt
                textureArrayId = VisualCubeProfile.CubesProfile[cubeid].Textures[face];//TODO BUG here , cubeprofile index vs palette
            }
            
            //TODO indices : good for perf & ram 

         
            
            if (blockType == 0)//BuildFaceVertices is normally not called with blocktype 0 but let's handle it anyway
                overlay = 0;
            else if (overlay == 0)
                overlay = textureArrayId; //TODO find another way to ignore overlay 0 than lerping between 2 same values in shader 
            
            ByteVector4 vertexInfo = new ByteVector4((byte) 0, (byte) faceDir, overlay, (byte) 0);

            switch (faceDir)
            {
                case CubeFace.Right: //X+
                    {
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                    }
                    break;

                case CubeFace.Left: //X-
                    {
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z, face), textureArrayId, ref color, ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y + 1, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                    }
                    break;

                case CubeFace.Top: //Y+
                    {
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y + 1, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                    }
                    break;

                case CubeFace.Bottom: //Y-
                    {
                        face = (int) CubeFace.Bottom;
                     
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z, face), textureArrayId, ref color, ref vertexInfo));
                    }
                    break;

                case CubeFace.Back: //Z+
                    {
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y + 1, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y, z + 1, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                    }
                    break;

                case CubeFace.Front: //Z-
                    {
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z, face), textureArrayId, ref color, ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x, y, z, face), textureArrayId, ref color, ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                        vertice.Add(new VertexCubeSolid(new ByteVector4(x + 1, y + 1, z, face), textureArrayId, ref color,
                                                        ref vertexInfo));
                    }
                    break;
            }
        }


        public VertexBuffer<VertexCubeSolid> InitBuffer(List<VertexCubeSolid> vertice)
        {
            return new VertexBuffer<VertexCubeSolid>(_d3DEngine, vertice.Count,
                                                     VertexCubeSolid.VertexDeclaration,
                                                     PrimitiveTopology.TriangleList,
                                                     "VoxelMeshFactory_VB",
                                                     ResourceUsage.Default, 
                                                     10);
        }
    }
}