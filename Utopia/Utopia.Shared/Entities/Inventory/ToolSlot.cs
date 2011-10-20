using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    public class EquipmentSlot<T> : ContainedSlot
    {
        public T Equipment
        {
            get { return (T)Item; }
        }

        public static EquipmentSlot<T> FromBase(ContainedSlot slot)
        {
            return new EquipmentSlot<T> { Item = slot.Item, ItemsCount = slot.ItemsCount };;
        }
    }
}
