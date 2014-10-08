using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Inputs.Actions
{
    public class MouseTriggeredAction
    {
        public int ActionId;
        public MouseTriggerMode TriggerType;
        public bool? WithCursorLocked;
        public MouseButton Binding;
        /// <summary>
        /// Will reset the ButtonPressed event, and make it fired again with a given timelapse in s
        /// </summary>
        public bool WithAutoResetButtonPressed;
        public float AutoResetTimeInS;
        public long StartTimeAutoResetTick;
    }
}
