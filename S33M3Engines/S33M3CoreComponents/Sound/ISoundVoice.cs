using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.XAudio2;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundVoice : IDisposable
    {
        SourceVoice Voice { get; }
        bool IsLooping { get; set; }
        ISoundDataSource PlayingDataSource { get; set; }
    }
}
