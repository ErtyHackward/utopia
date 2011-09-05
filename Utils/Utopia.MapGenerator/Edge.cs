using System.Drawing;
using BenTools.Mathematics;

namespace Utopia.MapGenerator
{
    public class Edge
    {
        public readonly Point Start;
        public readonly Point End;

        public Corner StartCorner;
        public Corner EndCorner;

        public int WaterFlow;

        public Edge(Vector v1, Vector v2)
        {
            WaterFlow = 0;
            Start = new Point((int)v1[0], (int)v1[1]);
            End = new Point((int)v2[0], (int)v2[1]);
            StartCorner = null;
            EndCorner = null;

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
    }
}