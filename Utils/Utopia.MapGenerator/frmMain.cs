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

                foreach (var polygon in map)
                {
                    // draw rivers
                    foreach (var edge in polygon.Edges)
                    {
                        if (edge.WaterFlow > 0)
                        {
                            g.DrawLine(new Pen(Color.Blue, 1 /*edge.WaterFlow*/),edge.Start, edge.End);
                        }
                    }
                }

                foreach (var corners in _corners)
                {
                    g.DrawEllipse(Pens.Black, corners.Point.X - 2, corners.Point.Y - 2, 4, 4);
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

                var elevationNoise = new SimplexNoise(new Random((int)noiseSeedNumeric.Value));
                elevationNoise.SetParameters((double)noiseZoomNumeric.Value, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne );

                foreach (var poly in _map)
                {
                    var noiseVal = elevationNoise.GetNoise2DValue(poly.Center.X, poly.Center.Y, 4, 0.8);
                    var col = 255 / noiseVal.MaxValue * noiseVal.Value;
                    poly.Elevation = (int)col;
                }

                // elevate each corner

                foreach (var poly in _map)
                {
                    foreach (var corner in poly.Corners)
                    {
                        if (corner.Elevation == 0)
                        {
                            corner.Elevation = (int) corner.Polygons.Average(p => p.Elevation);
                        }
                    }
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

            if (moisturizeCheck.Checked)
            {
                var noise = new SimplexNoise(new Random((int)noiseSeedNumeric.Value));
                noise.SetParameters((double)0.0002, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

                foreach (var poly in _map)
                {
                    var noiseVal = noise.GetNoise2DValue(poly.Center.X, poly.Center.Y, 4, 0.8);
                    var col = 255 / noiseVal.MaxValue * noiseVal.Value;
                    poly.Moisture = 127;// (int)col;

                    foreach (var corner in poly.Corners)
                    {
                        noiseVal = noise.GetNoise2DValue(corner.Point.X, corner.Point.Y, 4, 0.8);
                        col = 255 / noiseVal.MaxValue * noiseVal.Value;
                        corner.WaterFlow = 127;// (int)col;
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

                    // get water flow
                    var en = corner.Polygons.Where(p => p.Elevation >= corner.Elevation);
                    if (en.Count() > 0)
                    {
                        lowestEdge.WaterFlow = (int)en.Average(p => p.Moisture);
                        lowestEdge.GetOpposite(corner).WaterFlow += lowestEdge.WaterFlow;
                    }
                }

            }

            DrawGraph(_map);
        }
        private Dictionary<Point,int> _propagationLayers = new Dictionary<Point, int>();
        private HashSet<Corner> _corners;

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

        private void button2_Click_1(object sender, EventArgs e)
        {
            var r = new Random((int)noiseSeedNumeric.Value);
            var noise = new SimplexNoise(r);
            noise.SetParameters((double)0.0002, SimplexNoise.InflectionMode.NoInflections, SimplexNoise.ResultScale.ZeroToOne);

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    var val = noise.GetNoise2DValue(x, y, 4, 0.8);

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
