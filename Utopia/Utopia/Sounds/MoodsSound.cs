using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Settings;
using Utopia.Shared.Sounds;

namespace Utopia.Sounds
{
    public class MoodsSoundSource : SoundSource, IUtopiaSoundSource
    {
        public TimeOfDaySound TimeOfDay { get; set; }
    }

    public enum MoodType
    {
        Peace,
        Fear,
        Dead
    }

    public class MoodSoundKey
    {
        public MoodType Type;
        public TimeOfDaySound TimeOfDay;

        public override int GetHashCode()
        {
            int hash = (int)Type;
            hash = 31 * hash + (int)TimeOfDay;
            return hash;
        }

        public override bool Equals(object obj)
        {
            MoodSoundKey objTyped = (MoodSoundKey)obj;
            return objTyped == this;
        }

        public static bool operator ==(MoodSoundKey a, MoodSoundKey b)
        {
            if ((object)a == null || (object)b == null) return false;
            if (a.TimeOfDay != b.TimeOfDay) return false;
            if (a.Type != b.Type) return false;
            return true;
        }

        public static bool operator !=(MoodSoundKey a, MoodSoundKey b)
        {
            return !(a == b);
        }
    }
}
