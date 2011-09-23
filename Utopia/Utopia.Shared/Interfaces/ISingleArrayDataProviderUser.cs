using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Interfaces
{
    public interface ISingleArrayDataProviderUser
    {
        Vector2I ChunkPositionBlockUnit { get; }
    }
}
