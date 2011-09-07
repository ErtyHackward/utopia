using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Config;

namespace Utopia.Action
{
    public struct KeyboardTriggeredAction
    {
        public Actions Action;
        public KeyboardTriggerMode TriggerType;
        public KeyWithModifier Binding;
    }
}
