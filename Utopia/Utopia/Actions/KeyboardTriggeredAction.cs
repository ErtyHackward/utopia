using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Config;

namespace Utopia.Actions
{
    public struct KeyboardTriggeredAction
    {
        public enuActions Action;
        public enuKeyboardTriggerMode TriggerType;
        public KeyWithModifier Binding;
    }
}
