using System;
using Utopia.Shared.Structs;

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

        public override void Update(DynamicUpdateState gameTime)
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
