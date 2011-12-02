using Utopia.Shared.Roleplay;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents a character with special RPG system
    /// </summary>
    public abstract class SpecialCharacterEntity : CharacterEntity
    {
        protected SpecialCharacterEntity()
        {
            PrimaryAttributes = new CharacterPrimaryAttributes();
            SecondaryAttributes = new CharacterSecondaryAttributes();
            Experience = new CharacterExperience();
        }

        /// <summary>
        /// Gets character primary attributes
        /// </summary>
        public CharacterPrimaryAttributes PrimaryAttributes { get; set; }

        /// <summary>
        /// Gets character secondary attributes (skills)
        /// </summary>
        public CharacterSecondaryAttributes SecondaryAttributes { get; set; }

        /// <summary>
        /// Gets character level and experience
        /// </summary>
        public CharacterExperience Experience { get; set; }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);
            PrimaryAttributes.Save(writer);
            SecondaryAttributes.Save(writer);
            Experience.Save(writer);
        }

        public override void Load(System.IO.BinaryReader reader, EntityFactory factory)
        {
            base.Load(reader, factory);
            PrimaryAttributes.Load(reader);
            SecondaryAttributes.Load(reader);
            Experience.Load(reader);
        }
    }
}
