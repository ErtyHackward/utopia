using ProtoBuf;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class BasicCollector : ResourcesCollector
    {
        public override ushort ClassId { get { return EntityClassId.BasicCollector; } }

        public BasicCollector()
        {
            Type = EntityType.Gear;
            Name = "Basic Collector Tool";
            IsPlayerCollidable = false;
            IsPickable = true;
        }

        //By default Basic collector use Block configuration for Picking
        public override Inventory.PickType CanPickBlock(CubeProfile cubeProfile)
        {
            if (cubeProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;

            //Default Block Behaviours here
            return cubeProfile.IsPickable ? PickType.Pick : PickType.Stop;
        }
    }
}
