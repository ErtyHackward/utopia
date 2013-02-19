using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Allows to create simple items
    /// </summary>
    [ProtoContract]
    [Description("Use for different stuff.")]
    public class Stuff : Item
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Stuff; }
        }
    }
}
