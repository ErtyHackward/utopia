using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class EntityBodyRotationEventArgs : EventArgs
    {
        public IDynamicEntity Entity { get; set; }
    }
}
