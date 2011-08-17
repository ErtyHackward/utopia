using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Planets.Terran.World;
using S33M3Engines.Struct;
using S33M3Engines.Maths;
using Utopia.Shared.Structs;
using Utopia.Shared.Landscaping;
using S33M3Engines.Shared.Math;

namespace Utopia.Planets.Terran.Chunk
{
    public static class ChunkFinder
    {
        static LandScape _landscape;
        static Location3<int> _landscapeBufferSize;
        static TerraChunk[] _chunks;

        public static void Init(LandScape landscape, TerraChunk[] Chunks, Location3<int> landscapeBufferSize)
        {
            _landscape = landscape;
            _landscapeBufferSize = landscapeBufferSize;
            _chunks = Chunks;
        }

        public static TerraChunk GetChunk(int X, int Z)
        {
            //From World Coord to Cube Array Coord
            int arrayX = MathHelper.Mod(X, _landscapeBufferSize.X);
            int arrayZ = MathHelper.Mod(Z, _landscapeBufferSize.Z);

            //From Cube Array coord to Chunk Array coord
            int chunkX = arrayX >> LandscapeBuilder.ChunkPOWsize;
            int chunkZ = arrayZ >> LandscapeBuilder.ChunkPOWsize;

            //Console.WriteLine("x=" + X + " ; (Mod" + _landscapeBufferSize.X + ")=" + arrayX + " ; /16 (int)=" + chunkX);
            //Console.WriteLine("z=" + Z + " ; (Mod" + _landscapeBufferSize.Z + ")=" + arrayZ + " ; /16 (int)=" + chunkZ);
            //Console.WriteLine(chunkX + chunkZ * TerraWorld.ChunkGridSize);

            return _chunks[chunkX + chunkZ * LandscapeBuilder.ChunkGridSize];
        }

        public static bool isBlockBorder(int X, int Z, out bool XMaxCorner, out bool XMinCorner, out bool ZMaxCorner, out bool ZMinCorner)
        {
            //From World Coord to Cube Array Coord
            int arrayX = MathHelper.Mod(X, _landscapeBufferSize.X);
            int arrayZ = MathHelper.Mod(Z, _landscapeBufferSize.Z);

            //From Cube Array coord to Chunk Array coord
            int chunkX = arrayX >> LandscapeBuilder.ChunkPOWsize;
            int chunkZ = arrayZ >> LandscapeBuilder.ChunkPOWsize;

            XMaxCorner = (arrayX + 1) / 16f == (int)((arrayX + 1) / 16f);
            XMinCorner = (arrayX) / 16f == (int)((arrayX) / 16f);
            ZMaxCorner = (arrayZ + 1) / 16f == (int)((arrayZ + 1) / 16f);
            ZMinCorner = (arrayZ) / 16f == (int)((arrayZ) / 16f);

            return XMaxCorner || XMinCorner || ZMaxCorner || ZMinCorner;
        }

        public static bool isBorderChunk(int X, int Z, ref Range<int> worldRange)
        {
            if(X == worldRange.Min.X ||
               Z == worldRange.Min.Z ||
               X == worldRange.Max.X - LandscapeBuilder.Chunksize ||
               Z == worldRange.Max.Z - LandscapeBuilder.Chunksize)
            {
                return true;
            }
            return false;
        }

        public static IEnumerable<TerraChunk> GetChunksWithFixedX(int FixedX, int WorldMinZ)
        {
            int Z;
            Z = WorldMinZ;
            for (int chunkInd = 0; chunkInd < LandscapeBuilder.ChunkGridSize; chunkInd++)
            {
                yield return GetChunk(FixedX, Z);
                Z += LandscapeBuilder.Chunksize;
            }
        }

        public static IEnumerable<TerraChunk> GetChunksWithFixedZ(int FixedZ, int WorldMinX)
        {
            int X;
            X = WorldMinX;
            for (int chunkInd = 0; chunkInd < LandscapeBuilder.ChunkGridSize; chunkInd++)
            {
                yield return GetChunk(X, FixedZ);
                X += LandscapeBuilder.Chunksize;
            }
        }


        public static bool GetSafeChunk(float X, float Z, ref Range<int> RangeWorld, out TerraChunk chunk)
        {
            return GetSafeChunk((int)X, (int)Z, ref RangeWorld, out chunk);
        }

        public static bool GetSafeChunk(int X, int Z, ref Range<int> RangeWorld, out TerraChunk chunk)
        {
            if (X < RangeWorld.Min.X || X > RangeWorld.Max.X || Z < RangeWorld.Min.Z || Z > RangeWorld.Max.Z)
            {
                chunk = null;
                return false;
            }

            int arrayX = MathHelper.Mod(X, _landscapeBufferSize.X);
            int arrayZ = MathHelper.Mod(Z, _landscapeBufferSize.Z);

            int chunkX = arrayX >> LandscapeBuilder.ChunkPOWsize;
            int chunkZ = arrayZ >> LandscapeBuilder.ChunkPOWsize;

            chunk = _chunks[chunkX + chunkZ * LandscapeBuilder.ChunkGridSize];
            return true;
        }
    }
}
