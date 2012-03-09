using SharpDX;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Shared.ClassExt
{
    public static class RectangleExtensions
    {
        /// <summary>
        /// Determines does this rectangle contains a point
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="vect"></param>
        /// <returns></returns>
        public static bool Contains(this Rectangle rect, Vector2I vect)
        {
            return rect.Left <= vect.X && rect.Top <= vect.Y && rect.Right > vect.X && rect.Bottom > vect.Y;
        }

        /// <summary>
        /// Determines does this rectangle contains a point
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="vect"></param>
        /// <returns></returns>
        public static bool Contains(this Rectangle rect, Vector3 vect)
        {
            return rect.Left <= vect.X && rect.Top <= vect.Z && rect.Right > vect.X && rect.Bottom > vect.Z;
        }
    }
}