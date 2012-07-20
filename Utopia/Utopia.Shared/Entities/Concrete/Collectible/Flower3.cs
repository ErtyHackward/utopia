namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Flower3 : Plant
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Flower3; }
        }

        public override string DisplayName
        {
            get { return "Strange flower"; }
        }

        public override string Description
        {
            get { return "Juicy green grass. Collect, dry and smoke!"; }
        }

        public override int MaxStackSize
        {
            get { return 20; }
        }
        
        public override string ModelName
        {
            get { return "Flower3"; }
        }
    }
}
