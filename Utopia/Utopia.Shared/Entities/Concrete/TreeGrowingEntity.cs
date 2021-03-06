using ProtoBuf;
using System.ComponentModel;
using Utopia.Shared.Entities.Models;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// System tree seed entity, should be inaccessible in the editor. One blueprint per tree blueprint.
    /// </summary>
    [ProtoContract]
    [EditorHide]
    public class TreeGrowingEntity : GrowingEntity
    {
        /// <summary>
        /// Contains index of tree blueprint in the configuration
        /// </summary>
        [Category("Growing")]
        [TypeConverter(typeof(TreeListEditor))]
        [ProtoMember(1)]
        public int TreeTypeId { get; set; }

        /// <summary>
        /// Actual scale of the model [0;1]
        /// </summary>
        [ProtoMember(3)]
        public float Scale { get; set; }

        [ProtoMember(4)]
        public int TreeRndSeed { get; set; }

        public override string StackType
        {
            get { return "TreeSeed" + TreeTypeId + "_" + TreeRndSeed; }
        }
    }
}