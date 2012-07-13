using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    //dont implement this one on server for now its merely a placeholder for having soemthing not abstract
    public class EditableVoxelEntity : Item
    {
        public override ushort ClassId
        {
            get { return EntityClassId.EditableEntity; }
        }

        public override string DisplayName
        {
            get { return "Editable"; }
        }

        public override int MaxStackSize
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string Description
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}