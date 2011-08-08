using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using Utopia.Planets.Terran.World;
using Utopia.Planets.Terran.Cube;
using System.Diagnostics;

namespace Utopia.Planets.Terran.Flooding
{
    public static class TerraFlooding
    {
        public enum FloodDirection
        {
            None = 0,
            Right = 1,
            Left = 2,
            Front = 3,
            Back = 4,
            FrontRight = 5,
            FrontLeft = 6,
            BackRight = 7,
            BackLeft = 8,
            Fall = 9,
            Undefined = 255,
        }
    }
}
