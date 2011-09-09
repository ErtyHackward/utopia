using System;
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

        public Point[] points;

        public int WaterFlow;

        public Edge(Vector v1, Vector v2)
        {
            WaterFlow = 0;
            Start = new Point((int)v1[0], (int)v1[1]);
            End = new Point((int)v2[0], (int)v2[1]);
            StartCorner = null;
            EndCorner = null;
            points = new[] { Start, End };
        }

        public void Split(Random r, int move)
        {
            var newPoints = new Point[points.Length * 2 - 1];

            for (int i = 0; i < points.Length - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];

                newPoints[i * 2] = points[i];
                newPoints[i * 2 + 1] = new Point((p1.X + p2.X) / 2 + r.Next(-move, move), (p1.Y + p2.Y) / 2 + r.Next(-move, move));
            }
            newPoints[newPoints.Length - 1] = points[points.Length - 1];
            points = newPoints;
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