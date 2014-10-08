using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs.Vertex;
using SharpDX;

namespace S33M3Resources.Primitives
{
    /// <summary>
    /// Will create Primitive Base vertex/Index data
    /// </summary>
    public static class Generator
    {

        public struct CubeWithFaceStruct
        {
            public enum CubeFaces : byte
            {
                Back = 0,
                Front = 1,
                Bottom = 2,
                Top = 3,
                Left = 4,
                Right = 5
            }
            public Vector3 Position;
            public CubeFaces CubeFace;
        }

        public static void CubeWithFace(float size, out CubeWithFaceStruct[] vertices, out short[] indices)
        {
            size /= 2;

            vertices = new CubeWithFaceStruct[24];

            //Front
            vertices[0] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Front, Position = new Vector3(-size, size, size) };
            vertices[1] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Front, Position = new Vector3(size, size, size) };
            vertices[2] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Front, Position = new Vector3(-size, -size, size) };
            vertices[3] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Front, Position = new Vector3(size, -size, size) };
            vertices[4] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Back, Position = new Vector3(-size, size, -size) };
            vertices[5] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Back, Position = new Vector3(size, size, -size) };
            vertices[6] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Back, Position = new Vector3(-size, -size, -size) };
            vertices[7] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Back, Position = new Vector3(size, -size, -size) };
            vertices[8] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Top, Position = new Vector3(-size, size, -size) };
            vertices[9] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Top, Position = new Vector3(size, size, size) };
            vertices[10] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Top, Position = new Vector3(-size, size, size) };
            vertices[11] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Top, Position = new Vector3(size, size, -size) };
            vertices[12] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Bottom, Position = new Vector3(-size, -size, size) };
            vertices[13] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Bottom, Position = new Vector3(-size, -size, -size) };
            vertices[14] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Bottom, Position = new Vector3(size, -size, size) };
            vertices[15] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Bottom, Position = new Vector3(size, -size, -size) };
            vertices[16] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Left, Position = new Vector3(-size, size, -size) };
            vertices[17] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Left, Position = new Vector3(-size, size, size) };
            vertices[18] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Left, Position = new Vector3(-size, -size, -size) };
            vertices[19] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Left, Position = new Vector3(-size, -size, size) };
            vertices[20] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Right, Position = new Vector3(size, size, -size) };
            vertices[21] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Right, Position = new Vector3(size, size, size) };
            vertices[22] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Right, Position = new Vector3(size, -size, size) };
            vertices[23] = new CubeWithFaceStruct() { CubeFace = CubeWithFaceStruct.CubeFaces.Right, Position = new Vector3(size, -size, -size) };

            indices = new short[] { 0, 2, 1, 2, 3, 1 , 4, 5, 6, 6, 5, 7, 8, 10, 9, 8, 9, 11, 12, 13, 14, 13, 15, 14, 16, 18, 19, 17, 16, 19, 20, 22, 23, 21, 22, 20};

        }

        public static void Cube(float size, out Vector3[] vertices, out short[] indices)
        {
            size /= 2;

            vertices = new Vector3[8];
              
            //
            //        4 ----- 5
            //       / |     /| 
            //      /  |    / |
            //     /   6 --/--7 
            //    0 --/-- 1  /
            //    |  /    | /
            //    | /     |/
            //    2 ----- 3

            //Front
            vertices[0] = new Vector3(-size, size,-size);
            vertices[1] = new Vector3(size, size,-size);
            vertices[2] = new Vector3(-size, -size, -size);
            vertices[3] = new Vector3(size, -size, -size);
            vertices[4] = new Vector3(-size, size, size);
            vertices[5] = new Vector3(size, size, size);
            vertices[6] = new Vector3(-size, -size, size);
            vertices[7] = new Vector3(size, -size, size);

            indices = new short[] { 7, 5, 4, 4, 6, 7,
                                    2, 0, 1, 1, 3, 2, 
                                    6, 2, 3, 3, 7, 6,
                                    0, 4, 5, 5, 1, 0,
                                    6, 4, 0, 0, 2, 6,
                                    3, 1, 5, 5, 7, 3
                                    };
        }

        public enum PrimitiveType
        {
            LineList,
            TriangleList
        }

        public static void Box(Vector3 size, PrimitiveType primitiveOutput, out Vector3[] vertices, out ushort[] indices)
        {
            size /= 2;

            vertices = new Vector3[8];

            vertices[0] = new Vector3(-size.X, size.Y, -size.Z);
            vertices[1] = new Vector3(size.X, size.Y, -size.Z);
            vertices[2] = new Vector3(-size.X, -size.Y, -size.Z);
            vertices[3] = new Vector3(size.X, -size.Y, -size.Z);
            vertices[4] = new Vector3(-size.X, size.Y, size.Z);
            vertices[5] = new Vector3(size.X, size.Y, size.Z);
            vertices[6] = new Vector3(-size.X, -size.Y, size.Z);
            vertices[7] = new Vector3(size.X, -size.Y, size.Z);

            //
            //        4 ----- 5
            //       / |     /| 
            //      /  |    / |
            //     /   6 --/--7 
            //    0 --/-- 1  /
            //    |  /    | /
            //    | /     |/
            //    2 ----- 3


            indices = null;
            switch (primitiveOutput)
            {
                case PrimitiveType.LineList:
                    indices = new ushort[] { 0, 1, 1, 3, 3, 2, 2, 0, 
                                            4, 5, 5, 7, 7, 6, 6, 4,
                                            0, 4, 2, 6, 1, 5, 3, 7};
                    break;
                case PrimitiveType.TriangleList:
                    indices = new ushort[] { 7, 5, 4, 4, 6, 7,
                                    2, 0, 1, 1, 3, 2, 
                                    6, 2, 3, 3, 7, 6,
                                    0, 4, 5, 5, 1, 0,
                                    6, 4, 0, 0, 2, 6,
                                    3, 1, 5, 5, 7, 3
                                    };
                    break;
            }




        }

    }
}
