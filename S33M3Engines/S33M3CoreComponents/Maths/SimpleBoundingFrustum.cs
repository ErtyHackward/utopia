using System;
using SharpDX;

/// <summary>
/// Represents a bounding frustum in three dimensional space.
/// </summary>
[Serializable]
public class SimpleBoundingFrustum
{
    Plane near;
    Plane far;
    Plane top;
    Plane bottom;
    Plane left;
    Plane right;

    /// <summary>
    /// Initializes a new instance of the BoundingFrustum class.
    /// </summary>
    /// <param name="value">The Matrix to extract the planes from.</param>
    public SimpleBoundingFrustum(ref Matrix value)
    {
        SetMatrix(ref value);
    }

    /// <summary>
    /// Sets the matrix that represents this instance of BoundingFrustum.
    /// </summary>
    /// <param name="value">The Matrix to extract the planes from.</param>
    public void SetMatrix(ref Matrix value)
    {
        //Near
        near.Normal.X = value.M13;
        near.Normal.Y = value.M23;
        near.Normal.Z = value.M33;
        near.D = value.M43;

        //Far
        far.Normal.X = value.M14 - value.M13;
        far.Normal.Y = value.M24 - value.M23;
        far.Normal.Z = value.M34 - value.M33;
        far.D = value.M44 - value.M43;

        //Top
        top.Normal.X = value.M14 - value.M12;
        top.Normal.Y = value.M24 - value.M22;
        top.Normal.Z = value.M34 - value.M32;
        top.D = value.M44 - value.M42;

        //Bottom
        bottom.Normal.X = value.M14 + value.M12;
        bottom.Normal.Y = value.M24 + value.M22;
        bottom.Normal.Z = value.M34 + value.M32;
        bottom.D = value.M44 + value.M42;

        //Left
        left.Normal.X = value.M14 + value.M11;
        left.Normal.Y = value.M24 + value.M21;
        left.Normal.Z = value.M34 + value.M31;
        left.D = value.M44 + value.M41;

        //Right
        right.Normal.X = value.M14 - value.M11;
        right.Normal.Y = value.M24 - value.M21;
        right.Normal.Z = value.M34 - value.M31;
        right.D = value.M44 - value.M41;
    }

    public bool Intersects(BoundingBox box)
    {

        return near.Intersects(ref box) != PlaneIntersectionType.Back &&
               far.Intersects(ref box) != PlaneIntersectionType.Back &&
               top.Intersects(ref box) != PlaneIntersectionType.Back &&
               bottom.Intersects(ref box) != PlaneIntersectionType.Back &&
               left.Intersects(ref box) != PlaneIntersectionType.Back &&
               right.Intersects(ref box) != PlaneIntersectionType.Back;
    }
}