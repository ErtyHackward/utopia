using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml.Serialization;
using BenTools.Mathematics;
using S33M3CoreComponents.Maths.Noises;

namespace Utopia.Shared.World.PlanGenerator
{
    /// <summary>
    /// Represents a main world plan object. Plan is based on Voronoi diagram polygons.
    /// </summary>
    [Serializable]
    public class WorldPlan
    {
        private readonly Biome _biome = new Biome(new ParameterVariation(127, 255), new ParameterVariation(0, 150));

        // temp values for propagation function
        private readonly Dictionary<Point, int> _propagationLayers = new Dictionary<Point, int>();

        // moisture stuff
        private readonly List<RiverBranch> _riverRoots = new List<RiverBranch>();
        private readonly HashSet<Edge> _rivers = new HashSet<Edge>();
        private HashSet<Corner> _corners1;
        private List<Corner> _waterCorners;

        private readonly Dictionary<Point, Polygon> _polygons = new Dictionary<Point, Polygon>();
        private readonly Dictionary<Point, Corner> _corners = new Dictionary<Point, Corner>(); 
        
        /// <summary>
        /// Collection of polygons where key is polygon center
        /// </summary>
        [XmlIgnore]
        public Dictionary<Point, Polygon> Polygons
        {
            get { return _polygons; }
        }

        /// <summary>
        /// Gets or sets main plan generation parameters
        /// </summary>
        [XmlElement("Parameters")]
        public GenerationParameters Parameters { get; set; }

        [XmlIgnore]
        public Image RenderMapTemplate { get; set; }

        [XmlIgnore]
        public Image RenderContinentTemplate { get; set; }

        [XmlIgnore]
        public Image[] RenderWavePatterns { get; set; }

        [XmlIgnore]
        public Image RenderForest { get; set; }

        [XmlIgnore]
        public Image RenderTropicalForest { get; set; }

        /// <summary>
        /// Do not use this in code
        /// </summary>
        [XmlArray("PolygonsCollection")]
        public Polygon[] PolygonsCollectionForSerialization
        {
            get {
                return  new List<Polygon>(_polygons.Values).ToArray();
            }
            set { 
                _polygons.Clear();

                foreach (var polygon in value)
                {
                    _polygons.Add(polygon.Center, polygon);
                }
                
                GetPoints();
            }

        }

        /// <summary>
        /// Creates new empty instance of world plan. For serialization purpose
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public WorldPlan()
        {
            
        }

        /// <summary>
        /// Creates new instance of world plan
        /// </summary>
        /// <param name="pars"></param>
        public WorldPlan(GenerationParameters pars)
        {
            Parameters = pars;   
        }

        /// <summary>
        /// Do not use this method in code
        /// </summary>
        /// <param name="o"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Add(object o)
        {
            var p = o as Polygon;
            if (p != null)
            {
                if(!_polygons.ContainsKey(p.Center))
                    _polygons.Add(p.Center, p);
            }
        }

        protected Corner GetCorner(Point p)
        {
            if (!_corners.ContainsKey(p))
                _corners.Add(p, new Corner(p));
            return _corners[p];
        }

        private void Relax(VoronoiGraph graph, List<Vector> vectors)
        {
            foreach (var vector in vectors)
            {
                var v0 = 0.0d;
                var v1 = 0.0d;
                var edgesCount = 0;

                foreach (var edge in graph.Edges)
                {
                    if (edge.LeftData == vector || edge.RightData == vector)
                    {
                        var p0 = (edge.VVertexA[0] + edge.VVertexB[0]) / 2;
                        var p1 = (edge.VVertexA[1] + edge.VVertexB[1]) / 2;
                        v0 += double.IsNaN(p0) ? 0 : p0;
                        v1 += double.IsNaN(p1) ? 0 : p1;
                        edgesCount++;
                    }
                }

                vector[0] = v0 / edgesCount;
                vector[1] = v1 / edgesCount;
            }
        }

        /// <summary>
        /// Starts plan generation process 
        /// </summary>
        public void Generate()
        {
            var datapoints = new List<Vector>(Parameters.PolygonsCount);

            var r = new Random(Parameters.GridSeed);

            for (int i = 0; i < Parameters.PolygonsCount; i++)
            {
                datapoints.Add(new Vector(r.Next(0, Parameters.MapSize.X), r.Next(0, Parameters.MapSize.Y)));
            }

            var graph = Fortune.ComputeVoronoiGraph(datapoints);

            for (int i = 0; i < Parameters.RelaxCount; i++)
            {
                Relax(graph, datapoints);
                graph = Fortune.ComputeVoronoiGraph(datapoints);
            }

            FillMap(graph);
            
            {
                // adding elevation data

                var elevationNoise = new SimplexNoise(new Random(Parameters.ElevationSeed));
                elevationNoise.SetParameters(0.0008, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

                foreach (var poly in this)
                {
                    var noiseVal = elevationNoise.GetNoise2DValue(poly.Center.X, poly.Center.Y, 4, 0.8);
                    var col = 255 / noiseVal.MaxValue * noiseVal.Value;
                    poly.Elevation = (int)col;
                }

                //// elevate each corner
                //if (!Parameters.CenterElevation)
                //    ElevateCorners();

            }

            if(Parameters.CenterElevation)
            {
                var poly = GetAtPoint(new Point(Parameters.MapSize.X / 2, Parameters.MapSize.Y / 2));
                poly.Elevation = 200;
                StartPropagation(poly, 15);
                
            }

            // making island
            {
                //r = new Random((int)voronoiSeedNumeric.Value);
                var borderElevation = 80;
                var step = 20;
                for (int x = 0; x < Parameters.MapSize.X; x += 5)
                {
                    var poly = GetAtPoint(new Point(x, 0));

                    poly.Elevation = borderElevation;// r.Next(0, 100);
                    StartPropagation(poly, step);


                    poly = GetAtPoint(new Point(x, Parameters.MapSize.Y));
                    poly.Elevation = borderElevation;// r.Next(0, 100);
                    StartPropagation(poly, step);

                }

                for (int y = 0; y < Parameters.MapSize.Y; y += 5)
                {
                    var poly = GetAtPoint(new Point(0, y));

                    poly.Elevation = borderElevation;// r.Next(0, 100);
                    StartPropagation(poly, step);


                    poly = GetAtPoint(new Point(Parameters.MapSize.X, y));

                    poly.Elevation = borderElevation;// r.Next(0, 100);
                    StartPropagation(poly, step);

                }
            }

            ElevateCorners();

            #region Moisturizing
            {
                var noise = new SimplexNoise(new Random(Parameters.ElevationSeed));
                noise.SetParameters(0.0008d, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

                foreach (var poly in this)
                {
                    var noiseVal = noise.GetNoise2DValue(poly.Center.X, poly.Center.Y, 2, 0.8);
                    var col = 100 / noiseVal.MaxValue * noiseVal.Value;
                    poly.Moisture = (int)col;

                    foreach (var corner in poly.Corners)
                    {
                        noiseVal = noise.GetNoise2DValue(corner.Point.X, corner.Point.Y, 2, 0.8);
                        col = 2 / noiseVal.MaxValue * noiseVal.Value;
                        corner.WaterFlow = (int)col;
                    }

                }

                // fix heights

                foreach (var poly in this)
                {
                    foreach (var neighbor in poly.Neighbors)
                    {
                        if (poly.Elevation == neighbor.Elevation)
                        {
                            neighbor.Elevation = (int)neighbor.Neighbors.Average(n => n.Elevation);
                        }
                    }
                }

                // calculate rivers
                _corners1 = new HashSet<Corner>();

                // get unique corners
                foreach (var poly in this)
                {
                    foreach (var corner in poly.Corners)
                    {
                        if (!_corners1.Contains(corner) && corner.Polygons.Find(p => p.Elevation <= 127) == null)
                        {
                            _corners1.Add(corner);
                        }
                    }
                }

                var list = new List<Corner>(_corners1);
                list.Sort(new CornerHeightComparer());

                // propagate flow
                foreach (var corner in list)
                {
                    // find lowest edge
                    Edge lowestEdge = corner.Edges[0];
                    int height = lowestEdge.GetOpposite(corner).Elevation;
                    for (int i = 0; i < corner.Edges.Count; i++)
                    {
                        var tmp = corner.Edges[i].GetOpposite(corner).Elevation;
                        if (tmp < height)
                        {
                            height = tmp;
                            lowestEdge = corner.Edges[i];
                        }
                    }

                    lowestEdge.WaterFlow += corner.WaterFlow;
                    lowestEdge.GetOpposite(corner).WaterFlow += corner.WaterFlow;
                }

                // remove rivers that not going to oceans

                _waterCorners = new List<Corner>();

                foreach (var poly in this)
                {
                    foreach (var corner in poly.Corners)
                    {
                        int solid = corner.Polygons.Count(p => p.Elevation > 127);

                        if (solid == 2)
                        {
                            _waterCorners.Add(corner);
                        }
                    }
                }

                // collect all correct edges
                _rivers.Clear();

                foreach (var waterCorner in _waterCorners)
                {
                    var root = new RiverBranch();
                    CollectRiver(waterCorner, root);
                    if (!root.Final)
                        _riverRoots.Add(root);
                }

                // fix river flows
                // stage 1: remove all flows
                foreach (var riverBranch in _riverRoots)
                {
                    EnumerateTree(riverBranch, b => { if (b.Edge != null) b.Edge.WaterFlow = 0; });
                }
                // stage 2: reflow it
                foreach (var riverBranch in _riverRoots)
                {
                    FillRiver(riverBranch);
                }

                // remove all non-river
                foreach (var poly in this)
                {
                    foreach (var edge in poly.Edges)
                    {
                        if (edge.WaterFlow > 0 && !_rivers.Contains(edge))
                            edge.WaterFlow = 0;
                    }
                }

                // update moisture for polygons
                foreach (var poly in this)
                {
                    if (poly.Elevation > 127)
                    {
                        poly.Moisture = poly.Neighbors.Sum(p =>
                        {
                            var v = p.Neighbors.Sum(n => n.Edges.Sum(ed => ed.WaterFlow > 0 ? 1 : 0));
                            v = v + p.Neighbors.Sum(n => n.Elevation < 127 && !n.Ocean ? 3 : 0);
                            return p.Edges.Sum(ed => ed.WaterFlow > 0 ? 1 : 0) + v;
                        });
                    }
                }

            }
            #endregion

            // detect ocean
            {
                SetOcean(GetAtPoint(new Point(0, 0)));
                SetOcean(GetAtPoint(new Point(Parameters.MapSize.X, 0)));
                SetOcean(GetAtPoint(new Point(0, Parameters.MapSize.Y)));
                SetOcean(GetAtPoint(new Point(Parameters.MapSize.X, Parameters.MapSize.Y)));
            }

            // set biomes
            {
                foreach (var poly in this)
                {
                    if (poly.Elevation > 127)
                    {
                        poly.Biome = _biome.GetBiomeWith(poly.Elevation, poly.Moisture > _biome.Moisture.Maximum ? _biome.Moisture.Maximum : poly.Moisture);
                    }
                }
            }

            //find coastline
            foreach (var polygon in this)
            {
                foreach (var edge in polygon.Edges)
                {
                    Polygon poly;
                    Polygon poly2;
                    if (((poly = edge.Polygons.Find(p => p.Elevation > 127)) != null) && ((poly2 = edge.Polygons.Find((p => p.Elevation <= 127))) != null))
                    {
                        poly.Coast = true;
                        poly2.Coast = true;
                        edge.Coast = true;
                    }
                }
            }
        }

        private void SetOcean(Polygon polygon)
        {
            if (polygon.Elevation <= 127 && !polygon.Ocean)
            {
                polygon.Ocean = true;
                foreach (var neighbor in polygon.Neighbors)
                {
                    SetOcean(neighbor);
                }
            }
        }

        private int FillRiver(RiverBranch branch)
        {
            int flood;
            if (branch.Final)
            {
                flood = 1;
            }
            else
            {
                flood = branch.Branches.Sum(b => FillRiver(b)) + 1;
            }
            if (branch.Edge != null)
                branch.Edge.WaterFlow = flood;
            return flood;
        }

        private void EnumerateTree(RiverBranch branch, Action<RiverBranch> action)
        {
            action(branch);
            branch.Branches.ForEach(b => EnumerateTree(b, action));
        }

        private void CollectRiver(Corner c, RiverBranch riverBranch)
        {
            foreach (var edge in c.Edges)
            {
                if (edge.WaterFlow > 1)
                {
                    if (!_rivers.Contains(edge))
                    {
                        var newBranch = new RiverBranch { Edge = edge, ParentBranch = riverBranch };
                        riverBranch.Branches.Add(newBranch);
                        _rivers.Add(edge);
                        CollectRiver(edge.GetOpposite(c), newBranch);
                    }
                }
            }
        }
        
        private void StartPropagation(Polygon p, int thresold)
        {
            _propagationLayers.Clear();
            _propagationLayers[p.Center] = 6;
            PropagateValue(p, thresold, 5);
        }

        private void PropagateValue(Polygon p, int thresold, int token)
        {
            var list = new List<Polygon>();

            foreach (var poly in p.Neighbors)
            {
                if (_propagationLayers.ContainsKey(poly.Center))
                {
                    if (_propagationLayers[poly.Center] > token)
                        continue;
                }

                if (poly.Elevation - p.Elevation > thresold)
                {
                    poly.Elevation = p.Elevation + thresold;
                    _propagationLayers[poly.Center] = token - 1;
                    list.Add(poly);
                    //PropagateValue(poly, thresold, token);
                }
                if (poly.Elevation - p.Elevation < -thresold)
                {
                    poly.Elevation = p.Elevation - thresold;
                    _propagationLayers[poly.Center] = token - 1;
                    list.Add(poly);
                    //PropagateValue(poly, thresold, token);
                }
            }

            foreach (var polygon in list)
            {
                PropagateValue(polygon, thresold, token - 1);
            }
        }

        private void ElevateCorners()
        {
            foreach (var poly in this)
            {
                foreach (var corner in poly.Corners)
                {
                    if (corner.Elevation == 0)
                    {
                        corner.Elevation = (int)corner.Polygons.Average(p => p.Elevation);
                    }
                }
            }
        }

        public void FillMap(VoronoiGraph graph)
        {

            var listEdge = new List<VoronoiEdge>();

            foreach (VoronoiEdge edge in graph.Edges)
            {
                listEdge.Add(edge);
                if (!double.IsNaN(edge.VVertexA[0]) && !double.IsNaN(edge.VVertexA[1]) && !double.IsNaN(edge.VVertexB[0]) && !double.IsNaN(edge.VVertexB[1]))
                {
                    var e = new Edge(edge.VVertexA, edge.VVertexB);
                    var p1 = AddEdge(new Point((int)edge.LeftData[0], (int)edge.LeftData[1]), e);
                    var p2 = AddEdge(new Point((int)edge.RightData[0], (int)edge.RightData[1]), e);
                    p1.AddNeighbor(p2);
                    p2.AddNeighbor(p1);

                    var c1 = GetCorner(edge.VVertexA.ToPoint());
                    var c2 = GetCorner(edge.VVertexB.ToPoint());

                    c1.AddPolygon(p1);
                    c2.AddPolygon(p1);
                    c1.AddPolygon(p2);
                    c2.AddPolygon(p2);
                    c1.AddEdge(e);
                    c2.AddEdge(e);

                    e.AddPolygon(p1);
                    e.AddPolygon(p2);

                    e.StartCorner = c1;
                    e.EndCorner = c2;

                    p1.AddCorner(c1);
                    p1.AddCorner(c2);
                    p2.AddCorner(c1);
                    p2.AddCorner(c2);

                }
            }

            var r = new Random(0);

            foreach (var poly in this)
            {
                foreach (var edge in poly.Edges)
                {

                    for (int i = 0; i < 2; i++)
                    {
                        if (edge.Points.Length < 17)
                            edge.Split(ref r);
                    }
                }
            }

            

            GetPoints();
        }

        private Polygon AddEdge(Point center, Edge edge)
        {
            Polygon p;
            if (!_polygons.ContainsKey(center))
            {
                _polygons.Add(center, p = new Polygon { Center = center });
            }
            else p = _polygons[center];

            _polygons[center].Edges.Add(edge);
            return p;
        }

        protected void GetPoints()
        {
            foreach (var polygon in Polygons)
            {
                polygon.Value.GetPoints();
            }
        }

        /// <summary>
        /// Performs search for polygon
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Polygon GetAtPoint(Point p)
        {
            Polygon selected = null;
            double distance = 0;
            foreach (var poly in _polygons.Values)
            {
                if (selected == null)
                {
                    selected = poly;
                    distance = DistanceSquared(p, poly.Center);
                    continue;
                }

                var d = DistanceSquared(p, poly.Center);

                if (d < distance)
                {
                    selected = poly;
                    distance = d;
                }
            }
            return selected;
        }

        private double DistanceSquared(Point p1, Point p2)
        {
            var dx = (uint)(p1.X - p2.X);
            var dy = (uint)(p1.Y - p2.Y);
            return dx * dx + dy * dy;
        }

        public IEnumerator<Polygon> GetEnumerator()
        {
            foreach (var polygon in _polygons)
            {
                yield return polygon.Value;
            }
        }

        private Color GetPolygonColor(Polygon polygon)
        {
            Color col;
            if (polygon.Elevation > 127)
            {
                //earth
                if (polygon.Biome != BiomeType.None)
                {

                    col = Biome.GetBiomeColor(polygon.Biome);
                }
                else col = Color.FromArgb((byte)(100 - polygon.Elevation), (byte)(80 - polygon.Elevation), (byte)(50 - polygon.Elevation));
            }
            else
            {
                if (polygon.Ocean)
                {
                    //                    col = Color.FromArgb((byte) (2 + polygon.Elevation), (byte) (108 + polygon.Elevation),
                    //                     (byte) (85 + polygon.Elevation));
                    col = Color.FromArgb(2, 108, 185);
                }
                else
                {
                    col = Color.FromArgb((byte)(polygon.Elevation), (byte)(polygon.Elevation), 255);
                }
            }


            return col;
        }

        private GraphicsPath CollectPath(Edge e, List<Edge> edges)
        {
            var p = new GraphicsPath();

            var currentEdge = e;
            edges.Remove(currentEdge);
            p.AddCurve(e.Points);

            while (currentEdge == e || (e.Start != currentEdge.End && e.Start != currentEdge.Start))
            {
                bool reverse = false;
                var nextEdge = currentEdge.EndCorner.Edges.Find(ed => ed != currentEdge && ed.Coast);

                if (!edges.Contains(nextEdge))
                {
                    nextEdge = currentEdge.StartCorner.Edges.Find(ed => ed != currentEdge && ed.Coast);
                    reverse = true;
                }

                currentEdge = nextEdge;

                edges.Remove(currentEdge);
                p.AddCurve(reverse ? currentEdge.Points.Reverse().Skip(1).ToArray() : currentEdge.Points.Skip(1).ToArray());
            }

            return p;
        }

        public Bitmap Render()
        {
            if (Parameters.MapSize.X == 0 || Parameters.MapSize.Y == 0) return null;
            
            Bitmap bmp = new Bitmap(Parameters.MapSize.X, Parameters.MapSize.Y,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            Random r = new Random(1);

            #region Continent Path
            //// to render solid border we need to collect all continents
            
            //// first collect all coast edges
            //var _edges = new List<Edge>();
            //foreach (var polygon in this)
            //{
            //    foreach (var edge in polygon.Edges)
            //    {
            //        if (edge.Coast && !_edges.Contains(edge))
            //            _edges.Add(edge);
            //    }
            //}

            //var continentsPaths = new List<GraphicsPath>();
            ////then create continents
            //while (_edges.Count > 0)
            //{
            //    continentsPaths.Add(CollectPath(_edges[0], _edges));
            //}


            #endregion

            using (var g = Graphics.FromImage(bmp))
            {
                if (RenderMapTemplate != null)
                    g.DrawImage(RenderMapTemplate, new Rectangle(0, 0, Parameters.MapSize.X, Parameters.MapSize.Y));

                Brush polyBrush = new SolidBrush(Color.FromArgb(219, 164, 74));
                if (RenderContinentTemplate != null)
                    polyBrush = new TextureBrush(RenderContinentTemplate);
                

                foreach (var polygon in this)
                {
                    //if (bordersCheckBox.Checked)
                    //{
                    //    foreach (var edge in polygon.Edges)
                    //    {
                    //        g.DrawLine(Pens.White, edge.Start, edge.End);
                    //    }
                    //}
                    //if (_selectedPolygon == polygon)
                    //{
                    //    g.FillPolygon(new SolidBrush(GetPolygonColor(polygon)), polygon.points);
                    //    g.DrawPolygon(new Pen(Color.Black, 2), polygon.points);
                    //    if (bordersCheckBox.Checked)
                    //        g.DrawEllipse(Pens.Green, polygon.Center.X - 2, polygon.Center.Y - 2, 4, 4);
                    //}
                    //else
                    {
                        //Color col = GetPolygonColor(polygon);

                        try
                        {
                            //if (!polygon.Ocean)
                            //    g.FillPolygon(new SolidBrush(col), polygon.points);
                            //else
                            //    g.FillPolygon(new SolidBrush(Color.SeaGreen), polygon.points);

                            //if (polygon.Coast)
                            //{
                            //    var brush = new LinearGradientBrush(

                            //    g.FillPolygon(polyBrush, polygon.points);
                            //}

                            //if (polygon.Elevation > 127) // if (!polygon.Ocean && polygon.Elevation > 127)
                            //    g.FillPolygon(polyBrush, polygon.points);

                            //if (RenderWavePatterns != null)
                            //{
                            //    if (polygon.Ocean && !polygon.Coast && !polygon.Neighbors.Exists(p => p.HasPattern) && r.NextDouble() > 0.5)
                            //    {
                            //        polygon.HasPattern = true;
                            //        var center = polygon.Center;
                            //        center.Offset(-32, -16);
                            //        g.DrawImage(RenderWavePatterns[r.Next(0,RenderWavePatterns.Length)], new Rectangle(center, new Size(64, 32)));
                            //    }
                            //}

                            //if (RenderForest != null && !polygon.Coast && polygon.Biome == BiomeType.TemperateDeciduousForest || polygon.Biome == BiomeType.TemperateRainForest)
                            //{
                            //                                        var center = polygon.Center;
                            //        center.Offset(-16, -16);
                            //        g.DrawImage(RenderForest, new Rectangle(center, new Size(32, 32)));
                            //}

                            //if (RenderTropicalForest != null && !polygon.Coast && polygon.Biome == BiomeType.TropicalRainForest || polygon.Biome == BiomeType.TropicalRainForest)
                            //{
                            //    var center = polygon.Center;
                            //    center.Offset(-16, -16);
                            //    g.DrawImage(RenderTropicalForest, new Rectangle(center, new Size(32, 32)));
                            //}

                        }
                        catch (OverflowException)
                        {

                        }
                        //if (bordersCheckBox.Checked)
                        //    g.DrawEllipse(Pens.Brown, polygon.Center.X - 2, polygon.Center.Y - 2, 4, 4);
                    }

                }

               

                //foreach (var polygon in this)
                //{
                //    // draw rivers
                //    foreach (var edge in polygon.Edges)
                //    {
                //        if (edge.Polygons[0].Biome != edge.Polygons[1].Biome)
                //        {

                //        }
                //    }
                //}
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var coastPen = new Pen(Brushes.Gray, 1);
                
                foreach (var polygon in this)
                {
                    // draw rivers
                    foreach (var edge in polygon.Edges)
                    {
                        if (edge.WaterFlow > 0)
                        {
                            var strong = (int)Math.Sqrt(edge.WaterFlow);
                            var p = new Pen(Color.FromArgb(83,68,44), strong);
                            //p.EndCap = LineCap.ArrowAnchor;

                            g.DrawCurve(p, edge.Points);
                            //g.DrawLines(p, edge.Points);


                            //g.DrawLine(p,edge.Start, edge.End);
                            //else
                            //    g.DrawLine(p, edge.End, edge.Start);
                        }

                        if (edge.Coast)
                        {
                            g.DrawCurve(coastPen, edge.Points);
                        }

                    }
                }

                //foreach (var continentsPath in continentsPaths)
                //{
                //    var pathBrush = new PathGradientBrush(continentsPath);
                //    pathBrush.CenterColor = Color.Transparent;
                //    pathBrush.SurroundColors = new[] { Color.FromArgb(82,65,44) };
                //    pathBrush.FocusScales = new PointF(0.985f, 0.985f);
                //    g.FillPath(pathBrush, continentsPath);
                //}



            }
            return bmp;
        }


    }
}