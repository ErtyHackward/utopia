using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class HealthStateChangeEventArgs : EventArgs
    {
        public DynamicEntityHealthState PreviousState;
        public DynamicEntityHealthState NewState;
        public IDynamicEntity DynamicEntity;
    }

}
