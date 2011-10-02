using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World.PlanGenerator;

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
                MapSize = new System.Drawing.Size(1920, 1080),
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
                                            var chunk = chunks[pos.X - generationRange.Position.X, pos.Y - generationRange.Position.Y];

                                            

                                        });
        }


        private int GetElevationAt(Point p)
        {
            var mappoint = new Point((int)(p.X / Scale),(int)(p.Y / Scale));

            mappoint.Offset(-WorldPlan.Parameters.MapSize.Width / 2, -WorldPlan.Parameters.MapSize.Height / 2);
            
            var poly = WorldPlan.GetAtPoint(mappoint);
            
            var distances = poly.Corners.Select(corner => Vector2I.Distance(new Vector2I(corner.Point.X, corner.Point.Y), new Vector2I(mappoint.X, mappoint.Y))).ToList();

            return 0;

        }

        public void Dispose()
        {
            
        }
    }
}
