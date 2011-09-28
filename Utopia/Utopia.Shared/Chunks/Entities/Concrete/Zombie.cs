using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks.Entities.Concrete
{
    public class Zombie : CharacterEntity
    {
        public override EntityClassId ClassId
        {
            get { return EntityClassId.Zombie; }
        }

        public override string DisplayName
        {
            get { return "Zombie "+ CharacterName; }
        }

        public Zombie()
        {
            Type = EntityType.Dynamic;
        }

    }
}
