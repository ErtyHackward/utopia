using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using SharpDX;
using S33M3Engines.Shared.Math;

namespace S33M3Engines.Maths.Graphics
{
    public class BoundingFrustum : IEquatable<BoundingFrustum>
    {
        // Fields
        private const int BottomPlaneIndex = 5;
        internal Vector3[] cornerArray;
        public const int CornerCount = 8;
        private const int FarPlaneIndex = 1;
        private Gjk gjk;
        private const int LeftPlaneIndex = 2;
        private Matrix matrix;
        private const int NearPlaneIndex = 0;
        private const int NumPlanes = 6;
        private Plane[] planes;
        private const int RightPlaneIndex = 3;
        private const int TopPlaneIndex = 4;

        // Methods
        private BoundingFrustum()
        {
            this.planes = new Plane[6];
            this.cornerArray = new Vector3[8];
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="value">view × projection matrix.</param>
        public BoundingFrustum(Matrix value)
        {
            this.planes = new Plane[6];
            this.cornerArray = new Vector3[8];
            this.SetMatrix(ref value);
        }

        private static Vector3 ComputeIntersection(ref Plane plane, ref Ray ray)
        {
            float num = (-plane.D - Vector3.Dot(plane.Normal, ray.Position)) / Vector3.Dot(plane.Normal, ray.Direction);
            return (ray.Position + ((Vector3)(ray.Direction * num)));
        }

        private static Ray ComputeIntersectionLine(ref Plane p1, ref Plane p2)
        {
            Ray ray = new Ray();
            ray.Direction = Vector3.Cross(p1.Normal, p2.Normal);
            float num = ray.Direction.LengthSquared();
            ray.Position = (Vector3)(Vector3.Cross((Vector3)((-p1.D * p2.Normal) + (p2.D * p1.Normal)), ray.Direction) / num);
            return ray;
        }

        public ContainmentType Contains(BoundingBox box)
        {
            bool flag = false;
            for (int i = 0; i < this.planes.Length; i++)
            {
                switch (box.Intersects(ref this.planes[i]))
                {
                    case PlaneIntersectionType.Front:
                        return ContainmentType.Disjoint;

                    case PlaneIntersectionType.Intersecting:
                        flag = true;
                        break;
                }
            }

            if (!flag)
            {
                return ContainmentType.Contains;
            }
            return ContainmentType.Intersects;
        }

        public ContainmentType Contains(BoundingFrustum frustum)
        {
            if (frustum == null)
            {
                throw new ArgumentNullException("frustum");
            }
            ContainmentType disjoint = ContainmentType.Disjoint;
            if (this.Intersects(frustum))
            {
                disjoint = ContainmentType.Contains;
                for (int i = 0; i < this.cornerArray.Length; i++)
                {
                    if (this.Contains(frustum.cornerArray[i]) == ContainmentType.Disjoint)
                    {
                        return ContainmentType.Intersects;
                    }
                }
            }
            return disjoint;
        }

        public ContainmentType Contains(BoundingSphere sphere)
        {
            Vector3 center = sphere.Center;
            float radius = sphere.Radius;
            int num2 = 0;
            foreach (Plane plane in this.planes)
            {
                float num5 = ((plane.Normal.X * center.X) + (plane.Normal.Y * center.Y)) + (plane.Normal.Z * center.Z);
                float num3 = num5 + plane.D;
                if (num3 > radius)
                {
                    return ContainmentType.Disjoint;
                }
                if (num3 < -radius)
                {
                    num2++;
                }
            }
            if (num2 != 6)
            {
                return ContainmentType.Intersects;
            }
            return ContainmentType.Contains;
        }

        public ContainmentType Contains(Vector3 point)
        {
            foreach (Plane plane in this.planes)
            {
                float num2 = (((plane.Normal.X * point.X) + (plane.Normal.Y * point.Y)) + (plane.Normal.Z * point.Z)) + plane.D;
                if (num2 > 1E-05f)
                {
                    return ContainmentType.Disjoint;
                }
            }
            return ContainmentType.Contains;
        }

        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            bool flag = false;
            for (int i = 0; i < this.planes.Length; i++)
            {
                switch (box.Intersects(ref this.planes[i]))
                {
                    case PlaneIntersectionType.Front:
                        result = ContainmentType.Disjoint;
                        return;

                    case PlaneIntersectionType.Intersecting:
                        flag = true;
                        break;
                }
            }
            result = flag ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        public void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            Vector3 center = sphere.Center;
            float radius = sphere.Radius;
            int num2 = 0;
            foreach (Plane plane in this.planes)
            {
                float num5 = ((plane.Normal.X * center.X) + (plane.Normal.Y * center.Y)) + (plane.Normal.Z * center.Z);
                float num3 = num5 + plane.D;
                if (num3 > radius)
                {
                    result = ContainmentType.Disjoint;
                    return;
                }
                if (num3 < -radius)
                {
                    num2++;
                }
            }
            result = (num2 == 6) ? ContainmentType.Contains : ContainmentType.Intersects;
        }

        public void Contains(ref Vector3 point, out ContainmentType result)
        {
            foreach (Plane plane in this.planes)
            {
                float num2 = (((plane.Normal.X * point.X) + (plane.Normal.Y * point.Y)) + (plane.Normal.Z * point.Z)) + plane.D;
                if (num2 > 1E-05f)
                {
                    result = ContainmentType.Disjoint;
                    return;
                }
            }
            result = ContainmentType.Contains;
        }

        public bool Equals(BoundingFrustum other)
        {
            if (other == null)
            {
                return false;
            }
            return (this.matrix == other.matrix);
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            BoundingFrustum frustum = obj as BoundingFrustum;
            if (frustum != null)
            {
                flag = this.matrix == frustum.matrix;
            }
            return flag;
        }

        public Vector3[] GetCorners()
        {
            return (Vector3[])this.cornerArray.Clone();
        }

        public void GetCorners(Vector3[] corners)
        {
            if (corners == null)
            {
                throw new ArgumentNullException("corners");
            }
            if (corners.Length < 8)
            {
                throw new ArgumentOutOfRangeException("corners", "Not enough Corners");
            }
            this.cornerArray.CopyTo(corners, 0);
        }

        public override int GetHashCode()
        {
            return this.matrix.GetHashCode();
        }

        public bool Intersects(BoundingBox box)
        {
            bool flag;
            this.Intersects(ref box, out flag);
            return flag;
        }

        public bool Intersects(BoundingFrustum frustum)
        {
            Vector3 closestPoint;
            if (frustum == null)
            {
                throw new ArgumentNullException("frustum");
            }
            if (this.gjk == null)
            {
                this.gjk = new Gjk();
            }
            this.gjk.Reset();
            Vector3.Subtract(ref this.cornerArray[0], ref frustum.cornerArray[0], out closestPoint);
            if (closestPoint.LengthSquared() < 1E-05f)
            {
                Vector3.Subtract(ref this.cornerArray[0], ref frustum.cornerArray[1], out closestPoint);
            }
            float maxValue = float.MaxValue;
            float num3 = 0f;
            do
            {
                Vector3 vector2;
                Vector3 vector3;
                Vector3 vector4;
                Vector3 vector5;
                vector5.X = -closestPoint.X;
                vector5.Y = -closestPoint.Y;
                vector5.Z = -closestPoint.Z;
                this.SupportMapping(ref vector5, out vector4);
                frustum.SupportMapping(ref closestPoint, out vector3);
                Vector3.Subtract(ref vector4, ref vector3, out vector2);
                float num4 = ((closestPoint.X * vector2.X) + (closestPoint.Y * vector2.Y)) + (closestPoint.Z * vector2.Z);
                if (num4 > 0f)
                {
                    return false;
                }
                this.gjk.AddSupportPoint(ref vector2);
                closestPoint = this.gjk.ClosestPoint;
                float num2 = maxValue;
                maxValue = closestPoint.LengthSquared();
                num3 = 4E-05f * this.gjk.MaxLengthSquared;
                if ((num2 - maxValue) <= (1E-05f * num2))
                {
                    return false;
                }
            }
            while (!this.gjk.FullSimplex && (maxValue >= num3));
            return true;
        }

        public bool Intersects(BoundingSphere sphere)
        {
            bool flag;
            this.Intersects(ref sphere, out flag);
            return flag;
        }

        public PlaneIntersectionType Intersects(Plane plane)
        {
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                float num3;
                MVector3.Dot(ref this.cornerArray[i], ref plane.Normal, out num3);
                if ((num3 + plane.D) > 0f)
                {
                    num |= 1;
                }
                else
                {
                    num |= 2;
                }
                if (num == 3)
                {
                    return PlaneIntersectionType.Intersecting;
                }
            }
            if (num != 1)
            {
                return PlaneIntersectionType.Back;
            }
            return PlaneIntersectionType.Front;
        }

        public float? Intersects(Ray ray)
        {
            float? nullable;
            this.Intersects(ref ray, out nullable);
            return nullable;
        }

        public void Intersects(ref BoundingBox box, out bool result)
        {
            Vector3 closestPoint;
            Vector3 vector2;
            Vector3 vector3;
            Vector3 vector4;
            Vector3 vector5;
            if (this.gjk == null)
            {
                this.gjk = new Gjk();
            }
            this.gjk.Reset();
            Vector3.Subtract(ref this.cornerArray[0], ref box.Minimum, out closestPoint);
            if (closestPoint.LengthSquared() < 1E-05f)
            {
                Vector3.Subtract(ref this.cornerArray[0], ref box.Maximum, out closestPoint);
            }
            float maxValue = float.MaxValue;
            float num3 = 0f;
            result = false;
        Label_006D:
            vector5.X = -closestPoint.X;
            vector5.Y = -closestPoint.Y;
            vector5.Z = -closestPoint.Z;
            this.SupportMapping(ref vector5, out vector4);
            MBoundingBox.SupportMapping(ref closestPoint, ref box.Minimum, ref box.Maximum, out vector3);
            Vector3.Subtract(ref vector4, ref vector3, out vector2);
            float num4 = ((closestPoint.X * vector2.X) + (closestPoint.Y * vector2.Y)) + (closestPoint.Z * vector2.Z);
            if (num4 <= 0f)
            {
                this.gjk.AddSupportPoint(ref vector2);
                closestPoint = this.gjk.ClosestPoint;
                float num2 = maxValue;
                maxValue = closestPoint.LengthSquared();
                if ((num2 - maxValue) > (1E-05f * num2))
                {
                    num3 = 4E-05f * this.gjk.MaxLengthSquared;
                    if (!this.gjk.FullSimplex && (maxValue >= num3))
                    {
                        goto Label_006D;
                    }
                    result = true;
                }
            }
        }

        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
            Vector3 unitX;
            Vector3 vector2;
            Vector3 vector3;
            Vector3 vector4;
            Vector3 vector5;
            if (this.gjk == null)
            {
                this.gjk = new Gjk();
            }
            this.gjk.Reset();
            Vector3.Subtract(ref this.cornerArray[0], ref sphere.Center, out unitX);
            if (unitX.LengthSquared() < 1E-05f)
            {
                unitX = Vector3.UnitX;
            }
            float maxValue = float.MaxValue;
            float num3 = 0f;
            result = false;
        Label_005A:
            vector5.X = -unitX.X;
            vector5.Y = -unitX.Y;
            vector5.Z = -unitX.Z;
            this.SupportMapping(ref vector5, out vector4);
            MBoundingSphere.SupportMapping(ref unitX, ref sphere.Radius, ref sphere.Center, out vector3);
            Vector3.Subtract(ref vector4, ref vector3, out vector2);
            float num4 = ((unitX.X * vector2.X) + (unitX.Y * vector2.Y)) + (unitX.Z * vector2.Z);
            if (num4 <= 0f)
            {
                this.gjk.AddSupportPoint(ref vector2);
                unitX = this.gjk.ClosestPoint;
                float num2 = maxValue;
                maxValue = unitX.LengthSquared();
                if ((num2 - maxValue) > (1E-05f * num2))
                {
                    num3 = 4E-05f * this.gjk.MaxLengthSquared;
                    if (!this.gjk.FullSimplex && (maxValue >= num3))
                    {
                        goto Label_005A;
                    }
                    result = true;
                }
            }
        }

        public void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                float num3;
                MVector3.Dot(ref this.cornerArray[i], ref plane.Normal, out num3);
                if ((num3 + plane.D) > 0f)
                {
                    num |= 1;
                }
                else
                {
                    num |= 2;
                }
                if (num == 3)
                {
                    result = PlaneIntersectionType.Intersecting;
                    return;
                }
            }
            result = (num == 1) ? PlaneIntersectionType.Front : PlaneIntersectionType.Back;
        }

        public void Intersects(ref Ray ray, out float? result)
        {
            ContainmentType type;
            this.Contains(ref ray.Position, out type);
            if (type == ContainmentType.Contains)
            {
                result = 0f;
            }
            else
            {
                float minValue = float.MinValue;
                float maxValue = float.MaxValue;
                result = 0;
                foreach (Plane plane in this.planes)
                {
                    float num3;
                    float num6;
                    Vector3 normal = plane.Normal;
                    MVector3.Dot(ref ray.Direction, ref normal, out num6);
                    MVector3.Dot(ref ray.Position, ref normal, out num3);
                    num3 += plane.D;
                    if (Math.Abs(num6) < 1E-05f)
                    {
                        if (num3 > 0f)
                        {
                            return;
                        }
                    }
                    else
                    {
                        float num = -num3 / num6;
                        if (num6 < 0f)
                        {
                            if (num > maxValue)
                            {
                                return;
                            }
                            if (num > minValue)
                            {
                                minValue = num;
                            }
                        }
                        else
                        {
                            if (num < minValue)
                            {
                                return;
                            }
                            if (num < maxValue)
                            {
                                maxValue = num;
                            }
                        }
                    }
                }
                float num7 = (minValue >= 0f) ? minValue : maxValue;
                if (num7 >= 0f)
                {
                    result = new float?(num7);
                }
            }
        }

        public static bool operator ==(BoundingFrustum a, BoundingFrustum b)
        {
            return object.Equals(a, b);
        }

        public static bool operator !=(BoundingFrustum a, BoundingFrustum b)
        {
            return !object.Equals(a, b);
        }

        private void SetMatrix(ref Matrix value)
        {
            this.matrix = value;
            this.planes[2].Normal.X = -value.M14 - value.M11;
            this.planes[2].Normal.Y = -value.M24 - value.M21;
            this.planes[2].Normal.Z = -value.M34 - value.M31;
            this.planes[2].D = -value.M44 - value.M41;
            this.planes[3].Normal.X = -value.M14 + value.M11;
            this.planes[3].Normal.Y = -value.M24 + value.M21;
            this.planes[3].Normal.Z = -value.M34 + value.M31;
            this.planes[3].D = -value.M44 + value.M41;
            this.planes[4].Normal.X = -value.M14 + value.M12;
            this.planes[4].Normal.Y = -value.M24 + value.M22;
            this.planes[4].Normal.Z = -value.M34 + value.M32;
            this.planes[4].D = -value.M44 + value.M42;
            this.planes[5].Normal.X = -value.M14 - value.M12;
            this.planes[5].Normal.Y = -value.M24 - value.M22;
            this.planes[5].Normal.Z = -value.M34 - value.M32;
            this.planes[5].D = -value.M44 - value.M42;
            this.planes[0].Normal.X = -value.M13;
            this.planes[0].Normal.Y = -value.M23;
            this.planes[0].Normal.Z = -value.M33;
            this.planes[0].D = -value.M43;
            this.planes[1].Normal.X = -value.M14 + value.M13;
            this.planes[1].Normal.Y = -value.M24 + value.M23;
            this.planes[1].Normal.Z = -value.M34 + value.M33;
            this.planes[1].D = -value.M44 + value.M43;
            for (int i = 0; i < 6; i++)
            {
                float num2 = this.planes[i].Normal.Length();
                this.planes[i].Normal = (Vector3)(this.planes[i].Normal / num2);
                this.planes[i].D /= num2;
            }
            Ray ray = ComputeIntersectionLine(ref this.planes[0], ref this.planes[2]);
            this.cornerArray[0] = ComputeIntersection(ref this.planes[4], ref ray);
            this.cornerArray[3] = ComputeIntersection(ref this.planes[5], ref ray);
            ray = ComputeIntersectionLine(ref this.planes[3], ref this.planes[0]);
            this.cornerArray[1] = ComputeIntersection(ref this.planes[4], ref ray);
            this.cornerArray[2] = ComputeIntersection(ref this.planes[5], ref ray);
            ray = ComputeIntersectionLine(ref this.planes[2], ref this.planes[1]);
            this.cornerArray[4] = ComputeIntersection(ref this.planes[4], ref ray);
            this.cornerArray[7] = ComputeIntersection(ref this.planes[5], ref ray);
            ray = ComputeIntersectionLine(ref this.planes[1], ref this.planes[3]);
            this.cornerArray[5] = ComputeIntersection(ref this.planes[4], ref ray);
            this.cornerArray[6] = ComputeIntersection(ref this.planes[5], ref ray);
        }

        internal void SupportMapping(ref Vector3 v, out Vector3 result)
        {
            float num3;
            int index = 0;
            MVector3.Dot(ref this.cornerArray[0], ref v, out num3);
            for (int i = 1; i < this.cornerArray.Length; i++)
            {
                float num2;
                MVector3.Dot(ref this.cornerArray[i], ref v, out num2);
                if (num2 > num3)
                {
                    index = i;
                    num3 = num2;
                }
            }
            result = this.cornerArray[index];
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{Near:{0} Far:{1} Left:{2} Right:{3} Top:{4} Bottom:{5}}}", new object[] { this.Near.ToString(), this.Far.ToString(), this.Left.ToString(), this.Right.ToString(), this.Top.ToString(), this.Bottom.ToString() });
        }

        // Properties
        public Plane Bottom
        {
            get
            {
                return this.planes[5];
            }
        }

        public Plane Far
        {
            get
            {
                return this.planes[1];
            }
        }

        public Plane Left
        {
            get
            {
                return this.planes[2];
            }
        }

        public Matrix Matrix
        {
            get
            {
                return this.matrix;
            }
            set
            {
                this.SetMatrix(ref value);
            }
        }

        public Plane Near
        {
            get
            {
                return this.planes[0];
            }
        }

        public Plane Right
        {
            get
            {
                return this.planes[3];
            }
        }

        public Plane Top
        {
            get
            {
                return this.planes[4];
            }
        }
    }


    internal class Gjk
    {
        // Fields
        private static int[] BitsToIndices = new int[] { 0, 1, 2, 0x11, 3, 0x19, 0x1a, 0xd1, 4, 0x21, 0x22, 0x111, 0x23, 0x119, 0x11a, 0x8d1 };
        private Vector3 closestPoint;
        private float[][] det = new float[0x10][];
        private float[][] edgeLengthSq = new float[][] { new float[4], new float[4], new float[4], new float[4] };
        private Vector3[][] edges = new Vector3[][] { new Vector3[4], new Vector3[4], new Vector3[4], new Vector3[4] };
        private float maxLengthSq;
        private int simplexBits;
        private Vector3[] y = new Vector3[4];
        private float[] yLengthSq = new float[4];

        // Methods
        public Gjk()
        {
            for (int i = 0; i < 0x10; i++)
            {
                this.det[i] = new float[4];
            }
        }

        public bool AddSupportPoint(ref Vector3 newPoint)
        {
            int index = (BitsToIndices[this.simplexBits ^ 15] & 7) - 1;
            this.y[index] = newPoint;
            this.yLengthSq[index] = newPoint.LengthSquared();
            for (int i = BitsToIndices[this.simplexBits]; i != 0; i = i >> 3)
            {
                int num2 = (i & 7) - 1;
                Vector3 vector = this.y[num2] - newPoint;
                this.edges[num2][index] = vector;
                this.edges[index][num2] = -vector;
                this.edgeLengthSq[index][num2] = this.edgeLengthSq[num2][index] = vector.LengthSquared();
            }
            this.UpdateDeterminant(index);
            return this.UpdateSimplex(index);
        }

        private Vector3 ComputeClosestPoint()
        {
            float num3 = 0f;
            Vector3 zero = Vector3.Zero;
            this.maxLengthSq = 0f;
            for (int i = BitsToIndices[this.simplexBits]; i != 0; i = i >> 3)
            {
                int index = (i & 7) - 1;
                float num4 = this.det[this.simplexBits][index];
                num3 += num4;
                zero += (Vector3)(this.y[index] * num4);
                this.maxLengthSq = MathHelper.Max(this.maxLengthSq, this.yLengthSq[index]);
            }
            return (Vector3)(zero / num3);
        }

        private static float Dot(ref Vector3 a, ref Vector3 b)
        {
            return (((a.X * b.X) + (a.Y * b.Y)) + (a.Z * b.Z));
        }

        private bool IsSatisfiesRule(int xBits, int yBits)
        {
            for (int i = BitsToIndices[yBits]; i != 0; i = i >> 3)
            {
                int index = (i & 7) - 1;
                int num3 = ((int)1) << index;
                if ((num3 & xBits) != 0)
                {
                    if (this.det[xBits][index] <= 0f)
                    {
                        return false;
                    }
                }
                else if (this.det[xBits | num3][index] > 0f)
                {
                    return false;
                }
            }
            return true;
        }

        public void Reset()
        {
            this.simplexBits = 0;
            this.maxLengthSq = 0f;
        }

        private void UpdateDeterminant(int xmIdx)
        {
            int index = ((int)1) << xmIdx;
            this.det[index][xmIdx] = 1f;
            int num14 = BitsToIndices[this.simplexBits];
            int num8 = num14;
            for (int i = 0; num8 != 0; i++)
            {
                int num = (num8 & 7) - 1;
                int num12 = ((int)1) << num;
                int num6 = num12 | index;
                this.det[num6][num] = Dot(ref this.edges[xmIdx][num], ref this.y[xmIdx]);
                this.det[num6][xmIdx] = Dot(ref this.edges[num][xmIdx], ref this.y[num]);
                int num11 = num14;
                for (int j = 0; j < i; j++)
                {
                    int num3 = (num11 & 7) - 1;
                    int num5 = ((int)1) << num3;
                    int num9 = num6 | num5;
                    int num4 = (this.edgeLengthSq[num][num3] < this.edgeLengthSq[xmIdx][num3]) ? num : xmIdx;
                    this.det[num9][num3] = (this.det[num6][num] * Dot(ref this.edges[num4][num3], ref this.y[num])) + (this.det[num6][xmIdx] * Dot(ref this.edges[num4][num3], ref this.y[xmIdx]));
                    num4 = (this.edgeLengthSq[num3][num] < this.edgeLengthSq[xmIdx][num]) ? num3 : xmIdx;
                    this.det[num9][num] = (this.det[num5 | index][num3] * Dot(ref this.edges[num4][num], ref this.y[num3])) + (this.det[num5 | index][xmIdx] * Dot(ref this.edges[num4][num], ref this.y[xmIdx]));
                    num4 = (this.edgeLengthSq[num][xmIdx] < this.edgeLengthSq[num3][xmIdx]) ? num : num3;
                    this.det[num9][xmIdx] = (this.det[num12 | num5][num3] * Dot(ref this.edges[num4][xmIdx], ref this.y[num3])) + (this.det[num12 | num5][num] * Dot(ref this.edges[num4][xmIdx], ref this.y[num]));
                    num11 = num11 >> 3;
                }
                num8 = num8 >> 3;
            }
            if ((this.simplexBits | index) == 15)
            {
                int num2 = (this.edgeLengthSq[1][0] < this.edgeLengthSq[2][0]) ? ((this.edgeLengthSq[1][0] < this.edgeLengthSq[3][0]) ? 1 : 3) : ((this.edgeLengthSq[2][0] < this.edgeLengthSq[3][0]) ? 2 : 3);
                this.det[15][0] = ((this.det[14][1] * Dot(ref this.edges[num2][0], ref this.y[1])) + (this.det[14][2] * Dot(ref this.edges[num2][0], ref this.y[2]))) + (this.det[14][3] * Dot(ref this.edges[num2][0], ref this.y[3]));
                num2 = (this.edgeLengthSq[0][1] < this.edgeLengthSq[2][1]) ? ((this.edgeLengthSq[0][1] < this.edgeLengthSq[3][1]) ? 0 : 3) : ((this.edgeLengthSq[2][1] < this.edgeLengthSq[3][1]) ? 2 : 3);
                this.det[15][1] = ((this.det[13][0] * Dot(ref this.edges[num2][1], ref this.y[0])) + (this.det[13][2] * Dot(ref this.edges[num2][1], ref this.y[2]))) + (this.det[13][3] * Dot(ref this.edges[num2][1], ref this.y[3]));
                num2 = (this.edgeLengthSq[0][2] < this.edgeLengthSq[1][2]) ? ((this.edgeLengthSq[0][2] < this.edgeLengthSq[3][2]) ? 0 : 3) : ((this.edgeLengthSq[1][2] < this.edgeLengthSq[3][2]) ? 1 : 3);
                this.det[15][2] = ((this.det[11][0] * Dot(ref this.edges[num2][2], ref this.y[0])) + (this.det[11][1] * Dot(ref this.edges[num2][2], ref this.y[1]))) + (this.det[11][3] * Dot(ref this.edges[num2][2], ref this.y[3]));
                num2 = (this.edgeLengthSq[0][3] < this.edgeLengthSq[1][3]) ? ((this.edgeLengthSq[0][3] < this.edgeLengthSq[2][3]) ? 0 : 2) : ((this.edgeLengthSq[1][3] < this.edgeLengthSq[2][3]) ? 1 : 2);
                this.det[15][3] = ((this.det[7][0] * Dot(ref this.edges[num2][3], ref this.y[0])) + (this.det[7][1] * Dot(ref this.edges[num2][3], ref this.y[1]))) + (this.det[7][2] * Dot(ref this.edges[num2][3], ref this.y[2]));
            }
        }

        private bool UpdateSimplex(int newIndex)
        {
            int yBits = this.simplexBits | (((int)1) << newIndex);
            int xBits = ((int)1) << newIndex;
            for (int i = this.simplexBits; i != 0; i--)
            {
                if (((i & yBits) == i) && this.IsSatisfiesRule(i | xBits, yBits))
                {
                    this.simplexBits = i | xBits;
                    this.closestPoint = this.ComputeClosestPoint();
                    return true;
                }
            }
            bool flag = false;
            if (this.IsSatisfiesRule(xBits, yBits))
            {
                this.simplexBits = xBits;
                this.closestPoint = this.y[newIndex];
                this.maxLengthSq = this.yLengthSq[newIndex];
                flag = true;
            }
            return flag;
        }

        // Properties
        public Vector3 ClosestPoint
        {
            get
            {
                return this.closestPoint;
            }
        }

        public bool FullSimplex
        {
            get
            {
                return (this.simplexBits == 15);
            }
        }

        public float MaxLengthSquared
        {
            get
            {
                return this.maxLengthSq;
            }
        }
    }


}
