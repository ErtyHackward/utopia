using System;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Roleplay
{
    /// <summary>
    /// Represents character level and experience container
    /// </summary>
    public class CharacterExperience : IBinaryStorable
    {
        static CharacterExperience()
        {
            Step = 120;    
        }

        /// <summary>
        /// Gets difference in amount of experience points between levels
        /// </summary>
        public static int Step { get; private set; }

        /// <summary>
        /// Gets experience points thresold for level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static uint LevelThresold(int level)
        {
            return (uint)(Step * (level - 1) * level * 0.5);
        }

        /// <summary>
        /// Occurs when charater gets new level
        /// </summary>
        public event EventHandler LevelUp;

        private void OnLevelUp()
        {
            EventHandler handler = LevelUp;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Gets character next level thresold
        /// </summary>
        public uint NextLevelPoints
        {
            get { return LevelThresold(Level + 1); }
        }
        
        /// <summary>
        /// Gets character experience points
        /// </summary>
        public uint ExperiencePoints { get; private set; }
        
        /// <summary>
        /// Gets character level
        /// </summary>
        public byte Level { get; private set; }

        /// <summary>
        /// Adds experience to the character (raises LevelUp event when needed)
        /// </summary>
        /// <param name="experience"></param>
        public void AddExperience(int experience)
        {
            ExperiencePoints = (uint)(ExperiencePoints + experience);
            while (ExperiencePoints >= NextLevelPoints)
            {
                Level++;
                OnLevelUp();
            }
        }

        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(Level);
            writer.Write(ExperiencePoints);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            Level = reader.ReadByte();
            ExperiencePoints = reader.ReadUInt32();
        }


    }
}
