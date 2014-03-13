using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Entities.Dynamic
{
    [Flags]
    public enum DynamicEntityAfflictionState
    {
        Stunned = 1,
        Poisoned = 2,
        Starving = 4
    }
}
