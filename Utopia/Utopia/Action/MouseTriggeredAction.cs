using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Action
{
    public struct MouseTriggeredAction
    {
        public Actions Action;
        public MouseTriggerMode TriggerType;
        public MouseButton Binding;
    }
}
