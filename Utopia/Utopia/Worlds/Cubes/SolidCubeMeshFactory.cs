using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Chunks;
using Utopia.Shared.Enums;
using Utopia.Shared.Settings;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using Utopia.Resources.VertexFormats;
using Utopia.Shared.Configuration;

namespace Utopia.Worlds.Cubes
{
    public class SolidCubeMeshFactory : ICubeMeshFactory
    {
        private SingleArrayChunkContainer _cubesHolder;

        public SolidCubeMeshFactory(SingleArrayChunkContainer cubesHolder)
        {
            _cubesHolder = cubesHolder;
        }

        //Default Face Generation Checks !
        public bool FaceGenerationCheck(ref TerraCube cube, ref Vector3I cubePosiInWorld, CubeFaces cubeFace, ref TerraCube NeightBorFaceCube, int seaLevel)
        {
            //By default I don't need to trace the cubeFace of my cube if the face NeightBor cube is blocking light ! (Not see-through)
            CubeProfile cubeProfile = RealmConfiguration.CubeProfiles[cube.Id];
            CubeProfile NeighborCubeProfile = RealmConfiguration.CubeProfiles[NeightBorFaceCube.Id];
            if ((NeighborCubeProfile.IsSeeThrough) || 
                ((cubeProfile.SideOffsetMultiplier > 0 ^ NeighborCubeProfile.SideOffsetMultiplier > 0) && cube.Id != NeightBorFaceCube.Id)) return true;
            //Else draw the face
            return false;
        }

        public void GenCubeFace(ref TerraCube cube, CubeFaces cubeFace, ref Vector4B cubePosition, ref Vector3I cubePosiInWorld, VisualChunk chunk, ref TerraCube topCube)
        {
            byte yBlockOffset = 0;
            int verticeCubeOffset = chunk.SolidCubeVertices.Count;
            int indiceCubeOffset = chunk.SolidCubeIndices.Count;
            ByteColor newColor = cube.EmissiveColor;

            CubeProfile cubeProfile = RealmConfiguration.CubeProfiles[cube.Id];
            bool IsEmissiveColor = cubeProfile.IsEmissiveColorLightSource;
            //Les 4 vertex de ma face.... en fct de leur position dans le cube leur valeur en Z va changer ! (Face Top, Bottom, ...
            Vector4B topLeft;
            Vector4B topRight;
            Vector4B bottomLeft;
            Vector4B bottomRight;

            int cubeFaceType = (int)cubeFace;

            //x = Is Upper VErtex or not
            //y = Cube Face
            //z = Not used
            //w = Cube "Offset"

            //GetBlock Offset
            //This "natural" offset should only be made when the upper block is not the same as the block itself
            if (topCube.Id != cube.Id)
            {
                yBlockOffset = (byte)(cubeProfile.YBlockOffset * 255);
            }

            Vector4B vertexInfo = new Vector4B((byte)0, (byte)cubeFace, (byte)(0), yBlockOffset);

            long hashVertex;
            int generatedVertex = 0;
            int vertexOffset0, vertexOffset1, vertexOffset2, vertexOffset3;

            //Get the index of the current cube.
            int baseIndex = _cubesHolder.Index(ref cubePosiInWorld);

            int[] ind = new int[9];

            ChunkColumnInfo chunkInfo =  chunk.BlockData.GetColumnInfo(new Vector2I(cubePosition.X, cubePosition.Z));

            Vector4B biomeInfo = new Vector4B(chunkInfo.Moisture, chunkInfo.Temperature, cubeProfile.BiomeColorArrayTexture, cubeProfile.SideOffsetMultiplier);

            switch (cubeFace)
            {
                case CubeFaces.Front:

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

                    topLeft = cubePosition + new Vector4B(0, 1, 1, cubeFaceType);
                    topRight = cubePosition + new Vector4B(1, 1, 1, cubeFaceType);
                    bottomLeft = cubePosition + new Vector4B(0, 0, 1, cubeFaceType);
                    bottomRight = cubePosition + new Vector4B(1, 0, 1, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackTop_Cube, BackLeftTop_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Front, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackTop_Cube, BackRightTop_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Front, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackLeft_Cube, BackBottom_Cube, BackLeftBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Front, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Back_Cube, BackRight_Cube, BackBottom_Cube, BackRightBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Front, ref  newColor, ref vertexInfo, ref biomeInfo));
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
                case CubeFaces.Back:

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

                    topLeft = cubePosition + new Vector4B(1, 1, 0, cubeFaceType);
                    topRight = cubePosition + new Vector4B(0, 1, 0, cubeFaceType);
                    bottomLeft = cubePosition + new Vector4B(1, 0, 0, cubeFaceType);
                    bottomRight = cubePosition + new Vector4B(0, 0, 0, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontLeftTop_Cube, FrontLeft_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Back, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontTop_Cube, FrontRight_Cube, FrontRightTop_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Back, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontLeft_Cube, FrontLeftBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Back, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Front_Cube, FrontBottom_Cube, FrontRight_Cube, FrontRightBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Back, ref newColor, ref vertexInfo, ref biomeInfo));
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
                case CubeFaces.Top:

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

                    topLeft = cubePosition + new Vector4B(0, 1, 0, cubeFaceType);
                    topRight = cubePosition + new Vector4B(1, 1, 0, cubeFaceType);
                    bottomLeft = cubePosition + new Vector4B(0, 1, 1, cubeFaceType);
                    bottomRight = cubePosition + new Vector4B(1, 1, 1, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomLeft_Cube, BottomLeftTop_Cube, BottomTop_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Top, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomRight_Cube, BottomBottom_Cube, BottomRightBottom_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Top, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomBottom_Cube, BottomLeft_Cube, BottomLeftBottom_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Top, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Bottom_Cube, BottomTop_Cube, BottomRight_Cube, BottomRightTop_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Top, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));

                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));

                    break;

                case CubeFaces.Bottom:

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

                    topLeft = cubePosition + new Vector4B(0, 0, 1, cubeFaceType);
                    topRight = cubePosition + new Vector4B(1, 0, 1, cubeFaceType);
                    bottomLeft = cubePosition + new Vector4B(0, 0, 0, cubeFaceType);
                    bottomRight = cubePosition + new Vector4B(1, 0, 0, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopLeft_Cube, TopLeftBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopLeft_Cube, TopLeftTop_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopBottom_Cube, TopRight_Cube, TopRightBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Top_Cube, TopTop_Cube, TopRight_Cube, TopRightTop_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Bottom, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));

                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));

                    break;

                case CubeFaces.Left:

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

                    topLeft = cubePosition + new Vector4B(0, 1, 0, cubeFaceType);
                    bottomRight = cubePosition + new Vector4B(0, 0, 1, cubeFaceType);
                    bottomLeft = cubePosition + new Vector4B(0, 0, 0, cubeFaceType);
                    topRight = cubePosition + new Vector4B(0, 1, 1, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightRight_Cube, RightRightTop_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Left, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightTop_Cube, RightLeft_Cube, RightLeftTop_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Left, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightRight_Cube, RightRightBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Left, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Right_Cube, RightBottom_Cube, RightLeft_Cube, RightLeftBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Left, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));

                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));
                    break;
                case CubeFaces.Right:

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

                    topLeft = cubePosition + new Vector4B(1, 1, 1, cubeFaceType);
                    topRight = cubePosition + new Vector4B(1, 1, 0, cubeFaceType);
                    bottomLeft = cubePosition + new Vector4B(1, 0, 1, cubeFaceType);
                    bottomRight = cubePosition + new Vector4B(1, 0, 0, cubeFaceType);

                    hashVertex = (long)cubeFace + ((long)topRight.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset0) == false)
                    {
                        vertexOffset0 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset0);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LefttRight_Cube, LeftRightTop_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Right, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)topLeft.GetHashCode() << 8) + ((long)cube.Id << 40) + ((long)yBlockOffset << 48);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset1) == false)
                    {
                        vertexOffset1 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset1);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftTop_Cube, LeftLeft_Cube, LeftLeftTop_Cube);
                        vertexInfo.X = 1;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref topLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Right, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomLeft.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset2) == false)
                    {
                        vertexOffset2 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset2);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LeftLeft_Cube, LeftLeftBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomLeft, RealmConfiguration.CubeProfiles[cube.Id].Tex_Right, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    hashVertex = (long)cubeFace + ((long)bottomRight.GetHashCode() << 8) + ((long)cube.Id << 40);
                    if (chunk.CubeVerticeDico.TryGetValue(hashVertex, out vertexOffset3) == false)
                    {
                        vertexOffset3 = generatedVertex + verticeCubeOffset;
                        chunk.CubeVerticeDico.Add(hashVertex, vertexOffset3);
                        if (!IsEmissiveColor) newColor = ByteColor.Average(Left_Cube, LeftBottom_Cube, LefttRight_Cube, LeftRightBottom_Cube);
                        vertexInfo.X = 0;
                        chunk.SolidCubeVertices.Add(new VertexCubeSolid(ref bottomRight, RealmConfiguration.CubeProfiles[cube.Id].Tex_Right, ref newColor, ref vertexInfo, ref biomeInfo));
                        generatedVertex++;
                    }

                    //Create Vertices
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset3));

                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset1));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset2));
                    chunk.SolidCubeIndices.Add((ushort)(vertexOffset0));

                    break;
            }
        }

    }
}
