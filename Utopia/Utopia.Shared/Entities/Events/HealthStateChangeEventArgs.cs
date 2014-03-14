using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Shared.Entities.Events
{
    public class HealthStateChangeEventArgs : EventArgs
    {
        public DynamicEntityHealthState PreviousState;
        public DynamicEntityHealthState NewState;
        public uint DynamicEntityId;
    }

}
