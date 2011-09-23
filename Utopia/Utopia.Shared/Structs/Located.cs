using System;

namespace Utopia.Shared.Structs
{
    public struct Located<T> where T : IEquatable<T>
    {
        public Vector3I Location;
        public T Value;
    }
}
