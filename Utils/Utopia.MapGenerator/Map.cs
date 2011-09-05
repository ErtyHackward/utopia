using System;
using System.Collections.Generic;
using System.Drawing;
using BenTools.Mathematics;

namespace Utopia.MapGenerator
{
    public class Map : IEnumerable<Polygon>
    {
        private Dictionary<Point, Polygon> _polygons = new Dictionary<Point, Polygon>();

        private Dictionary<Point, Corner> _corners = new Dictionary<Point, Corner>(); 

        public Dictionary<Point, Polygon> Polygons
        {
            get { return _polygons; }
        }

        public Corner GetCorner(Point p)
        {
            if (!_corners.ContainsKey(p))
                _corners.Add(p, new Corner(p));
            return _corners[p];
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

                    var c1 = GetCorner(edge.VVertexA.ToPoint());
                    var c2 = GetCorner(edge.VVertexB.ToPoint());

                    c1.AddPolygon(p1);
                    c2.AddPolygon(p1);
                    c1.AddPolygon(p2);
                    c2.AddPolygon(p2);
                    c1.AddEdge(e);
                    c2.AddEdge(e);

                    e.StartCorner = c1;
                    e.EndCorner = c2;

                    p1.AddCorner(c1);
                    p1.AddCorner(c2);
                    p2.AddCorner(c1);
                    p2.AddCorner(c2);

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
}