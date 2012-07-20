namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Grass : Plant
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Grass; }
        }

        public override string DisplayName
        {
            get { return "Grass"; }
        }

        public override string Description
        {
            get { return "Juicy green grass. Collect, dry and smoke!"; }
        }

        public override int MaxStackSize
        {
            get
            {
                return 20;
            }
        }

        public override string ModelName
        {
            get { return "Grass1"; }
        }
    }
}
