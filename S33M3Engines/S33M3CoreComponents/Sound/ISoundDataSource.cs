using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundDataSource : ISoundDataSourceBase, IDisposable
    {
        DataSourcePlayMode PlayMode { get; }
        WaveFormat WaveFormat { get; set; }
        AudioBuffer AudioBuffer { get; }

        int GetSoundFormatCategory();
    }
}
