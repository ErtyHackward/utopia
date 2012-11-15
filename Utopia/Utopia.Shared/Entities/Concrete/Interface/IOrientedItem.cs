using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Entities.Concrete.Interface
{
    public interface IOrientedItem
    {
        OrientedCubePlaceableItem.OrientedItem Orientation { get; set; }
    }
}
