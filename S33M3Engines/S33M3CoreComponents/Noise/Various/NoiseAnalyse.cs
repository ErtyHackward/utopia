using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using System.Diagnostics;

namespace S33M3CoreComponents.Noise.Various
{
    public static class NoiseAnalyse
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        #region Public Methods
        public static string Analyse(INoise2 noiseFct, int iteration)
        {
            FastRandom rnd = new FastRandom();
            //Generate randomIteration number in array
            int[,] inputNumber = new int[iteration, 2];
            for (int i = 0; i < iteration; i++)
            {
                inputNumber[i, 0] = rnd.Next();
                inputNumber[i, 1] = rnd.Next();
            }

            long from = Stopwatch.GetTimestamp();
            double min = double.MaxValue;
            double max = double.MinValue;

            for (int i = 0; i < iteration; i++)
            {
                double val = noiseFct.Get(inputNumber[i, 0], inputNumber[i, 1]);
                if (val < min) min = val;
                if (val > max) max = val;
            }

            long to = Stopwatch.GetTimestamp();

            return "INoise2 analysed for " + iteration + " iteration. Time needed : " + ((to - from)/ (double)Stopwatch.Frequency * 1000.0) + " ms; Min : " + min + " max : " + max;
        }

        public static string Analyse(INoise3 noiseFct, int iteration)
        {
            FastRandom rnd = new FastRandom();
            //Generate randomIteration number in array
            int[,] inputNumber = new int[iteration, 3];
            for (int i = 0; i < iteration; i++)
            {
                inputNumber[i, 0] = rnd.Next();
                inputNumber[i, 1] = rnd.Next();
                inputNumber[i, 2] = rnd.Next();
            }

            long from = Stopwatch.GetTimestamp();
            double min = double.MaxValue;
            double max = double.MinValue;


            for (int i = 0; i < iteration; i++)
            {
                double val = noiseFct.Get(inputNumber[i, 0], inputNumber[i, 1], inputNumber[i, 2]);
                if (val < min) min = val;
                if (val > max) max = val;
            }

            long to = Stopwatch.GetTimestamp();

            return "INoise3 analysed for " + iteration + " iteration. Time needed : " + ((to - from) / (double)Stopwatch.Frequency * 1000.0) + " ms; Min : " + min + " max : " + max;
        }

        #endregion

        #region Private Methods
        #endregion

    }
}
