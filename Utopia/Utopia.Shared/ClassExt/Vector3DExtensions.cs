using System;

namespace S33M3Resources.Structs
{
    public static class Vector3DExtensions
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
    }
}
