using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World.PlanGenerator;
using Utopia.Shared.Entities.Concrete.Collectible;

namespace Utopia.Shared.World.Processors
{
    public class PlanWorldProcessor : IWorldProcessor
    {
        private readonly WorldParameters _worldParameters;

        public int PercentCompleted
        {
            get { throw new NotImplementedException(); }
        }

        public string ProcessorName
        {
            get { return "Plan processor"; }
        }

        public string ProcessorDescription
        {
            get { return "Generates world chunks based on world plan"; }
        }

        public WorldPlan WorldPlan { get; set; }

        public double Scale { get; set; }

        public PlanWorldProcessor(WorldParameters worldParameters)
        {
            _worldParameters = worldParameters;
            WorldPlan = new WorldPlan(new GenerationParameters
            {
                CenterElevation = true,
                ElevationSeed = _worldParameters.Seed,
                GridSeed = _worldParameters.Seed,
                MapSize = new Vector2I(1920, 1080),
                PolygonsCount = 600,
                RelaxCount = 3
            });

            Scale = 1;

            WorldPlan.Generate();
        }

        public void Generate(Range2 generationRange, GeneratedChunk[,] chunks)
        {
            
            generationRange.Foreach(pos =>
                                        {
                                            var r = new FastRandom(_worldParameters.Seed + pos.GetHashCode());
                                            var chunk = chunks[pos.X - generationRange.Position.X, pos.Y - generationRange.Position.Y];
                                            
                                            for (int x = 0; x < AbstractChunk.ChunkSize.X; x++)
                                            {
                                                for (int z = 0; z < AbstractChunk.ChunkSize.Z; z++)
                                                {
                                                    var pointData = GetPointData(new Point(pos.X * AbstractChunk.ChunkSize.X + x, pos.Y * AbstractChunk.ChunkSize.Z +z));

                                                    var topGroundBlock = CubeId.Grass;
                                                    var undegroundBlock = CubeId.Dirt;

                                                    if (pointData.IsRiver)
                                                        topGroundBlock = CubeId.Water;
                                                    
                                                    if (pointData.Biome == BiomeType.SubtropicalDesert || pointData.Biome == BiomeType.TemperateDesert)
                                                    {
                                                        topGroundBlock = CubeId.Sand;
                                                        undegroundBlock = CubeId.Sand;
                                                    }
                                                    bool trees = Biome.IsForest(pointData.Biome);

                                                    for (int y = 0; y < AbstractChunk.ChunkSize.Y; y++)
                                                    {
                                                        var globalPos = new Vector3D(pos.X * AbstractChunk.ChunkSize.X + x, y, pos.Y * AbstractChunk.ChunkSize.Z + z);

                                                        if (y <= pointData.Elevation)
                                                        {
                                                            chunk.BlockData[new Vector3I(x, y, z)] = undegroundBlock;
                                                        }
                                                        else if (y <= _worldParameters.SeaLevel)
                                                        {
                                                            chunk.BlockData[new Vector3I(x, y, z)] = CubeId.Water;
                                                        }

                                                        if (y == pointData.Elevation)
                                                        {
                                                            if (y >= _worldParameters.SeaLevel)
                                                            {
                                                                chunk.BlockData[new Vector3I(x, y, z)] = topGroundBlock;

                                                                if (topGroundBlock == CubeId.Grass)
                                                                {

                                                                    if (trees && r.NextDouble() < 0.005d)
                                                                    {
                                                                        AddTree(chunk, new Vector3I(x, y + 1, z));
                                                                    }
                                                                    else
                                                                        if (r.NextDouble() < 0.03)
                                                                        {
                                                                            double result = r.NextDouble();
                                                                            if (result <= 0.4)
                                                                            {
                                                                                chunk.Entities.Add(new Grass
                                                                                {
                                                                                    GrowPhase = (byte)r.Next(0, 5),
                                                                                    Position = globalPos + new Vector3D(0.5, 1, 0.5),
                                                                                    LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z)
                                                                                });
                                                                            }
                                                                            else if (result <= 0.6)
                                                                            {
                                                                                chunk.Entities.Add(new Flower1
                                                                                {
                                                                                    Position = globalPos + new Vector3D(0.5, 1, 0.5),
                                                                                    LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z)
                                                                                });
                                                                            }
                                                                            else if (result <= 0.7)
                                                                            {
                                                                                chunk.Entities.Add(new Flower2
                                                                                {
                                                                                    Position = globalPos + new Vector3D(0.5, 1, 0.5),
                                                                                    LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z)
                                                                                });
                                                                            }
                                                                            else if (result <= 0.9)
                                                                            {
                                                                                chunk.Entities.Add(new Mushr1
                                                                                {
                                                                                    Position = globalPos + new Vector3D(0.5, 1, 0.5),
                                                                                    LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z)
                                                                                });
                                                                            }
                                                                            else if (result <= 1)
                                                                            {
                                                                                chunk.Entities.Add(new Mushr2
                                                                                {
                                                                                    Position = globalPos + new Vector3D(0.5, 1, 0.5),
                                                                                    LinkedCube = new Vector3I(globalPos.X, globalPos.Y, globalPos.Z)
                                                                                });
                                                                            }
                                                                        }
                                                                }

                                                                break;
                                                            }
                                                            chunk.BlockData[new Vector3I(x, y, z)] = CubeId.Sand;
                                                        }
                                                    }
                                                }
                                            }
                                            

                                        });
        }

        private void AddTree(GeneratedChunk chunk, Vector3I vector3i)
        {
            // don't add tree at the edge of chunk
            if (vector3i.X == 0 || vector3i.X == AbstractChunk.ChunkSize.X - 1 || vector3i.Z == 0 || vector3i.Z == AbstractChunk.ChunkSize.Z - 1)
                return;

            var tree = new Tree();
            tree.Position = new Vector3D(vector3i.X, vector3i.Y, vector3i.Z);
            chunk.Entities.Add(tree);

            for (int i = 0; i < 7; i++)
            {
                TryAddBlock(chunk, vector3i, CubeId.Trunk);
                vector3i.Y++;
            }

            var radius = 2;
            for (int y = 0; y < 4; y++)
            {
                if (y == 0 || y == 3) radius = 1; else radius = 2;
                for (int x = -radius; x <= radius; x++)
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        TryAddBlock(chunk, new Vector3I(vector3i.X + x, vector3i.Y, vector3i.Z + z), CubeId.Leaves);
                    }
                }

                vector3i.Y--;
            }

        }

        private void TryAddBlock(GeneratedChunk chunk, Vector3I pos, byte value)
        {
            if (pos.X >= 0 && pos.X < AbstractChunk.ChunkSize.X && pos.Y >= 0 && pos.Y < AbstractChunk.ChunkSize.Y && pos.Z >= 0 && pos.Z < AbstractChunk.ChunkSize.Z)
            {
                chunk.BlockData[pos] = value;
            }
        }

        private struct PointData
        {
            public int Elevation;
            public int Moisture;
            public bool IsRiver;
            public BiomeType Biome;
        }

        private PointData GetPointData(Point p)
        {
            var data = new PointData();
            var mappoint = new Point((int)(p.X / Scale), (int)(p.Y / Scale));

            mappoint.Offset(WorldPlan.Parameters.MapSize.X / 2, WorldPlan.Parameters.MapSize.Y / 2);

            if (mappoint.X <= 0 && mappoint.Y <= 0 && mappoint.X > WorldPlan.Parameters.MapSize.X && mappoint.Y > WorldPlan.Parameters.MapSize.Y)
                return data;

            var poly = WorldPlan.GetAtPoint(mappoint);

            // river check
            foreach (var edge in poly.Edges)
            {
                
                if (edge.WaterFlow > 0)
                {
                    var riverPath = new GraphicsPath();
                    var pen = new Pen(Brushes.Black, edge.WaterFlow);
                    riverPath.AddLine(edge.Start, edge.End);
                    if (riverPath.IsOutlineVisible(mappoint, pen))
                    {
                        data.IsRiver = true;
                        break;
                    }
                }

            }
            
            //graphicsPath.IsOutlineVisible(

            data.Biome = poly.Biome;
            data.Moisture = poly.Moisture;
            var cor = poly.Corners.Find(c => c.Point == mappoint);
            if (cor != null)
            {
                data.Elevation = cor.Elevation / 2;
                return data;
            }

            // collect all nontouching corners

            //var corners = (from neighboor in poly.Neighbors from corner in neighboor.Corners where !poly.Corners.Contains(corner) select corner).ToList();


            var distances = poly.Corners.Select(corner => 1 / Math.Pow(Vector2I.Distance(new Vector2I(corner.Point.X, corner.Point.Y), new Vector2I(mappoint.X, mappoint.Y)),2)).ToList();

            var distSumm = distances.Sum();

            data.Elevation = (int)Math.Round(poly.Corners.Select((c, i) => (distances[i] * c.Elevation / 2) / distSumm).Sum());

            return data;
        }

        public void Dispose()
        {
            
        }
    }
}
