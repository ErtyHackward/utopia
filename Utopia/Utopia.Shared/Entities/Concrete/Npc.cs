using System.ComponentModel;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [Description("Basic dynamic entity")]
    public class Npc : CharacterEntity
    {
        [Browsable(false)]
        public override ushort ClassId
        {
            get { return EntityClassId.NonPlayerCharacter; }
        }

        public Npc()
        {
            DefaultSize = new Vector3(0.8f, 1.4f, 0.8f);
            Name = "NPC";
            MoveSpeed = 2.2f;
        }

    }
}
