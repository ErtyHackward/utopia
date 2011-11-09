namespace Utopia.Shared.Entities.Concrete
{
    public class Tree : MetaEntity
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Tree; }
        }

        public override string DisplayName
        {
            get { return "Tree"; }
        }
    }
}
