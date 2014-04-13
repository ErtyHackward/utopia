using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class PlantGrowingEntity : GrowingEntity
    {
        protected override void OnBeforePut(Item item)
        {
            base.OnBeforePut(item);

            var plant = (PlantGrowingEntity)item;
            plant.CurrentGrowLevelIndex = 0;
            plant.CurrentGrowTime = new UtopiaTimeSpan();
            plant.LastGrowUpdate = new UtopiaTime();
        }
    }
}