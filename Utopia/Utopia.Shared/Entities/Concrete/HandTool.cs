using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Special tool used when no tool is set
    /// </summary>
    public class HandTool : Item
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Hand; }
        }

        public override PickType CanPickBlock(byte blockId)
        {
            // don't allow to pick blocks by hand
            return PickType.Stop;
        }
    }
}
