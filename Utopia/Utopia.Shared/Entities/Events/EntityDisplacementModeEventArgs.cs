using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Entities.Events
{
    public class EntityDisplacementModeEventArgs : EventArgs
    {
        public EntityDisplacementModes PreviousDisplacement { get; set; }
        public EntityDisplacementModes CurrentDisplacement { get; set; }
    }
}
