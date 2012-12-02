namespace Utopia.Shared.Entities.Inventory
{
    public class EquipmentSlot<T> : ContainedSlot where T: Item
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
