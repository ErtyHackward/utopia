using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Structs
{
    public struct Range<T> where T : IEquatable<T>
    {
        public Location3<T> Min;
        public Location3<T> Max;
    }
}
