namespace Utopia.Shared.Chunks.Entities.Concrete
{
    /// <summary>
    /// Represents a voxel sword
    /// </summary>
    public class SwordEntity : VoxelEntity
    {
        /// <summary>
        /// Gets or sets sword wear
        /// </summary>
        public byte Wear { get; set; }

        // we need to override save and load!

        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);

            Wear = reader.ReadByte();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            writer.Write(Wear);
        }
    }
}
