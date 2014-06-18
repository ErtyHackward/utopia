using System;
using System.ComponentModel;
using ProtoBuf;
using S33M3CoreComponents.Sound;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Entities.Sound;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents an two-state item, that can be switched between them by use operation
    /// </summary>
    [ProtoContract]
    [Description("Provides door entity functionality. Can be opened or closed. Bind according model states to the class properties.")]
    public class Door : OrientedBlockItem, IUsableEntity
    {
        [Category("Door")]
        [Description("Current entity state (initial)")]
        [ProtoMember(1)]
        public bool IsOpen { get; set; }
        
        [Category("Door")]
        [Description("Model state if the door is opened")]
        [TypeConverter(typeof(ModelStateConverter))]
        [ProtoMember(2)]
        public string OpenedState { get; set; }

        [Category("Door")]
        [Description("Model state if the door is closed")]
        [TypeConverter(typeof(ModelStateConverter))]
        [ProtoMember(3)]
        public string ClosedState { get; set; }

        [Category("Sound")]
        [Description("Sound of the door opening/closing")]
        [TypeConverter(typeof(ShortSoundSelector))]
        [ProtoMember(6)]
        public StaticEntitySoundSource StartSound { get; set; }

        [Category("Sound")]
        [Description("Sound of the door impact on close/open")]
        [TypeConverter(typeof(ShortSoundSelector))]
        [ProtoMember(7)]
        public StaticEntitySoundSource FinishSound { get; set; }

        protected override void OnInstanceChanged(VoxelModelInstance prev)
        {
            if (prev != null)
            {
                prev.StateChanged -= ModelInstanceOnStateChanged;
            }

            if (ModelInstance != null)
            {
                var newState = IsOpen ? OpenedState : ClosedState;
                if (!string.IsNullOrEmpty(newState))
                    ModelInstance.SetState(newState);

                ModelInstance.StateChanged += ModelInstanceOnStateChanged;
            }
        }

        private void ModelInstanceOnStateChanged(object sender, EventArgs eventArgs)
        {
            if (FinishSound != null && SoundEngine != null)
            {
                SoundEngine.StartPlay3D(FinishSound, Position.AsVector3());
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

            if (StartSound != null && SoundEngine != null)
            {
                SoundEngine.StartPlay3D(StartSound, Position.AsVector3());
            }

            NotifyParentContainerChange();
        }
    }
}
