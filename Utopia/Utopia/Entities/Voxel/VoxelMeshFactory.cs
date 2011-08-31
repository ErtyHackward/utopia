using System;
using System.Collections.Generic;
using S33M3Engines;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.Enums;

namespace Utopia.Entities.Voxel
{
    public class VoxelMeshFactory
    {
        //TODO (Simon) VoxelMeshFactory generic implementation with <VertexPositionColor> or <VertexPositionColorTexture>

        public VertexBuffer<VertexPositionColor> VertexBuffer;
        private readonly List<VertexPositionColor> _vertexList = new List<VertexPositionColor>();
        public ByteVector3 Size;
        public byte[,,] Blocks;
        private readonly D3DEngine _d3DEngine;

        public VoxelMeshFactory(D3DEngine d3DEngine)
        {
            _d3DEngine = d3DEngine;
        }

        public void GenCubesFaces()
        {
            for (int x = 0; x < Blocks.GetLength(0); x++)
            {
                for (int y = 0; y < Blocks.GetLength(1); y++)
                {
                    for (int z = 0; z < Blocks.GetLength(2); z++)
                    {
                        byte blockType = Blocks[x, y, z];

                        if (blockType == 0) continue;

                        BuildBlockVertices(_vertexList, blockType, x, y, z);
                    }
                }
            }
        }


        //Temporary method, drawing will be in a separate class
        public void SendMeshToGraphicCard()
        {
            if (_vertexList.Count == 0)
            {
                if (VertexBuffer != null) VertexBuffer.Dispose();
                VertexBuffer = null;
                return;
            }

            if (VertexBuffer == null)
            {
                VertexBuffer = new VertexBuffer<VertexPositionColor>(_d3DEngine, _vertexList.Count,
                                                                      VertexPositionColor.VertexDeclaration,
                                                                      PrimitiveTopology.TriangleList,
                                                                      ResourceUsage.Default, 10);
            }
            VertexBuffer.SetData(_vertexList.ToArray());
            _vertexList.Clear();
        }

        protected void BuildBlockVertices(List<VertexPositionColor> vertexList, byte blockType, int x, int y, int z)
        {
            byte blockXDecreasing = BlockAt(x - 1, y, z);
            byte blockXIncreasing = BlockAt(x + 1, y, z);
            byte blockYDecreasing = BlockAt(x, y - 1, z);
            byte blockYIncreasing = BlockAt(x, y + 1, z);
            byte blockZDecreasing = BlockAt(x, y, z - 1);
            byte blockZIncreasing = BlockAt(x, y, z + 1);

            if (blockXDecreasing == 0)
                BuildFaceVertices(x, y, z, CubeFace.Left, blockType); //X-
            if (blockXIncreasing == 0)
                BuildFaceVertices(x, y, z, CubeFace.Right, blockType); //X+
            if (blockYDecreasing == 0)
                BuildFaceVertices(x, y, z, CubeFace.Bottom, blockType); //Y-
            if (blockYIncreasing == 0)
                BuildFaceVertices(x, y, z, CubeFace.Top, blockType); //Y+
            if (blockZDecreasing == 0)
                BuildFaceVertices(x, y, z, CubeFace.Front, blockType); //Z-
            if (blockZIncreasing == 0)
                BuildFaceVertices(x, y, z, CubeFace.Back, blockType); //Z+
        }

        private byte BlockAt(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) return 0;
            if (x > Blocks.GetLength(0) - 1 || y > Blocks.GetLength(1) - 1 || z > Blocks.GetLength(2) - 1) return 0;
            return Blocks[x, y, z];
        }

        private void BuildFaceVertices(int x, int y, int z, CubeFace faceDir, byte blockType)
        {
            //actually only handles 64 colors, so all blockType> 63 will have default color
            Color color = ColorLookup.Colours[blockType];

            switch (faceDir)
            {
                case CubeFace.Right: //X+
                    {
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y + 1, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z), color));
                    }
                    break;

                case CubeFace.Left: //X-
                    {
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y, z), color));
                    }
                    break;

                case CubeFace.Top: //Y+
                    {
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y + 1, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y + 1, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z + 1), color));
                    }
                    break;

                case CubeFace.Bottom: //Y-
                    {
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y, z), color));
                    }
                    break;

                case CubeFace.Back: //Z+
                    {
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y + 1, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z + 1), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y, z + 1), color));
                    }
                    break;

                case CubeFace.Front: //Z-
                    {
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x + 1, y, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y + 1, z), color));
                        _vertexList.Add(new VertexPositionColor(new Vector3(x, y, z), color));
                    }
                    break;
            }
        }

        public void RandomFill(int emptyProbabilityPercent)
        {
            Random r = new Random();
            for (uint x = 0; x < Blocks.GetLength(0); x++)
            {
                for (uint y = 0; y < Blocks.GetLength(1); y++)
                {
                    for (uint z = 0; z < Blocks.GetLength(2); z++)
                    {
                        if (r.Next(100) < emptyProbabilityPercent)
                            Blocks[x, y, z] = 0;
                        else
                            Blocks[x, y, z] = (byte) r.Next(63);
                    }
                }
            }
        }
    }
}