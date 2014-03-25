using System.Collections.Generic;
using System.Linq;
using S33M3DXEngine.Main;
using SharpDX;
using SharpDX.Direct3D;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Enums;
using S33M3Resources.Structs.Vertex;
using S33M3DXEngine.Buffers;
using S33M3DXEngine;
using S33M3Resources.Structs;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Creates meshes for voxel models
    /// </summary>
    public class VoxelMeshFactory : BaseComponent
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

        /// <summary>
        /// Calculates frame bounding boxes for the shape collision test
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public static List<BoundingBox> GenerateShapeBBoxes(InsideDataProvider blockData)
        {
            // first of all create bb for each non-empty block
            var list = blockData.AllBlocks().Select(pair => new BoundingBox(pair.Key, pair.Key + Vector3.One)).ToList();

            // enlarge each block as possible

            for (int i = 0; i < list.Count; i++)
            {
                var box = list[i];

                while (TryEnlargeBBox(ref box, blockData)) { }

                list[i] = box;
            }

            // sort boxes by their volume desc
            list.Sort((b1, b2) => b2.GetVolume().CompareTo(b1.GetVolume()));

            // remove boxes that completely includes other and duplicates
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var box = list[i];

                bool remove = false;

                for (int j = 0; j < list.Count; j++)
                {
                    if (i != j && (box == list[j] || list[j].Contains(ref box) == ContainmentType.Contains))
                    {
                        remove = true;
                        break;
                    }
                }

                if (remove)
                    list.RemoveAt(i);
            }

            return list;
        }

        private static bool TryEnlargeBBox(ref BoundingBox box, InsideDataProvider blockData)
        {
            // try to enlarge each side

            var size = box.GetSize();
            bool canEnlarge = true;
            bool enlarged = false;

            #region top

            if (box.Maximum.Y < blockData.ChunkSize.Y)
            {
                for (int x = 0; canEnlarge && x < size.X; x++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        if (blockData.GetBlock(x + (int)box.Minimum.X, (int)box.Maximum.Y, z + (int)box.Minimum.Z) == 0)
                        {
                            canEnlarge = false;
                            break;
                        }
                    }
                }

                if (canEnlarge)
                {
                    box.Maximum.Y++;
                    enlarged = true;
                    size = box.GetSize();
                }
            }

            #endregion

            #region bottom
            canEnlarge = true;
            if (box.Minimum.Y > 0)
            {
                for (int x = 0; canEnlarge && x < size.X; x++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        if (blockData.GetBlock(x + (int)box.Minimum.X, (int)box.Minimum.Y - 1, z + (int)box.Minimum.Z) == 0)
                        {
                            canEnlarge = false;
                            break;
                        }
                    }
                }

                if (canEnlarge)
                {
                    box.Minimum.Y--;
                    enlarged = true;
                    size = box.GetSize();
                }
            }

            #endregion

            #region left
            canEnlarge = true;
            if (box.Minimum.X > 0)
            {
                for (int y = 0; canEnlarge && y < size.Y; y++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        if (blockData.GetBlock((int)box.Minimum.X - 1, y + (int)box.Minimum.Y, z + (int)box.Minimum.Z) == 0)
                        {
                            canEnlarge = false;
                            break;
                        }
                    }
                }

                if (canEnlarge)
                {
                    box.Minimum.X--;
                    enlarged = true;
                    size = box.GetSize();
                }
            }
            
            #endregion

            #region right
            canEnlarge = true;
            if (box.Maximum.X < blockData.ChunkSize.X)
            {
                for (int y = 0; canEnlarge && y < size.Y; y++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        if (blockData.GetBlock((int)box.Maximum.X, y + (int)box.Minimum.Y, z + (int)box.Minimum.Z) == 0)
                        {
                            canEnlarge = false;
                            break;
                        }
                    }
                }

                if (canEnlarge)
                {
                    box.Maximum.X++;
                    enlarged = true;
                    size = box.GetSize();
                }
            }
            #endregion

            #region front
            canEnlarge = true;
            if (box.Maximum.Z < blockData.ChunkSize.Z)
            {
                for (int y = 0; canEnlarge && y < size.Y; y++)
                {
                    for (int x = 0; x < size.X; x++)
                    {
                        if (blockData.GetBlock(x + (int)box.Minimum.X, y + (int)box.Minimum.Y, (int)box.Maximum.Z) == 0)
                        {
                            canEnlarge = false;
                            break;
                        }
                    }
                }

                if (canEnlarge)
                {
                    box.Maximum.Z++;
                    enlarged = true;
                    size = box.GetSize();
                }
            }
            #endregion 

            #region back
            canEnlarge = true;
            if (box.Minimum.Z > 0)
            {
                for (int y = 0; canEnlarge && y < size.Y; y++)
                {
                    for (int x = 0; x < size.X; x++)
                    {
                        if (blockData.GetBlock(x + (int)box.Minimum.X, y + (int)box.Minimum.Y, (int)box.Minimum.Z - 1) == 0)
                        {
                            canEnlarge = false;
                            break;
                        }
                    }
                }

                if (canEnlarge)
                {
                    box.Minimum.Z--;
                    enlarged = true;
                    size = box.GetSize();
                }
            }
            #endregion

            return enlarged;
        }

        public void GenerateVoxelFaces(VoxelFrame frame, out List<VertexVoxelInstanced> vertices, out List<ushort> indices)
        {
            var blockData = frame.BlockData;
            var size      = blockData.ChunkSize;
            vertices      = new List<VertexVoxelInstanced>();
            indices       = new List<ushort>();
            var dico      = new Dictionary<long, int>();


            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        byte blockType = blockData.GetBlock(x, y, z);
                        if (blockType == 0) 
                            continue;
                        var vec = new Vector4B(x, y, z, blockType);
                        if (IsEmpty(ref blockData, ref size, x, y, z - 1))
                            GenerateFaces(ref frame, CubeFaces.Back, ref dico, vec, ref vertices, ref indices);
                        
                        if (IsEmpty(ref blockData, ref size, x, y - 1, z))
                            GenerateFaces(ref frame, CubeFaces.Bottom, ref dico, vec, ref vertices, ref indices);

                        if (IsEmpty(ref blockData, ref size, x, y, z + 1))
                            GenerateFaces(ref frame, CubeFaces.Front, ref dico, vec, ref vertices, ref indices);
                        
                        if (IsEmpty(ref blockData, ref size, x - 1, y, z))
                            GenerateFaces(ref frame, CubeFaces.Left, ref dico, vec, ref vertices, ref indices);

                        if (IsEmpty(ref blockData, ref size, x + 1, y, z))
                            GenerateFaces(ref frame, CubeFaces.Right, ref dico, vec, ref vertices, ref indices);

                        if (IsEmpty(ref blockData, ref size, x, y + 1, z))
                            GenerateFaces(ref frame, CubeFaces.Top, ref dico, vec, ref vertices, ref indices);
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

        private bool IsEmpty(ref InsideDataProvider blockData, ref Vector3I size, int x, int y, int z, FrameMirror mirror)
        {
            if (x < 0)
            {
                if ((mirror & FrameMirror.MirrorLeft) == FrameMirror.MirrorLeft)
                {
                    x = 0;
                } 
                else if ((mirror & FrameMirror.TileLeft) == FrameMirror.TileLeft)
                {
                    x = size.X + x;
                }
                else 
                    return true;
            }
            if (x == size.X)
            {
                if ((mirror & FrameMirror.MirrorRight) == FrameMirror.MirrorRight)
                {
                    x = size.X - 1;
                }
                else if ((mirror & FrameMirror.TileRight) == FrameMirror.TileRight)
                {
                    x = 0;
                }
                else
                    return true;
            }
            if (y < 0)
            {
                if ((mirror & FrameMirror.MirrorBottom) == FrameMirror.MirrorBottom)
                {
                    y = 0;
                } 
                else if ((mirror & FrameMirror.TileBottom) == FrameMirror.TileBottom)
                {
                    y = size.Y - 1;
                } 
                else 
                    return true;
            }
            if (y == size.Y)
            {
                if ((mirror & FrameMirror.MirrorTop) == FrameMirror.MirrorTop)
                {
                    y = size.Y - 1;
                }
                else if ((mirror & FrameMirror.TileTop) == FrameMirror.TileTop)
                {
                    y = 0;
                }
                else
                    return true;
            }

            if (z < 0)
            {
                if ((mirror & FrameMirror.MirrorFront) == FrameMirror.MirrorFront)
                {
                    z = 0;
                } 
                else if ((mirror & FrameMirror.TileFront) == FrameMirror.TileFront)
                {
                    z = size.Z - 1;
                }
                else
                    return true;
            }
            if (z == size.Z)
            {
                if ((mirror & FrameMirror.MirrorBack) == FrameMirror.MirrorBack)
                {
                    z = size.Z - 1;
                }
                else if ((mirror & FrameMirror.TileBack) == FrameMirror.TileBack)
                {
                    z = 0;
                }
                else
                    return true;
            }

            return blockData.GetBlock(x, y, z) == 0;
        }

        private byte Avg(int b1, int b2, int b3, int b4)
        {
            return (byte)((b1 + b2 + b3 + b4)/4);
        }

        private long Compress(byte faceType, byte x, byte y, byte z, byte color)
        {
            return faceType + (x << 8) + (y << 16) + ((long)z << 32) + ((long)color << 40);
        }

        private void GenerateFaces(ref VoxelFrame voxelFrame, CubeFaces cubeFace, ref Dictionary<long, int> dico, Vector4B cubePosition, ref List<VertexVoxelInstanced> vertices, ref List<ushort> indices)
        {
            // hash and index

            var blockData = voxelFrame.BlockData;

            Vector4B topLeft;
            Vector4B topRight;
            Vector4B bottomLeft;
            Vector4B bottomRight;

            var chunkSize = blockData.ChunkSize;
            var cubeColor = blockData.GetBlock(cubePosition.X, cubePosition.Y, cubePosition.Z);
            var cubeFaceType = (byte)cubeFace;
            var faceTypeByte = (byte)cubeFace;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;
            long hashVertex;
            bool vertexInDico;
            int generatedVertex = 0;
            var verticeCubeOffset = vertices.Count;
            
            switch (cubeFace)
            {
                #region Front
                case CubeFaces.Front:
                    {
                        var mirror = cubePosition.Z != chunkSize.Z - 1 ? voxelFrame.FrameMirror : FrameMirror.None;

                        var lfront              = IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y,     cubePosition.Z + 1, mirror) ? 255 : 0;
                        var ltopFront           = IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y + 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lbottomFront        = IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y - 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lrightFront         = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y,     cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lfrontLeft          = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y,     cubePosition.Z + 1, mirror) ? 255 : 0;
                        var ltopLeftFront       = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var ltopFrontRight      = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lbottomLeftFront    = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lbottomFrontRight   = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z + 1, mirror) ? 255 : 0;

                        topLeft     = cubePosition + new Vector4B(0, 1, 1, 0); // topLeftFront
                        topRight    = cubePosition + new Vector4B(1, 1, 1, 0); // topRightFront
                        bottomLeft  = cubePosition + new Vector4B(0, 0, 1, 0); // bottomLeftFront
                        bottomRight = cubePosition + new Vector4B(1, 0, 1, 0); // bottomRightFront

                        hashVertex = Compress(cubeFaceType, topLeft.X, topLeft.Y, topLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // topLeftFront
                            var light = Avg(lfront, lfrontLeft, ltopFront, ltopLeftFront);

                            vertices.Add(new VertexVoxelInstanced(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, topRight.X, topRight.Y, topRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topRightFront
                            var light = Avg(lfront, ltopFront, lrightFront, ltopFrontRight);

                            vertices.Add(new VertexVoxelInstanced(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomLeft.X, bottomLeft.Y, bottomLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomLeftFront
                            var light = Avg(lfront, lfrontLeft, lbottomFront, lbottomLeftFront);

                            vertices.Add(new VertexVoxelInstanced(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomRight.X, bottomRight.Y, bottomRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottomRightFront
                            var light = Avg(lfront, lrightFront, lbottomFront, lbottomFrontRight);

                            vertices.Add(new VertexVoxelInstanced(bottomRight, faceTypeByte, light));
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
                        var mirror = cubePosition.Z != 0 ? voxelFrame.FrameMirror : FrameMirror.None;

                        var lback =             IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y,     cubePosition.Z - 1, mirror) ? 255 : 0;
                        var ltopBack =          IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y + 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lbottomBack =       IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y - 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lbackRight =        IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y,     cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lleftback =         IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y,     cubePosition.Z - 1, mirror) ? 255 : 0;
                        var ltopRightBack =     IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var ltopBackLeft =      IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lbottomRightBack =  IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lbottomBackLeft =   IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z - 1, mirror) ? 255 : 0;

                        topLeft     = cubePosition + new Vector4B(1, 1, 0, 0); // topRightBack
                        topRight    = cubePosition + new Vector4B(0, 1, 0, 0); // topLeftBack
                        bottomLeft  = cubePosition + new Vector4B(1, 0, 0, 0); // bottomRightBack
                        bottomRight = cubePosition + new Vector4B(0, 0, 0, 0); // bottomLeftBack

                        hashVertex = Compress(cubeFaceType, topRight.X, topRight.Y, topRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // topLeftBack
                            var light = Avg(lback, ltopBack, lleftback, ltopBackLeft);

                            vertices.Add(new VertexVoxelInstanced(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, topLeft.X, topLeft.Y, topLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topRightBack
                            var light = Avg(lback, lbackRight, ltopBack, ltopRightBack);
                            
                            vertices.Add(new VertexVoxelInstanced(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomRight.X, bottomRight.Y, bottomRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomLeftBack
                            var light = Avg(lback, lbottomBack, lleftback, lbottomBackLeft);

                            vertices.Add(new VertexVoxelInstanced(bottomRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomLeft.X, bottomLeft.Y, bottomLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottomRightBack
                            var light = Avg(lback, lbackRight, lbottomBack, lbottomRightBack);
                            
                            vertices.Add(new VertexVoxelInstanced(bottomLeft, faceTypeByte, light));
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
                        var mirror = cubePosition.Y != chunkSize.Y - 1 ? voxelFrame.FrameMirror : FrameMirror.None;

                        var ltop           = IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y + 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var ltopLeft       = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var ltopBack       = IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y + 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var ltopRight      = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var ltopFront      = IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y + 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var ltopLeftFront  = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var ltopFrontRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var ltopRightBack  = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var ltopBackLeft   = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z - 1, mirror) ? 255 : 0;

                        topLeft     = cubePosition + new Vector4B(0, 1, 0, 0); // topLeftBack
                        topRight    = cubePosition + new Vector4B(1, 1, 0, 0); // topRightBack
                        bottomLeft  = cubePosition + new Vector4B(0, 1, 1, 0); // topLeftFront
                        bottomRight = cubePosition + new Vector4B(1, 1, 1, 0); // topRightFront

                        hashVertex = Compress(cubeFaceType, topLeft.X, topLeft.Y, topLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);
                            
                            // topLeftBack
                            var light = Avg(ltop, ltopLeft, ltopBack, ltopBackLeft);

                            vertices.Add(new VertexVoxelInstanced(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomRight.X, bottomRight.Y, bottomRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topRightFront
                            var light = Avg(ltop, ltopRight, ltopFront, ltopFrontRight);

                            vertices.Add(new VertexVoxelInstanced(bottomRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomLeft.X, bottomLeft.Y, bottomLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // topLeftFront
                            var light = Avg(ltop, ltopLeft, ltopFront, ltopLeftFront);

                            vertices.Add(new VertexVoxelInstanced(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, topRight.X, topRight.Y, topRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // topRightBack
                            var light = Avg(ltop, ltopRight, ltopBack, ltopRightBack);

                            vertices.Add(new VertexVoxelInstanced(topRight, faceTypeByte, light));
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
                        var mirror = cubePosition.Y != 0 ? voxelFrame.FrameMirror : FrameMirror.None;

                        var lbottom           = IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y - 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var lbottomLeft       = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var lbottomBack       = IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y - 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lbottomright      = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var lbottomFront      = IsEmpty(ref blockData, ref chunkSize, cubePosition.X,     cubePosition.Y - 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lbottomLeftFront  = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lbottomFrontRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lbottomRightBack  = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lbottomBackLeft   = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z - 1, mirror) ? 255 : 0;

                        topLeft     = cubePosition + new Vector4B(0, 0, 1, 0); // bottomLeftFront
                        topRight    = cubePosition + new Vector4B(1, 0, 1, 0); // bottomRightFront
                        bottomLeft  = cubePosition + new Vector4B(0, 0, 0, 0); // bottomLeftBack
                        bottomRight = cubePosition + new Vector4B(1, 0, 0, 0); // bottomRightBack

                        hashVertex = Compress(cubeFaceType, topLeft.X, topLeft.Y, topLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // bottomLeftFront
                            var light = Avg(lbottom, lbottomLeft, lbottomFront, lbottomLeftFront);

                            vertices.Add(new VertexVoxelInstanced(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomLeft.X, bottomLeft.Y, bottomLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // bottomLeftBack
                            var light = Avg(lbottom, lbottomLeft, lbottomBack, lbottomBackLeft);

                            vertices.Add(new VertexVoxelInstanced(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, topRight.X, topRight.Y, topRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomRightFront
                            var light = Avg(lbottom, lbottomright, lbottomFront, lbottomFrontRight);

                            vertices.Add(new VertexVoxelInstanced(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomRight.X, bottomRight.Y, bottomRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottomRightBack
                            var light = Avg(lbottom, lbottomright, lbottomBack, lbottomRightBack);

                            vertices.Add(new VertexVoxelInstanced(bottomRight, faceTypeByte, light));
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
                        var mirror = cubePosition.X != 0 ? voxelFrame.FrameMirror : FrameMirror.None;

                        var lleft            = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y,     cubePosition.Z,     mirror) ? 255 : 0;
                        var ltopLeft         = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var lbottomLeft      = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var lfrontLeft       = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y,     cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lleftback        = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y,     cubePosition.Z - 1, mirror) ? 255 : 0;
                        var ltopLeftFront    = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var ltopBackLeft     = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y + 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lbottomLeftFront = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lbottomBackLeft  = IsEmpty(ref blockData, ref chunkSize, cubePosition.X - 1, cubePosition.Y - 1, cubePosition.Z - 1, mirror) ? 255 : 0;

                        topLeft     = cubePosition + new Vector4B(0, 1, 0, 0); // topLeftBack
                        bottomRight = cubePosition + new Vector4B(0, 0, 1, 0); // bottomLeftFront
                        bottomLeft  = cubePosition + new Vector4B(0, 0, 0, 0); // bottomLeftBack
                        topRight    = cubePosition + new Vector4B(0, 1, 1, 0); // topLeftFront

                        hashVertex = Compress(cubeFaceType, topLeft.X, topLeft.Y, topLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // topLeftBack
                            var light = Avg(lleft, ltopLeft, lleftback, ltopBackLeft);

                            vertices.Add(new VertexVoxelInstanced(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, topRight.X, topRight.Y, topRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topLeftFront
                            var light = Avg(lleft, ltopLeft, lfrontLeft, ltopLeftFront);

                            vertices.Add(new VertexVoxelInstanced(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomLeft.X, bottomLeft.Y, bottomLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomLeftBack
                            var light = Avg(lleft, lbottomLeft, lleftback, lbottomBackLeft);

                            vertices.Add(new VertexVoxelInstanced(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomRight.X, bottomRight.Y, bottomRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottomLeftFront
                            var light = Avg(lleft, lbottomLeft, lfrontLeft, lbottomLeftFront);

                            vertices.Add(new VertexVoxelInstanced(bottomRight, faceTypeByte, light));
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
                        var mirror = cubePosition.X != chunkSize.X - 1 ? voxelFrame.FrameMirror : FrameMirror.None;

                        var lright            = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y,     cubePosition.Z,     mirror) ? 255 : 0;
                        var ltopRight         = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var lbottomright      = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z,     mirror) ? 255 : 0;
                        var lbackRight        = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y,     cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lrightFront       = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y,     cubePosition.Z + 1, mirror) ? 255 : 0;
                        var ltopFrontRight    = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var ltopRightBack     = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y + 1, cubePosition.Z - 1, mirror) ? 255 : 0;
                        var lbottomFrontRight = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z + 1, mirror) ? 255 : 0;
                        var lbottomRightBack  = IsEmpty(ref blockData, ref chunkSize, cubePosition.X + 1, cubePosition.Y - 1, cubePosition.Z - 1, mirror) ? 255 : 0;

                        topLeft     = cubePosition + new Vector4B(1, 1, 1, 0); // topRightFront
                        topRight    = cubePosition + new Vector4B(1, 1, 0, 0); // topRightBack
                        bottomLeft  = cubePosition + new Vector4B(1, 0, 1, 0); // bottomRightFront
                        bottomRight = cubePosition + new Vector4B(1, 0, 0, 0); // bottonRightBack

                        hashVertex = Compress(cubeFaceType, topRight.X, topRight.Y, topRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset0);
                        if (!vertexInDico)
                        {
                            vertexOffset0 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset0);

                            // topRightBack
                            var light = Avg(lright, ltopRight, lbackRight, ltopRightBack);

                            vertices.Add(new VertexVoxelInstanced(topRight, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, topLeft.X, topLeft.Y, topLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset1);
                        if (!vertexInDico)
                        {
                            vertexOffset1 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset1);

                            // topRightFront
                            var light = Avg(lright, ltopRight, lrightFront, ltopFrontRight);

                            vertices.Add(new VertexVoxelInstanced(topLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomLeft.X, bottomLeft.Y, bottomLeft.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset2);
                        if (!vertexInDico)
                        {
                            vertexOffset2 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset2);

                            // bottomRightFront
                            var light = Avg(lright, lbottomright, lrightFront, lbottomFrontRight);

                            vertices.Add(new VertexVoxelInstanced(bottomLeft, faceTypeByte, light));
                            generatedVertex++;
                        }

                        hashVertex = Compress(cubeFaceType, bottomRight.X, bottomRight.Y, bottomRight.Z, cubeColor);
                        vertexInDico = dico.TryGetValue(hashVertex, out vertexOffset3);
                        if (!vertexInDico)
                        {
                            vertexOffset3 = generatedVertex + verticeCubeOffset;
                            dico.Add(hashVertex, vertexOffset3);

                            // bottonRightBack
                            var light = Avg(lright, lbottomright, lbackRight, lbottomRightBack);

                            vertices.Add(new VertexVoxelInstanced(bottomRight, faceTypeByte, light));
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

        public InstancedVertexBuffer<VertexVoxelInstanced, VoxelInstanceData> InitBuffer(List<VertexVoxelInstanced> vertice)
        {
            
            var vb = ToDispose(new InstancedVertexBuffer<VertexVoxelInstanced, VoxelInstanceData>(_d3DEngine.Device, PrimitiveTopology.TriangleList, "VoxelMeshFactory"));
            if(vertice.Count > 0)
                vb.SetFixedData(vertice.ToArray());
            return vb;
        }

        public IndexBuffer<ushort> InitBuffer(List<ushort> indices)
        {
            var ib = ToDispose(new IndexBuffer<ushort>(_d3DEngine.Device, indices.Count, "VoxelMeshFactory_IB"));

            if(indices.Count > 0)
                ib.SetData(_d3DEngine.ImmediateContext, indices.ToArray());

            return ib;
        }
    }
}