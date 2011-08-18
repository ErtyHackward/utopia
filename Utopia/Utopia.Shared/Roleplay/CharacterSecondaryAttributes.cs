namespace Utopia.Shared.Roleplay
{
    /// <summary>
    /// Secondary character's attributes in [0; 255] range
    /// </summary>
    public class CharacterSecondaryAttributes
    {
        /// <summary>
        /// Defines a bow attack skill
        /// </summary>
        public byte Bows { get; set; }
        /// <summary>
        /// Defines a sword attack skill
        /// </summary>
        public byte Swords { get; set; }
        /// <summary>
        /// Defines how much resources character can mine from one block
        /// </summary>
        public byte Mine { get; set; }
        /// <summary>
        /// Defines how profitable a character can trade
        /// </summary>
        public byte Barter { get; set; }
        /// <summary>
        /// Defines how strong a character can heal
        /// </summary>
        public byte Doctor { get; set; }
        /// <summary>
        /// Determines how well a character can fix something
        /// </summary>
        public byte Repair { get; set; }
        /// <summary>
        /// Defines how well a character can affects hi-technology thins like computers, robors, girls.
        /// </summary>
        public byte Science { get; set; }
        /// <summary>
        /// Determines how a character can influence others while talking
        /// </summary>
        public byte Speech { get; set; }
        /// <summary>
        /// Determines how well a character steals 
        /// </summary>
        public byte Steal { get; set; }
        /// <summary>
        /// Defines a quality of a food that a character can produce
        /// </summary>
        public byte Cook { get; set; }
        /// <summary>
        /// Defines how well a character can sneak
        /// </summary>
        public byte Sneak { get; set; }

        /// <summary>
        /// Returns a base level of secondary skills according to a character primary skills (attributes)
        /// </summary>
        /// <param name="primary"></param>
        /// <returns></returns>
        public static CharacterSecondaryAttributes GetBaseStart(CharacterPrimaryAttributes primary)
        {
            var attr = new CharacterSecondaryAttributes();

            // todo: finish base skills level

            attr.Speech = (byte) (2*primary.Charisma);

            attr.Swords = (byte) (4*primary.Strength);

            attr.Bows = (byte) (2*primary.Agility + 2*primary.Strength);

            return attr;
        }
    }
}
