using SharpDX.IO;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Sound
{

    public class SoundEngine : ISoundEngine, IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private static readonly int _soundChannels = 8;
        private SourceVoice[] _soundQueues = new SourceVoice[_soundChannels]; //By default create the possibility to play 8 songs at the same time.
        private Dictionary<string, AudioBufferAndMetaData> AudioBuffers = new Dictionary<string, AudioBufferAndMetaData>();
        private MasteringVoice _masteringVoice;

        private XAudio2 _soundDevice;
        private sealed class AudioBufferAndMetaData : AudioBuffer
        {
            public WaveFormat WaveFormat { get; set; }
            public uint[] DecodedPacketsInfo { get; set; }
        }
        #endregion

        #region Public Properties
        public MasteringVoice MasteringVoice
        {
            get { return _masteringVoice; }
        }

        public XAudio2 XAudio
        {
            get { return _soundDevice; }
        }
        #endregion

        public SoundEngine()
        {
            //Create new Xaudio2 engine
            _soundDevice = new XAudio2();
            //Create a Mastering Voice
            _masteringVoice = new MasteringVoice(_soundDevice);                       //Default interface sending sound stream to Hardware
            _masteringVoice.SetVolume(1, 0);
            _soundDevice.StartEngine();
        }

        public void Dispose()
        {
            foreach(var channel in _soundQueues.Where(x => x != null)) channel.Dispose();
            _soundDevice.StopEngine();
            _soundDevice.Dispose();
            _masteringVoice.Dispose();
            _soundDevice.Dispose();
        }
        
        #region Public Methods
        public void PlaySound(string soundfile, float volume = 1, int forcedChannel = -1)
        {
            var buffer = GetBuffer(soundfile);
            SourceVoice sourceVoice;
            if (GetChannel(buffer.WaveFormat, forcedChannel, out sourceVoice))
            {
                sourceVoice.SetVolume(volume, XAudio2.CommitNow);
                sourceVoice.SubmitSourceBuffer(buffer, buffer.DecodedPacketsInfo);
                sourceVoice.Start(); //Play the song
            }
            else
            {
                logger.Warn("Sound playing skipped because no sound channel are IDLE : {0}", soundfile);
                //Error sound not played, no channel free for use !
            }
        }

        #endregion

        #region Private Methods
        private AudioBufferAndMetaData GetBuffer(string soundfile)
        {
            AudioBufferAndMetaData buffer;

            if (AudioBuffers.TryGetValue(soundfile, out buffer) == false)
            {
                //Load the sound and bufferize it
                var nativefilestream = new NativeFileStream(soundfile, NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);
                var soundstream = new SoundStream(nativefilestream);

                buffer = new AudioBufferAndMetaData()
                {
                    Stream = soundstream.ToDataStream(),
                    AudioBytes = (int)soundstream.Length,
                    Flags = BufferFlags.EndOfStream,
                    WaveFormat = soundstream.Format,
                    DecodedPacketsInfo = soundstream.DecodedPacketsInfo
                };
                AudioBuffers[soundfile] = buffer;
            }

            return buffer;
        }

        private bool GetChannel(WaveFormat waveFormat,int forcedChannel, out SourceVoice source)
        {
            if (forcedChannel > -1)
            {
                if (_soundQueues[forcedChannel] == null) _soundQueues[forcedChannel] = new SourceVoice(_soundDevice, waveFormat);
                source = _soundQueues[forcedChannel];
                return true;
            }

            for (int i = 0; i < _soundChannels; i++)
            {
                source = _soundQueues[i];
                if (source == null)
                {
                    source = _soundQueues[i] = new SourceVoice(_soundDevice, waveFormat);
                    return true;
                }
                if (source.State.BuffersQueued == 0)
                {
                    return true;
                }
            }

            //By default, if all channel are playing a sound, skip Sound playing
            source = null;
            return false;
        }
        #endregion
    }
}
