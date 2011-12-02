namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Represente an entity that has a Sprite Body
    /// </summary>
    public abstract class SpriteEntity : StaticEntity
    {
        /// <summary>
        /// The sprite format, give the number of quad that will be draw, and how they intersect
        /// </summary>
        public SpriteFormat Format = SpriteFormat.Billboard;

        /// <summary>
        /// in case we want the sprite to move with the wind
        /// </summary>
        public bool IsAnimated;

        // we need to override save and load!
        public override void Load(System.IO.BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            Format = (SpriteFormat)reader.ReadByte();
            IsAnimated = reader.ReadBoolean();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write((byte)Format);
            writer.Write(IsAnimated);
        }
    }
}
