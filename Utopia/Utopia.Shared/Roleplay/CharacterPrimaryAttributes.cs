namespace Utopia.Shared.Roleplay
{
    /// <summary>
    /// Role-playing players and NPC attributes. All parameters can be in range [0; 10]
    /// </summary>
    public class CharacterPrimaryAttributes
    {
        /// <summary>
        /// Defines how much weight a character can carry, how power is a character's attack is, how fast he can destruct a block
        /// </summary>
        public byte Strength { get; set; }
        /// <summary>
        /// Defines how far a character can see (entites, not landscape)
        /// </summary>
        public byte Perception { get; set; }
        /// <summary>
        /// Defines how long a character can run
        /// </summary>
        public byte Endurance { get; set; }
        /// <summary>
        /// Defines how the others NPC likes a character
        /// </summary>
        public byte Charisma { get; set; }
        /// <summary>
        /// Defines how much a character is smart
        /// </summary>
        public byte Intellect { get; set; }
        /// <summary>
        /// Defines how fast a character coordinates his actions
        /// </summary>
        public byte Agility { get; set; }
        /// <summary>
        /// Defines a probability in any random events
        /// </summary>
        public byte Luck { get; set; }
    }
}
