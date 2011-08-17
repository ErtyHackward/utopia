using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.Shared.Math.Noises
{
    //From Paper at : http://webstaff.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf
    public class SimplexNoise : INoise2D, INoise3D
    {
        private double _offsetX, _offsetY, _offsetZ;
        private int[][] grad3 ={
                                          new int[] {1,1,0},
                                          new int[] {-1,1,0},
                                          new int[] {1,-1,0},
                                          new int[] {-1,-1,0},
                                          new int[] {1,0,1},
                                          new int[] {-1,0,1},
                                          new int[] {1,0,-1},
                                          new int[] {-1,0,-1},
                                          new int[] {0,1,1},
                                          new int[] {0,-1,1},
                                          new int[] {0,1,-1},
                                          new int[] {0,-1,-1}
                                       };

        //Permutation table, the values inside will be considered as the seed of the noise. 
        private int[] p = new int[256];

        // To remove the need for index wrapping, double the permutation table length
        private int[] perm = new int[512];

        public SimplexNoise(Random rnd)
        {
            _offsetX = rnd.NextDouble() * 256.0;
            _offsetY = rnd.NextDouble() * 256.0;
            _offsetZ = rnd.NextDouble() * 256.0;

            InitNoise(rnd);
        }

        private void InitNoise(Random rnd)
        {
            for (int i = 0; i < 256; i++) p[i] = rnd.Next(256);

            for (int i = 0; i < 512; i++) perm[i] = p[i & 255];
        }

        public enum InflectionMode
        {
            NoInflections,
            ABSFct,
            InvABSFct,
            InvFct
        }

        public enum ResultScale
        {
            ZeroToOne,
            MinOneToOne
        }

        // To remove the need for index wrapping, double the permutation table length
        //private static int[] perm = new int[512];
        //static { for(int i=0; i<512; i++) perm[i]=p[i & 255]; }
        //3D noise
        private double noise(double x, double y, double z, ResultScale scale)
        {
            double n0, n1, n2, n3; // Noise contributions from the four corner

            // Skew the input space to determine which simplex cell we're in
            const double F3 = 1.0 / 3.0;
            double s = (x + y + z) * F3; // Very nice and simple skew factor for 3D
            int i = MathHelper.Fastfloor(x + s);
            int j = MathHelper.Fastfloor(y + s);
            int k = MathHelper.Fastfloor(z + s);

            const double G3 = 1.0 / 6.0; // Very nice and simple unskew factor, too
            double t = (i + j + k) * G3;
            double X0 = i - t;  // Unskew the cell origin back to (x,y,z) spac
            double Y0 = j - t;
            double Z0 = k - t;
            double x0 = x - X0; // The x,y,z distances from the cell origin
            double y0 = y - Y0;
            double z0 = z - Z0;

            // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
            // Determine which simplex we are in.
            int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
            int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coord

            if (x0 >= y0)
            {
                if (y0 >= z0)
                {
                    i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; // X Y Z order
                }
                else if (x0 >= z0)
                {
                    i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1;  // X Z Y orde
                }
                else
                {
                    i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1;  // Z X Y orde
                }
            }
            else
            {
                if (y0 < z0)
                {
                    i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; // X Y Z order
                }
                else if (x0 < z0)
                {
                    i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; // X Z Y orde
                }
                else
                {
                    i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; // Z X Y orde
                }
            }

            double x1 = x0 - i1 + G3;// Offsets for second corner in (x,y,z) coords
            double y1 = y0 - j1 + G3;
            double z1 = z0 - k1 + G3;
            double x2 = x0 - i2 + 2.0 * G3; // Offsets for third corner in (x,y,z) coords
            double y2 = y0 - j2 + 2.0 * G3;
            double z2 = z0 - k2 + 2.0 * G3;
            double x3 = x0 - 1.0 + 3.0 * G3; // Offsets for last corner in (x,y,z) coords
            double y3 = y0 - 1.0 + 3.0 * G3;
            double z3 = z0 - 1.0 + 3.0 * G3;

            // Work out the hashed gradient indices of the four simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int gi0 = perm[ii + perm[jj + perm[kk]]] % 12;
            int gi1 = perm[ii + i1 + perm[jj + j1 + perm[kk + k1]]] % 12;
            int gi2 = perm[ii + i2 + perm[jj + j2 + perm[kk + k2]]] % 12;
            int gi3 = perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]] % 12;

            // Calculate the contribution from the four corners
            double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * MathHelper.Dot(grad3[gi0], ref x0, ref y0, ref z0);
            }

            double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * MathHelper.Dot(grad3[gi1], ref x1, ref y1, ref z1);
            }

            double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * MathHelper.Dot(grad3[gi2], ref x2, ref y2, ref z2);
            }

            double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0) n3 = 0.0f;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * MathHelper.Dot(grad3[gi3], ref x3, ref y3, ref z3);
            }

            if (scale == ResultScale.MinOneToOne)
            {
                return 32.0 * (n0 + n1 + n2 + n3);
            }
            else
            {
                return 16.0 * (n0 + n1 + n2 + n3) + 0.5;
            }
        }

        //2D noise
        private double noise(double x, double y, ResultScale scale)
        {
            double n0, n1, n2; // Noise contributions from the three corner

            // Skew the input space to determine which simplex cell we're in
            const double F2 = 0.3660254; //0.5*(Math.Sqrt(3.0)-1.0);
            double s = (x + y) * F2; // Hairy factor for 2D
            int i = MathHelper.Fastfloor(x + s);
            int j = MathHelper.Fastfloor(y + s);
            const double G2 = 0.2113248; //(3.0-Math.Sqrt(3.0))/6.0;
            double t = (i + j) * G2;
            double X0 = i - t; // Unskew the cell origin back to (x,y) space
            double Y0 = j - t;
            double x0 = x - X0; // The x,y distances from the cell origin
            double y0 = y - Y0;

            // For the 2D case, the simplex shape is an equilateral triangle.
            // Determine which simplex we are in.
            int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
            if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)

            // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
            // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
            // c = (3-sqrt(3))/6
            double x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
            double y1 = y0 - j1 + G2;
            double x2 = x0 - 1.0 + 2.0 * G2; // Offsets for last corner in (x,y) unskewed coords
            double y2 = y0 - 1.0 + 2.0 * G2;

            // Work out the hashed gradient indices of the three simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int gi0 = perm[ii + perm[jj]] % 12;
            int gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
            int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;

            // Calculate the contribution from the three corners
            double t0 = 0.5 - x0 * x0 - y0 * y0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * MathHelper.Dot(grad3[gi0], ref x0, ref y0);  // (x,y) of grad3 used for 2D gradient
            }

            double t1 = 0.5 - x1 * x1 - y1 * y1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * MathHelper.Dot(grad3[gi1], ref x1, ref y1);
            }

            double t2 = 0.5 - x2 * x2 - y2 * y2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * MathHelper.Dot(grad3[gi2], ref x2, ref y2);
            }

            // Add contributions from each corner to get the final noise value.
            if (scale == ResultScale.MinOneToOne)
            {
                return 70.0 * (n0 + n1 + n2);
            }

            if (scale == ResultScale.ZeroToOne)
            {
                return 35.0 * (n0 + n1 + n2) + 0.5;
            }
            return 0;
        }


        double _zoom;
        InflectionMode _inflection;
        ResultScale _scale;

        public void SetParameters(double Zoom, InflectionMode inflection, ResultScale scale)
        {
            _zoom = Zoom;
            _inflection = inflection;
            _scale = scale;
        }

        /*
        Persistence	:   In multi-scale noise, persistence refers to the amount of contribution smaller scales make to the overall texture. 
                        A higher persistence will result in a noisier, more complicated looking texture, where the fine details are strong and drastic. 
                        This is also sometimes referred to as the fractal dimension.
        */
        public NoiseResult GetNoise3DValue(double X, double Y, double Z, int octaves, double persistence)
        {
            X += _offsetX;
            Y += _offsetY;
            Z += _offsetZ;

            double value = 0.0f;
            double frequence;
            double amplitude;
            double MinValue, MaxValue;

            MinValue = 0;
            MaxValue = 0;
            //Get Basic landscape
            for (int i = 0; i < octaves; i++)
            {
                frequence = System.Math.Pow(2, i);           // Change the wave frequency, A big i will give a more wavy felling !
                amplitude = System.Math.Pow(persistence, i); //==> To change the transition power between octaves (<0.5 soft, >0.5 Hard)
                switch (_inflection)
                {
                    case InflectionMode.NoInflections:
                        value += noise(X * frequence * _zoom, Y * frequence * _zoom, Z * frequence * _zoom, _scale) * amplitude;
                        if (_scale == ResultScale.MinOneToOne)
                        {
                            MinValue += -1 * amplitude;
                            MaxValue += 1 * amplitude;
                        }
                        else
                        {
                            MinValue += 0 * amplitude;
                            MaxValue += 1 * amplitude;
                        }
                        break;
                    case InflectionMode.ABSFct:
                        value += System.Math.Abs(noise(X * frequence * _zoom, Y * frequence * _zoom, Z * frequence * _zoom, ResultScale.MinOneToOne)) * amplitude;

                        MinValue += 0 * amplitude;
                        MaxValue += 1 * amplitude;
                        break;
                    case InflectionMode.InvABSFct:

                        value += (1 - System.Math.Abs(noise(X * frequence * _zoom, Y * frequence * _zoom, Z * frequence * _zoom, ResultScale.MinOneToOne))) * amplitude;

                        MinValue += 0 * amplitude;
                        MaxValue += 1 * amplitude;
                        break;

                    case InflectionMode.InvFct:

                        value += (1 - noise(X * frequence * _zoom, Z * frequence * _zoom, ResultScale.ZeroToOne)) * amplitude;

                        MinValue += 0 * amplitude;
                        MaxValue += 1 * amplitude;
                        break;
                }
            }

            return new NoiseResult() { Value = value, MinValue = MinValue, MaxValue = MaxValue };
        }


        private double SimpleNoise3DValue(double X, double Y, double Z, int octaves, double persistence, ref NoiseResult Range)
        {
            X += _offsetX;
            Y += _offsetY;
            Z += _offsetZ;

            double value = 0.0f;
            double frequence;
            double amplitude;

            for (int i = 0; i < octaves; i++)
            {
                frequence = System.Math.Pow(2, i);           // Changer la fréquence des vagues ! plus i est grand plus les vagues sont serées !
                amplitude = System.Math.Pow(persistence, i); //==> Pour changer la douceur des changements par la valeur persistence (<0.5 doux, >0.5 abrupte)

                switch (_inflection)
                {
                    case InflectionMode.NoInflections:

                        value += noise(X * frequence * _zoom, Y * frequence * _zoom, Z * frequence * _zoom, _scale) * amplitude;

                        if (Range.isSet) break;

                        if (_scale == ResultScale.MinOneToOne)
                        {
                            Range.MinValue += -1 * amplitude;
                            Range.MaxValue += 1 * amplitude;
                        }
                        else
                        {
                            Range.MinValue += 0 * amplitude;
                            Range.MaxValue += 1 * amplitude;
                        }
                        break;
                    case InflectionMode.ABSFct:

                        value += System.Math.Abs(noise(X * frequence * _zoom, Y * frequence * _zoom, Z * frequence * _zoom, ResultScale.MinOneToOne)) * amplitude;

                        if (Range.isSet) break;

                        Range.MinValue += 0 * amplitude;
                        Range.MaxValue += 1 * amplitude;
                        break;
                    case InflectionMode.InvABSFct:

                        value += (1 - System.Math.Abs(noise(X * frequence * _zoom, Y * frequence * _zoom, Z * frequence * _zoom, ResultScale.MinOneToOne))) * amplitude;

                        if (Range.isSet) break;

                        Range.MinValue += 0 * amplitude;
                        Range.MaxValue += 1 * amplitude;
                        break;

                    case InflectionMode.InvFct:

                        value += (1 - noise(X * frequence * _zoom, Z * frequence * _zoom, ResultScale.ZeroToOne)) * amplitude;
                        if (Range.isSet) break;

                        Range.MinValue += 0 * amplitude;
                        Range.MaxValue += 1 * amplitude;
                        break;
                }


            }

            Range.isSet = true;

            return value;
        }


        public void GetNoise3DValueWithAccumulation(ref double[] result,
                                                    int FromX, int ToX, int SamplingStepsCountX,
                                                    int FromY, int ToY, int SamplingStepsCountY,
                                                    int FromZ, int ToZ, int SamplingStepsCountZ,
                                                    int octaves, double persistence, out NoiseResult Range)
        {
            SamplingStepsCountX++;
            SamplingStepsCountY++;
            SamplingStepsCountZ++;
            Range = new NoiseResult() { isSet = false };

            if (result == null) result = new double[(SamplingStepsCountX) * (SamplingStepsCountY) * (SamplingStepsCountZ)];

            int samplingStepX = (ToX - FromX) / (SamplingStepsCountX - 1);
            int samplingStepY = (ToY - FromY) / (SamplingStepsCountY - 1);
            int samplingStepZ = (ToZ - FromZ) / (SamplingStepsCountZ - 1);

            int generatedNoise = 0;

            for (int X = 0; X < SamplingStepsCountX; X++)
            {
                for (int Z = 0; Z < SamplingStepsCountZ; Z++)
                {
                    for (int Y = 0; Y < SamplingStepsCountY; Y++)
                    {
                        result[generatedNoise] = SimpleNoise3DValue((X * samplingStepX) + FromX, (Y * samplingStepY) + FromY, (Z * samplingStepZ) + FromZ, octaves, persistence, ref Range);
                        generatedNoise++;
                    }
                }
            }
        }



        /// <summary>
        /// Get a noise value
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="octaves">Number of noise fct that will be used</param>
        /// <param name="persistence">Between 0 and 1. The weight of each octave in the final result 1 = All same weight
        ///                           sample for 1/4 : Oct1 = 1; Oct2 = 1/4;Oct3 = 1/8;Oct4 = 1/16; ...,  </param>
        /// <param name="Zoom">The Base frenquency to be used</param>
        /// <param name="inflection">Kind Of Noise fonction to use</param>
        /// <param name="scale">Only used for NoInflection, will scale each octave result into this specified range</param>
        /// <returns></returns>
        public NoiseResult GetNoise2DValue(double X, double Z, int octaves, double persistence)
        {
            X += _offsetX;
            Z += _offsetZ;

            double value = 0.0f;
            double frequence;
            double amplitude;
            double MinValue, MaxValue;

            MinValue = 0;
            MaxValue = 0;
            //Get Basic landscape
            for (int i = 0; i < octaves; i++)
            {
                frequence = System.Math.Pow(2, i);           // Changer la fréquence des vagues ! plus i est grand plus les vagues sont serées !
                amplitude = System.Math.Pow(persistence, i); //==> Pour changer la douceur des changements par la valeur persistence (<0.5 doux, >0.5 abrupte)
                switch (_inflection)
                {
                    case InflectionMode.NoInflections:
                        value += noise(X * frequence * _zoom, Z * frequence * _zoom, _scale) * amplitude;
                        if (_scale == ResultScale.MinOneToOne)
                        {
                            MinValue += -1 * amplitude;
                            MaxValue += 1 * amplitude;
                        }
                        else
                        {
                            MinValue += 0 * amplitude;
                            MaxValue += 1 * amplitude;
                        }
                        break;
                    case InflectionMode.ABSFct:
                        value += System.Math.Abs(noise(X * frequence * _zoom, Z * frequence * _zoom, ResultScale.MinOneToOne)) * amplitude;

                        MinValue += 0 * amplitude;
                        MaxValue += 1 * amplitude;
                        break;
                    case InflectionMode.InvABSFct:

                        value += (1 - System.Math.Abs(noise(X * frequence * _zoom, Z * frequence * _zoom, ResultScale.MinOneToOne))) * amplitude;

                        MinValue += 0 * amplitude;
                        MaxValue += 1 * amplitude;
                        break;

                    case InflectionMode.InvFct:

                        value += (1 - noise(X * frequence * _zoom, Z * frequence * _zoom, ResultScale.ZeroToOne)) * amplitude;
                        MinValue += 0 * amplitude;
                        MaxValue += 1 * amplitude;
                        break;
                }
            }

            return new NoiseResult() { Value = value, MinValue = MinValue, MaxValue = MaxValue };
        }

        private double SimpleNoise2DValue(double X, double Z, int octaves, double persistence, ref NoiseResult Range)
        {
            X += _offsetX;
            Z += _offsetZ;

            double value = 0.0f;
            double frequence;
            double amplitude;

            for (int i = 0; i < octaves; i++)
            {
                frequence = System.Math.Pow(2, i);           // Changer la fréquence des vagues ! plus i est grand plus les vagues sont serées !
                amplitude = System.Math.Pow(persistence, i); //==> Pour changer la douceur des changements par la valeur persistence (<0.5 doux, >0.5 abrupte)

                switch (_inflection)
                {
                    case InflectionMode.NoInflections:
                        value += noise(X * frequence * _zoom, Z * frequence * _zoom, _scale) * amplitude;
                        if (Range.isSet) break;

                        if (_scale == ResultScale.MinOneToOne)
                        {
                            Range.MinValue += -1 * amplitude;
                            Range.MaxValue += 1 * amplitude;
                        }
                        else
                        {
                            Range.MinValue += 0 * amplitude;
                            Range.MaxValue += 1 * amplitude;
                        }

                        break;
                    case InflectionMode.ABSFct:

                        value += System.Math.Abs(noise(X * frequence * _zoom, Z * frequence * _zoom, ResultScale.MinOneToOne)) * amplitude;
                        if (Range.isSet) break;
                        Range.MinValue += 0 * amplitude;
                        Range.MaxValue += 1 * amplitude;
                        break;
                    case InflectionMode.InvABSFct:

                        value += (1 - System.Math.Abs(noise(X * frequence * _zoom, Z * frequence * _zoom, ResultScale.MinOneToOne))) * amplitude;
                        if (Range.isSet) break;
                        Range.MinValue += 0 * amplitude;
                        Range.MaxValue += 1 * amplitude;
                        break;

                    case InflectionMode.InvFct:

                        value += (1 - noise(X * frequence * _zoom, Z * frequence * _zoom, ResultScale.ZeroToOne)) * amplitude;
                        if (Range.isSet) break;
                        Range.MinValue += 0 * amplitude;
                        Range.MaxValue += 1 * amplitude;
                        break;
                }
            }

            Range.isSet = true;
            return value;
        }

        public void GetNoise2DValueWithAccumulation(ref double[] result,
                                                    int FromX, int ToX, int SamplingStepsCountX,
                                                    int FromZ, int ToZ, int SamplingStepsCountZ,
                                                    int octaves, double persistence, out NoiseResult Range)
        {
            SamplingStepsCountX++;
            SamplingStepsCountZ++;
            Range = new NoiseResult() { isSet = false };

            if (result == null) result = new double[(SamplingStepsCountX) * (SamplingStepsCountZ)];

            int samplingStepX = (ToX - FromX) / (SamplingStepsCountX - 1);
            int samplingStepZ = (ToZ - FromZ) / (SamplingStepsCountZ - 1);

            int generatedNoise = 0;

            for (int X = 0; X < SamplingStepsCountX; X++)
            {
                for (int Z = 0; Z < SamplingStepsCountZ; Z++)
                {
                    result[generatedNoise] = SimpleNoise2DValue((X * samplingStepX) + FromX, (Z * samplingStepZ) + FromZ, octaves, persistence, ref Range);
                    generatedNoise++;
                }
            }
        }




    }
}

