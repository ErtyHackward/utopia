using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.Various
{
    public class Voronoi2 : ValueNoiseBasis
    {
        private static readonly double Sqrt3 = 1.7320508075688772935;

        public double Frequency { get; set; }
        public double Displacement { get; set; }
        public bool DistanceEnabled { get; set; }
        public int Seed { get; set; }

        public Voronoi2() 
        {
            Frequency = 1.0;
            Displacement = 1.0;
            Seed = 0;
            DistanceEnabled = false;
        }

        public double GetValue(double x, double y, out double distance)
        {
            x *= Frequency;
            y *= Frequency;

            int xInt = (x > 0.0 ? (int)x : (int)x - 1);
            int yInt = (y > 0.0 ? (int)y : (int)y - 1);

            double minDist = 2147483647.0;
            double xCellCenter = 0;
            double yCellCenter = 0;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.

            for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++)
            {
                for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++)
                {
                    // Calculate the position and distance to the seed point inside of
                    // this unit cube.
                    double xPos = xCur + ValueNoise(xCur, yCur, Seed);
                    double yPos = yCur + ValueNoise(xCur, yCur, Seed + 1);
                    double xDist = xPos - x;
                    double yDist = yPos - y;
                    double dist = xDist * xDist + yDist * yDist;

                    if (dist < minDist)
                    {
                        // This seed point is closer to any others found so far, so record
                        // this seed point.
                        minDist = dist;
                        xCellCenter = xPos;
                        yCellCenter = yPos;
                    }
                }
            }

            if (DistanceEnabled)
            {
                // Determine the distance to the nearest seed point.
                double xDist = xCellCenter - x;
                double yDist = yCellCenter - y;
                distance = (System.Math.Sqrt(xDist * xDist + yDist * yDist)) * Sqrt3 - 1.0;
            }
            else
            {
                distance = 0.0;
            }

            int x0 = (xCellCenter > 0.0 ? (int)xCellCenter : (int)xCellCenter - 1);
            int y0 = (yCellCenter > 0.0 ? (int)yCellCenter : (int)yCellCenter - 1);

            // Return the calculated distance with the displacement value applied.
            return Displacement * (double)ValueNoise(x0, y0);
        }
    }

    public class ValueNoiseBasis        
    {
        private const int XNoiseGen = 1619;
        private const int YNoiseGen = 31337;
        private const int SeedNoiseGen = 1013;
        private const int ShiftNoiseGen = 8;

        public int IntValueNoise(int x, int y, int seed)
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
