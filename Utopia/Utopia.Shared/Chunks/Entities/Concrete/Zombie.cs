using System;

namespace Utopia.Shared.Chunks.Entities.Concrete
{
    public class Zombie : CharacterEntity
    {
        public override void AddArea(Management.MapArea area)
        {
            
        }

        public override void RemoveArea(Management.MapArea area)
        {
            
        }

        public override void Update(DateTime gameTime)
        {
            
        }

        public override EntityClassId ClassId
        {
            get { return EntityClassId.Zombie; }
        }

        public override string DisplayName
        {
            get { return "Zombie "+ CharacterName; }
        }
    }
}
