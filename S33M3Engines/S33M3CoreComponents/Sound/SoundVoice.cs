using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Sound
{
    public class SoundVoice : ISoundVoice, IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private SourceVoice _voice;
        private WaveFormat _linkedWaveFormat;
        private ISoundDataSource _playingDataSource;
        private Action<IntPtr> _callback;
        private ISoundEngine _soundEngine;
        #endregion

        #region Public Properties
        public Emitter Emitter { get; set; }
        public bool is3DSound { get; set; }
        public SourceVoice Voice { get { return _voice; } }
        public ISoundDataSource PlayingDataSource
        {
            get { return _playingDataSource; }
            set { _playingDataSource = value; CheckSourceVoice(value); }
        }
        public Vector3 Position
        {
            get { return Emitter.Position; }
            set { Emitter.Position = Position; Refresh3DParameters(); }
        }
        public bool IsLooping{ get; set; }
        #endregion

        public SoundVoice(ISoundEngine soundEngine, WaveFormat linkedWaveFormat, Action<IntPtr> callBack)
        {
            if (linkedWaveFormat == null) throw new ArgumentNullException();
            _callback = callBack;
            _soundEngine = soundEngine;
            _linkedWaveFormat = linkedWaveFormat;
            CheckSourceVoice(_linkedWaveFormat);
        }

        #region Public Methods
        public void Dispose()
        {
            if (_voice != null && _voice.IsDisposed == false) _voice.Dispose();
        }

        public void Refresh3DParameters()
        {
            if (is3DSound)
            {
                DspSettings settings3D = _soundEngine.X3DAudio.Calculate(_soundEngine.Listener, Emitter, CalculateFlags.Matrix | CalculateFlags.Doppler, 1, _soundEngine.DeviceDetail.OutputFormat.Channels);
                float soundVolume = _playingDataSource.SoundVolume * (1.0f - (Math.Max(0.0f, Math.Min(1.0f, settings3D.EmitterToListenerDistance / _playingDataSource.SoundPower))));

                Voice.SetVolume(soundVolume, XAudio2.CommitNow);
                Voice.SetOutputMatrix(1, _soundEngine.DeviceDetail.OutputFormat.Channels, settings3D.MatrixCoefficients);
                Voice.SetFrequencyRatio(settings3D.DopplerFactor);
            }
        }

        public void Stop()
        {
            //Reset to default ChannelVolumes

            IsLooping = false;
            _voice.Stop();
            _voice.FlushSourceBuffers();
        }
        #endregion

        #region Private Methods
        private void CheckSourceVoice(ISoundDataSource playingDataSource)
        {
            CheckSourceVoice(playingDataSource.WaveFormat);
        }

        private void CheckSourceVoice(WaveFormat format)
        {
            if (_voice == null)
            {
                _linkedWaveFormat = format;
                _voice = new SourceVoice(_soundEngine.Xaudio2, _linkedWaveFormat, true);
                _voice.BufferEnd += _callback;
            }
            else
            {
                if (_linkedWaveFormat.SampleRate != format.SampleRate)
                {
                    _voice.SourceSampleRate = format.SampleRate;
                    _linkedWaveFormat = format;
                }
            }
        }
        #endregion
    }
}
