using ProtoBuf;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Allows to create simple items
    /// </summary>
    [ProtoContract]
    public class Stuff : Item
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Stuff; }
        }
    }
}
