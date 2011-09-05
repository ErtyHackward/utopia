using System.Collections.Generic;
using System.Drawing;

namespace Utopia.MapGenerator
{
    public class Corner
    {
        public Corner(Point p)
        {
            Point = p;
        }

        public int WaterFlow { get; set; }

        public int Elevation { get; set; }
        
        public Point Point { get; set; }

        public List<Polygon> Polygons = new List<Polygon>();

        public List<Edge> Edges = new List<Edge>();

        public void AddEdge(Edge e)
        {
            if(!Edges.Contains(e))
                Edges.Add(e);
        }

        public void AddPolygon(Polygon e)
        {
            if (!Polygons.Contains(e))
                Polygons.Add(e);
        }

        public static bool operator ==(Corner c1, Corner c2)
        {
            if (ReferenceEquals(c1, c2)) return true;
            if (ReferenceEquals(c1, null)) return false;
            if (ReferenceEquals(c2, null)) return false;
            return c1.Point == c2.Point;
        }

        public static bool operator !=(Corner c1, Corner c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Corner)) return false;
            return Equals((Corner) obj);
        }

        public override string ToString()
        {
            return string.Format("Corner [{0}]", Point.ToString());
        }

        public bool Equals(Corner other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Point.Equals(Point);
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode();
        }
    }
}