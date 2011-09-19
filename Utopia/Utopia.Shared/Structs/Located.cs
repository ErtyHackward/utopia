using System;

namespace Utopia.Shared.Structs
{
    public struct Located<T> where T : IEquatable<T>
    {
        public Location3<int> Location;
        public T Value;
    }
}
