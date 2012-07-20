namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Flower2 : Plant
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Flower2; }
        }

        public override string DisplayName
        {
            get { return "Beautiful Rose"; }
        }

        public override string Description
        {
            get { return "Juicy red rose. Collect, dry and smoke!"; }
        }

        public override int MaxStackSize
        {
            get { return 20; }
        }
        
        public override string ModelName 
        { 
            get { return "Flower2"; } 
        }
    }
}
