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
    }
}
