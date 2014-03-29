using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [Description("Animal npc")]
    public class Animal : Npc
    {
        /// <summary>
        /// Gets or sets weapon that animal will use
        /// </summary>
        [ProtoMember(1)]
        [Description("Weapon of the animal")]
        [Category("Gameplay")]
        [TypeConverter(typeof(BlueprintSelector))]
        public ushort WeaponBlueprint { get; set; }


    }
}