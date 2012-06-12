using System;
using SharpDX;

/// <summary>
/// Represents a bounding frustum in three dimensional space.
/// </summary>
[Serializable]
public class SimpleBoundingFrustum
{
    private Plane _near;
    private Plane _far;
    private Plane _top;
    private Plane _bottom;
    private Plane _left;
    private Plane _right;

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
        _near.Normal.X = value.M13;
        _near.Normal.Y = value.M23;
        _near.Normal.Z = value.M33;
        _near.D = value.M43;

        //Far
        _far.Normal.X = value.M14 - value.M13;
        _far.Normal.Y = value.M24 - value.M23;
        _far.Normal.Z = value.M34 - value.M33;
        _far.D = value.M44 - value.M43;

        //Top
        _top.Normal.X = value.M14 - value.M12;
        _top.Normal.Y = value.M24 - value.M22;
        _top.Normal.Z = value.M34 - value.M32;
        _top.D = value.M44 - value.M42;

        //Bottom
        _bottom.Normal.X = value.M14 + value.M12;
        _bottom.Normal.Y = value.M24 + value.M22;
        _bottom.Normal.Z = value.M34 + value.M32;
        _bottom.D = value.M44 + value.M42;

        //Left
        _left.Normal.X = value.M14 + value.M11;
        _left.Normal.Y = value.M24 + value.M21;
        _left.Normal.Z = value.M34 + value.M31;
        _left.D = value.M44 + value.M41;

        //Right
        _right.Normal.X = value.M14 - value.M11;
        _right.Normal.Y = value.M24 - value.M21;
        _right.Normal.Z = value.M34 - value.M31;
        _right.D = value.M44 - value.M41;
    }

    public bool Intersects(ref BoundingBox box)
    {

        return _near.Intersects(ref box) != PlaneIntersectionType.Back &&
               _far.Intersects(ref box) != PlaneIntersectionType.Back &&
               _top.Intersects(ref box) != PlaneIntersectionType.Back &&
               _bottom.Intersects(ref box) != PlaneIntersectionType.Back &&
               _left.Intersects(ref box) != PlaneIntersectionType.Back &&
               _right.Intersects(ref box) != PlaneIntersectionType.Back;
    }

    public bool IntersectsWithoutFar(ref BoundingBox box)
    {

        return _near.Intersects(ref box) != PlaneIntersectionType.Back &&
               _top.Intersects(ref box) != PlaneIntersectionType.Back &&
               _bottom.Intersects(ref box) != PlaneIntersectionType.Back &&
               _left.Intersects(ref box) != PlaneIntersectionType.Back &&
               _right.Intersects(ref box) != PlaneIntersectionType.Back;
    }

    private Vector3 Get3PlanesInterPoint(ref Plane p1, ref Plane p2, ref Plane p3)
    {
        //P = -d1 * N2xN3 / N1.N2xN3 - d2 * N3xN1 / N2.N3xN1 - d3 * N1xN2 / N3.N1xN2 
        Vector3 v =
            -p1.D * Vector3.Cross(p2.Normal, p3.Normal) / Vector3.Dot(p1.Normal, Vector3.Cross(p2.Normal, p3.Normal))
            - p2.D * Vector3.Cross(p3.Normal, p1.Normal) / Vector3.Dot(p2.Normal, Vector3.Cross(p3.Normal, p1.Normal))
            - p3.D * Vector3.Cross(p1.Normal, p2.Normal) / Vector3.Dot(p3.Normal, Vector3.Cross(p1.Normal, p2.Normal));

        return v;
    }

    public Vector3[] GetCorners()
    {
        var corners = new Vector3[8];
        corners[0] = Get3PlanesInterPoint(ref _near, ref  _bottom, ref  _right);    //Near1
        corners[1] = Get3PlanesInterPoint(ref _near, ref  _top, ref  _right);       //Near2
        corners[2] = Get3PlanesInterPoint(ref _near, ref  _top, ref  _left);        //Near3
        corners[3] = Get3PlanesInterPoint(ref _near, ref  _bottom, ref  _left);     //Near3
        corners[4] = Get3PlanesInterPoint(ref _far, ref  _bottom, ref  _right);    //Far1
        corners[5] = Get3PlanesInterPoint(ref _far, ref  _top, ref  _right);       //Far2
        corners[6] = Get3PlanesInterPoint(ref _far, ref  _top, ref  _left);        //Far3
        corners[7] = Get3PlanesInterPoint(ref _far, ref  _bottom, ref  _left);     //Far3
        return corners;
    }
}