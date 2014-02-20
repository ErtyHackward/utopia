using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using System.Diagnostics;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundVoice : IDisposable
    {
        bool IsPlaying { get; set; }
        bool IsLooping { get; set; }
        bool IsFadingMode { get; set; }
        ISoundDataSource PlayingDataSource { get; set; }
        Emitter Emitter { get; set; }
        Vector3 Position { get; set; }
        SourceVoice Voice { get; set; }
        bool is3DSound { get; set; }
        uint MinDefferedStart { get; set; }
        uint MaxDefferedStart { get; set; }
        string Id { get; set; }

        VoiceState State { get; }
        void RefreshVoices();

        //Voice supported operations

        void SetVolume(float volume, int operationSet);

        void PushDataSourceForPlaying();

        /// <summary>
        /// Start playing the sound.
        /// <param name="fadeIn">fadeIn time in ms, 0 = directly started at full volume</param>
        /// </summary>
        void Start(uint fadeIn = 0);
        /// <summary>
        /// Start playing the sound with selected soundVolumne coef.
        /// </summary>
        /// <param name="fadeIn">fadeIn time in ms, 0 = directly started at full volume</param>
        /// <param name="soundVolume"></param>
        void Start(float soundVolume, uint fadeIn = 0);

        /// <summary>
        /// Stop the currently playing sound
        /// </summary>
        /// <param name="fadeOut">fadeOut time in ms, 0 = directly stopped</param>
        void Stop(uint fadeOut = 0);

        /// <summary>
        /// Time since the reading start up of the song
        /// </summary>
        Stopwatch PlayingTime { get; set;}

        /// <summary>
        /// Start up sound priority
        /// </summary>
        int Priority { get; set; }
    }
}
