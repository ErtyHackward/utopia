using System;
using System.Drawing;

namespace Utopia.Shared.Structs
{
    ///<summary>
    /// Helper class contaning geometry of line
    ///</summary>
    public struct Line
    {
        private float a;
        private float b;
        private float c;
        public float A
        {
            get
            {
                return a;
            }
            set
            {
                a = value;
            }
        }
        public float B
        {
            get
            {
                return b;
            }
            set
            {
                b = value;
            }
        }
        public float C
        {
            get
            {
                return c;
            }
            set
            {
                c = value;
            }
        }
        public float Slope
        {
            get
            {
                return -a / b;
            }
        }
        public float ATanSlope
        {
            get
            {
                return (float)Math.Atan(Slope);
            }
        }
        public Point GetRemotePoint(Point p, float distance, bool direction)
        {
            //if (!IsOnLine(p)) return Point.Empty;
            if (distance == 0) return p;
            Point ret = new Point();
            if (direction)
            {
                if (a < 1 && a>-1)
                {
                    ret.X = p.X + (int)Math.Sqrt((b * b * distance * distance) / (b * b + a * a));
                    ret.Y = (int)GetYFromX(ret.X);
                }
                else
                {
                    ret.Y = p.Y + (int)Math.Sqrt((a * a * distance * distance) / (b * b + a * a));
                    ret.X = (int)GetXFromY(ret.Y);
                }
            }
            else
            {
                if (a < 1 && a>-1)
                {
                    ret.X = p.X - (int)Math.Sqrt((b * b * distance * distance) / (b * b + a * a));
                    ret.Y = (int)GetYFromX(ret.X);
                }
                else
                {
                    ret.Y = p.Y - (int)Math.Sqrt((a * a * distance * distance) / (b * b + a * a));
                    ret.X = (int)GetXFromY(ret.Y);
                }
            }
            return ret;

        }
        public float GetXFromY(float Y)
        {
            if (a == 0) return -c;
            return -(b * Y + c) / a;
        }
        public float GetYFromX(float X)
        {
            if (b == 0) return -c;
            return -(a * X + c) / b;
        }
        public Line(Point p1, Point p2)
        {
            if (p1.X == p2.X)
            {
                a = 1f;
                b = 0;
                c = -p1.X;
            }
            else
            {
                a = (float)(p2.Y - p1.Y) / (float)(p1.X - p2.X);
                b = 1f;
                c = -a * (float)p1.X - b * (float)p1.Y;
            }
        }
        public Line PerpendicularLine(Point p)
        {
            return PerpendicularLine(this, p);
        }
        public bool IsOnLine(Point p)
        {
            return a * (float)p.X + b * (float)p.Y + c == 0;
        }
        public static Line PerpendicularLine(Line l,Point p)
        {
            l.ToShortForm();
            if (l.b == 0)
            {
                l.a = 0;
                l.b = 1f;
            }
            else if (l.a == 0)
            {
                l.a = 1f;
                l.b = 0;
            }
            else
            {
                l.a = -1f / l.a;
                l.b = 1f;
            }
            l.c = -l.a * (float)p.X - l.b * (float)p.Y;
            return l;
        }
        public static Line ToShortForm(Line l)
        {
            if (l.b == 0) return l;
            l.a = l.a / l.b;
            l.c = l.c / l.b;
            l.b = l.b / l.b;
            return l;
        }
        public static Line FromPoints(Point p1, Point p2)
        {
            return new Line(p1, p2);
        }
        public void ToShortForm()
        {
            if (b == 0) return;
            a /= b;
            c /= b;
            b /= b;
        }
        public override string ToString()
        {
            return string.Format("Line A={0}, B={1}, C={2}, Slope={3}, Atan={4};", a, b, c,Slope,ATanSlope);
        }
        public void Draw(Graphics g)
        {
            if (a == 0)
            {
                Point p1 = new Point((int)this.GetXFromY(-1000), -1000);
                Point p2 = new Point((int)this.GetXFromY(1000), 1000);
                //if (p1.Y < -5000) p1.Y = -5000;
                //if (p2.Y < -5000) p2.Y = -5000;
                g.DrawLine(Pens.LightBlue, p1, p2);
            }
            else
            {
                Point p1 = new Point(-1000, (int)this.GetYFromX(-1000));
                Point p2 = new Point(1000, (int)this.GetYFromX(1000));
                if (p1.Y < -50000) p1.Y = -50000;
                if (p2.Y < -50000) p2.Y = -50000;

                g.DrawLine(Pens.LightBlue, p1, p2);
            }
            
        }
        public static double Lenght(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
        public static Point MiddlePoint(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }
        public static bool HasIntersection(Line l1, Line l2)
        {
            if (l1.Slope != l2.Slope) return true;
            else return false;
        }
        public static Point GetIntersectionPoint(Line l1, Line l2)
        {
            int x,y;

            float d = l1.A * l2.B - l2.A * l1.B;
            float dx = l2.C * l1.B - l1.C * l2.B;
            float dy = l2.A * l1.C - l1.A * l2.C;
            x = (int)(dx / d);
            y = (int)(dy / d);
            //  x = (int)((s2 - s1) / (l2.C - l1.C));
            // y = (int)l1.GetYFromX(x);

            return new Point(x, y);
        }
        public static float SlopeBetween(Line l1, Line l2)
        {
            return (l2.Slope - l1.Slope) / (1 + l1.Slope * l2.Slope);
        }
        public static int SlopeToDegree(float slope)
        {
            return (int)(slope * 90);
        }
        public static int SlopeToDegree(Line l)
        {
            return SlopeToDegree(l.Slope);
        }



    }
}