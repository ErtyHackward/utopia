using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Structs;

namespace Utopia.Entities.Interfaces
{
    public interface IVisualEntity
    {
        SpriteEntity SpriteEntity { get; set; }
        ByteColor color { get; set; }
    }
}
