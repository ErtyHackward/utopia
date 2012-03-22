using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace S33M3CoreComponents.Physics
{
    public static class SimulatorCst
    {
        public static float Gravity = 15f; // In M/ms
        public static Vector3 GravityAcceleration = new Vector3(0, -Gravity, 0);
    }
}
