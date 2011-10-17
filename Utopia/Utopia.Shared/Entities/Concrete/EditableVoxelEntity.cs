namespace Utopia.Shared.Entities.Concrete
{

    //dont implement this one on server for now its merely a placeholder for having soemthing not abstract
    public class EditableVoxelEntity : VoxelEntity
    {
        public override ushort ClassId
        {
            get { return EntityClassId.EditableEntity; }
        }

        public override string DisplayName
        {
            get { return "Editable"; }
        }
    }
}