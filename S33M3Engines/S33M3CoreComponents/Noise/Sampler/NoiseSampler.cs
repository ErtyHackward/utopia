using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;

namespace S33M3CoreComponents.Noise.Sampler
{
    public static class NoiseSampler
    {

        /// <summary>
        /// Will present the result of the noise inside a single Array[]
        /// The 3dimension are present inside this array in this order : X, Z and then Y
        /// It means to compute the array indice, use this formula : X * Zlength * Ylength + Z * Ylength + Y
        /// </summary>
        /// <param name="noiseFct">The Noise function</param>
        /// <param name="NoiseSampledSteps">The qt of steps that will be evaluated into by the noise fct, if qt below StepsCountX, the remaining Steps will be interpolated !! SampledSteps values MUST be a multiple of StepsCountn</param>
        /// <param name="FromX">Low X sampling Range</param>
        /// <param name="ToX">High X sampling Range</param>
        /// <param name="StepsCountX">Nbr of step sampled at equal distance from FromX to ToX</param>
        /// <param name="FromY">Low Y sampling Range</param>
        /// <param name="ToY">High Y sampling Range</param>
        /// <param name="StepsCountY">Nbr of step sampled at equal distance from FromY to ToY</param>
        /// <param name="FromZ">Low Z sampling Range</param>
        /// <param name="ToZ">High Z sampling Range</param>
        /// <param name="StepsCountZ">Nbr of step sampled at equal distance from FromZ to ToZ</param>
        /// <returns>Signle dimension array with the noise result</returns>
        public static double[,] NoiseSampling(Vector3I NoiseSampledSteps,
                                            double FromX, double ToX, int StepsCountX,
                                            double FromY, double ToY, int StepsCountY,
                                            double FromZ, double ToZ, int StepsCountZ,
                                            params INoise3[] noiseFcts)
        {
            bool needInterpolation = false;

            if (NoiseSampledSteps.X != StepsCountX || NoiseSampledSteps.Y != StepsCountY || NoiseSampledSteps.Z != StepsCountZ)
            {
                if (StepsCountX % NoiseSampledSteps.X != 0 ||
                    StepsCountY % NoiseSampledSteps.Y != 0 ||
                    StepsCountZ % NoiseSampledSteps.Z != 0)
                {
                    throw new Exception("NoiseSampling using NoiseSampledSteps that are not a multiple of StepsCount");
                }
                //CHECK for NoiseSampledSteps validity !
                needInterpolation = true;
            }

            int sampledStepCountX = NoiseSampledSteps.X;
            int sampledStepCountY = NoiseSampledSteps.Y;
            int sampledStepCountZ = NoiseSampledSteps.Z;
            if (needInterpolation)
            {
                sampledStepCountX++;// +1 If in Lerping Mode
                sampledStepCountY++;// +1 If in Lerping Mode
                sampledStepCountZ++;// +1 If in Lerping Mode
            }

            int NbrNoisesToParse = noiseFcts.Length;
            double[,] result = new double[(sampledStepCountX) * (sampledStepCountY) * (sampledStepCountZ), NbrNoisesToParse];

            double samplingStepDeltaX = (ToX - FromX) / (sampledStepCountX - 1);
            double samplingStepDeltaY = (ToY - FromY) / (sampledStepCountY - 1);
            double samplingStepDeltaZ = (ToZ - FromZ) / (sampledStepCountZ - 1);

            int generatedNoise = 0;
            double valX, valY, valZ;

            //Sampled the Noise
            valX = FromX;
            for (int X = 0; X < sampledStepCountX; X++)
            {
                valZ = FromZ;
                for (int Z = 0; Z < sampledStepCountZ; Z++)
                {
                    valY = FromY;
                    for (int Y = 0; Y < sampledStepCountY; Y++)
                    {
                        for (int noiseId = 0; noiseId < NbrNoisesToParse; noiseId++)
                        {
                            result[generatedNoise, noiseId] = noiseFcts[noiseId].Get(valX, valY, valZ);
                        }

                        generatedNoise++;
                        valY += samplingStepDeltaY;
                    }
                    valZ += samplingStepDeltaZ;
                }
                valX += samplingStepDeltaX;
            }

            if (needInterpolation)
            {
                result = InterpolateResult(result, NoiseSampledSteps, new Vector3I(StepsCountX, StepsCountY, StepsCountZ));
            }

            return result;
        }


        private static double[,] InterpolateResult(double[,] dataNoises,
                                                  Vector3I NoiseSampledSteps,
                                                  Vector3I StepsCount)
        {
            int nbrNoises = dataNoises.GetLength(1);

            //Create a new array with expended size
            double[,] result = new double[StepsCount.X * StepsCount.Y * StepsCount.Z, nbrNoises];

            int dataNoisesXSize = NoiseSampledSteps.X + 1;
            int dataNoisesYSize = NoiseSampledSteps.Y + 1;
            int dataNoisesZSize = NoiseSampledSteps.Z + 1;

            int XPointLerpedCount = StepsCount.X / NoiseSampledSteps.X;
            int ZPointLerpedCount = StepsCount.Z / NoiseSampledSteps.Z;
            int YPointLerpedCount = StepsCount.Y / NoiseSampledSteps.Y;

            //Loop against the generated noise values
            int noiseGeneratedResultIndex = 0;

            for (int noiseId = 0; noiseId < nbrNoises; noiseId++)
            {

                for (int SampledpointX = 0; SampledpointX < NoiseSampledSteps.X; SampledpointX++)
                {
                    for (int SampledpointZ = 0; SampledpointZ < NoiseSampledSteps.Z; SampledpointZ++)
                    {
                        for (int SampledpointY = 0; SampledpointY < NoiseSampledSteps.Y; SampledpointY++)
                        {
                            double NoiseX0Z0Y0 = dataNoises[((SampledpointX + 0) * dataNoisesZSize + (SampledpointZ + 0)) * dataNoisesYSize + (SampledpointY + 0), noiseId];
                            double NoiseX0Z1Y0 = dataNoises[((SampledpointX + 0) * dataNoisesZSize + (SampledpointZ + 1)) * dataNoisesYSize + (SampledpointY + 0), noiseId];
                            double NoiseX1Z0Y0 = dataNoises[((SampledpointX + 1) * dataNoisesZSize + (SampledpointZ + 0)) * dataNoisesYSize + (SampledpointY + 0), noiseId];
                            double NoiseX1Z1Y0 = dataNoises[((SampledpointX + 1) * dataNoisesZSize + (SampledpointZ + 1)) * dataNoisesYSize + (SampledpointY + 0), noiseId];

                            double DeltaX0Z0 = (dataNoises[((SampledpointX + 0) * dataNoisesZSize + (SampledpointZ + 0)) * dataNoisesYSize + (SampledpointY + 1), noiseId] - NoiseX0Z0Y0) / YPointLerpedCount; // 128 / 16 = 8 points need to be lerped 4 times !
                            double DeltaX0Z1 = (dataNoises[((SampledpointX + 0) * dataNoisesZSize + (SampledpointZ + 1)) * dataNoisesYSize + (SampledpointY + 1), noiseId] - NoiseX0Z1Y0) / YPointLerpedCount; // 128 / 16 = 8 points need to be lerped 4 times !
                            double DeltaX1Z0 = (dataNoises[((SampledpointX + 1) * dataNoisesZSize + (SampledpointZ + 0)) * dataNoisesYSize + (SampledpointY + 1), noiseId] - NoiseX1Z0Y0) / YPointLerpedCount; // 128 / 16 = 8 points need to be lerped 4 times !
                            double DeltaX1Z1 = (dataNoises[((SampledpointX + 1) * dataNoisesZSize + (SampledpointZ + 1)) * dataNoisesYSize + (SampledpointY + 1), noiseId] - NoiseX1Z1Y0) / YPointLerpedCount; // 128 / 16 = 8 points need to be lerped 4 times !

                            for (int Y = 0; Y < YPointLerpedCount; Y++)
                            {
                                double NoiseZ0 = NoiseX0Z0Y0;
                                double NoiseZ1 = NoiseX1Z0Y0;
                                double DeltaZ0 = (NoiseX0Z1Y0 - NoiseX0Z0Y0) / ZPointLerpedCount; // Chunk X length = 16 / 4 = 4 points needs to be lerped Twice!
                                double DeltaZ1 = (NoiseX1Z1Y0 - NoiseX1Z0Y0) / ZPointLerpedCount;
                                int nY = (SampledpointY * YPointLerpedCount) + Y;

                                for (int Z = 0; Z < ZPointLerpedCount; Z++)
                                {
                                    double Noise3 = NoiseZ0;
                                    double DeltaX = (NoiseZ1 - NoiseZ0) / XPointLerpedCount; // Chunk Z length = 16 / 4 = 4 points needs to be lerped Once!
                                    int nZ = (SampledpointZ * ZPointLerpedCount) + Z;
                                    for (int X = 0; X < XPointLerpedCount; X++)
                                    {
                                        int nX = (SampledpointX * XPointLerpedCount) + X;
                                        result[nX * StepsCount.Z * StepsCount.Y + nZ * StepsCount.Y + nY, noiseId] = Noise3;

                                        noiseGeneratedResultIndex++;

                                        Noise3 += DeltaX;
                                    }
                                    NoiseZ0 += DeltaZ0;
                                    NoiseZ1 += DeltaZ1;
                                }
                                NoiseX0Z0Y0 += DeltaX0Z0;
                                NoiseX0Z1Y0 += DeltaX0Z1;
                                NoiseX1Z0Y0 += DeltaX1Z0;
                                NoiseX1Z1Y0 += DeltaX1Z1;
                            }
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Will present the result of the noise inside a single Array[]
        /// The 3dimension are present inside this array in this order : X and then Y
        /// It means to compute the array indice, use this formula : X * Ylength + Y
        /// </summary>
        /// <param name="noiseFct">The Noise function</param>
        /// <param name="NoiseSampledSteps">The qt of steps that will be evaluated into by the noise fct, if qt below StepsCountX, the remaining Steps will be interpolated !! SampledSteps values MUST be a multiple of StepsCountn</param>
        /// <param name="FromX">Low X sampling Range</param>
        /// <param name="ToX">High X sampling Range</param>
        /// <param name="StepsCountX">Nbr of step sampled at equal distance from FromX to ToX</param>
        /// <param name="FromY">Low Y sampling Range</param>
        /// <param name="ToY">High Y sampling Range</param>
        /// <param name="StepsCountY">Nbr of step sampled at equal distance from FromY to ToY</param>
        /// <returns>Signle dimension array with the noise result</returns>
        public static double[,] NoiseSampling(Vector2I NoiseSampledSteps,
                                            double FromX, double ToX, int StepsCountX,
                                            double FromY, double ToY, int StepsCountY,
                                            params INoise2[] noiseFcts)
        {
            bool needInterpolation = false;

            if (NoiseSampledSteps.X != StepsCountX || NoiseSampledSteps.Y != StepsCountY)
            {
                if (StepsCountX % NoiseSampledSteps.X != 0 ||
                    StepsCountY % NoiseSampledSteps.Y != 0)
                {
                    throw new Exception("NoiseSampling using NoiseSampledSteps that are not a multiple of StepsCount");
                }
                needInterpolation = true;
            }

            int sampledStepCountX = NoiseSampledSteps.X;
            int sampledStepCountY = NoiseSampledSteps.Y;
            if (needInterpolation)
            {
                sampledStepCountX++;// +1 If in Lerping Mode
                sampledStepCountY++;// +1 If in Lerping Mode
            }

            int NbrNoisesToParse = noiseFcts.Length;
            double[,] result = new double[(StepsCountX) * (StepsCountY), NbrNoisesToParse];

            double samplingStepDeltaX = (ToX - FromX) / (sampledStepCountX - 1);
            double samplingStepDeltaY = (ToY - FromY) / (sampledStepCountY - 1);

            int generatedNoise = 0;
            double valX, valY;

            valX = FromX;
            for (int X = 0; X < sampledStepCountX; X++)
            {
                valY = FromY;
                for (int Y = 0; Y < sampledStepCountY; Y++)
                {
                    for (int noiseId = 0; noiseId < NbrNoisesToParse; noiseId++)
                    {
                        result[generatedNoise, noiseId] = noiseFcts[noiseId].Get(valX, valY);
                    }
                    generatedNoise++;
                    valY += samplingStepDeltaY;
                }
                valX += samplingStepDeltaX;
            }

            if (needInterpolation)
            {
                result = InterpolateResult(result, NoiseSampledSteps, new Vector2I(StepsCountX, StepsCountY));
            }

            return result;
        }


        private static double[,] InterpolateResult(double[,] dataNoises,
                                                  Vector2I NoiseSampledSteps,
                                                  Vector2I StepsCount)
        {
            int nbrNoises = dataNoises.GetLength(1);

            //Create a new array with expended size
            double[,] result = new double[StepsCount.X * StepsCount.Y, nbrNoises];

            int dataNoisesXSize = NoiseSampledSteps.X + 1;
            int dataNoisesYSize = NoiseSampledSteps.Y + 1;

            int XPointLerpedCount = StepsCount.X / NoiseSampledSteps.X;
            int YPointLerpedCount = StepsCount.Y / NoiseSampledSteps.Y;

            for (int noiseId = 0; noiseId < nbrNoises; noiseId++)
            {
                for (int SampledpointX = 0; SampledpointX < NoiseSampledSteps.X; SampledpointX++)
                {
                    for (int SampledpointY = 0; SampledpointY < NoiseSampledSteps.Y; SampledpointY++)
                    {
                        double NoiseZ0 = dataNoises[(SampledpointX + 0) * dataNoisesYSize + SampledpointY, noiseId];
                        double NoiseZ1 = dataNoises[(SampledpointX + 1) * dataNoisesYSize + SampledpointY, noiseId];

                        double DeltaZ0 = (dataNoises[(SampledpointX + 0) * dataNoisesYSize + (SampledpointY + 1), noiseId] - NoiseZ0) / YPointLerpedCount;
                        double DeltaZ1 = (dataNoises[(SampledpointX + 1) * dataNoisesYSize + (SampledpointY + 1), noiseId] - NoiseZ1) / YPointLerpedCount;

                        for (int Y = 0; Y < YPointLerpedCount; Y++)
                        {
                            double Noise = NoiseZ0;
                            double DeltaX = (NoiseZ1 - NoiseZ0) / XPointLerpedCount; // Chunk Z length = 16 / 4 = 4 points needs to be lerped Once!

                            int nY = (SampledpointY * YPointLerpedCount) + Y;

                            for (int X = 0; X < XPointLerpedCount; X++)
                            {
                                int nX = (SampledpointX * XPointLerpedCount) + X;

                                result[nX * StepsCount.Y + nY, noiseId] = Noise;

                                Noise += DeltaX;
                            }

                            NoiseZ0 += DeltaZ0;
                            NoiseZ1 += DeltaZ1;
                        }
                    }
                }
            }

            return result;
        }
    }
}
