using System;
using SharpDX;

namespace S33M3Resources.Structs
{
    public static class OtherExtensions
    {
        /// <summary>
        /// Returns cube position based on entity position
        /// </summary>
        /// <param name="entityPosition"></param>
        /// <returns></returns>
        public static Vector3I ToCubePosition(this Vector3D entityPosition)
        {
            return new Vector3I((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z));
        }

        /// <summary>
        /// Returns cube position based on entity position
        /// </summary>
        /// <param name="entityPosition"></param>
        /// <returns></returns>
        public static Vector3I ToCubePosition(this Vector3 entityPosition)
        {
            return new Vector3I((int)Math.Floor(entityPosition.X), (int)entityPosition.Y, (int)Math.Floor(entityPosition.Z));
        }

        /// <summary>
        /// Gets the center point of the bounding box
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static Vector3 GetCenter(this BoundingBox box)
        {
            return (box.Maximum - box.Minimum) / 2 + box.Minimum;
        }

        /// <summary>
        /// Gets the size of the bounding box
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static Vector3 GetSize(this BoundingBox box)
        {
            return box.Maximum - box.Minimum;
        }

        /// <summary>
        /// Transforms all corners of the bounding box and produces a new box containing all corners
        /// </summary>
        /// <param name="box"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static BoundingBox Transform(this BoundingBox box, Matrix transform)
        {
            var max = Vector3.TransformCoordinate(box.Minimum, transform);
            var min = max;

            foreach (var corner in box.GetCorners())
            {
                var transformedCorner =  Vector3.TransformCoordinate(corner, transform);

                max = Vector3.Max(transformedCorner, max);
                min = Vector3.Min(transformedCorner, min);
            }

            return new BoundingBox(min, max);
        }
    }
}
