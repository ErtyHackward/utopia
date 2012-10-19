using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundEngine
    {
        void PlaySound(string soundfile, float volume = 1, int forcedChannel = -1);
    }
}
