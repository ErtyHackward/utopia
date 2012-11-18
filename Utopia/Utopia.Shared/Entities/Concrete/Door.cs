using System.ComponentModel;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents an two-state item, that can be switched between them by use operation
    /// </summary>
    public class Door : OrientedBlockItem, IUsableEntity
    {
        [Category("Door")]
        [Description("Model state if the door is opened")]
        [TypeConverter(typeof(ModelStateSelector))]
        public string OpenedState { get; set; }

        [Category("Door")]
        [Description("Model state if the door is closed")]
        [TypeConverter(typeof(ModelStateSelector))]
        public string ClosedState { get; set; }

        [Category("Door")]
        [Description("Current entity state (initial)")]
        public bool IsOpen { get; set; }

        public override ushort ClassId
        {
            get
            {
                return EntityClassId.Door;
            }
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);

            writer.Write(OpenedState ?? string.Empty);
            writer.Write(ClosedState ?? string.Empty);
            writer.Write(IsOpen);
        }

        public override void Load(System.IO.BinaryReader reader, EntityFactory factory)
        {
            base.Load(reader, factory);

            OpenedState = reader.ReadString();
            ClosedState = reader.ReadString();
            IsOpen = reader.ReadBoolean();
        }

        protected override void OnInstanceChanged()
        {
            if (ModelInstance != null)
            {
                var newState = IsOpen ? OpenedState : ClosedState;
                if (!string.IsNullOrEmpty(newState))
                    ModelInstance.SetState(newState);
            }
        }

        public void Use()
        {
            IsOpen = !IsOpen;

            if (ModelInstance != null)
            {
                var newState = IsOpen ? OpenedState : ClosedState;
                if (!string.IsNullOrEmpty(newState))
                    ModelInstance.SwitchState(newState);
            }
            NotifyParentContainerChange();
        }
    }
}
