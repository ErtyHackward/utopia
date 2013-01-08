using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Maths
{
    public static class QuaternionExtension
    {
        public static bool EqualsEpsilon(this Quaternion rotation, Quaternion other, float epsilon)
        {
            if (Math.Abs(Math.Abs(rotation.X) - Math.Abs(other.X)) < epsilon &&
                Math.Abs(Math.Abs(rotation.Y) - Math.Abs(other.Y)) < epsilon &&
                Math.Abs(Math.Abs(rotation.Z) - Math.Abs(other.Z)) < epsilon && 
                Math.Abs(Math.Abs(rotation.W) - Math.Abs(other.W)) < epsilon)
                return true;
            return false;
        }

        public static Vector3 GetLookAtVector(this Quaternion rotation)
        {
            Matrix entityRotation = Matrix.RotationQuaternion(Quaternion.Conjugate(rotation));
            Vector3 lookAt = new Vector3(-entityRotation.M13, -entityRotation.M23, -entityRotation.M33);
            lookAt.Normalize();
            return lookAt;
        }
    }
}
