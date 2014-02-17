using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Inputs.KeyboardHandler;

namespace S33M3CoreComponents.Inputs.Actions
{
    public class KeyboardTriggeredAction
    {
        public int ActionId;
        public KeyboardTriggerMode TriggerType;
        public KeyWithModifier Binding;
        public bool WithTimeElapsed;
        public float MaxTimeElapsedInS;
        public long StartTimeElapsedInTick;
        /// <summary>
        /// Will reset the KeyPressed event, and make it fired again with a given timelapse in s
        /// </summary>
        public bool WithAutoResetButtonPressed;
        public float AutoResetTimeInS;
        public long StartTimeAutoResetTick;
    }
}
