using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noises.Interfaces;

namespace S33M3CoreComponents.Noises.Sampler
{
    public static class NoiseSampler
    {
        public static float[] NoiseSampling(INoise3 noiseFct,
                                            int FromX, int ToX, int SamplingStepsCountX,
                                            int FromY, int ToY, int SamplingStepsCountY,
                                            int FromZ, int ToZ, int SamplingStepsCountZ)
        {
            float[] result = new float[(SamplingStepsCountX) * (SamplingStepsCountY) * (SamplingStepsCountZ)];

            float samplingStepX = (float)(ToX - FromX) / (SamplingStepsCountX - 1);
            float samplingStepY = (float)(ToY - FromY) / (SamplingStepsCountY - 1);
            float samplingStepZ = (float)(ToZ - FromZ) / (SamplingStepsCountZ - 1);

            int generatedNoise = 0;
            float valX, valY, valZ;

            valX = FromX;
            for (int X = 0; X < SamplingStepsCountX; X++)
            {
                valZ = FromZ;
                for (int Z = 0; Z < SamplingStepsCountZ; Z++)
                {
                    valY = FromY;
                    for (int Y = 0; Y < SamplingStepsCountY; Y++)
                    {
                        result[generatedNoise] = noiseFct.GetValue(valX, valY, valZ);
                        generatedNoise++;
                        valY += samplingStepY;
                    }
                    valZ += samplingStepZ;
                }
                valX += samplingStepX;
            }
            return result;
        }

        public static float[] NoiseSampling(INoise2 noiseFct,
                                            int FromX, int ToX, int SamplingStepsCountX,
                                            int FromZ, int ToZ, int SamplingStepsCountZ)
        {
            float[] result = new float[(SamplingStepsCountX) * (SamplingStepsCountZ)];

            float samplingStepX = (float)(ToX - FromX) / (SamplingStepsCountX - 1);
            float samplingStepZ = (float)(ToZ - FromZ) / (SamplingStepsCountZ - 1);

            int generatedNoise = 0;
            float valX, valZ;

            valX = FromX;
            for (int X = 0; X < SamplingStepsCountX; X++)
            {
                valZ = FromZ;
                for (int Z = 0; Z < SamplingStepsCountZ; Z++)
                {
                    result[generatedNoise] = noiseFct.GetValue(valX, valZ);
                    generatedNoise++;
                    valZ += samplingStepZ;
                }
                valX += samplingStepX;
            }
            return result;
        }
    }
}
