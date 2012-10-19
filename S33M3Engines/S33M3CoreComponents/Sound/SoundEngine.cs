using SharpDX.IO;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Sound
{

    public class SoundEngine : ISoundEngine
    {
        #region Private Variables
        private Dictionary<string, SourceVoice> _loadedSounds = new Dictionary<string, SourceVoice>();
        private Dictionary<string, AudioBufferAndMetaData> AudioBuffers = new Dictionary<string, AudioBufferAndMetaData>();
        private MasteringVoice _masteringVoice;

        private static XAudio2 _xAudio;

        private sealed class AudioBufferAndMetaData : AudioBuffer
        {
            public WaveFormat WaveFormat { get; set; }
            public uint[] DecodedPacketsInfo { get; set; }
        }
        #endregion

        #region Public Properties
        public MasteringVoice MasteringVoice
        {
            get
            {
                if (_masteringVoice == null)
                {
                    _masteringVoice = new MasteringVoice(_xAudio);
                    _masteringVoice.SetVolume(1, 0);
                }
                return _masteringVoice;
            }
        }
        #endregion

        public SoundEngine()
        {
        }
        
        #region Public Methods
        public void PlaySound(string soundfile, float volume = 1)
        {
            SourceVoice sourceVoice;
            if (!_loadedSounds.ContainsKey(soundfile))
            {
                var buffer = GetBuffer(soundfile);
                sourceVoice = new SourceVoice(_xAudio, buffer.WaveFormat, true);
                sourceVoice.SetVolume(volume, SharpDX.XAudio2.XAudio2.CommitNow);
                sourceVoice.SubmitSourceBuffer(buffer, buffer.DecodedPacketsInfo);
                sourceVoice.Start();
            }
            else
            {
                sourceVoice = _loadedSounds[soundfile];
                if (sourceVoice != null) sourceVoice.Stop();
            }
        }
        #endregion

        #region Private Methods
        private AudioBufferAndMetaData GetBuffer(string soundfile)
        {
            if (!AudioBuffers.ContainsKey(soundfile))
            {
                var nativefilestream = new NativeFileStream(soundfile, NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);

                var soundstream = new SoundStream(nativefilestream);

                var buffer = new AudioBufferAndMetaData()
                {
                    Stream = soundstream.ToDataStream(),
                    AudioBytes = (int)soundstream.Length,
                    Flags = BufferFlags.EndOfStream,
                    WaveFormat = soundstream.Format,
                    DecodedPacketsInfo = soundstream.DecodedPacketsInfo
                };
                AudioBuffers[soundfile] = buffer;
            }
            return AudioBuffers[soundfile];

        }
        #endregion
    }
}
