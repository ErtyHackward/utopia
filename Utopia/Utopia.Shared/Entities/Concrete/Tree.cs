using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    public class Tree : Entity, IStaticEntity
    {
        public override ushort ClassId
        {
            get { return EntityClassId.Tree; }
        }

        public override string DisplayName
        {
            get { return "Tree"; }
        }

        public uint StaticId
        {
            get;
            set;
        }
    }
}
