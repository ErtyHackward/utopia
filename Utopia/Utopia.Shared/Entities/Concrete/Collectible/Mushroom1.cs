namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Mushroom1 : Plant
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Mushroom1; }
        }

        public override string DisplayName
        {
            get { return "Mushroom"; }
        }

        public override string Description
        {
            get { return "Mushroom !"; }
        }

        public override int MaxStackSize
        {
            get { return 20; }
        }
        
        public override string ModelName
        {
            get { return "Mushroom1"; }
        }
    }
}
