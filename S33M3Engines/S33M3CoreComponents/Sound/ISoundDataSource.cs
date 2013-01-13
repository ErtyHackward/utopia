using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundDataSource : IDisposable
    {
        string SoundAlias { get; set; }

        float SoundVolume { get; set; }
        float SoundPower { get; set; }

        DataSourcePlayMode PlayMode { get; }

        WaveFormat WaveFormat { get; set; }
        AudioBuffer AudioBuffer { get; }

        SourceCategory Category { get; set; }

        int GetSoundFormatCategory();
    }
}
