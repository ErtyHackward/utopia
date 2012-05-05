using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Noise.Generator
{
    public class Perlin : INoise, ISeedable
    {
        public enum Interpolation
        {
            None,
            Linear,
            Hermite,
            Quintic
        }

        #region Private Variables
        /// <summary>
        /// Initial permutation table
        /// </summary>
        private static int[] _source = {
		                                151, 160, 137,  91,  90,  15, 131,  13, 201,  95,  96,  53, 194, 233,   7, 225, 140,  36, 103,  30,  69, 142, 
	                                      8,  99,  37, 240,  21,  10,  23, 190,   6, 148, 247, 120, 234,  75,   0,  26, 197,  62,  94, 252, 219, 203, 
	                                    117,  35,  11,  32,  57, 177,  33,  88, 237, 149,  56,  87, 174,  20, 125, 136, 171, 168,  68, 175,  74, 165, 
	                                     71, 134, 139,  48,  27, 166,  77, 146, 158, 231,  83, 111, 229, 122,  60, 211, 133, 230, 220, 105,  92,  41,
	                                     55,  46, 245,  40, 244, 102, 143,  54,  65,  25,  63, 161,   1, 216,  80,  73, 209,  76, 132, 187, 208,  89, 
	                                     18, 169, 200, 196, 135, 130, 116, 188, 159,  86, 164, 100, 109, 198, 173, 186,   3,  64,  52, 217, 226, 250, 
	                                    124, 123,   5, 202,  38, 147, 118, 126, 255,  82,  85, 212, 207, 206,  59, 227,  47,  16,  58,  17, 182, 189, 
	                                     28,  42, 223, 183, 170, 213, 119, 248, 152,   2,  44, 154, 163,  70, 221, 153, 101, 155, 167,  43, 172,   9, 
	                                    129,  22,  39, 253,  19,  98, 108, 110,  79, 113, 224, 232, 178, 185, 112, 104, 218, 246,  97, 228, 251,  34, 
	                                    242, 193, 238, 210, 144,  12, 191, 179, 162, 241,  81,  51, 145, 235, 249,  14, 239, 107,  49, 192, 214,  31, 
	                                    181, 199, 106, 157, 184,  84, 204, 176, 115, 121,  50,  45, 127,   4, 150, 254, 138, 236, 205,  93, 222, 114, 
	                                     67,  29,  24,  72, 243, 141, 128, 195,  78,  66, 215,  61, 156, 180
                                       };
        private int[] _random;

        private int _seed;
        #endregion

        #region Public Properties
        public int Seed
        {
            get { return _seed; }
            set { _seed = value; }
        }
        #endregion

        private delegate double InterpolationFct(double val);
        private InterpolationFct interpolationFct;

        public Perlin(int seed, Interpolation interpType = Interpolation.Quintic)
        {
            switch (interpType)
	        {
		        case Interpolation.None:
                    interpolationFct = noInterp;
                    break;
                case Interpolation.Linear:
                    interpolationFct = linearInterp;
                    break;
                case Interpolation.Hermite:
                    interpolationFct = hermiteInterp;
                    break;
                case Interpolation.Quintic:
                    interpolationFct = quinticInterp;
                    break;
                default:
                    break;
	        }
            _seed = seed;

            Randomize(_seed);
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            // Fast floor
            int xf = MathHelper.Fastfloor(x);
            int yf = MathHelper.Fastfloor(y);

            // Compute the cell coordinates
            int X = xf & 255;
            int Y = yf & 255;

            // Retrieve the decimal part of the cell
            x -= xf;
            y -= yf;

            // Smooth the curve
            double u,v;

            u = interpolationFct(x);
            v = interpolationFct(y);

            // Fetch some randoms values from the table
            int A = _random[X] + Y;
            int B = _random[X + 1] + Y;

            // Interpolate between directions 
            return MathHelper.Lerp(
                        MathHelper.Lerp(
                            Grad(_random[A], x, y),
                            Grad(_random[B], x - 1, y),
                            u
                        ),
                        MathHelper.Lerp(
                            Grad(_random[A + 1], x, y - 1),
                            Grad(_random[B + 1], x - 1, y - 1),
                            u
                        ),
                        v
                    );
        }

        public double Get(double x, double y, double z)
        {
            // Fast floor
            int xf = MathHelper.Fastfloor(x);
            int yf = MathHelper.Fastfloor(y);
            int zf = MathHelper.Fastfloor(z);

            // Compute the cell coordinates
            // Find unit cube that contains the point
            int X = xf & 255;
            int Y = yf & 255;
            int Z = zf & 255;

            // Retrieve the decimal part of the cell = relative X,Y,Z of point in cube
            x -= xf;
            y -= yf;
            z -= zf;

            // Smooth the curve
            double u, v, w;

            u = interpolationFct(x);
            v = interpolationFct(y);
            w = interpolationFct(z);

            // Hash coordinates of the 8 cubes corners
            // Fetch some randoms values from the table
            int A = _random[X] + Y;
            int AA = _random[A] + Z;
            int AB = _random[A + 1] + Z;
            int B = _random[X + 1] + Y;
            int BA = _random[B] + Z;
            int BB = _random[B + 1] + Z;

            // Interpolate between directions
            return
                MathHelper.Lerp(
                        MathHelper.Lerp(
                            MathHelper.Lerp(
                                Grad(_random[AA], x, y, z),
                                Grad(_random[BA], x - 1, y, z),
                                u
                            ),
                            MathHelper.Lerp(
                                Grad(_random[AB], x, y - 1, z),
                                Grad(_random[BB], x - 1, y - 1, z),
                                u
                            ),
                            v
                        ),
                        MathHelper.Lerp(
                            MathHelper.Lerp(
                                Grad(_random[AA + 1], x, y, z - 1),
                                Grad(_random[BA + 1], x - 1, y, z - 1),
                                u
                            ),
                            MathHelper.Lerp(
                                Grad(_random[AB + 1], x, y - 1, z - 1),
                                Grad(_random[BB + 1], x - 1, y - 1, z - 1),
                                u
                            ),
                            v
                        ),
                        w
                );
        }

        public double Get(double x, double y, double z, double w)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        //Interpolation methods
        private double noInterp(double t)
        {
            return 0;
        }

        private double linearInterp(double t)
        {
            return t;
        }

        private double hermiteInterp(double t)
        {
            return (t*t*(3-2*t));
        }

        private double quinticInterp(double t)
        {
            return t*t*t*(t*(t*6-15)+10);

        }

        /// <summary>
        /// Modifies the result by adding a directional bias
        /// </summary>
        /// <param name="hash">The random value telling in which direction the bias will occur</param>
        /// <param name="x">The amount of the bias on the X axis</param>
        /// <param name="y">The amount of the bias on the Y axis</param>
        /// <returns>The directional bias strength</returns>
        protected double Grad(int hash, double x, double y)
        {
            // Fetch the last 3 bits
            int h = hash & 3;
            double u = (h & 2) == 0 ? x : -x;
            double v = (h & 1) == 0 ? y : -y;

            return u + v;
        }

        /// <summary>
        /// Modifies the result by adding a directional bias
        /// </summary>
        /// <param name="hash">The random value telling in which direction the bias will occur</param>
        /// <param name="x">The amount of the bias on the X axis</param>
        /// <param name="y">The amount of the bias on the Y axis</param>
        /// <param name="z">The amount of the bias on the Z axis</param>
        /// <returns>The directional bias strength</returns>
        protected double Grad(int hash, double x, double y, double z)
        {
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        /// <summary>
        /// Initializes the random values
        /// 
        /// </summary>
        /// <param name="seed">The seed used to generate the random values</param>
        protected void Randomize(int seed)
        {
            _random = new int[256 * 2];
            if (seed != 0)
            {
                // Shuffle the array using the given seed
                // Unpack the seed into 4 bytes then perform a bitwise XOR operation
                // with each byte
                byte[] F = new byte[4];
                UnpackLittleUint32(seed, ref F);

                for (int i = 0; i < _source.Length; i++)
                {
                    _random[i] = _source[i] ^ F[0];
                    _random[i] ^= F[1];
                    _random[i] ^= F[2];
                    _random[i] ^= F[3];

                    _random[i + 256] = _random[i];
                }
            }
            else
            {
                for (int i = 0; i < 256; i++)
                {
                    _random[i + 256] = _random[i] = _source[i];
                }
            }
        }

        /// <summary>
        /// Unpack the given integer (int32) to an array of 4 bytes  in little endian format.
        /// If the length of the buffer is too smal, it wil be resized.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convert">the output buffer</param>
        private byte[] UnpackLittleUint32(int value, ref byte[] buffer)
        {
            if (buffer.Length < 4)
            {
                Array.Resize<byte>(ref buffer, 4);
            }

            buffer[0] = (byte)(value & 0x00ff);
            buffer[1] = (byte)((value & 0xff00) >> 8);
            buffer[2] = (byte)((value & 0x00ff0000) >> 16);
            buffer[3] = (byte)((value & 0xff000000) >> 24);

            return buffer;
        }
        #endregion

    }
}
