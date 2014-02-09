using ProtoBuf;
using Utopia.Shared.Roleplay;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Represents a character with RPG skills system
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(PlayerCharacter))]
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
        [ProtoMember(1)]
        public CharacterPrimaryAttributes PrimaryAttributes { get; set; }

        /// <summary>
        /// Gets character secondary attributes (skills)
        /// </summary>
        [ProtoMember(2)]
        public CharacterSecondaryAttributes SecondaryAttributes { get; set; }

        /// <summary>
        /// Gets character's free experience points that he can convert into the skill points
        /// </summary>
        [ProtoMember(3)]
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
    }
}
