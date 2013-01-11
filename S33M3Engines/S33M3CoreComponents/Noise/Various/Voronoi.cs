using S33M3CoreComponents.Maths;
using S33M3CoreComponents.Noise.Generator;
using S33M3Resources.Structs;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.Various
{
    public class Voronoi : ISeedable
    {
        public struct VoronoiResult
        {
            public float distance;
            public Vector2 delta;
            public uint id;
        }

        #region Private Variables
        private Vector2 _offset;
        private static readonly float DensityAdjustment = 0.39815f;
        private static readonly float InverseDensityAdjustment = 1.0f / DensityAdjustment;

        /// <summary>
        /// Lookup table for cube feature counts.
        /// </summary>
        private int[] poissonCount = new int[256]
        {
            4,3,1,1,1,2,4,2,2,2,5,1,0,2,1,2,2,0,4,3,2,1,2,1,3,2,2,4,2,2,5,1,2,3,
            2,2,2,2,2,3,2,4,2,5,3,2,2,2,5,3,3,5,2,1,3,3,4,4,2,3,0,4,2,2,2,1,3,2,
            2,2,3,3,3,1,2,0,2,1,1,2,2,2,2,5,3,2,3,2,3,2,2,1,0,2,1,1,2,1,2,2,1,3,
            4,2,2,2,5,4,2,4,2,2,5,4,3,2,2,5,4,3,3,3,5,2,2,2,2,2,3,1,1,4,2,1,3,3,
            4,3,2,4,3,3,3,4,5,1,4,2,4,3,1,2,3,5,3,2,1,3,1,3,3,3,2,3,1,5,5,4,2,2,
            4,1,3,4,1,5,3,3,5,3,4,3,2,2,1,1,1,1,1,2,4,5,4,5,4,2,1,5,1,1,2,3,3,3,
            2,5,2,3,3,2,0,2,1,1,4,2,1,3,2,1,2,2,3,2,5,5,3,4,5,5,2,4,4,5,3,2,2,2,
            1,4,2,3,3,4,2,5,4,2,4,2,2,2,4,5,3,2
        };
        #endregion

        #region Public Properties
        public int Seed { get; set; }
        #endregion

        public Voronoi(int seed)
        {
            Seed = seed;
            Initialize();
        }

        #region Public Methods
        //Return the closest point position in the voronoi surface
        public VoronoiResult[] Get(double x, double y)
        {
            Vector2 at = new Vector2((float)x, (float)y);

            VoronoiResult[] results = new VoronoiResult[4];
            for (int i = 0; i < results.Length; i++)
            {
                results[i].distance = float.MaxValue;
            }

            at *= DensityAdjustment;
            at += _offset;

            Vector2I cell = new Vector2I(MathHelper.Floor(at.X), MathHelper.Floor(at.Y));

            ProcessCell(cell, at, results);

            Vector2 cellPos = at - cell;
            Vector2 distMax = new Vector2(new Vector2(1 - cellPos.X, 0).LengthSquared(), new Vector2(0, 1 - cellPos.Y).LengthSquared());
            Vector2 distMin = new Vector2(new Vector2(cellPos.X, 0).LengthSquared(), new Vector2(0, cellPos.Y).LengthSquared());

            // Test near cells
            if (distMin.X < results[results.Length - 1].distance) ProcessCell(cell - Vector2I.XAxis, at, results);
            if (distMin.Y < results[results.Length - 1].distance) ProcessCell(cell - Vector2I.YAxis, at, results);
            if (distMax.X < results[results.Length - 1].distance) ProcessCell(cell + Vector2I.XAxis, at, results);
            if (distMax.Y < results[results.Length - 1].distance) ProcessCell(cell + Vector2I.YAxis, at, results);

            // Test further cells
            if (distMin.X + distMin.Y < results[results.Length - 1].distance) ProcessCell(cell - Vector2I.One, at, results);
            if (distMax.X + distMax.Y < results[results.Length - 1].distance) ProcessCell(cell + Vector2I.One, at, results);
            if (distMin.X + distMax.Y < results[results.Length - 1].distance) ProcessCell(cell - Vector2I.XAxis + Vector2I.YAxis, at, results);
            if (distMax.X + distMin.Y < results[results.Length - 1].distance) ProcessCell(cell + Vector2I.XAxis - Vector2I.YAxis, at, results);

            for (int i = 0; i < results.Length; i++)
            {
                results[i].delta *= InverseDensityAdjustment;
                results[i].distance *= InverseDensityAdjustment * InverseDensityAdjustment;
            }

            return results;
        }

        public double Get(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public double Get(double x, double y, double z, double w)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            Random rnd = new Random(Seed);
            _offset = 99999 * new Vector2(rnd.Next(), rnd.Next());
        }

        private uint incrementSeed(uint last)
        {
            uint newSeed = 1402024253 * last + 586950981;
            return newSeed;
        }

        private void ProcessCell(Vector2I cell, Vector2 at, VoronoiResult[] results)
        {
            uint seed = (uint)(702395077 * cell.X + 915488749 * cell.Y);
            // Number of features
            int count = poissonCount[seed >> 24];
            seed = incrementSeed(seed);

            for (int point = 0; point < count; point++)
            {
                uint id = seed;
                seed = incrementSeed(seed);

                float x, y;
                x = (seed + 0.5f) / 4294967296.0f;
                seed = incrementSeed(seed);
                y = (seed + 0.5f) / 4294967296.0f;
                seed = incrementSeed(seed);
                Vector2 innerPos = new Vector2(x, y);
                Vector2 delta = cell + innerPos - at;

                float dist = delta.LengthSquared();

                if (dist < results[results.Length - 1].distance)
                {
                    int index = results.Length - 1;
                    while (index > 0 && dist < results[index - 1].distance) index--;

                    for (int i = results.Length - 1; i > index; i--)
                    {
                        results[i] = results[i - 1];
                    }
                    results[index].distance = dist;
                    results[index].delta = delta;
                    results[index].id = id;
                }
            }
        #endregion
        }
    }
}
