using System.ComponentModel;
using ProtoBuf;

namespace Utopia.Shared.Roleplay
{
    /// <summary>
    /// Secondary character's attributes in [0; 255] range
    /// </summary>
    [ProtoContract]
    public class CharacterSecondaryAttributes
    {
        /// <summary>
        /// Defines a bow attack skill
        /// </summary>
        [Description("Defines a bow attack skill")]
        [ProtoMember(1)]
        public byte Bows { get; set; }

        /// <summary>
        /// Defines a sword attack skill
        /// </summary>
        [Description("Defines a sword attack skill")]
        [ProtoMember(2)]
        public byte Swords { get; set; }

        /// <summary>
        /// Defines how much resources character can mine from one block
        /// </summary>
        [Description("Defines how much resources character can mine from one block")]
        [ProtoMember(3)]
        public byte Mine { get; set; }

        /// <summary>
        /// Defines how profitable a character can trade
        /// </summary>
        [Description("Defines how profitable a character can trade")]
        [ProtoMember(4)]
        public byte Barter { get; set; }

        /// <summary>
        /// Defines how strong a character can heal
        /// </summary>
        [Description("Defines how strong a character can heal")]
        [ProtoMember(5)]
        public byte Doctor { get; set; }

        /// <summary>
        /// Determines how well a character can fix something
        /// </summary>
        [Description("Determines how well a character can fix something")]
        [ProtoMember(6)]
        public byte Repair { get; set; }

        /// <summary>
        /// Defines how well a character can affects hi-technology thins like computers, robots, girls.
        /// </summary>
        [Description("Defines how well a character can affects hi-technology thins like computers, robots")]
        [ProtoMember(7)]
        public byte Science { get; set; }
        
        /// <summary>
        /// Determines how a character can influence others while talking
        /// </summary>
        [Description("Determines how a character can influence others while talking")]
        [ProtoMember(8)]
        public byte Speech { get; set; }

        /// <summary>
        /// Determines how well a character steals 
        /// </summary>
        [Description("Determines how well a character steals ")]
        [ProtoMember(9)]
        public byte Steal { get; set; }

        /// <summary>
        /// Defines a quality of a food that a character can produce
        /// </summary>
        [Description("Defines a quality of a food that a character can produce")]
        [ProtoMember(10)]
        public byte Cook { get; set; }

        /// <summary>
        /// Defines how well a character can sneak
        /// </summary>
        [Description("Defines how well a character can sneak")]
        [ProtoMember(11)]
        public byte Sneak { get; set; }

        /// <summary>
        /// Returns a base level of secondary skills according to a character primary skills (attributes)
        /// </summary>
        /// <param name="primary"></param>
        /// <returns></returns>
        public static CharacterSecondaryAttributes GetStartLevel(CharacterPrimaryAttributes primary)
        {
            var attr = new CharacterSecondaryAttributes();

            // todo: finish base skills level

            // strength
            // perception
            // endurance
            // charisma
            // intellect
            // agility
            // luck
            
            attr.Bows = (byte)(2 * primary.Dexterity + primary.Strength + primary.Stamina);
            attr.Swords = (byte)(3 * primary.Strength + primary.Stamina);
            attr.Mine = (byte)(3 * primary.Strength + primary.Dexterity);
            attr.Barter = (byte)(2 * primary.Charisma + primary.Perception + primary.Intellect);
            attr.Doctor = (byte)(primary.Perception + 2 * primary.Intellect + primary.Luck);
            attr.Repair = (byte)(primary.Perception + 2 * primary.Intellect + primary.Dexterity);
            attr.Science = (byte)(4 * primary.Intellect);
            attr.Speech = (byte)(4 * primary.Charisma);
            attr.Steal = (byte)(3 * primary.Dexterity + primary.Luck);
            attr.Cook = (byte)(primary.Perception + 2 * primary.Intellect + primary.Dexterity);
            attr.Sneak = (byte)(3 * primary.Dexterity + primary.Luck);

            return attr;
        }
    }
}
