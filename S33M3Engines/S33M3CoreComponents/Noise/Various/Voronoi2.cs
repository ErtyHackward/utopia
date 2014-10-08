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
            public double DistanceSquared;
            public Vector2I CellId;
        }

        public enum VoronoiMode
        {
            RandomValue,
            FrontierDetection
        }

        public double Frequency { get; set; }
        public int Seed { get; set; }
        public double Noisiness { get; set; }
        public INoise NoiseBorder { get; set; }
        public VoronoiMode Mode { get; set; }

        public Voronoi2(int seed,  double frequency = 1.0, double Noisiness = 0.6, INoise NoiseBorder = null, VoronoiMode mode = VoronoiMode.RandomValue) 
        {
            this.Mode = mode;
            this.Seed = seed;
            this.Frequency = frequency;
            this.Noisiness = Noisiness;
            this.NoiseBorder = NoiseBorder;
        }

        public VoronoiCell[] GetCells(double x, double y, int nbrCells = 4)
        {
            VoronoiCell[] cells = new VoronoiCell[nbrCells];
            //Init result
            for (int i = 0; i < nbrCells; i++)
            {
                cells[i] = new VoronoiCell() { DistanceSquared = double.MaxValue };
            }

            x *= Frequency;
            y *= Frequency;

            if (NoiseBorder != null)
            {
                x += NoiseBorder.Get(x, 0);
                y += NoiseBorder.Get(0, y);
            }

            int xInt = (x > 0.0 ? (int)x : (int)x - 1);
            int yInt = (y > 0.0 ? (int)y : (int)y - 1);



            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.
            int highestFreeCellsIndex = 0;

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

                    for (int c = 0; c < nbrCells; c++)
                    {
                        if (dist < cells[c].DistanceSquared)
                        {
                            if(c < highestFreeCellsIndex)
                            {
                                //Shift array to right if needed
                                Array.Copy(cells, c, cells, c + 1, cells.Length - c - 1);
                            }

                            //Add new item at correct place (keeping the cells ordered in the list)
                            cells[c].DistanceSquared = dist;
                            cells[c].CellId = new Vector2I(xCur, yCur);
                            highestFreeCellsIndex++;
                            break;
                        }
                    }
                }
            }
            
            // Return the calculated distance with the displacement value applied.
            return cells;
        }

        public double Get(double x)
        {
            throw new NotImplementedException();
        }

        public double Get(double x, double y)
        {
            if (Mode == VoronoiMode.FrontierDetection)
            {
                var result = GetCells(x, y, 2);
                //Return the distance between the two nearest points
                return result[1].DistanceSquared - result[0].DistanceSquared;
            }
            else
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
            //return 1.0 - ((double)IntValueNoise(x, y, seed) / 1073741824.0); // -1 to 1 range
            return ((double)IntValueNoise(x, y, seed) / 1073741824.0) / 2.0; //0 to 1 range
        }     
    }
}
