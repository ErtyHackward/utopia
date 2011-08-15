namespace Utopia.Shared.Chunks.Entities.Concrete
{
    /// <summary>
    /// Represents a voxel weapon
    /// </summary>
    public class WeaponEntity : VoxelEntity
    {
        /// <summary>
        /// Gets or sets weapon wear
        /// </summary>
        public byte Wear { get; set; }

        /// <summary>
        /// This is name can vary for concrete class instance (Example: Simon's steel sword)
        /// </summary>
        public string UniqueName { get; set; }

        // we need to override save and load!

        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);

            Wear = reader.ReadByte();
            UniqueName = reader.ReadString();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);

            writer.Write(Wear);
            writer.Write(UniqueName);
        }

        /// <summary>
        /// Gets a displayed entity name
        /// </summary>
        public override string DisplayName
        {
            get { return UniqueName; }
        }
    }
}
