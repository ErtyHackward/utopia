using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [Description("Provides mine-tool functionality.")]
    public class BasicCollector : ResourcesCollector
    {
        public BasicCollector()
        {
            Name = "Basic Collector Tool";
        }

        //By default Basic collector use Block configuration for Picking
        public override PickType CanPickBlock(BlockProfile blockProfile)
        {
            if (blockProfile.Id == WorldConfiguration.CubeId.Air)
                return PickType.Transparent;

            //Default Block Behaviours here
            return blockProfile.IsPickable ? PickType.Pick : PickType.Stop;
        }
    }
}
