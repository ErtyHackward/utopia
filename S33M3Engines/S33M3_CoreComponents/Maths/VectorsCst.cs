using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3_CoreComponents.Maths;
using S33M3_Resources.Structs;

namespace S33M3_CoreComponents.Maths
{
    public class VectorsCst
    {
        public static readonly Vector3 Up3 = new Vector3(0, 1, 0);
        public static readonly Vector3 Down3 = new Vector3(0, -1, 0);
        public static readonly Vector3 Right3 = new Vector3(1, 0, 0);
        public static readonly Vector3 Left3 = new Vector3(-1, 0, 0);
        public static readonly Vector3 Forward3 = new Vector3(0, 0, -1);
        public static readonly Vector3 Backward3 = new Vector3(0, 0, 1);
    }
}
