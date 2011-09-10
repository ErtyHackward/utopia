using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace Utopia.MapGenerator
{
    [Serializable]
    public class Polygon
    {
        public Point Center { get; set; }
        [XmlIgnore]
        public Point[] points;
        [XmlAttribute]
        public int Elevation { get; set; }
        [XmlAttribute]
        public int Moisture { get; set; }
        [XmlIgnore]
        public List<Polygon> Neighbors = new List<Polygon>();
        [XmlIgnore]
        public List<Corner> Corners = new List<Corner>();
        [XmlAttribute]
        public bool Ocean { get; set; }

        public BiomeType Biome { get; set; }

        public HashSet<Edge> Edges { get; set; }

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
        
        

        public void GetPoints()
        {
            var _points = new List<Point>(Edges.Count);

            var tmpSet = new HashSet<Edge>(Edges);

            var edge = tmpSet.First();

            _points.AddRange(edge.Points);
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
                        //_points.Add(edge.Start);
                        _points.AddRange(edge.Points);
                    }
                    else
                    {
                        lastPoint = edge.Start;
                        //_points.Add(edge.End);
                        _points.AddRange(edge.Points.Reverse());
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