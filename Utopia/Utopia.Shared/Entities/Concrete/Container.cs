using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents a container block entity
    /// </summary>
    [ProtoContract]
    [Description("Allows to store other entities inside this one.")]
    public class Container : OrientedBlockLinkedItem
    {
        SlotContainer<ContainedSlot> _content;

        [Category("Container")]
        [Description("Model state if the container is opened")]
        [TypeConverter(typeof(ModelStateSelector))]
        [ProtoMember(1)]
        public string OpenedState { get; set; }

        [Category("Container")]
        [Description("Model state if the container is closed")]
        [TypeConverter(typeof(ModelStateSelector))]
        [ProtoMember(2)]
        public string ClosedState { get; set; }

        public override bool RequiresLock
        {
            get{ return true; }
        }

        /// <summary>
        /// Gets or sets value indicating the entity is locked
        /// This is runtime parameter that is not stored
        /// </summary>
        public override bool Locked
        {
            get
            {
                return base.Locked;
            }
            set
            {
                base.Locked = value;
                if (ModelInstance != null)
                {
                    if (base.Locked)
                    {
                        if (!string.IsNullOrEmpty(OpenedState))
                            ModelInstance.SwitchState(OpenedState);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ClosedState))
                            ModelInstance.SwitchState(ClosedState);
                    }
                }
            }
        }

        public override ushort ClassId
        {
            get { return EntityClassId.Container; }
        }

        [Browsable(false)]
        [ProtoMember(3)]
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

        protected override void OnInstanceChanged(VoxelModelInstance prev)
        {
            if (ModelInstance != null)
            {
                if (base.Locked)
                {
                    if (!string.IsNullOrEmpty(OpenedState))
                        ModelInstance.SetState(OpenedState);
                }
                else
                {
                    if (!string.IsNullOrEmpty(ClosedState))
                        ModelInstance.SetState(ClosedState);
                }
            }
        }

        void ContentItemsChanged(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            NotifyParentContainerChange();
        }

        public Container()
        {
            Content = new SlotContainer<ContainedSlot>(this);
            MountPoint = BlockFace.Top;
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
