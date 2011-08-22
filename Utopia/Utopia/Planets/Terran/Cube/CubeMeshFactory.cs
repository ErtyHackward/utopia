using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using S33M3Engines.Struct.Vertex;
using Utopia.Planets.Terran.World;
using SharpDX;
using Utopia.Planets.Terran.Flooding;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Worlds.Chunks.Enums;

namespace Utopia.Planets.Terran.Cube
{
    public static class CubeMeshFactory
    {
        private static TerraWorld _world;

        public static void Init(TerraWorld world)
        {
            _world = world;
        }

        public static void GenSolidCubeFace(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, ref List<VertexCubeSolid> cubeVertices, ref List<ushort> cubeIndices, ref Dictionary<string, int> cubeVerticeDico)
        {
            int verticeCubeOffset = cubeVertices.Count;
            int indiceCubeOffset = cubeIndices.Count;
            ByteColor newColor = cube.EmissiveColor;

            RenderCubeProfile cubeProfile = RenderCubeProfile.CubesProfile[cube.Id];
            bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;

            //Les 4 vertex de ma face.... en fct de leur position dans le cube leur valeur en Z va changer ! (Face Top, Bottom, ...
            ByteVector4 topLeft;
            ByteVector4 topRight;
            ByteVector4 bottomLeft;
            ByteVector4 bottomRight;

            int cubeFaceType = (int)cubeFace;

            ByteVector4 vertexInfo = new ByteVector4((byte)0, (byte)cubeFace, (byte)0, (byte)0);

            string hashVertex;
            int generatedVertex = 0;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;
            int baseindex, baseIndexP1, baseIndexM1;

            switch (cubeFace)
            {
                case CubeFace.Front:

                    ByteColor Back_Cube, BackLeft_Cube, BackRight_Cube, BackTop_Cube, BackBottom_Cube, BackLeftBottom_Cube, BackRightBottom_Cube, BackLeftTop_Cube, BackRightTop_Cube;
                    try
                    {
                        baseindex = _world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z + 1);
                        baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Plus1);
                        baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Minus1);

                        //Get the 9 Facing cubes to the face
                        Back_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                        BackLeft_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                        BackRight_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                        BackTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        if (cubePosiInWorld.Y > 0)
                        {
                            BackBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            BackLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            BackRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                        }
                        else
                        {
                            BackBottom_Cube = new ByteColor();
                            BackLeftBottom_Cube = new ByteColor();
                            BackRightBottom_Cube = new ByteColor();
                        }
                        BackLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        BackRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;

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

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackTop_Cube, BackLeftTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackTop_Cube, BackRightTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackBottom_Cube, BackLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackBottom_Cube, BackRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref  newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset3));

                    break;
                case CubeFace.Back:

                    ByteColor Front_Cube, FrontLeft_Cube, FrontRight_Cube, FrontTop_Cube, FrontBottom_Cube, FrontLeftBottom_Cube, FrontRightBottom_Cube, FrontLeftTop_Cube, FrontRightTop_Cube;
                    try
                    {
                        baseindex = _world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z - 1);
                        baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Plus1);
                        baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Minus1);

                        //Get the 9 Facing cubes to the face
                        Front_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                        FrontLeft_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                        FrontRight_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                        FrontTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;

                        if (cubePosiInWorld.Y > 0)
                        {
                            FrontBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            FrontLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            FrontRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                        }
                        else
                        {
                            FrontBottom_Cube = new ByteColor();
                            FrontLeftBottom_Cube = new ByteColor();
                            FrontRightBottom_Cube = new ByteColor();
                        }

                        FrontLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        FrontRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;

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

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontLeftTop_Cube, FrontLeft_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontRight_Cube, FrontRightTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontLeft_Cube, FrontLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontRight_Cube, FrontRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset1));

                    break;
                case CubeFace.Top:

                    ByteColor Bottom_Cube, BottomLeft_Cube, BottomRight_Cube, BottomTop_Cube, BottomBottom_Cube, BottomLeftTop_Cube, BottomRightTop_Cube, BottomLeftBottom_Cube, BottomRightBottom_Cube;
                    try
                    {
                        baseindex = _world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y + 1, cubePosiInWorld.Z);
                        baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Plus1);
                        baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Minus1);

                        //Get the 9 Facing cubes to the face
                        Bottom_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                        BottomLeft_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                        BottomRight_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                        BottomTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        BottomBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;
                        BottomLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        BottomRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        BottomLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;
                        BottomRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;
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

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomLeft_Cube, BottomLeftTop_Cube, BottomTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomRight_Cube, BottomBottom_Cube, BottomRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomBottom_Cube, BottomLeft_Cube, BottomLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomTop_Cube, BottomRight_Cube, BottomRightTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    break;

                case CubeFace.Bottom:

                    ByteColor Top_Cube, TopLeft_Cube, TopRight_Cube, TopTop_Cube, TopBottom_Cube, TopLeftTop_Cube, TopRightTop_Cube, TopLeftBottom_Cube, TopRightBottom_Cube;
                    try
                    {
                        baseindex = _world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y - 1, cubePosiInWorld.Z);
                        baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Plus1);
                        baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Minus1);

                        //Get the 9 Facing cubes to the face
                        Top_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                        TopLeft_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                        TopRight_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                        TopTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        TopBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;
                        TopLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        TopRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                        TopLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;
                        TopRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;

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

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopLeft_Cube, TopLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopLeft_Cube, TopLeftTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopRight_Cube, TopRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopRight_Cube, TopRightTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    break;

                case CubeFace.Left:

                    ByteColor Right_Cube, RightLeft_Cube, RightRight_Cube, RightTop_Cube, RightBottom_Cube, RightLeftBottom_Cube, RightRightBottom_Cube, RightLeftTop_Cube, RightRightTop_Cube;
                    try
                    {
                        baseindex = _world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y, cubePosiInWorld.Z);
                        baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1);
                        baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1);

                        //Get the 9 Facing cubes to the face
                        Right_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                        RightLeft_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                        RightRight_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                        RightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;

                        if (cubePosiInWorld.Y > 0)
                        {
                            RightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            RightLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            RightRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                        }
                        else
                        {
                            RightBottom_Cube = new ByteColor();
                            RightLeftBottom_Cube = new ByteColor();
                            RightRightBottom_Cube = new ByteColor();
                        }
                        RightLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        RightRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;

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

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightRight_Cube, RightRightTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightLeft_Cube, RightLeftTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightRight_Cube, RightRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightLeft_Cube, RightLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset0));
                    break;
                case CubeFace.Right:

                    ByteColor Left_Cube, LeftLeft_Cube, LefttRight_Cube, LeftTop_Cube, LeftBottom_Cube, LeftLeftBottom_Cube, LeftRightBottom_Cube, LeftLeftTop_Cube, LeftRightTop_Cube;
                    try
                    {
                        baseindex = _world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y, cubePosiInWorld.Z);
                        baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1);
                        baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1);

                        //Get the 9 Facing cubes to the face
                        Left_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                        LeftLeft_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                        LefttRight_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                        LeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;

                        if (cubePosiInWorld.Y > 0)
                        {
                            LeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            LeftLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                            LeftRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                        }
                        else
                        {
                            LeftBottom_Cube = new ByteColor();
                            LeftLeftBottom_Cube = new ByteColor();
                            LeftRightBottom_Cube = new ByteColor();
                        }

                        LeftLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                        LeftRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;

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

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LefttRight_Cube, LeftRightTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LeftLeft_Cube, LeftLeftTop_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LeftLeft_Cube, LeftLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LefttRight_Cube, LeftRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    break;
            }
        }

        public static void GenSolidCubeFaceTEST(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, ref List<VertexCubeSolid> cubeVertices, ref List<ushort> cubeIndices, ref Dictionary<string, int> cubeVerticeDico)
        {
            int verticeCubeOffset = cubeVertices.Count;
            int indiceCubeOffset = cubeIndices.Count;
            ByteColor newColor = cube.EmissiveColor;

            RenderCubeProfile cubeProfile = RenderCubeProfile.CubesProfile[cube.Id];
            bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;

            //Les 4 vertex de ma face.... en fct de leur position dans le cube leur valeur en Z va changer ! (Face Top, Bottom, ...
            ByteVector4 topLeft;
            ByteVector4 topRight;
            ByteVector4 bottomLeft;
            ByteVector4 bottomRight;

            ByteVector4 vertexInfo = new ByteVector4((byte)0, (byte)cubeFace, (byte)0, (byte)0);

            switch (cubeFace)
            {
                case CubeFace.Front:

                    //Get the 9 Facing cubes to the face
                    ByteColor Back_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BackLeft_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BackRight_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BackTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y + 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BackBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y - 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BackLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BackRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BackLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BackRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z + 1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(0, 1, 1);
                    topRight = cubePosition + new ByteVector4(1, 1, 1);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 1);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 1);

                    //Doing the averaging !
                    if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackTop_Cube, BackLeftTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackTop_Cube, BackRightTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackBottom_Cube, BackLeftBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackBottom_Cube, BackRightBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref  newColor, ref vertexInfo));

                    //Create Vertices
                    cubeIndices.Add((ushort)(0 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(3 + verticeCubeOffset));

                    break;
                case CubeFace.Back:

                    //Get the 9 Facing cubes to the face
                    ByteColor Front_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor FrontLeft_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor FrontRight_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor FrontTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y + 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor FrontBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y - 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor FrontLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor FrontRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor FrontLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor FrontRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z - 1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(1, 1, 0);
                    topRight = cubePosition + new ByteVector4(0, 1, 0);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 0);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 0);

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontLeftTop_Cube, FrontLeft_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontRight_Cube, FrontRightTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontLeft_Cube, FrontLeftBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontRight_Cube, FrontRightBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo));

                    //Create Vertices
                    cubeIndices.Add((ushort)(0 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(3 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));

                    break;
                case CubeFace.Top:

                    //Get the 9 Facing cubes to the face
                    ByteColor Bottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y + 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor BottomLeft_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor BottomRight_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor BottomTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y + 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor BottomBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y + 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BottomLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor BottomRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor BottomLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor BottomRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z + 1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(0, 1, 0);
                    topRight = cubePosition + new ByteVector4(1, 1, 0);
                    bottomLeft = cubePosition + new ByteVector4(0, 1, 1);
                    bottomRight = cubePosition + new ByteVector4(1, 1, 1);

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomLeft_Cube, BottomLeftTop_Cube, BottomTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomRight_Cube, BottomBottom_Cube, BottomRightBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomBottom_Cube, BottomLeft_Cube, BottomLeftBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomTop_Cube, BottomRight_Cube, BottomRightTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo));

                    //Create Vertices
                    cubeIndices.Add((ushort)(0 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(0 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(3 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));
                    break;
                case CubeFace.Bottom:

                    //Get the 9 Facing cubes to the face
                    ByteColor Top_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y - 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor TopLeft_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor TopRight_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor TopTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y - 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor TopBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y - 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor TopLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor TopRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor TopLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor TopRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z + 1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(0, 0, 1);
                    topRight = cubePosition + new ByteVector4(1, 0, 1);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0);

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopLeft_Cube, TopLeftBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopLeft_Cube, TopLeftTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopRight_Cube, TopRightBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopRight_Cube, TopRightTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo));

                    //Create Vertices
                    cubeIndices.Add((ushort)(0 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(3 + verticeCubeOffset));
                    break;

                case CubeFace.Left:
                    //Get the 9 Facing cubes to the face
                    ByteColor Right_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor RightLeft_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor RightRight_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor RightTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor RightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor RightLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor RightRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor RightLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor RightRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z - 1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(0, 1, 0);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 1);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0);
                    topRight = cubePosition + new ByteVector4(0, 1, 1);

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightRight_Cube, RightRightTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightLeft_Cube, RightLeftTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightRight_Cube, RightRightBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightLeft_Cube, RightLeftBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo));

                    //Create Vertices
                    cubeIndices.Add((ushort)(0 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(3 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(3 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(0 + verticeCubeOffset));
                    break;
                case CubeFace.Right:
                    //Get the 9 Facing cubes to the face
                    ByteColor Left_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor LeftLeft_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor LefttRight_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor LeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor LeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z)].EmissiveColor;
                    ByteColor LeftLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor LeftRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y + 1, cubePosiInWorld.Z - 1)].EmissiveColor;
                    ByteColor LeftLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z + 1)].EmissiveColor;
                    ByteColor LeftRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y - 1, cubePosiInWorld.Z - 1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(1, 1, 1);
                    topRight = cubePosition + new ByteVector4(1, 1, 0);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 1);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0);

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LefttRight_Cube, LeftRightTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LeftLeft_Cube, LeftLeftTop_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LeftLeft_Cube, LeftLeftBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));

                    if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LefttRight_Cube, LeftRightBottom_Cube);
                    cubeVertices.Add(new VertexCubeSolid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo));

                    //Create Vertices
                    cubeIndices.Add((ushort)(0 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(3 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(1 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(0 + verticeCubeOffset));
                    cubeIndices.Add((ushort)(2 + verticeCubeOffset));
                    break;
            }

        }

        public static void GenLiquidCubeFace(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, ref List<VertexCubeLiquid> cubeVertices, ref List<ushort> cubeIndices, ref Dictionary<string, int> cubeVerticeDico)
        {
            int verticeCubeOffset = cubeVertices.Count;
            int indiceCubeOffset = cubeIndices.Count;
            ByteColor newColor = cube.EmissiveColor;

            RenderCubeProfile cubeProfile = RenderCubeProfile.CubesProfile[cube.Id];
            bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;

            //Les 4 vertex de ma face.... en fct de leur position dans le cube leur valeur en Z va changer ! (Face Top, Bottom, ...
            ByteVector4 topLeft;
            ByteVector4 topRight;
            ByteVector4 bottomLeft;
            ByteVector4 bottomRight;

            //Faut !
            //En fonction de ma face, il faut que j'aille cherher la lumière des cubes entourant celle-ci !
            Color cubeColor = new Color(cube.EmissiveColor.R, cube.EmissiveColor.G, cube.EmissiveColor.B, cube.EmissiveColor.SunLight);

            Vector4 vertexInfo2 = new Vector4(0, 0, 0, 0);
            ByteVector4 vertexInfo1 = new ByteVector4((byte)cubeFace,
                                                      (byte)cubeProfile.LiquidType,
                //cubeFace == CubeFace.Top || (cube.FloodingData == (byte)TerraFlooding.FloodDirection.Fall && cubeFace != CubeFace.Top && cubeFace != CubeFace.Bottom) ? (byte)cube.FloodingData : (byte)0,
                                                      cubeFace == CubeFace.Top ? (byte)cube.MetaData3 : (byte)TerraFlooding.FloodDirection.Fall,
                                                      (byte)0);
            string hashVertex;
            int generatedVertex = 0;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;
            int baseindex, baseIndexP1, baseIndexM1;

            switch (cubeFace)
            {
                case CubeFace.Front:

                    baseindex = _world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z + 1);
                    baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Plus1);
                    baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Minus1);

                    //Get the 9 Facing cubes to the face
                    ByteColor Back_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                    ByteColor BackLeft_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                    ByteColor BackRight_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                    ByteColor BackTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor BackBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                    ByteColor BackLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor BackRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor BackLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                    ByteColor BackRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(0, 1, 1);
                    topRight = cubePosition + new ByteVector4(1, 1, 1);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 1);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 1);

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackTop_Cube, BackLeftTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackTop_Cube, BackRightTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackBottom_Cube, BackLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackBottom_Cube, BackRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Front, ref  newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset2));

                    break;
                case CubeFace.Back:

                    baseindex = _world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y, cubePosiInWorld.Z - 1);
                    baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Plus1);
                    baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Minus1);

                    //Get the 9 Facing cubes to the face
                    ByteColor Front_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                    ByteColor FrontLeft_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                    ByteColor FrontRight_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                    ByteColor FrontTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor FrontBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                    ByteColor FrontLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor FrontRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor FrontLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                    ByteColor FrontRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(1, 1, 0);
                    topRight = cubePosition + new ByteVector4(0, 1, 0);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 0);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 0);

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontLeftTop_Cube, FrontLeft_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontRight_Cube, FrontRightTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontLeft_Cube, FrontLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontRight_Cube, FrontRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Back, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset3));

                    break;
                case CubeFace.Top:

                    baseindex = _world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y + 1, cubePosiInWorld.Z);
                    baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Plus1);
                    baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Minus1);

                    //Get the 9 Facing cubes to the face
                    ByteColor Bottom_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                    ByteColor BottomLeft_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                    ByteColor BottomRight_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                    ByteColor BottomTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                    ByteColor BottomBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;
                    ByteColor BottomLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                    ByteColor BottomRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                    ByteColor BottomLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;
                    ByteColor BottomRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(0, 1, 0);
                    topRight = cubePosition + new ByteVector4(1, 1, 0);
                    bottomLeft = cubePosition + new ByteVector4(0, 1, 1);
                    bottomRight = cubePosition + new ByteVector4(1, 1, 1);

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomLeft_Cube, BottomLeftTop_Cube, BottomTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomRight_Cube, BottomBottom_Cube, BottomRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomBottom_Cube, BottomLeft_Cube, BottomLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomTop_Cube, BottomRight_Cube, BottomRightTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Top, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset0));
                    break;

                case CubeFace.Bottom:

                    baseindex = _world.Landscape.Index(cubePosiInWorld.X, cubePosiInWorld.Y - 1, cubePosiInWorld.Z);
                    baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Plus1);
                    baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.X, IdxRelativeMove.X_Minus1);

                    //Get the 9 Facing cubes to the face
                    ByteColor Top_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                    ByteColor TopLeft_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                    ByteColor TopRight_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                    ByteColor TopTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                    ByteColor TopBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;
                    ByteColor TopLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                    ByteColor TopRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1)].EmissiveColor;
                    ByteColor TopLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;
                    ByteColor TopRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(0, 0, 1);
                    topRight = cubePosition + new ByteVector4(1, 0, 1);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0);

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopLeft_Cube, TopLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopLeft_Cube, TopLeftTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopRight_Cube, TopRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopRight_Cube, TopRightTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    break;

                case CubeFace.Left:

                    baseindex = _world.Landscape.Index(cubePosiInWorld.X - 1, cubePosiInWorld.Y, cubePosiInWorld.Z);
                    baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1);
                    baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1);

                    //Get the 9 Facing cubes to the face
                    ByteColor Right_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                    ByteColor RightLeft_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                    ByteColor RightRight_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                    ByteColor RightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor RightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                    ByteColor RightLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor RightRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor RightLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                    ByteColor RightRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(0, 1, 0);
                    bottomRight = cubePosition + new ByteVector4(0, 0, 1);
                    bottomLeft = cubePosition + new ByteVector4(0, 0, 0);
                    topRight = cubePosition + new ByteVector4(0, 1, 1);

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightRight_Cube, RightRightTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightLeft_Cube, RightLeftTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightRight_Cube, RightRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightLeft_Cube, RightLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Left, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset0));
                    break;
                case CubeFace.Right:

                    baseindex = _world.Landscape.Index(cubePosiInWorld.X + 1, cubePosiInWorld.Y, cubePosiInWorld.Z);
                    baseIndexP1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Plus1);
                    baseIndexM1 = _world.Landscape.FastIndex(baseindex, cubePosiInWorld.Z, IdxRelativeMove.Z_Minus1);

                    //Get the 9 Facing cubes to the face
                    ByteColor Left_Cube = _world.Landscape.Cubes[baseindex].EmissiveColor;
                    ByteColor LeftLeft_Cube = _world.Landscape.Cubes[baseIndexP1].EmissiveColor;
                    ByteColor LefttRight_Cube = _world.Landscape.Cubes[baseIndexM1].EmissiveColor;
                    ByteColor LeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor LeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseindex, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                    ByteColor LeftLeftTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor LeftRightTop_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Plus1)].EmissiveColor;
                    ByteColor LeftLeftBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexP1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;
                    ByteColor LeftRightBottom_Cube = _world.Landscape.Cubes[_world.Landscape.FastIndex(baseIndexM1, cubePosiInWorld.Y, IdxRelativeMove.Y_Minus1)].EmissiveColor;

                    topLeft = cubePosition + new ByteVector4(1, 1, 1);
                    topRight = cubePosition + new ByteVector4(1, 1, 0);
                    bottomLeft = cubePosition + new ByteVector4(1, 0, 1);
                    bottomRight = cubePosition + new ByteVector4(1, 0, 0);

                    hashVertex = cubeFace.GetHashCode().ToString() + topRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LefttRight_Cube, LeftRightTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + topLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LeftLeft_Cube, LeftLeftTop_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref topLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomLeft.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LeftLeft_Cube, LeftLeftBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomLeft, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    hashVertex = cubeFace.GetHashCode().ToString() + bottomRight.GetHashCode().ToString() + cube.Id.GetHashCode().ToString() + vertexInfo1.Z.ToString();
                    if (cubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        cubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LefttRight_Cube, LeftRightBottom_Cube);
                        cubeVertices.Add(new VertexCubeLiquid(ref bottomRight, RenderCubeProfile.CubesProfile[cube.Id].Tex_Right, ref newColor, ref vertexInfo2, ref vertexInfo1));
                        generatedVertex++;
                    }

                    //Create Vertices
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset3));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    cubeIndices.Add((ushort)(vertexOffset1));
                    cubeIndices.Add((ushort)(vertexOffset0));
                    cubeIndices.Add((ushort)(vertexOffset2));
                    break;
            }
        }

    }
}
