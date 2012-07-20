namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Flower1 : Plant
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Flower1; }
        }

        public override string DisplayName
        {
            get { return "Small flowers"; }
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
            get { return "Flower1"; }
        }
    }
}
