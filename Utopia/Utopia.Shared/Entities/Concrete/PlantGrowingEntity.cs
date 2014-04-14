using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class PlantGrowingEntity : GrowingEntity
    {
        [ProtoMember(1)]
        [Category("Growing")]
        public List<GrowLevel> GrowLevels { get; set; }

        [ProtoMember(2)]
        [Browsable(false)]
        [Category("Growing")]
        public int CurrentGrowLevelIndex { get; set; }

        [Browsable(false)]
        public GrowLevel CurrentGrowLevel { get { return GrowLevels.Count > CurrentGrowLevelIndex ? GrowLevels[CurrentGrowLevelIndex] : default(GrowLevel); } }

        [Browsable(false)]
        public bool IsLastGrowLevel
        {
            get { return CurrentGrowLevelIndex == GrowLevels.Count - 1; }
        }

        public PlantGrowingEntity()
        {
            GrowLevels = new List<GrowLevel>();
        }

        protected override void OnBeforePut(Item item)
        {
            base.OnBeforePut(item);

            var plant = (PlantGrowingEntity)item;
            plant.CurrentGrowLevelIndex = 0;
            plant.CurrentGrowTime = new UtopiaTimeSpan();
            plant.LastGrowUpdate = new UtopiaTime();
        }

        public override object Clone()
        {
            var cloned = (PlantGrowingEntity)base.Clone();
            
            cloned.GrowLevels = new List<GrowLevel>(GrowLevels);
            for (int i = 0; i < cloned.GrowLevels.Count; i++)
            {
                cloned.GrowLevels[i] = (GrowLevel)cloned.GrowLevels[i].Clone();
            }

            return cloned;
        }
    }
}