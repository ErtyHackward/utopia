using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Math;

namespace Utopia.Entities.Interfaces
{
    public interface IVisualStaticEntity
    {
        Vector3D WorldPosition { get; }
        ByteColor color { get; set; }
    }
}
