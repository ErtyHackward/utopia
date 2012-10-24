using S33M3DXEngine.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundEngine : IDisposable
    {
        void PlaySound(string soundfile, float volume = 1, int forcedChannel = -1);
        void StartPlayingSound(string soundfile, float volume = 1, int delay = 0);
        void StopPlayingSound(string soundfile);
    }
}
