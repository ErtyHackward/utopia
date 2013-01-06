using System;
using System.ComponentModel;
using ProtoBuf;
using S33M3CoreComponents.Sound;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// Represents an two-state item, that can be switched between them by use operation
    /// </summary>
    [ProtoContract]
    public class Door : OrientedBlockItem, IUsableEntity, ISoundEmitterEntity
    {
        [Category("Door")]
        [Description("Current entity state (initial)")]
        [ProtoMember(1)]
        public bool IsOpen { get; set; }
        
        [Category("Door")]
        [Description("Model state if the door is opened")]
        [TypeConverter(typeof(ModelStateSelector))]
        [ProtoMember(2)]
        public string OpenedState { get; set; }

        [Category("Door")]
        [Description("Model state if the door is closed")]
        [TypeConverter(typeof(ModelStateSelector))]
        [ProtoMember(3)]
        public string ClosedState { get; set; }

        [Category("Sound")]
        [Description("Sound of the door opening/closing")]
        [TypeConverter(typeof(SoundSelector))]
        [ProtoMember(4)]
        public string StartSound { get; set; }

        [Category("Sound")]
        [Description("Sound of the door impact on close/open")]
        [TypeConverter(typeof(SoundSelector))]
        [ProtoMember(5)]
        public string FinishSound { get; set; }


        private ISoundEngine _soundEngine;

        public ISoundEngine SoundEngine
        {
            get { return _soundEngine; }
            set { _soundEngine = value; }
        }

        public override ushort ClassId
        {
            get
            {
                return EntityClassId.Door;
            }
        }

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
            if (!string.IsNullOrEmpty(FinishSound) && SoundEngine != null)
            {
                SoundEngine.StartPlay3D(FinishSound, FinishSound, Position.AsVector3());
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

            if (!string.IsNullOrEmpty(StartSound) && SoundEngine != null)
            {
                SoundEngine.StartPlay3D(StartSound, StartSound, Position.AsVector3());
            }

            NotifyParentContainerChange();
        }
    }
}
