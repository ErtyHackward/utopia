using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using S33M3Resources.Structs;
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
    public class Container : OrientedBlockItem, IContainerEntity
    {
        SlotContainer<ContainedSlot> _content;

        [Category("Container")]
        [Description("Model state if the container is opened")]
        [TypeConverter(typeof(ModelStateConverter))]
        [ProtoMember(1)]
        public string OpenedState { get; set; }

        [Category("Container")]
        [Description("Model state if the container is closed")]
        [TypeConverter(typeof(ModelStateConverter))]
        [ProtoMember(2)]
        public string ClosedState { get; set; }

        [Browsable(false)]
        [ProtoMember(3)]
        public SlotContainer<ContainedSlot> Content
        {
            get { return _content; }
            set
            {
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

        [Category("Container")]
        [Description("How many slots container has")]
        [TypeConverter(typeof(Vector2IConverter))]
        public Vector2I ContainerSize
        {
            get { return _content.GridSize; }
            set { _content.GridSize = value; }
        }

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
            _content = new SlotContainer<ContainedSlot>(this);
        }

        public override object Clone()
        {
            var obj = base.Clone();

            var cont = obj as Container;

            if (cont != null)
                cont.Content = new SlotContainer<ContainedSlot>(cont, ContainerSize);

            return obj;
        }

        public bool TakeItems(ushort blueprintId, int count)
        {
            return _content.TakeItem(blueprintId, count);
        }

        public bool PutItems(IItem item, int count)
        {
            return _content.PutItem(item, count);
        }

        public IEnumerable<ContainedSlot> Slots()
        {
            return _content;
        }
    }

    public interface IContainerEntity
    {
        bool TakeItems(ushort blueprintId, int count);
        bool PutItems(IItem item, int count);

        IEnumerable<ContainedSlot> Slots();
    }
}
