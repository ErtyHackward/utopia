using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_CoreComponents.Inputs.KeyboardHandler;

namespace S33M3_CoreComponents.Inputs.Actions
{
    public class KeyboardTriggeredAction
    {
        public int ActionId;
        public KeyboardTriggerMode TriggerType;
        public KeyWithModifier Binding;
        public bool WithTimeElapsed;
        public float MaxTimeElapsedInS;
        public float StartTimeElapsedInTick;
    }
}
