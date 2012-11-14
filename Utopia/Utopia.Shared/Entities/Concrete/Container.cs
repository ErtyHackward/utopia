using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents a container block entity
    /// </summary>
    public class Container : OrientedCubePlaceableItem
    {
        readonly SlotContainer<ContainedSlot> _content;

        public override bool RequiresLock
        {
            get{ return true; }
        }

        public override ushort ClassId
        {
            get { return EntityClassId.Container; }
        }

        public SlotContainer<ContainedSlot> Content { get { return _content; } }

        public Container()
        {
            _content = new SlotContainer<ContainedSlot>(this);
            MountPoint = Interfaces.BlockFace.Top;
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);

            _content.Save(writer);
        }

        public override void Load(System.IO.BinaryReader reader, EntityFactory factory)
        {
            base.Load(reader, factory);

            _content.Load(reader, factory);
        }

    }
}
