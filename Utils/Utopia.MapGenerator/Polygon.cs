using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Utopia.MapGenerator
{
    public class Polygon
    {
        public Point Center { get; set; }

        public Point[] points;

        public int Elevation { get; set; }

        public int Moisture { get; set; }

        public List<Polygon> Neighbors = new List<Polygon>();

        public List<Corner> Corners = new List<Corner>();

        public void AddNeighbor(Polygon p)
        {
            if(!Neighbors.Contains(p) && p != this)
                Neighbors.Add(p);
        }

        public void AddCorner(Corner p)
        {
            if (!Corners.Contains(p))
                Corners.Add(p);
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