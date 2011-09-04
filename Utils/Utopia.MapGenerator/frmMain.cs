using System;
using System.Collections.Generic;
using System.Drawing;
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

        private Color GetColorFromElevation(int elevation)
        {
            Color col;
            if (elevation > 127)
            {
                //earth
                col = Color.FromArgb((byte)(100 - elevation), (byte)(80 - elevation), (byte)(50 - elevation));
            }
            else col = Color.FromArgb((byte)elevation, (byte)elevation, (byte)255);
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
                        g.FillPolygon(new SolidBrush(GetColorFromElevation(polygon.Elevation)), polygon.points);
                        g.DrawPolygon(new Pen(Color.Black,2), polygon.points);
                        if(bordersCheckBox.Checked)
                            g.DrawEllipse(Pens.Green, polygon.Center.X - 2, polygon.Center.Y - 2, 4, 4);
                    }
                    else
                    {
                        Color col = GetColorFromElevation(polygon.Elevation);

                        //if (_selectedPolygon != null && _selectedPolygon.Neighbors.Contains(polygon))
                        //{
                        //    g.FillPolygon(Brushes.Coral, polygon.points);
                        //}
                        //else 
                        g.FillPolygon(new SolidBrush(col), polygon.points); 

                        if (bordersCheckBox.Checked)
                            g.DrawEllipse(Pens.Brown, polygon.Center.X - 2, polygon.Center.Y - 2, 4, 4);    
                    }
                    
                }
            }
            
            pictureBox1.Image = bmp;
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

                SimplexNoise noise = new SimplexNoise(new Random((int)noiseSeedNumeric.Value));
                noise.SetParameters((double)noiseZoomNumeric.Value, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne );

                foreach (var poly in _map)
                {
                    var noiseVal = noise.GetNoise2DValue(poly.Center.X, poly.Center.Y, 4, 0.8);
                    var col = 255 / noiseVal.MaxValue * noiseVal.Value;
                    poly.Elevation = (int)col;
                }
                
            }

            if (centerElevationCheck.Checked)
            {
                var poly = _map.GetAtPoint(new Point(pictureBox1.Width / 2, pictureBox1.Height / 2));
                poly.Elevation = 200;
                StartPropagation(poly, 15);
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

            DrawGraph(_map);
        }
        private Dictionary<Point,int> _propagationLayers = new Dictionary<Point, int>();

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
            _selectedPolygon = _map.GetAtPoint(e.Location);
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
                    var val = noise.GetNoise2DValue(x, y, 4, 0.8);

                    var col = 255 / val.MaxValue * val.Value;

                    //bmp.SetPixel(x, y, Color.FromArgb((byte)col, (byte)col, (byte)col));

                    if (col > 127)
                    {
                        //earth
                        bmp.SetPixel(x, y, Color.FromArgb((byte)(100-col), (byte)(80-col), (byte)(50-col)));
                    }
                    else bmp.SetPixel(x, y, Color.FromArgb((byte)col, (byte)col, (byte)255));

                }
            }
            
            pictureBox1.Image = bmp;

        }


    }

    public class Map : IEnumerable<Polygon>
    {
        private Dictionary<Point, Polygon> _polygons = new Dictionary<Point, Polygon>();

        public Dictionary<Point, Polygon> Polygons
        {
            get { return _polygons; }
        }

        public void FillMap(VoronoiGraph graph)
        {
            foreach (VoronoiEdge edge in graph.Edges)
            {
                if (!double.IsNaN(edge.VVertexA[0]) && !double.IsNaN(edge.VVertexA[1]) && !double.IsNaN(edge.VVertexB[0]) && !double.IsNaN(edge.VVertexB[1]))
                {
                    var e = new Edge(edge.VVertexA, edge.VVertexB);
                    var p1 = AddEdge(new Point((int)edge.LeftData[0], (int)edge.LeftData[1]), e);
                    var p2 = AddEdge(new Point((int)edge.RightData[0], (int)edge.RightData[1]), e);
                    p1.AddNeighbor(p2);
                    p2.AddNeighbor(p1);
                }
            }

            GetPoints();
        }

        private Polygon AddEdge(Point center, Edge edge)
        {
            Polygon p = null;
            if (!_polygons.ContainsKey(center))
            {
                _polygons.Add(center, p = new Polygon() { Center = center });
            }
            else p = _polygons[center];

            _polygons[center].Edges.Add(edge);
            return p;
        }

        public void GetPoints()
        {
            foreach (var polygon in Polygons)
            {
                polygon.Value.GetPoints();
            }
        }

        public Polygon GetAtPoint(Point p)
        {
            Polygon selected = null;
            double distance = 0;
            foreach (var poly in _polygons.Values)
            {
                if (selected == null)
                {
                    selected = poly;
                    distance = Distance(p, poly.Center);
                    continue;
                }

                var d = Distance(p, poly.Center);

                if (d < distance)
                {
                    selected = poly;
                    distance = d;
                }
            }
            return selected;
        }

        private double Distance(Point p1, Point p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public IEnumerator<Polygon> GetEnumerator()
        {
            foreach (var polygon in _polygons)
            {
                yield return polygon.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var polygon in _polygons)
            {
                yield return polygon.Value;
            }
        }
    }

    public struct Edge
    {
        public readonly Point Start;
        public readonly Point End;

        public Edge(Vector v1, Vector v2)
        {
            Start = new Point((int)v1[0], (int)v1[1]);
            End = new Point((int)v2[0], (int)v2[1]);
        }



        public override int GetHashCode()
        {
            return Start.GetHashCode() + End.GetHashCode() << 8;
        }
    }

    public class Polygon
    {
        public Point Center { get; set; }

        public Point[] points;

        public int Elevation { get; set; }

        public int Moisture { get; set; }

        public List<Polygon> Neighbors = new List<Polygon>();

        public void AddNeighbor(Polygon p)
        {
            if(!Neighbors.Contains(p) && p != this)
                Neighbors.Add(p);
        }

        public Polygon()
        {
            Edges = new HashSet<Edge>();
        }

        public HashSet<Edge> Edges { get; set; }

        public void GetPoints()
        {
            var _points = new List<Point>(Edges.Count);

            var tmpSet = new HashSet<Edge>(Edges);

            var edge = tmpSet.First();

            _points.Add(edge.Start);
            Point lastPoint = edge.End;
            tmpSet.Remove(edge);
            while (tmpSet.Count > 0)
            {
                Point point = lastPoint;
                var en = tmpSet.Where(e => e.Start == point || e.End == point);

                if (en.Count() > 0)
                {
                    edge = en.First();

                    if (edge.Start == point)
                    {
                        lastPoint = edge.End;
                        _points.Add(edge.Start);
                    }
                    else
                    {
                        lastPoint = edge.Start;
                        _points.Add(edge.End);
                    }
                }
                else
                {
                    break;
                }

                tmpSet.Remove(edge);
            }

            points = _points.ToArray();
        }

        public override int GetHashCode()
        {
            return Center.GetHashCode();
        }
    }
}
