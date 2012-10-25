using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundVoice : IDisposable
    {
        SourceVoice Voice { get; }
        bool IsLooping { get; set; }
        ISoundDataSource PlayingDataSource { get; set; }
        Emitter Emitter { get; set; }
        Vector3 Position { get; set; }
        bool is3DSound { get; set; }
        void Refresh3DParameters();
        void Stop();
    }
}
