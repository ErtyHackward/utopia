using ProtoBuf;
using System.ComponentModel;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class TreeGrowingEntity : GrowingEntity
    {
        [ProtoMember(1)]
        [Category("Growing")]
        [TypeConverter(typeof(TreeListEditor))]
        public int TreeTypeId { get; set; }
    }
}