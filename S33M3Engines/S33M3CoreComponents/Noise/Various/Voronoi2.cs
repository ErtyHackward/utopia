using S33M3CoreComponents.Noise.Fractal;
using S33M3CoreComponents.Noise.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;

namespace S33M3CoreComponents.Noise.Various
{
    public class Voronoi2 : ValueNoiseBasis, INoise
    {
        public struct VoronoiCell
        {
            public double Distance;
            public Vector2I CellId;
        }

        public double Frequency { get; set; }
        public int Seed { get; set; }
        public double Noisiness { get; set; }
        public INoise NoiseBorder { get; set; }

        public Voronoi2(int seed,  double frequency = 1.0, double Noisiness = 0.6, INoise NoiseBorder = null) 
        {
            this.Seed = seed;
            this.Frequency = frequency;
            this.Noisiness = Noisiness;
            this.NoiseBorder = NoiseBorder;
        }

        public VoronoiCell GetCell(double x, double y)
        {
            VoronoiCell cell;

            x *= Frequency;
            y *= Frequency;

            if (NoiseBorder != null)
            {
                x += NoiseBorder.Get(x, 0);
                y += NoiseBorder.Get(0, y);
            }

            int xInt = (x > 0.0 ? (int)x : (int)x - 1);
            int yInt = (y > 0.0 ? (int)y : (int)y - 1);

            double minDist = double.MaxValue;
            int xCell = 0;
            int yCell = 0;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.

            for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++)
            {
                for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++)
                {
                    // Calculate the position and distance to the seed point inside of
                    // this unit cube.
                    double xPos = xCur + (ValueNoise(xCur, yCur, Seed) * Noisiness);
                    double yPos = yCur + (ValueNoise(xCur, yCur, Seed + 1) * Noisiness);
                    double xDist = xPos - x;
                    double yDist = yPos - y;
                    double dist = xDist * xDist + yDist * yDist;

                    if (dist < minDist)
                    {
                        // This seed point is closer to any others found so far, so record
                        minDist = dist;
                        xCell = xCur;
                        yCell = yCur;
                    }
                }
            }

            // Determine the distance to the nearest seed point.
            cell.Distance = (System.Math.Sqrt(minDist));

            cell.CellId = new Vector2I(xCell, yCell);

            // Return the calculated distance with the displacement value applied.
            return cell;
        }

        public double Get(double x)
        {
            throw new NotImplementedException();
        }

        public double Get(double x, double y)
        {
            x *= Frequency;
            y *= Frequency;

            if (NoiseBorder != null)
            {
                x += NoiseBorder.Get(x, 0);
                y += NoiseBorder.Get(0, y);
            }

            int xInt = (x > 0.0 ? (int)x : (int)x - 1);
            int yInt = (y > 0.0 ? (int)y : (int)y - 1);

            double minDist = double.MaxValue;
            int xCell = 0;
            int yCell = 0;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.

            for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++)
            {
                for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++)
                {
                    // Calculate the position and distance to the seed point inside of
                    // this unit cube.
                    double xPos = xCur + (ValueNoise(xCur, yCur, Seed + 1) * Noisiness);
                    double yPos = yCur + (ValueNoise(xCur, yCur, Seed + 2) * Noisiness);
                    double xDist = xPos - x;
                    double yDist = yPos - y;
                    double dist = xDist * xDist + yDist * yDist;

                    if (dist < minDist)
                    {
                        // This seed point is closer to any others found so far, so record
                        minDist = dist;
                        xCell = xCur;
                        yCell = yCur;
                    }
                }
            }

            // Return the calculated distance with the displacement value applied.
            return ValueNoise(xCell, yCell, Seed);
        }

        public double Get(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public double Get(double x, double y, double z, double w)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueNoiseBasis        
    {
        private const int XNoiseGen = 1619;
        private const int YNoiseGen = 31337;
        private const int SeedNoiseGen = 1013;
        private const int ShiftNoiseGen = 8;

        private int IntValueNoise(int x, int y, int seed)
        {
            // All constants are primes and must remain prime in order for this noise
            // function to work correctly.
            int n = (
                XNoiseGen * x
              + YNoiseGen * y
              + SeedNoiseGen * seed)
              & 0x7fffffff;
            n = (n >> 13) ^ n;
            return (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
        }

        public double ValueNoise(int x, int y)
        {
            return ValueNoise(x, y, 0);
        }

        public double ValueNoise(int x, int y, int seed)
        {
            return 1.0 - ((double)IntValueNoise(x, y, seed) / 1073741824.0);
        }     
    }
}
