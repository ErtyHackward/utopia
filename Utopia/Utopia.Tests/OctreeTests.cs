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

            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        var value = (byte)(r.NextDouble() * 255);
                        inside.SetBlock(new Vector3I(x, y, z), value);
                        octree.SetBlock(new Vector3I(x, y, z), value);
                    }
                }
            }

            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        Assert.AreEqual(inside.GetBlock(x, y, z), octree.GetBlock(x, y, z));
                    }
                }
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

            for (var x = 0; x < 100; x++)
                action();

            sw.Stop();
            Console.WriteLine(desc + " x100 = " + sw.ElapsedMilliseconds + " ms" );

        }

        public static InsideDataProvider inside;
        public static OctreeChunkDataProvider octree;


        [TestMethod]
        public void OctreePerfomanceTest()
        {            
            for (int i = 0; i < 3; i ++)
            {
                Console.WriteLine("Take " + (i + 1));

                #region Inside random -------------------------------------------------------

                MeasureMemoryUsage("Flattern array full chunk random data:", () =>
                {
                    inside = new InsideDataProvider(new Vector3I(16, 16, 16));
                    Random r = new Random(0);

                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                var value = (byte)(r.NextDouble() * 255);
                                inside.SetBlock(new Vector3I(x, y, z), value);
                            }
                        }
                    }
                });

                #endregion

                #region Inside empty -------------------------------------------------------

                MeasureMemoryUsage("Flattern array all zero:", () =>
                {
                    inside = new InsideDataProvider(new Vector3I(16, 16, 16));

                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                inside.SetBlock(new Vector3I(x, y, z), 0);
                            }
                        }
                    }
                });

                #endregion

                #region Flattern create

                MeasurePerformance("Flattern array create:", () =>
                {
                    Random r = new Random(0);
                    inside = new InsideDataProvider(new Vector3I(16, 16, 16));
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                var value = (byte)(r.NextDouble() * 255);
                                inside.SetBlock(new Vector3I(x, y, z), value);
                            }
                        }
                    }
                });

                #endregion

                #region Flattern access

                MeasurePerformance("Flattern array read:", () =>
                {
                    byte value;

                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                value = inside.GetBlock(new Vector3I(x, y, z));
                            }
                        }
                    }
                });

                #endregion

                #region Octree random -------------------------------------------------------

                MeasureMemoryUsage("Octree full chunk random data:", () =>
                {
                    octree = new OctreeChunkDataProvider();
                    Random r = new Random(0);
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                var value = (byte)(r.NextDouble() * 255);
                                octree.SetBlock(new Vector3I(x, y, z), value);
                            }
                        }
                    }
                });

                #endregion

                #region Octree zero

                MeasureMemoryUsage("Octree all zero:", () =>
                {
                    octree = new OctreeChunkDataProvider();
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                octree.SetBlock(new Vector3I(x, y, z), 0);
                            }
                        }
                    }
                });

                #endregion


                #region Octree half

                MeasureMemoryUsage("Octree half random:", () =>
                {
                    Random r = new Random(0);
                    octree = new OctreeChunkDataProvider();
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                var value = (byte)(r.NextDouble() * 255);
                                octree.SetBlock(new Vector3I(x, y, z), value);
                            }
                        }
                    }
                });

                #endregion

                #region Octree create

                MeasurePerformance("Octree create:", () =>
                {
                    Random r = new Random(0);
                    octree = new OctreeChunkDataProvider();
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                var value = (byte)(r.NextDouble() * 255);
                                octree.SetBlock(new Vector3I(x, y, z), value);
                            }
                        }
                    }
                });

                #endregion

                #region Octree access

                MeasurePerformance("Octree read:", () =>
                {
                    byte value;

                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                value = octree.GetBlock(new Vector3I(x, y, z));
                            }
                        }
                    }
                });

                #endregion
            }
        }


    }
}
