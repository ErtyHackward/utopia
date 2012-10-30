using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using S33M3CoreComponents.Maths;
using System.Diagnostics;

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
        private int _fadingSpeed = 100; // decrease/increase volume fading every 100 ms
        private float _fadingStepThreeshold = 0.0f; //Fading step per ms
        private float _fadingVolumeCoef;
        private float _voiceVolume;
        private Stopwatch _fadingTimer = new Stopwatch();
        #endregion

        #region Public Properties
        public Emitter Emitter { get; set; }
        public bool is3DSound { get; set; }
        public ISoundDataSource PlayingDataSource
        {
            get { return _playingDataSource; }
            set { _playingDataSource = value; PrepareVoiceSource(value); }
        }
        public Vector3 Position
        {
            get { return Emitter.Position; }
            set { Emitter.Position = Position; RefreshVoices(); }
        }
        public VoiceState State
        {
            get { return _voice.State; }
        }

        public bool IsLooping{ get; set; }
        public bool IsFadingMode { get; set; }
        #endregion

        public SoundVoice(ISoundEngine soundEngine, WaveFormat linkedWaveFormat, Action<IntPtr> callBack)
        {
            if (linkedWaveFormat == null) throw new ArgumentNullException();
            _callback = callBack;
            _soundEngine = soundEngine;
            _linkedWaveFormat = linkedWaveFormat;
            PrepareVoiceSource(_linkedWaveFormat);
        }

        #region Public Methods
        public void Dispose()
        {
            if (_voice != null && _voice.IsDisposed == false) _voice.Dispose();
        }

        public void RefreshVoices()
        {
            if (IsFadingMode) RefreshFading();

            if (is3DSound)
            {
                Refresh3DVoiceData();
            }
            else
            {
                Refresh2DVoiceData();
            }
        }

        //Enqueue the currently linked datasource buffer for playing
        public void PushDataSourceForPlaying()
        {
            _voice.SubmitSourceBuffer(_playingDataSource.AudioBuffer, _playingDataSource.DecodedPacketsInfo);

            if (is3DSound)
            {
                RefreshVoices();
            }
            else
            {
                //In 2D case reset the output sound value (in case it was used by a 3D sound previously)
                float[] coef = new float[_soundEngine.DeviceDetail.OutputFormat.Channels];
                for (int i = 0; i < coef.Length; i++) coef[i] = 1.0f;
                _voice.SetOutputMatrix(_playingDataSource.WaveFormat.Channels, _soundEngine.DeviceDetail.OutputFormat.Channels, coef);
            }
        }

        public void Start(float soundVolume, uint fadeIn = 0)
        {
            if (fadeIn > 0)
            {
                IsFadingMode = true;
                _fadingStepThreeshold = 1.0f / fadeIn;
                _fadingVolumeCoef = 0.0f;
                _fadingTimer.Restart();
            }
            else
            {
                _fadingVolumeCoef = 1.0f;
            }

            _voiceVolume = soundVolume;
            RefreshVoices();

            _voice.Start();
        }

        public void Start(uint fadeIn = 0)            
        {
            Start(_playingDataSource.SoundVolume, fadeIn);
        }

        public void Stop(uint fadeOut = 0)
        {
            if (fadeOut > 0)
            {
                IsFadingMode = true;
                _fadingStepThreeshold = 1.0f / fadeOut * -1;
                _fadingVolumeCoef = 1.0f;
                _fadingTimer.Restart();
            }
            else
            {
                IsFadingMode = false;
                IsLooping = false;
                _voice.Stop();
                _voice.FlushSourceBuffers();
            }
        }

        public void SetVolume(float volume, int operationSet)
        {
            _voiceVolume = volume;
            RefreshVoices();
        }
        #endregion

        #region Private Methods

        private void RefreshFading()
        {
            if (_fadingTimer.ElapsedMilliseconds > _fadingSpeed)
            {
                _fadingVolumeCoef += (_fadingStepThreeshold * _fadingTimer.ElapsedMilliseconds);
                if (_fadingVolumeCoef >= 1.0f)
                {
                    IsFadingMode = false;
                    _fadingVolumeCoef = 1.0f;
                    logger.Trace("Fading IN of sound {0} finished",_playingDataSource.SoundAlias);
                }
                else
                {
                    if (_fadingVolumeCoef <= 0.0f)
                    {
                        IsFadingMode = false;
                        _fadingVolumeCoef = 0.0f;
                        _fadingTimer.Reset();
                        Stop(0);
                        logger.Trace("Fading OUT of sound {0} finished", _playingDataSource.SoundAlias);
                        return;
                    }
                }

                _fadingTimer.Restart();
            }
        }

        private void Refresh3DVoiceData()
        {
            //Change output speakers parameter to simulate 3D sound
            DspSettings settings3D = _soundEngine.X3DAudio.Calculate(_soundEngine.Listener, Emitter, CalculateFlags.Matrix, 1, _soundEngine.DeviceDetail.OutputFormat.Channels);
            _voice.SetOutputMatrix(PlayingDataSource.WaveFormat.Channels, _soundEngine.DeviceDetail.OutputFormat.Channels, settings3D.MatrixCoefficients);

            //Set global input sound volume based on distance VS object
            float soundVolume = _fadingVolumeCoef * _voiceVolume * (1.0f - (Math.Max(0.0f, Math.Min(1.0f, settings3D.EmitterToListenerDistance / _playingDataSource.SoundPower))));
            _voice.SetVolume(soundVolume, XAudio2.CommitNow);
        }

        private void Refresh2DVoiceData()
        {
            float soundVolume = _fadingVolumeCoef * _voiceVolume;
            _voice.SetVolume(soundVolume, XAudio2.CommitNow);
        }

        private void PrepareVoiceSource(ISoundDataSource playingDataSource)
        {
            PrepareVoiceSource(playingDataSource.WaveFormat);
        }

        private void PrepareVoiceSource(WaveFormat format)
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
