namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Cactus : Plant
    {
        public override string ModelName
        {
            get { return "Cactus"; }
        }

        public override ushort ClassId
        {
            get { return EntityClassId.Cactus; }
        }

        public override string DisplayName
        {
            get { return "Cactus"; }
        }

        public override string Description
        {
            get { return "Juicy green grass. Collect, dry and smoke!"; }
        }

        public override int MaxStackSize
        {
            get { return 20; } 
        }
    }
}
