using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Utopia.Shared.Chunks.Entities
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
        /// In case an entity support "growing"
        /// This variable could be use to change the texture use to display the sprite
        /// </summary>
        public byte EvolutionPhase;

        /// <summary>
        /// The Sprite scale
        /// </summary>
        public Vector3 Scale;

        /// <summary>
        /// in case we want the sprite to move with the wind
        /// </summary>
        public bool IsAnimated;

        public SpriteEntity()
        {
        }

        // we need to override save and load!
        public override void Load(System.IO.BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);
            Format = (SpriteFormat)reader.ReadByte();
            EvolutionPhase = reader.ReadByte();
            Scale = new Vector3();
            Scale.X = reader.ReadSingle();
            Scale.Y = reader.ReadSingle();
            Scale.Z = reader.ReadSingle();
            IsAnimated = reader.ReadBoolean();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write((byte)Format);
            writer.Write(EvolutionPhase);
            writer.Write(Scale.X);
            writer.Write(Scale.Y);
            writer.Write(Scale.Z);
            writer.Write(IsAnimated);
        }
    }
}
