using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents a container block entity
    /// </summary>
    public class Container : OrientedBlockLinkedItem
    {
        SlotContainer<ContainedSlot> _content;

        public override bool RequiresLock
        {
            get{ return true; }
        }

        public override ushort ClassId
        {
            get { return EntityClassId.Container; }
        }

        public SlotContainer<ContainedSlot> Content 
        { 
            get { return _content; }
            set {

                if (_content == value)
                    return;

                if (_content != null)
                {
                    _content.ItemTaken -= ContentItemsChanged;
                    _content.ItemPut -= ContentItemsChanged;
                    _content.ItemExchanged -= ContentItemsChanged;
                }
                
                _content = value;

                if (_content != null)
                {
                    _content.ItemTaken += ContentItemsChanged;
                    _content.ItemPut += ContentItemsChanged;
                    _content.ItemExchanged += ContentItemsChanged;
                }
            }
        }

        void ContentItemsChanged(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            var container = Container;
            
            if (container is EntityCollection)
            {
                var collection = container as EntityCollection;
                collection.SetDirty();
            }
        }

        public Container()
        {
            Content = new SlotContainer<ContainedSlot>(this);
            MountPoint = BlockFace.Top;
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

        public override object Clone()
        {
            var obj = base.Clone();

            var cont = obj as Container;

            if (cont != null) 
                cont.Content = new SlotContainer<ContainedSlot>(cont);

            return obj;
        }
    }
}
