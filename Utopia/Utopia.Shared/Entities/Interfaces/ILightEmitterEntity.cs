using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface ILightEmitterEntity : IStaticEntity
    {
        ByteColor EmittedLightColor { get; set; }
    }
}
