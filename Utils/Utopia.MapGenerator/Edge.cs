using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;
using BenTools.Mathematics;

namespace Utopia.MapGenerator
{
    [Serializable]
    public class Edge
    {
        [XmlIgnore]
        public Point Start;
        [XmlIgnore]
        public Point End;

        [XmlIgnore]
        public Corner StartCorner;
        [XmlIgnore]
        public Corner EndCorner;

        private Point[] _points;
        public Point[] Points
        {
            get { return _points; }
            set { 
                _points = value;
                Start = _points[0];
                End = _points[_points.Length - 1];
            }
        }

        [XmlIgnore]
        public List<Polygon> Polygons { get; set; }

        [XmlAttribute]
        public int WaterFlow;

        public Edge()
        {
            
        }

        public Edge(Vector v1, Vector v2)
        {
            Polygons = new List<Polygon>();
            WaterFlow = 0;
            Start = new Point((int)v1[0], (int)v1[1]);
            End = new Point((int)v2[0], (int)v2[1]);
            StartCorner = null;
            EndCorner = null;
            Points = new[] { Start, End };
        }

        public void Split(Random r)
        {
            var newPoints = new Point[Points.Length * 2 - 1];

            for (int i = 0; i < Points.Length - 1; i++)
            {
                var p1 = Points[i];
                var p2 = Points[i + 1];

                var l = new Line(p1,p2);

                var middlePoint = Line.MiddlePoint(p1, p2);
                var pl = l.PerpendicularLine(middlePoint);
                var len = (int)Line.Lenght(p1, p2) / 3;
                if (len <= 0) len = 1;


                newPoints[i * 2] = Points[i];
                newPoints[i * 2 + 1] = pl.GetRemotePoint(middlePoint, (float)r.Next(-len, len), r.NextDouble() > 0.5);
            }
            newPoints[newPoints.Length - 1] = Points[Points.Length - 1];
            Points = newPoints;
        }

        public Corner GetOpposite(Corner c)
        {
            if (c == StartCorner)
                return EndCorner;
            return StartCorner;
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() + End.GetHashCode() << 8;
        }

        public void AddPolygon(Polygon p2)
        {
            if(!Polygons.Contains(p2))
                Polygons.Add(p2);
        }
    }
}