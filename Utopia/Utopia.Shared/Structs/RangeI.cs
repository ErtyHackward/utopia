using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;

namespace Utopia.Shared.Structs
{
    [Obsolete("Use Range3 instead of RangeI")]
    public struct RangeI
    {
        public Vector3I Min;
        public Vector3I Max;
    }
}
