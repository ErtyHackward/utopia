using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Shared.Entities.Concrete
{
    public class Zombie : CharacterEntity
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Zombie; }
        }

        public Zombie()
        {
            Type = EntityType.Dynamic;
            ModelName = "Girl";
            Name = "Zombie";
        }

    }
}
