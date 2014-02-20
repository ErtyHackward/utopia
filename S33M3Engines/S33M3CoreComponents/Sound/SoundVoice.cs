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
        private float[] _defaultChannelMapping;
        private WaveFormat _linkedWaveFormat;
        private ISoundDataSource _playingDataSource;
        private Action<IntPtr> _callback;
        private ISoundEngine _soundEngine;
        private int _fadingSpeed = 100; // decrease/increase volume fading every 100 ms
        private float _fadingStepThreeshold = 0.0f; //Fading step per ms
        private float _fadingVolumeCoef;
        private float _voiceVolume;
        private Stopwatch _fadingTimer = new Stopwatch();
        private Stopwatch _deferredStartTimer = new Stopwatch();
        private Stopwatch _playingTime = new Stopwatch();
        private uint _defferedStart;
        private bool _deferredPaused;
        private bool _isPlaying;
        private FastRandom _rnd = new FastRandom();
        private int _priority = -1;
        #endregion

        #region Public Properties
        public string Id { get; set; }

        public uint MaxDefferedStart { get; set; }
        public uint MinDefferedStart { get; set; }

        public SourceVoice Voice
        {
            get { return _voice; }
            set { _voice = value; }
        }

        public Stopwatch PlayingTime
        {
            get { return _playingTime; }
            set { _playingTime = value; }
        }

        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public bool IsPlaying
        {
            get
            {
                if (_isPlaying && _voice.State.BuffersQueued == 0 && IsLooping == false && _playingDataSource.PlayMode == DataSourcePlayMode.Buffered)
                {
                    Stop();
                }
                return _isPlaying;
            }
            set { _isPlaying = value; }
        }

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
            if (_voice != null && _voice.IsDisposed == false)
            {
                _voice.Stop();
                _voice.FlushSourceBuffers();
                _voice.DestroyVoice();
                _voice.Dispose();
            }
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
            //Do nothing, start will be deferred !
            if (MaxDefferedStart > 0)
            {
                //Not playing the sound with Buffered start => Assign new rnd start
                if (_voice.State.BuffersQueued == 0 && _deferredPaused == false)
                {
                    AssignNewRndStart();
                    _deferredPaused = true;
                    logger.Trace("AssignNewRndStart {0} {1}", this.Id, this.PlayingDataSource.Alias);
                }

                if (_defferedStart > _deferredStartTimer.ElapsedMilliseconds) return;
                _deferredPaused = false;
            }

            if (_playingDataSource is SoundStreamedDataSource)
            {
                ((SoundStreamedDataSource)_playingDataSource).StartVoiceDataFetching(this);
            }
            else
            {
                _voice.SubmitSourceBuffer(_playingDataSource.AudioBuffer, null);
            }

            if (is3DSound)
            {
                RefreshVoices();
            }
            else
            {
                //Reset Default Channel Mapping
                _voice.SetOutputMatrix(_playingDataSource.WaveFormat.Channels, _soundEngine.DeviceDetail.OutputFormat.Channels, _defaultChannelMapping);
            }
        }

        public void Start(float soundVolume, uint fadeIn = 0)
        {
            _playingTime.Start();
            if (fadeIn > 0 && !is3DSound)
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

            if (MaxDefferedStart > 0) _deferredPaused = false;

            _voiceVolume = soundVolume;
            RefreshVoices();

            _voice.Start();

            IsPlaying = true;
        }

        public void Start(uint fadeIn = 0)            
        {
            Start(_playingDataSource.Volume, fadeIn);
        }

        public void Stop(uint fadeOut = 0)
        {
            _playingTime.Stop();

            if (fadeOut > 0 && !is3DSound)
            {
                IsFadingMode = true;
                _fadingStepThreeshold = 1.0f / fadeOut * -1;
                _fadingVolumeCoef = 1.0f;
                _fadingTimer.Restart();
                _soundEngine.AddSoundWatching(this);
            }
            else
            {
                IsFadingMode = false;
                IsLooping = false;
                _voice.Stop();
                _fadingTimer.Stop();
                _voice.FlushSourceBuffers();
                _defferedStart = 0;
                MaxDefferedStart = 0;
                MinDefferedStart = 0;
                IsPlaying = false;
            }
        }

        public void SetVolume(float volume, int operationSet)
        {
            _voiceVolume = volume;
            RefreshVoices();
        }
        #endregion

        #region Private Methods

        private void AssignNewRndStart()
        {
            if (MinDefferedStart > MaxDefferedStart) MaxDefferedStart = MinDefferedStart;
            _defferedStart = (uint)_rnd.Next((int)MinDefferedStart, (int)MaxDefferedStart);
            _deferredStartTimer.Restart();
        }

        private void RefreshFading()
        {
            if (_fadingTimer.ElapsedMilliseconds > _fadingSpeed)
            {
                _fadingVolumeCoef += (_fadingStepThreeshold * _fadingTimer.ElapsedMilliseconds);
                if (_fadingVolumeCoef >= 1.0f)
                {
                    IsFadingMode = false;
                    _fadingVolumeCoef = 1.0f;
                    logger.Trace("Fading IN of sound {0} finished",_playingDataSource.Alias);
                }
                else
                {
                    if (_fadingVolumeCoef <= 0.0f)
                    {
                        IsFadingMode = false;
                        _fadingVolumeCoef = 0.0f;
                        _fadingTimer.Reset();
                        Stop(0);
                        logger.Trace("Fading OUT of sound {0} finished", _playingDataSource.Alias);
                        IsPlaying = false;
                        return;
                    }
                }

                _fadingTimer.Restart();
            }
        }

        private void Refresh3DVoiceData()
        {
            //Change output speakers parameter to simulate 3D sound
            DspSettings settings3D = _soundEngine.X3DAudio.Calculate(_soundEngine.Listener, Emitter, CalculateFlags.Matrix, PlayingDataSource.WaveFormat.Channels, _soundEngine.DeviceDetail.OutputFormat.Channels);
            _voice.SetOutputMatrix(PlayingDataSource.WaveFormat.Channels, _soundEngine.DeviceDetail.OutputFormat.Channels, settings3D.MatrixCoefficients);

            //Set global input sound volume based on distance VS object
            float soundVolume = _voiceVolume * 
                                (1.0f - (Math.Max(0.0f, Math.Min(1.0f, settings3D.EmitterToListenerDistance / _playingDataSource.Power)))) *
                                (_playingDataSource.Category == SourceCategory.Music ? _soundEngine.GlobalMusicVolume : _soundEngine.GlobalFXVolume);
            _voice.SetVolume(soundVolume, XAudio2.CommitNow);
        }

        private void Refresh2DVoiceData()
        {
            float soundVolume = _fadingVolumeCoef * _voiceVolume * (_playingDataSource.Category == SourceCategory.Music ? _soundEngine.GlobalMusicVolume : _soundEngine.GlobalFXVolume);
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
                //Voice Creation
                _voice = new SourceVoice(_soundEngine.Xaudio2, _linkedWaveFormat, true);

                //Do we have a special channel sound Mapping for this channel/speaker configuration ?
                float[] customMapping;
                if (_soundEngine.GetCustomChannelMapping(_voice.VoiceDetails.InputChannelCount, _soundEngine.DeviceDetail.OutputFormat.Channels, out customMapping))
                {
                    _defaultChannelMapping = customMapping;
                    _voice.SetOutputMatrix(_voice.VoiceDetails.InputChannelCount, _soundEngine.DeviceDetail.OutputFormat.Channels, _defaultChannelMapping);
                }
                else
                {
                    //Get default channel mapping
                    _defaultChannelMapping = new float[_voice.VoiceDetails.InputChannelCount * _soundEngine.DeviceDetail.OutputFormat.Channels];
                    _voice.GetOutputMatrix(null, _voice.VoiceDetails.InputChannelCount, _soundEngine.DeviceDetail.OutputFormat.Channels, _defaultChannelMapping);
                }
                
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
