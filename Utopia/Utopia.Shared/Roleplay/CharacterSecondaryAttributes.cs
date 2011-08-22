using System.ComponentModel;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Roleplay
{
    /// <summary>
    /// Secondary character's attributes in [0; 255] range
    /// </summary>
    public class CharacterSecondaryAttributes : IBinaryStorable
    {
        /// <summary>
        /// Defines a bow attack skill
        /// </summary>
        [Description("Defines a bow attack skill")]
        public byte Bows { get; set; }
        /// <summary>
        /// Defines a sword attack skill
        /// </summary>
        [Description("Defines a sword attack skill")]
        public byte Swords { get; set; }
        /// <summary>
        /// Defines how much resources character can mine from one block
        /// </summary>
        [Description("Defines how much resources character can mine from one block")]
        public byte Mine { get; set; }
        /// <summary>
        /// Defines how profitable a character can trade
        /// </summary>
        [Description("Defines how profitable a character can trade")]
        public byte Barter { get; set; }
        /// <summary>
        /// Defines how strong a character can heal
        /// </summary>
        [Description("Defines how strong a character can heal")]
        public byte Doctor { get; set; }
        /// <summary>
        /// Determines how well a character can fix something
        /// </summary>
        [Description("Determines how well a character can fix something")]
        public byte Repair { get; set; }
        /// <summary>
        /// Defines how well a character can affects hi-technology thins like computers, robots, girls.
        /// </summary>
        [Description("Defines how well a character can affects hi-technology thins like computers, robots")]
        public byte Science { get; set; }
        /// <summary>
        /// Determines how a character can influence others while talking
        /// </summary>
        [Description("Determines how a character can influence others while talking")]
        public byte Speech { get; set; }
        /// <summary>
        /// Determines how well a character steals 
        /// </summary>
        [Description("Determines how well a character steals ")]
        public byte Steal { get; set; }
        /// <summary>
        /// Defines a quality of a food that a character can produce
        /// </summary>
        [Description("Defines a quality of a food that a character can produce")]
        public byte Cook { get; set; }
        /// <summary>
        /// Defines how well a character can sneak
        /// </summary>
        [Description("Defines how well a character can sneak")]
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
            
            attr.Bows = (byte)(2 * primary.Agility + primary.Strength + primary.Endurance);
            attr.Swords = (byte)(3 * primary.Strength + primary.Endurance);
            attr.Mine = (byte)(3 * primary.Strength + primary.Agility);
            attr.Barter = (byte)(2 * primary.Charisma + primary.Perception + primary.Intellect);
            attr.Doctor = (byte)(primary.Perception + 2 * primary.Intellect + primary.Luck);
            attr.Repair = (byte)(primary.Perception + 2 * primary.Intellect + primary.Agility);
            attr.Science = (byte)(4 * primary.Intellect);
            attr.Speech = (byte)(4 * primary.Charisma);
            attr.Steal = (byte)(3 * primary.Agility + primary.Luck);
            attr.Cook = (byte)(primary.Perception + 2 * primary.Intellect + primary.Agility);
            attr.Sneak = (byte)(3 * primary.Agility + primary.Luck);

            return attr;
        }

        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(Bows);
            writer.Write(Swords);
            writer.Write(Mine);
            writer.Write(Barter);
            writer.Write(Doctor);
            writer.Write(Repair);
            writer.Write(Science);
            writer.Write(Speech);
            writer.Write(Steal);
            writer.Write(Cook);
            writer.Write(Sneak);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            Bows = reader.ReadByte();
            Swords = reader.ReadByte();
            Mine = reader.ReadByte();
            Barter = reader.ReadByte();
            Doctor = reader.ReadByte();
            Repair = reader.ReadByte();
            Science = reader.ReadByte();
            Speech = reader.ReadByte();
            Steal = reader.ReadByte();
            Cook = reader.ReadByte();
            Sneak = reader.ReadByte();
        }
    }
}
