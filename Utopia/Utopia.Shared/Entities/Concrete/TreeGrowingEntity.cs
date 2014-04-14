using ProtoBuf;
using System.ComponentModel;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class TreeGrowingEntity : GrowingEntity
    {
        /// <summary>
        /// Contains index of tree blueprint in the configuration
        /// </summary>
        [Category("Growing")]
        [TypeConverter(typeof(TreeListEditor))]
        [ProtoMember(1)]
        public int TreeTypeId { get; set; }
    }
}