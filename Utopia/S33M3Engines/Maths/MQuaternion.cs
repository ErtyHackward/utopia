﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.Shared.Math;

namespace S33M3Engines.Maths
{
    public static class MQuaternion
    {
        public static double getPitch(ref Quaternion quaternion)
        {
            return Math.Atan2(2 * (quaternion.Y * quaternion.Z + quaternion.W * quaternion.X), quaternion.W * quaternion.W - quaternion.X * quaternion.X - quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
        }

        public static double getYaw(ref Quaternion quaternion)
        {
            return Math.Asin(-2 * (quaternion.X * quaternion.Z - quaternion.W * quaternion.Y));
        }

        public static double getRoll(ref Quaternion quaternion)
        {
            return Math.Atan2(2 * (quaternion.X * quaternion.Y + quaternion.W * quaternion.Z), quaternion.W * quaternion.W + quaternion.X * quaternion.X - quaternion.Y * quaternion.Y - quaternion.Z * quaternion.Z);
        }

        public static Vector3 GetLookAtFromQuaternion(Quaternion rotation)
        {
            Matrix entityRotation = Matrix.RotationQuaternion(rotation);
            Matrix.Transpose(ref entityRotation, out entityRotation);
            Vector3 lookAt = new Vector3(-entityRotation.M13, -entityRotation.M23, -entityRotation.M33);
            lookAt.Normalize();
            return lookAt;
        }

        public static Vector3D GetLookAtFromQuaternion_V3D(Quaternion rotation)
        {
            Matrix entityRotation = Matrix.RotationQuaternion(rotation);
            Matrix.Transpose(ref entityRotation, out entityRotation);
            Vector3D lookAt = new Vector3D(-entityRotation.M13, -entityRotation.M23, -entityRotation.M33);
            lookAt.Normalize();
            return lookAt;
        }
    }
}
