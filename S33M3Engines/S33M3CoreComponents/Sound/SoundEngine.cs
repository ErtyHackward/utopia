﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using S33M3_DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;

namespace S33M3CoreComponents.Sound
{
    public class SoundEngine : BaseComponent, ISoundEngine
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        //States variables
        private bool _3DSoundEntitiesPositionsChanged = false;
        private Listener _listener;
        private float _defaultSoundVolume;
        private int _maxVoicePoolPerFileType = 16; //Can play up to 16 differents song in parallel for each file type

        //XAudio2 variables
        //Sound engine objects
        private XAudio2 _xaudio2;
        private X3DAudio _x3DAudio;
        private MasteringVoice _masteringVoice;
        private DeviceDetails _deviceDetail;
        private List<string> _soundDevices;

        //Buffers
        private Dictionary<string, ISoundDataSource> _soundDataSources;
        private Dictionary<int, ISoundVoice[]> _soundVoices;
        private List<ISoundVoice> _activelyLoopingSounds;

        //sound Threading Loop
        private ManualResetEvent _syncro;
        private Thread _thread;

        #endregion

        #region Public Properties
        public float DefaultSoundVolume
        {
            get { return _defaultSoundVolume; }
            set
            {
                _defaultSoundVolume = value;
                if (_masteringVoice != null) _masteringVoice.SetVolume(value, XAudio2.CommitNow);
            }
        }
        public float DefaultMaxDistance { get; set; }
        public float DefaultMinDistance { get; set; }
        public List<string> SoundDevices { get { return _soundDevices; } }
       
        #endregion

        public SoundEngine(int maxVoicesNbr = 8)
            : this(null, maxVoicesNbr)
        {
        }

        public SoundEngine(string SoundDeviceName, int maxVoicesNbr = 8)
        {
            Initialize(SoundDeviceName, maxVoicesNbr);
        }

        public override void AfterDispose()
        {
            _syncro.Set();
            while (_thread.IsAlive) { }
            _syncro.Dispose();
        }

        #region Public Methods
        public void SetListenerPosition(Vector3 pos, Vector3 lookDir, Vector3 velPerSecond, Vector3 upVector)
        {
            _listener.OrientFront = lookDir;
            _listener.Position = pos;
            _listener.Velocity = velPerSecond;
            _listener.OrientTop = upVector;
            _3DSoundEntitiesPositionsChanged = true;
        }

        public void SetListenerPosition(Vector3 pos, Vector3 lookDir)
        {
            _listener.OrientFront = lookDir;
            _listener.Position = pos;
            _listener.Velocity = Vector3.Zero;
            _listener.OrientTop = Vector3.UnitY;
            _3DSoundEntitiesPositionsChanged = true;
        }

        public void Update3DSounds()
        {
            if (!_3DSoundEntitiesPositionsChanged) return;

            foreach (var soundVoicesQueue in _soundVoices.Values)
            {
                foreach (var soundVoice in soundVoicesQueue.Where(x => x != null && x.is3DSound && (x.Voice.State.BuffersQueued > 0 || x.IsLooping)))
                {
                    soundVoice.Refresh3DParameters(); //Refresh because the Listener did change its position !
                }
            }


            _3DSoundEntitiesPositionsChanged = false;
        }

        public ISoundDataSource AddSoundSourceFromFile(string FilePath, string soundAlias, bool streamedSound = false)
        {
            ISoundDataSource soundDataSource;

            if (_soundDataSources.TryGetValue(soundAlias, out soundDataSource) == false)
            {
                //Check File
                if (File.Exists(FilePath) == false)
                {
                    logger.Error("Cannot find the sound file {0}", FilePath);
                    return null;
                }

                //Creating the source, was not existing
                soundDataSource = new SoundBufferedDataSource()
                {
                    MaxDistance = DefaultMaxDistance,
                    MinDistance = DefaultMinDistance,
                    SoundAlias = soundAlias,
                    SoundVolume = DefaultSoundVolume
                };

                if (!streamedSound)
                {
                    //Load the sound and bufferize it
                    SoundStream soundstream = new SoundStream(File.OpenRead(FilePath));
                    soundDataSource.DecodedPacketsInfo = soundstream.DecodedPacketsInfo;
                    soundDataSource.WaveFormat = soundstream.Format;

                    soundDataSource.AudioBuffer = new AudioBuffer()
                    {
                        Stream = soundstream.ToDataStream(),
                        AudioBytes = (int)soundstream.Length,
                        Flags = BufferFlags.EndOfStream
                    };
                    
                }

                //Add DataSound into collection
                _soundDataSources.Add(soundAlias, soundDataSource);
            }

            return soundDataSource;
        }

        public ISoundDataSource GetSoundSource(string soundAlias)
        {
            ISoundDataSource soundDataSource = null;
            _soundDataSources.TryGetValue(soundAlias, out soundDataSource);
            return soundDataSource;
        }

        public void RemoveAllSoundSources()
        {
            _soundDataSources.Clear();
        }

        public void RemoveSoundSource(string soundAlias)
        {
            _soundDataSources.Remove(soundAlias);
        }

        public ISoundVoice StartPlay2D(ISoundDataSource soundSource, bool playLooped = false)
        {
            if (soundSource == null) throw new ArgumentNullException();

            ISoundVoice soundVoice = null;
            if (GetVoice(soundSource, out soundVoice))
            {
                soundVoice.is3DSound = false;
                soundVoice.IsLooping = playLooped;
                soundVoice.PlayingDataSource = soundSource;
                soundVoice.Voice.SetVolume(soundSource.SoundVolume, XAudio2.CommitNow);
                soundVoice.Voice.SubmitSourceBuffer(soundSource.AudioBuffer, soundSource.DecodedPacketsInfo);
                soundVoice.Voice.Start();
                if (playLooped) _activelyLoopingSounds.Add(soundVoice);
            }
            else
            {
                logger.Warn("Sound playing skipped because no sound channel are IDLE : {0}", soundSource.SoundAlias);
            }

            return soundVoice;
        }

        public ISoundVoice StartPlay2D(string FilePath, string soundAlias, bool playLooped = false)
        {
            return StartPlay2D(AddSoundSourceFromFile(FilePath, soundAlias), playLooped);
        }

        public ISoundVoice StartPlay2D(string soundAlias, bool playLooped = false)
        {
            return StartPlay2D(AddSoundSourceFromFile(null, soundAlias), playLooped);
        }

        public ISoundVoice StartPlay3D(ISoundDataSource soundSource, Vector3 position, bool playLooped = false)
        {
            if (soundSource == null) throw new ArgumentNullException();

            ISoundVoice soundVoice = null;
            if (GetVoice(soundSource, out soundVoice))
            {
                soundVoice.is3DSound = true;
                soundVoice.Emitter.Position = position;
                soundVoice.Emitter.OrientTop = Vector3.UnitY;
                soundVoice.Emitter.Velocity = Vector3.Zero;
                soundVoice.IsLooping = playLooped;
                soundVoice.PlayingDataSource = soundSource;
                soundVoice.Voice.SetVolume(soundSource.SoundVolume, XAudio2.CommitNow);
                soundVoice.Voice.SubmitSourceBuffer(soundSource.AudioBuffer, soundSource.DecodedPacketsInfo);
                soundVoice.Refresh3DParameters();

                soundVoice.Voice.Start();
                if (playLooped) _activelyLoopingSounds.Add(soundVoice);
            }
            else
            {
                logger.Warn("Sound playing skipped because no sound channel are IDLE : {0}", soundSource.SoundAlias);
            }

            return soundVoice;

            //        public void MakeSound3D()
            //        {
            //            Emitter emitter = new Emitter();
            //            Listener listener = new Listener();
            //            emitter.ChannelCount = 1;
            //            emitter.CurveDistanceScaler = float.MinValue;

            //            emitter.OrientFront = new SharpDX.Vector3();
            //            emitter.OrientTop = new SharpDX.Vector3();
            //            emitter.Position = new SharpDX.Vector3();
            //            emitter.Velocity = new SharpDX.Vector3();

            //            listener.OrientFront = new SharpDX.Vector3();
            //            listener.OrientTop = new SharpDX.Vector3();
            //            listener.Position = new SharpDX.Vector3();
            //            listener.Velocity = new SharpDX.Vector3();

            //            var settings = _x3DAudio.Calculate(listener, emitter, CalculateFlags.Matrix, 1, _deviceDetail.OutputFormat.Channels);

            //            //Find the corresponding voice currently playing
            //            _soundQueues[0].SetOutputMatrix(1, _deviceDetail.OutputFormat.Channels, settings.MatrixCoefficients); //Change volume power.

            //        }
        }

        public ISoundVoice StartPlay3D(string soundAlias, Vector3 position, bool playLooped = false)
        {
            return StartPlay3D(AddSoundSourceFromFile(null, soundAlias), position, playLooped);
        }

        public ISoundVoice StartPlay3D(string FilePath, string soundAlias, Vector3 position, bool playLooped = false)
        {
            return StartPlay3D(AddSoundSourceFromFile(FilePath, soundAlias), position, playLooped);
        }

        public void StopAllSounds()
        {
            foreach (var soundCatQueue in _soundVoices.Values)
            {
                foreach (var sound in soundCatQueue.Where(x => x != null && x.Voice.State.BuffersQueued > 0))
                {
                    sound.Stop();
                }
            }
        }

        #endregion

        #region Private Methods
        private void Initialize(string SoundDeviceName, int maxVoicesNbr)
        {
            //Default Xaudio2 objects ==========
            _xaudio2 = ToDispose(new XAudio2());

            if (SoundDeviceName == null) _deviceDetail = _xaudio2.GetDeviceDetails(0);

            _soundDevices = new List<string>();
            int customDeviceId = 0;
            //Get all sound devices
            for (int i = 0; i < _xaudio2.DeviceCount; i++)
            {
                _soundDevices.Add(_xaudio2.GetDeviceDetails(i).DisplayName);
                if (SoundDeviceName == _xaudio2.GetDeviceDetails(i).DisplayName)
                {
                    _deviceDetail = _xaudio2.GetDeviceDetails(i);
                    customDeviceId = i;
                }
            }

            logger.Info("s33m3 sound engine started for device : " + _deviceDetail.DisplayName);

            _x3DAudio = new X3DAudio(_deviceDetail.OutputFormat.ChannelMask);

            if(SoundDeviceName == null) _masteringVoice = ToDispose(new MasteringVoice(_xaudio2));
            else _masteringVoice = ToDispose(new MasteringVoice(_xaudio2, 2, 44100, customDeviceId));

            //Default state values =============
            DefaultSoundVolume = 0.5f;
            DefaultMaxDistance = 100.0f;
            DefaultMinDistance = 0.0f;
            _maxVoicePoolPerFileType = maxVoicesNbr;

            _soundDataSources = new Dictionary<string, ISoundDataSource>();
            _soundVoices = new Dictionary<int, ISoundVoice[]>();
            _activelyLoopingSounds = new List<ISoundVoice>();

            _listener = new Listener();

            //Start Sound voice processing thread
            _syncro = new ManualResetEvent(false);
            _thread = new Thread(DataSoundPocessingAsync) { Name = "SoundEngine" }; //Start the main loop
            _thread.Start();

            _xaudio2.StartEngine();
        }

        private bool GetVoice(ISoundDataSource dataSource2Bplayed, out ISoundVoice soundVoice)
        {
            //Get the soundqueue following SoundFormatCategory
            ISoundVoice[] voiceQueue;
            if (!_soundVoices.TryGetValue(dataSource2Bplayed.GetSoundFormatCategory(), out voiceQueue))
            {
                _soundVoices.Add(dataSource2Bplayed.GetSoundFormatCategory(), voiceQueue = new ISoundVoice[_maxVoicePoolPerFileType]);
            }

            for (int i = 0; i < _maxVoicePoolPerFileType; i++)
            {
                soundVoice = voiceQueue[i];
                if (soundVoice == null)
                {
                    //logger.Info("NEW Voice Id : {0}, for queue {1}", i, dataSource2Bplayed.FormatType.ToString());
                    soundVoice = voiceQueue[i] = ToDispose(new SoundVoice(_xaudio2, dataSource2Bplayed.WaveFormat,_listener, _x3DAudio , _deviceDetail, Voice_BufferEnd));
                    soundVoice.Emitter = new Emitter();
                    return true; //Return a newly created voice 
                }
                if (soundVoice.Voice.State.BuffersQueued == 0 && soundVoice.IsLooping == false)
                {
                    //logger.Info("Reuse Voice Id {0}, for queue {1}", i, dataSource2Bplayed.FormatType.ToString());

                    return true;  //Return an already created voice, that was waiting to play a sound
                }
            }

            soundVoice = null;
            return false;
        }

        //Voice end sound reading call back
        private void Voice_BufferEnd(IntPtr obj)
        {
            _syncro.Set();
        }

        //Function that is running its own thread responsible to do background stuff concerning sound
        private void DataSoundPocessingAsync()
        {
            while (!IsDisposed)
            {
                LoopingSoundRefresh();
                _syncro.Reset();
                _syncro.WaitOne();
            }
        }

        /// <summary>
        /// Logic to implement "Looping" sound.
        /// </summary>
        /// <returns></returns>
        private void LoopingSoundRefresh()
        {
            ISoundVoice soundVoice;
            for (int i = _activelyLoopingSounds.Count - 1; i >= 0; i--)
            {
                soundVoice = _activelyLoopingSounds[i];
                if (soundVoice.Voice.State.BuffersQueued == 0)
                {
                    if (soundVoice.IsLooping)
                    {
                        soundVoice.Voice.SubmitSourceBuffer(soundVoice.PlayingDataSource.AudioBuffer, soundVoice.PlayingDataSource.DecodedPacketsInfo);
                    }
                    else
                    {
                        _activelyLoopingSounds.RemoveAt(i);
                    }
                }
            }
        }
        
        #endregion


       







    }
}
