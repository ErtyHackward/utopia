using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using Utopia.Shared.Structs;

namespace Liquid.plugin
{
    public class FloodingData
    {
        public Location3<int> CubeLocation;
        public int FloodingPower;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            FloodingData objloc = (FloodingData)obj;
            return (((this.CubeLocation.X.Equals(objloc.CubeLocation.X)) && (this.CubeLocation.Y.Equals(objloc.CubeLocation.Y))) && (this.CubeLocation.Z.Equals(objloc.CubeLocation.Z)));
        }

    }
}
