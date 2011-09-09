using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BenTools.Mathematics;
using S33M3Engines.Shared.Math.Noises;

namespace Utopia.MapGenerator
{
    public partial class frmMain : Form
    {
        private List<Vector> _datapoints;

        private Map _map;

        private Polygon _selectedPolygon;

        private Biome _biome = new Biome(new ParameterVariation(127,255), new ParameterVariation(0,150));

        public frmMain()
        {
            InitializeComponent();

            
        }

        private void Relax(VoronoiGraph graph, List<Vector> vectors )
        {
            foreach (var vector in vectors)
            {
                var v0 = 0.0d;
                var v1 = 0.0d;
                var edgesCount = 0;

                foreach (VoronoiEdge edge in graph.Edges)
                {
                    if (edge.LeftData == vector || edge.RightData == vector)
                    {
                        var p0 = (edge.VVertexA[0] + edge.VVertexB[0])/2;
                        var p1 = (edge.VVertexA[1] + edge.VVertexB[1])/2;
                        v0 += double.IsNaN(p0) ? 0 : p0;
                        v1 += double.IsNaN(p1) ? 0 : p1;
                        edgesCount++;
                    }
                }
                
                vector[0] = v0 / edgesCount;
                vector[1] = v1 / edgesCount;
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
                    col = Color.FromArgb((byte) (2 + polygon.Elevation), (byte) (108 + polygon.Elevation),
                                         (byte) (85 + polygon.Elevation));
                }
                else
                {
                    col = Color.FromArgb((byte)(polygon.Elevation), (byte)(polygon.Elevation), (byte)(255));
                }
            }


            return col;
        }

        private void DrawGraph(Map map)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);


            using (var g = Graphics.FromImage(bmp))
            {
                Random r = new Random();
                foreach (var polygon in map)
                {
                    if (bordersCheckBox.Checked)
                    {
                        foreach (var edge in polygon.Edges)
                        {
                            g.DrawLine(Pens.White, edge.Start, edge.End);
                        }
                    }
                    if (_selectedPolygon == polygon)
                    {
                        g.FillPolygon(new SolidBrush(GetPolygonColor(polygon)), polygon.points);
                        g.DrawPolygon(new Pen(Color.Black,2), polygon.points);
                        if(bordersCheckBox.Checked)
                            g.DrawEllipse(Pens.Green, polygon.Center.X - 2, polygon.Center.Y - 2, 4, 4);
                    }
                    else
                    {
                        Color col = GetPolygonColor(polygon);

                        g.FillPolygon(new SolidBrush(col), polygon.points); 

                        if (bordersCheckBox.Checked)
                            g.DrawEllipse(Pens.Brown, polygon.Center.X - 2, polygon.Center.Y - 2, 4, 4);    
                    }
                    
                }
                
                foreach (var polygon in map)
                {
                    // draw rivers
                    foreach (var edge in polygon.Edges)
                    {
                        if (edge.WaterFlow > 0)
                        {
                            var strong = (int)Math.Sqrt(edge.WaterFlow);
                            var p = new Pen(Color.Blue, strong);
                            //p.EndCap = LineCap.ArrowAnchor;
                            
                            if(edge.StartCorner.Elevation > edge.EndCorner.Elevation)
                            {
                                g.DrawLines(p,edge.points);
                            }

                            //g.DrawLine(p,edge.Start, edge.End);
                            //else
                            //    g.DrawLine(p, edge.End, edge.Start);
                        }
                    }
                }

            }
            
            pictureBox1.Image = bmp;
        }

        private void ElevateCorners()
        {
            foreach (var poly in _map)
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

        private void button1_Click(object sender, EventArgs e)
        {
            _datapoints = new List<Vector>((int)voronoiPolyNumeric.Value);

            Random r = new Random((int)voronoiSeedNumeric.Value);

            for (int i = 0; i < voronoiPolyNumeric.Value; i++)
            {
                _datapoints.Add(new Vector(r.Next(0, pictureBox1.Width), r.Next(0, pictureBox1.Height)));
            }

            var graph = Fortune.ComputeVoronoiGraph(_datapoints);

            for (int i = 0; i < voronoiRelaxNumeric.Value; i++)
            {
                Relax(graph, _datapoints);
                graph = Fortune.ComputeVoronoiGraph(_datapoints);
            }

            _map = new Map();
            _map.FillMap(graph);

            if (elevateCheckBox.Checked)
            {
                // adding elevation data

                var elevationNoise = new SimplexNoise(new Random((int)noiseSeedNumeric.Value));
                elevationNoise.SetParameters((double)noiseZoomNumeric.Value, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

                foreach (var poly in _map)
                {
                    var noiseVal = elevationNoise.GetNoise2DValue(poly.Center.X, poly.Center.Y, 4, 0.8);
                    var col = 255 / noiseVal.MaxValue * noiseVal.Value;
                    poly.Elevation = (int)col;
                }

                // elevate each corner
                if (!centerElevationCheck.Checked)
                    ElevateCorners();

            }

            if (centerElevationCheck.Checked)
            {
                var poly = _map.GetAtPoint(new Point(pictureBox1.Width / 2, pictureBox1.Height / 2));
                poly.Elevation = 200;
                StartPropagation(poly, 15);
                ElevateCorners();
            }

            if (makeIsandcheck.Checked)
            {
                //r = new Random((int)voronoiSeedNumeric.Value);
                var borderElevation = 80;
                var step = 20;
                for (int x = 0; x < pictureBox1.Width; x++)
                {
                    var poly = _map.GetAtPoint(new Point(x, 0));

                    poly.Elevation = borderElevation;// r.Next(0, 100);
                    StartPropagation(poly, step);


                    poly = _map.GetAtPoint(new Point(x, pictureBox1.Height));
                    poly.Elevation = borderElevation;// r.Next(0, 100);
                    StartPropagation(poly, step);

                }

                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    var poly = _map.GetAtPoint(new Point(0, y));

                    poly.Elevation = borderElevation;// r.Next(0, 100);
                    StartPropagation(poly, step);


                    poly = _map.GetAtPoint(new Point(pictureBox1.Width, y));

                    poly.Elevation = borderElevation;// r.Next(0, 100);
                    StartPropagation(poly, step);

                }
            }

            if (moisturizeCheck.Checked)
            {
                var noise = new SimplexNoise(new Random((int)noiseSeedNumeric.Value));
                noise.SetParameters((double)0.0008, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

                foreach (var poly in _map)
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

                foreach (var poly in _map)
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
                _corners = new HashSet<Corner>();

                // get unique corners
                foreach (var poly in _map)
                {
                    foreach (var corner in poly.Corners)
                    {
                        if (!_corners.Contains(corner) && corner.Polygons.Find(p => p.Elevation <= 127) == null)
                        {
                            _corners.Add(corner);
                        }
                    }
                }

                var list = new List<Corner>(_corners);
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

                foreach (var poly in _map)
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
                foreach (var poly in _map)
                {
                    foreach (var edge in poly.Edges)
                    {
                        if (edge.WaterFlow > 0 && !_rivers.Contains(edge))
                            edge.WaterFlow = 0;
                    }
                }

                // update moisture for polygons
                foreach (var poly in _map)
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

            // detect ocean
            {
                SetOcean(_map.GetAtPoint(new Point(0, 0)));
                SetOcean(_map.GetAtPoint(new Point(pictureBox1.Width, 0)));
                SetOcean(_map.GetAtPoint(new Point(0, pictureBox1.Height)));
                SetOcean(_map.GetAtPoint(new Point(pictureBox1.Width, pictureBox1.Height)));
            }

            if (biomesCheck.Checked)
            {
                foreach (var poly in _map)
                {
                    if (poly.Elevation > 127)
                    {
                        poly.Biome = _biome.GetBiomeWith(poly.Elevation, poly.Moisture > _biome.Moisture.Maximum ? _biome.Moisture.Maximum : poly.Moisture);
                    }
                }
            }



            DrawGraph(_map);
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
            int flood = 0;
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
                        var newBranch = new RiverBranch() { Edge = edge, ParentBranch = riverBranch };
                        riverBranch.Branches.Add(newBranch);
                        _rivers.Add(edge);
                        CollectRiver(edge.GetOpposite(c), newBranch);
                    }
                }
            }
        }
        
        private List<RiverBranch> _riverRoots = new List<RiverBranch>(); 
        private HashSet<Edge> _rivers = new HashSet<Edge>(); 
        private Dictionary<Point,int> _propagationLayers = new Dictionary<Point, int>();
        private HashSet<Corner> _corners;
        private List<Corner> _waterCorners;

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
                if(_propagationLayers.ContainsKey(poly.Center))
                {
                    if(_propagationLayers[poly.Center] > token)
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

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // try to find corner
            Rectangle r = new Rectangle(e.X-4,e.Y-4,8,8);
            foreach (var corner in _corners)
            {
                if (r.Contains(corner.Point))
                {
                    label6.Text = string.Format("Corner {0} Elevation: {1} Moisture: {2}", corner.Point.ToString(), corner.Elevation, corner.WaterFlow);
                    return;
                }
            }


            _selectedPolygon = _map.GetAtPoint(e.Location);
            label6.Text = string.Format("Polygon {0} Elevation: {1} Moisture: {2} Biome: {3}", _selectedPolygon.Center.ToString(), _selectedPolygon.Elevation, _selectedPolygon.Moisture, _selectedPolygon.Biome);
            DrawGraph(_map);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var r = new Random((int)noiseSeedNumeric.Value);
            var noise = new SimplexNoise(r);
            noise.SetParameters((double)noiseZoomNumeric.Value, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne );

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    var val = noise.GetNoise2DValue(x, y, (int)octavesNumeric.Value, (double)persistanceNumeric.Value);

                    var col = 255 / val.MaxValue * val.Value - (255/2) ;

                    var minVal = 255 * (double)x / pictureBox1.Width;

                    col = col + minVal;
                    if (col > 255) col = 255;
                    if (col < 0) col = 0;

                    //bmp.SetPixel(x, y, Color.FromArgb((byte)col, (byte)col, (byte)col));

                    if (col > 127)
                    {
                        //earth
                        bmp.SetPixel(x, y, Color.White); // Color.FromArgb((byte)(100-col), (byte)(80-col), (byte)(50-col)));
                    }
                    else bmp.SetPixel(x, y, Color.Black); // Color.FromArgb((byte)col, (byte)col, (byte)255));

                }
            }

            var xi = pictureBox1.Width / 2;
            var yi = 0;
            // find point
            for (yi = 0; yi < pictureBox1.Height; yi++)
            {
                var val = bmp.GetPixel(xi, yi).R;
                if (val > 127)
                {
                    
                    xi--;
                }
                else
                {

                    break;
                }
            }

            if (xi != pictureBox1.Width / 2)
            {
                for (int y = 0; y < yi; y++)
                {
                    MakeWhite(bmp, pictureBox1.Width / 2, y);
                }
            }



            pictureBox1.Image = bmp;

        }

        private void MakeWhite(Bitmap bmp, int  x, int y)
        {
            for (int i = 0; i < x; i++)
            {
                bmp.SetPixel(i,y, Color.Black);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            var r = new Random((int)noiseSeedNumeric.Value);
            var noise = new SimplexNoise(r);
            noise.SetParameters((double)noiseZoomNumeric.Value, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    var val = noise.GetNoise2DValue(x, y,(int) octavesNumeric.Value, (double)persistanceNumeric.Value);

                    var col = 255 / val.MaxValue * val.Value;

                    //bmp.SetPixel(x, y, Color.FromArgb((byte)col, (byte)col, (byte)col));

                    //if (col > 127)
                    //{
                    //    //earth
                    //    bmp.SetPixel(x, y, Color.FromArgb((byte)(100 - col), (byte)(80 - col), (byte)(50 - col)));
                    //}
                    //else 
                        
                        bmp.SetPixel(x, y, Color.FromArgb((byte)col, (byte)col, (byte)255));

                }
            }

            pictureBox1.Image = bmp;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GraphicsPath p = new GraphicsPath();

            var pointsCount = 6;

            var startPoint = new Point(pictureBox1.Width / 2, 0);
            var endpoint = new Point(pictureBox1.Width / 2, pictureBox1.Height);

            var points = new Point[pointsCount];

            points[0] = startPoint;
            points[pointsCount - 1] = endpoint;

            var ystep = pictureBox1.Height / (pointsCount);

            Random r = new Random();



            var lastX = pictureBox1.Width / 2 + r.Next(-40, 40);

            for (int i = 1; i < pointsCount-1; i++)
            {
                points[i] = new Point(lastX + r.Next(-40, 40), (i ) * ystep);
            }

            p.AddCurve(points);

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            using (var g = Graphics.FromImage(bmp))
            {
                //g.ScaleTransform(1,-0.8f);
                //g.TranslateTransform(0, + 150);

                g.FillPath(Brushes.Black, p);

                foreach (var point in points)
                {
                    g.DrawEllipse(Pens.Red, point.X - 2, point.Y - 2, 4, 4);
                }

            }
            pictureBox1.Image = bmp;

        }


    }

    public class RiverBranch
    {
        public Edge Edge { get; set; }

        public bool Final
        {
            get { return Branches.Count == 0; }
        }

        public RiverBranch ParentBranch { get; set; }

        public List<RiverBranch> Branches = new List<RiverBranch>();
    }

    public class CornerHeightComparer : IComparer<Corner>
    {
        public int Compare(Corner x, Corner y)
        {
            return -1 * x.Elevation.CompareTo(y.Elevation);
        }
    }

    public static class VectorExtensions
    {
        public static Point ToPoint(this Vector v)
        {
            return new Point((int)v[0], (int)v[1]);
        }
    }
}
