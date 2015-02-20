using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S33M3Resources.Structs;
using Utopia.Shared;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;

namespace Utopia.Tests
{
    [TestClass]
    public class OctreeTests
    {
        [TestMethod]
        public void CheckOctreeIntegrity()
        {
            var inside = new InsideDataProvider();
            var octree = new OctreeChunkDataProvider();
            Random r = new Random();

            foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
            {
                var value = (byte)(r.NextDouble() * 255);
                inside.SetBlock(pos, value);
                octree.SetBlock(pos, value);
            }


            foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
            {
                Assert.AreEqual(inside.GetBlock(pos), octree.GetBlock(pos));
            }
        }

        public void MeasureMemoryUsage(string desc, Action action)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var before = GC.GetTotalMemory(false);

            for (var x = 0; x < 10; x++)
                action();

            Console.WriteLine(desc + " " + BytesHelper.FormatBytes((GC.GetTotalMemory(false) - before) / 10));
        }

        public void MeasurePerformance(string desc, Action action)
        {
            var sw = Stopwatch.StartNew();

            for (var x = 0; x < 10; x++)
                action();

            sw.Stop();
            Console.WriteLine(desc + " x10 = " + sw.ElapsedMilliseconds + " ms" );

        }

        public static InsideDataProvider inside;
        public static OctreeChunkDataProvider octree;


        [TestMethod]
        public void OctreePerfomanceTest()
        {            

            MeasureMemoryUsage("4k array", () => { var array = new Byte[16 * 16 * 16]; });
            MeasureMemoryUsage("32k array", () => { var array = new Byte[16 * 128 * 16]; });

            for (int i = 0; i < 3; i ++)
            {
                Console.WriteLine("Take " + (i + 1));

                #region Inside random -------------------------------------------------------

                MeasureMemoryUsage("Flattern array full chunk random data:", () =>
                {
                    inside = new InsideDataProvider();
                    Random r = new Random(0);

                    foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
                    {
                        var value = (byte)(r.NextDouble() * 255);
                        inside.SetBlock(pos, value);
                    }
                });

                #endregion

                #region Inside empty -------------------------------------------------------

                MeasureMemoryUsage("Flattern array all zero:", () =>
                {
                    inside = new InsideDataProvider();

                    foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
                    {
                        inside.SetBlock(pos, 0);
                    }
                });

                #endregion

                #region Flattern create

                MeasurePerformance("Flattern array create:", () =>
                {
                    Random r = new Random(0);
                    inside = new InsideDataProvider();
                    foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
                    {
                        var value = (byte)(r.NextDouble() * 255);
                        inside.SetBlock(pos, value);
                    }
                });

                #endregion

                #region Flattern access

                MeasurePerformance("Flattern array read:", () =>
                {
                    byte value;

                    foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
                    {
                        value = inside.GetBlock(pos);
                    }
                });

                #endregion

                #region Octree random -------------------------------------------------------

                MeasureMemoryUsage("Octree full chunk random data:", () =>
                {
                    octree = new OctreeChunkDataProvider();
                    Random r = new Random(0);
                    foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
                    {
                        var value = (byte)(r.NextDouble() * 255);
                        octree.SetBlock(pos, value);
                    }
                });

                #endregion

                #region Octree zero

                MeasureMemoryUsage("Octree all zero:", () =>
                {
                    octree = new OctreeChunkDataProvider();
                    foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
                    {
                        octree.SetBlock(pos, 0);
                    }
                });

                #endregion


                #region Octree half

                MeasureMemoryUsage("Octree half random:", () =>
                {
                    Random r = new Random(0);
                    octree = new OctreeChunkDataProvider();
                    foreach (var pos in new Range3I(new Vector3I(), new Vector3I(16, 64, 16)))
                    {
                        var value = (byte)(r.NextDouble() * 255);
                        octree.SetBlock(pos, value);
                    }
                });

                #endregion

                #region Octree create

                MeasurePerformance("Octree create:", () =>
                {
                    Random r = new Random(0);
                    octree = new OctreeChunkDataProvider();
                    foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
                    {
                        var value = (byte)(r.NextDouble() * 255);
                        octree.SetBlock(pos, value);
                    }
                });

                #endregion

                #region Octree access

                MeasurePerformance("Octree read:", () =>
                {
                    byte value;

                    foreach (var pos in new Range3I(new Vector3I(), AbstractChunk.ChunkSize))
                    {
                        value = octree.GetBlock(pos);
                    }
                });

                #endregion
            }
        }


    }
}
