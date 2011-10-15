using SharpDX;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Represente an entity that has a Sprite Body
    /// </summary>
    public abstract class SpriteEntity: Entity
    {
        /// <summary>
        /// The sprite format, give the number of quad that will be draw, and how they intersect
        /// </summary>
        public SpriteFormat Format;

        /// <summary>
        /// The Sprite scale
        /// </summary>
        public Vector3 Scale;

        /// <summary>
        /// in case we want the sprite to move with the wind
        /// </summary>
        public bool IsAnimated;

        /// <summary>
        /// in case we want the sprite to move with the wind
        /// </summary>
        public bool IsColladable;

        public SpriteEntity()
        {
        }

        // we need to override save and load!
        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);
            Format = (SpriteFormat)reader.ReadByte();
            Scale = new Vector3();
            Scale.X = reader.ReadSingle();
            Scale.Y = reader.ReadSingle();
            Scale.Z = reader.ReadSingle();
            IsAnimated = reader.ReadBoolean();
            IsColladable = reader.ReadBoolean();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write((byte)Format);
            writer.Write(Scale.X);
            writer.Write(Scale.Y);
            writer.Write(Scale.Z);
            writer.Write(IsAnimated);
            writer.Write(IsColladable);
        }
    }
}
