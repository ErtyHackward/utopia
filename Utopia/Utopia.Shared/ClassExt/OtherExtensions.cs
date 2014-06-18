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
        /// Creates new bounding box with given offset
        /// </summary>
        /// <param name="box"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static BoundingBox Offset(this BoundingBox box, Vector3 offset)
        {
            return new BoundingBox(box.Minimum + offset, box.Maximum + offset);
        }

        /// <summary>
        /// Returns the volume of the bounding box
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static float GetVolume(this BoundingBox box)
        {
            var size = box.GetSize();
            return size.X * size.Y * size.Z;
        }
        
        /// <summary>
        /// Calculates normal vector from given surface point
        /// </summary>
        /// <param name="box"></param>
        /// <param name="surfacePoint"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static Vector3I GetPointNormal(this BoundingBox box, Vector3 surfacePoint, float epsilon = 0.001f)
        {
            var normal = new Vector3I();

            if (Math.Abs(surfacePoint.X - box.Minimum.X) < epsilon)
            {
                normal.X = -1;
            }
            if (Math.Abs(surfacePoint.X - box.Maximum.X) < epsilon)
            {
                normal.X = 1;
            }

            if (Math.Abs(surfacePoint.Y - box.Minimum.Y) < epsilon)
            {
                normal.Y = -1;
            }
            if (Math.Abs(surfacePoint.Y - box.Maximum.Y) < epsilon)
            {
                normal.Y = 1;
            }

            if (Math.Abs(surfacePoint.Z - box.Minimum.Z) < epsilon)
            {
                normal.Z = -1;
            }
            if (Math.Abs(surfacePoint.Z - box.Maximum.Z) < epsilon)
            {
                normal.Z = 1;
            }

            return normal;
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

        /// <summary>
        /// Transforms a ray using a matrix
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Ray Transform(this Ray ray, Matrix transform)
        {
            Vector3.TransformCoordinate(ref ray.Position, ref transform, out ray.Position);
            Vector3.TransformNormal(ref ray.Direction, ref transform, out ray.Direction);
            return ray;
        }

        public static bool IsRelatives(this Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsAssignableFrom(potentialBase) || potentialBase.IsAssignableFrom(potentialDescendant);
        }

    }
}
