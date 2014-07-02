using System.ComponentModel;
using System.Drawing.Design;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [ProtoInclude(100, typeof(Animal))]
    [Description("Basic dynamic entity")]
    public class Npc : CharacterEntity
    {
        /// <summary>
        /// Gets or sets weapon that animal will use
        /// </summary>
        [Description("Optional grave item. Created when npc dies to store its items.")]
        [Category("Gameplay")]
        [Editor(typeof(BlueprintTypeEditor<Container>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
        [ProtoMember(1)]
        public ushort GraveBlueprint { get; set; }

        public Npc()
        {
            DefaultSize = new Vector3(0.8f, 1.4f, 0.8f);
            Name = "NPC";
            MoveSpeed = 2.2f;
        }
    }
}
