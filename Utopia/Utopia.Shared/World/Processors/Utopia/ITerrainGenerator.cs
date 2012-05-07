using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise;

namespace Utopia.Shared.World.Processors.Utopia
{
    public interface ITerrainGenerator
    {
        INoise GetLandFormFct();
    }
}
