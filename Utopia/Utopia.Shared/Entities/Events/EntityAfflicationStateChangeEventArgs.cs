using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class EntityAfflicationStateChangeEventArgs : EventArgs
    {
        public DynamicEntityAfflictionState PreviousState;
        public DynamicEntityAfflictionState NewState;
        public IDynamicEntity DynamicEntity;
    }
}
