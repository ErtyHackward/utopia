using System;
using ProtoBuf;

namespace Utopia.Shared.Roleplay
{
    /// <summary>
    /// Role-playing players and NPC attributes. All parameters can be in range [1; 10]
    /// </summary>
    [ProtoContract]
    public class CharacterPrimaryAttributes
    {
        /// <summary>
        /// Defines how much weight a character can carry, how power is a character's attack is, how fast he can destruct a block
        /// </summary>
        [ProtoMember(1)]
        public byte Strength { get; set; }

        /// <summary>
        /// Defines how much a character is smart
        /// </summary>
        [ProtoMember(2)]
        public byte Intellect { get; set; }

        /// <summary>
        /// Defines how far a character can see (entites, not landscape)
        /// </summary>
        [ProtoMember(3)]
        public byte Perception { get; set; }

        /// <summary>
        /// Defines how long a character can run
        /// </summary>
        [ProtoMember(4)]
        public byte Stamina { get; set; }

        /// <summary>
        /// Defines how the others NPC likes a character
        /// </summary>
        [ProtoMember(5)]
        public byte Charisma { get; set; }

        /// <summary>
        /// Defines how fast a character coordinates his actions
        /// </summary>
        [ProtoMember(6)]
        public byte Dexterity { get; set; }

        /// <summary>
        /// Defines a probability in any random events
        /// </summary>
        [ProtoMember(7)]
        public byte Luck { get; set; }

        /// <summary>
        /// Slightly shuffles the attributes
        /// </summary>
        /// <param name="r"></param>
        public void Randomize(Random r = null)
        {
            if(r == null) 
                r = new Random(DateTime.Now.Millisecond);

            Strength += (byte)(r.Next(2) - 1);
            Perception += (byte)(r.Next(2) - 1);
            Stamina += (byte)(r.Next(2) - 1);
            Charisma += (byte)(r.Next(2) - 1);
            Intellect += (byte)(r.Next(2) - 1);
            Dexterity += (byte)(r.Next(2) - 1);
            Luck += (byte)(r.Next(2) - 1);

            // validation
            Validate();
        }

        /// <summary>
        /// Changes all attributes to correct value range [1; 10]
        /// </summary>
        public void Validate()
        {
            if (Strength > 10) Strength = 10;
            if (Intellect > 10) Intellect = 10;
            if (Perception > 10) Perception = 10;
            if (Stamina > 10) Stamina = 10;
            if (Charisma > 10) Charisma = 10;
            if (Dexterity > 10) Dexterity = 10;
            if (Luck > 10) Luck = 10;

            if (Strength < 1) Strength = 1;
            if (Intellect < 1) Intellect = 1;
            if (Perception < 1) Perception = 1;
            if (Stamina < 1) Stamina = 1;
            if (Charisma < 1) Charisma = 1;
            if (Dexterity < 1) Dexterity = 1;
            if (Luck < 1) Luck = 1;
        }
        

        //predefined classes use SpecialClassDesigner

        public static CharacterPrimaryAttributes SwordWarrior
        {
            get 
            {
                return new CharacterPrimaryAttributes { Strength = 9, Perception = 6, Stamina = 6, Charisma = 4, Intellect = 4, Dexterity = 6, Luck = 5 };
            }
        }

        public static CharacterPrimaryAttributes BowWarrior
        {
            get
            {
                return new CharacterPrimaryAttributes { Strength = 6, Perception = 6, Stamina = 6, Charisma = 4, Intellect = 4, Dexterity = 9, Luck = 5 };
            }
        }

        public static CharacterPrimaryAttributes Doctor
        {
            get
            {
                return new CharacterPrimaryAttributes { Strength = 4, Perception = 7, Stamina = 4, Charisma = 4, Intellect = 7, Dexterity = 7, Luck = 7 };
            }
        }
    }
}
