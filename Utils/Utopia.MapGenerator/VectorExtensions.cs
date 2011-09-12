using System.Drawing;
using BenTools.Mathematics;

namespace Utopia.MapGenerator
{
    public static class VectorExtensions
    {
        public static Point ToPoint(this Vector v)
        {
            return new Point((int)v[0], (int)v[1]);
        }
    }
}