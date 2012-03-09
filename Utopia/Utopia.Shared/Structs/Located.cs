using System;
using S33M3_Resources.Structs;

namespace Utopia.Shared.Structs
{
    public struct Located<T> where T : IEquatable<T>
    {
        public Vector3I Location;
        public T Value;
    }
}
