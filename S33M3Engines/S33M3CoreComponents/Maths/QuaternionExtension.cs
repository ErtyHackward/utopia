using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Maths
{
    public static class QuaternionExtension
    {
        public static Vector3 GetLookAtVector(this Quaternion rotation)
        {
            Matrix entityRotation = Matrix.RotationQuaternion(Quaternion.Conjugate(rotation));
            Vector3 lookAt = new Vector3(-entityRotation.M13, -entityRotation.M23, -entityRotation.M33);
            lookAt.Normalize();
            return lookAt;
        }
    }
}
