using Utopia.Shared.Roleplay;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents a character with RPG skills system
    /// </summary>
    public abstract class RpgCharacterEntity : CharacterEntity
    {
        protected RpgCharacterEntity()
        {
            PrimaryAttributes = new CharacterPrimaryAttributes();
            SecondaryAttributes = new CharacterSecondaryAttributes();
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
        /// Gets character's free experience points that he can convert into the skill points
        /// </summary>
        public uint Experience { get; set; }

        /// <summary>
        /// Returns amount of exp points needed to raise one skill point
        /// </summary>
        /// <param name="skillLevel"></param>
        /// <returns></returns>
        public int GetSkillPointPrice(byte skillLevel)
        {
            return (skillLevel / 10 + 1) * 20;
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);
            PrimaryAttributes.Save(writer);
            SecondaryAttributes.Save(writer);
            writer.Write(Experience);
        }

        public override void Load(System.IO.BinaryReader reader, EntityFactory factory)
        {
            base.Load(reader, factory);
            PrimaryAttributes.Load(reader);
            SecondaryAttributes.Load(reader);
            Experience = reader.ReadUInt32();
        }
    }
}
