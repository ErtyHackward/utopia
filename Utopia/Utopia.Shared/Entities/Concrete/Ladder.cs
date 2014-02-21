using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents a ladder entity that can be used to go up/down
    /// </summary>
    [ProtoContract]
    [Description("Provides ladder functionality. Giving the possibility for the entity to grab it to move without falling")]
    public class Ladder : OrientedBlockItem
    {
    }
}
