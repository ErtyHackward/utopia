using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.States.Interfaces;
using S33M3DXEngine.Main;

namespace S33M3CoreComponents.States
{
    public abstract class SwitchComponent : DrawableGameComponent, ISwitchComponent
    {
        public abstract event EventHandler SwitchMoment;
        public abstract event EventHandler EffectComplete;

        public SwitchComponent()
            : base()
        {
            this.IsSystemComponent = true;
        }

        public abstract void BeginSwitch();
        public abstract void FinishSwitch();
    }
}
