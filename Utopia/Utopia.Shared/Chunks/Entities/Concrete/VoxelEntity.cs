namespace Utopia.Shared.Chunks.Entities.Concrete
{
    /// <summary>
    /// Represents entites which has a voxel nature
    /// </summary>
    public abstract class VoxelEntity : Entity
    {
        // todo: change to real things
        public int VoxelRelatedProperty { get; set; }
        
        // we need to override save and load!

        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);

            VoxelRelatedProperty = reader.ReadInt32();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            writer.Write(VoxelRelatedProperty);
        }
    }
}
