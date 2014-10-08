using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using SharpDX.MediaFoundation;

namespace S33M3CoreComponents.Sound
{
    public class SoundEngine : BaseComponent, ISoundEngine
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        //States variables
        private bool _3DSoundEntitiesPositionsChanged = false;
        private Listener _listener;
        private float _generalSoundVolume;
        private int _maxVoicePoolPerFileType = 16; //Can play up to 32 differents song in parallel for each file type
        private D3DEngine _d3dEngine;

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
        private List<ISoundVoice> _soundProcessingQueue;
        private Object _soundQueueSync = new Object();

        //sound Threading Loop
        private ManualResetEvent _syncro;
        private Thread _thread;
        private bool _stopThreading;

        //Will hold custom Channel Mapping arrays
        private Dictionary<Vector2I, float[]> _customChannelMapping = new Dictionary<Vector2I, float[]>();

        private float _globalMusicVolume;
        private float _globalFXVolume;

        #endregion

        #region Public Properties
        public float GlobalMusicVolume
        {
            get { return _globalMusicVolume; }
            set { _globalMusicVolume = value; RefreshMusicSoundVolume(); }
        }

        public float GlobalFXVolume
        {
            get { return _globalFXVolume; }
            set { _globalFXVolume = value; RefreshFXSoundVolume(); }
        }

        public Listener Listener
        {
            get { return _listener; }
        }

        public DeviceDetails DeviceDetail
        {
            get { return _deviceDetail; }
        }

        public X3DAudio X3DAudio
        {
            get { return _x3DAudio; }
        }

        public XAudio2 Xaudio2
        {
            get { return _xaudio2; }
        }

        public float GeneralSoundVolume
        {
            get { return _generalSoundVolume; }
            set
            {
                _generalSoundVolume = value;
                if (_masteringVoice != null) _masteringVoice.SetVolume(value, XAudio2.CommitNow);
            }
        }
        public List<string> SoundDevices { get { return _soundDevices; } }
        #endregion

        public SoundEngine(D3DEngine d3dEngine, int maxVoicesNbr = 8)
            : this(d3dEngine, null, maxVoicesNbr)
        {
        }

        public SoundEngine(D3DEngine d3dEngine, string SoundDeviceName, int maxVoicesNbr = 8)
        {
            _d3dEngine = d3dEngine;
            Initialize(SoundDeviceName, maxVoicesNbr);
        }

        public override void BeforeDispose()
        {
            _d3dEngine.OnShuttingDown -= d3dEngine_OnShuttingDown;
            _stopThreading = true;
            _syncro.Set();
            while (_thread.IsAlive) { }
            foreach (var dataSources in _soundDataSources.Values) dataSources.Dispose();
            _syncro.Dispose();
        }

        #region Public Methods

        public bool AddCustomChannelMapping(int inputChannelNbr, int outputChannelNbr, float[] mapping)
        {
            Vector2I channelConfig = new Vector2I(inputChannelNbr, outputChannelNbr);
            if (_customChannelMapping.ContainsKey(channelConfig) == false)
            {
                _customChannelMapping.Add(channelConfig, mapping);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveCustomChannelMapping(int inputChannelNbr, int outputChannelNbr)
        {
            Vector2I channelConfig = new Vector2I(inputChannelNbr, outputChannelNbr);
            if (_customChannelMapping.ContainsKey(channelConfig))
            {
                _customChannelMapping.Remove(channelConfig);
                return true;
            }
            return false;
        }

        public bool GetCustomChannelMapping(int inputChannelNbr, int outputChannelNbr, out float[] mapping)
        {
            return _customChannelMapping.TryGetValue(new Vector2I(inputChannelNbr, outputChannelNbr), out mapping);
        }

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
                foreach (var soundVoice in soundVoicesQueue.Where(x => x != null && x.is3DSound && (x.State.BuffersQueued > 0 || x.IsLooping)))
                {
                    soundVoice.RefreshVoices(); //Refresh because the Listener did change its position !
                }
            }
            _3DSoundEntitiesPositionsChanged = false;
        }

        public void StopAllSounds()
        {
            foreach (var soundVoicesQueue in _soundVoices.Values)
            {
                foreach (var soundVoice in soundVoicesQueue.Where(x => x != null && x.IsPlaying))
                {
                    soundVoice.Stop();
                }
            }
        }

        public void AddSoundWatching(ISoundVoice voice)
        {
            lock (_soundQueueSync)
            {
                _soundProcessingQueue.Add(voice);
            }
            _syncro.Set();
        }

        #region Sounds repository management - creation/buffering

        public ISoundDataSource AddSoundSourceFromFile(string FilePath, string soundAlias, SourceCategory Category, bool? streamedSound = null, float soundPower = 16, int priority = 0)
        {
            ISoundDataSource soundDataSource;

            if (string.IsNullOrEmpty(soundAlias)) soundAlias = FilePath;

            if (_soundDataSources.TryGetValue(soundAlias, out soundDataSource) == false)
            {
                FileInfo fi = new FileInfo(FilePath);

                //Check File
                if (File.Exists(FilePath) == false)
                {
                    logger.Error("Cannot find the sound file {0}", FilePath);
                    return null;
                }

                string ext = fi.Extension.ToLower();
                if (ext != ".wav" && ext != ".wma")
                {
                    logger.Error("Cannot play the sound file format {0}", FilePath);
                    return null;
                }

                //By default wma files will be streamed, wav file will be buffered.
                bool isStream = false;
                if (streamedSound == null)
                {
                    if (ext == ".wav" && streamedSound == null) isStream = false;
                    else if (ext == ".wma" && streamedSound == null) isStream = true;
                }
                else
                {
                    isStream = (bool)streamedSound;
                }

                if (!isStream)
                {
                    soundDataSource = new SoundBufferedDataSource(fi);
                }
                else
                {
                    soundDataSource = new SoundStreamedDataSource(fi);
                }

                soundDataSource.Priority = priority;
                soundDataSource.Alias = soundAlias;
                soundDataSource.Power = soundPower;
                soundDataSource.Category = Category;

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

        #endregion

        #region 2d Sound playing

        public ISoundVoice StartPlay2D(ISoundDataSource soundSource, float volume, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            if (soundSource == null) throw new ArgumentNullException();

            ISoundVoice soundVoice = null;
            //Get an Idle voice ready to play a buffer
            if (GetVoice(soundSource, out soundVoice))
            {
                soundVoice.is3DSound = false;
                soundVoice.IsLooping = playLooped;
                soundVoice.PlayingDataSource = soundSource;
                if (maxDefferedStart == 0) soundVoice.PushDataSourceForPlaying();
                soundVoice.RefreshVoices();
                soundVoice.MaxDefferedStart = maxDefferedStart;
                soundVoice.MinDefferedStart = minDefferedStart;
                soundVoice.Start(volume, fadeIn);
                if (playLooped || fadeIn > 0 || maxDefferedStart > 0)
                {
                    AddSoundWatching(soundVoice);
                }
            }
            else
            {
                logger.Warn("Sound playing skipped because no sound channel are IDLE : {0}", soundSource.Alias);
            }

            return soundVoice;
        }

        public ISoundVoice StartPlay2D(ISoundDataSource soundSource, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            return StartPlay2D(soundSource, soundSource.Volume, playLooped, fadeIn, minDefferedStart, maxDefferedStart);
        }

        public ISoundVoice StartPlay2D(ISoundDataSourceBase soundSource, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            return StartPlay2D(AddSoundSourceFromFile(soundSource.FilePath, soundSource.Alias, soundSource.Category, soundSource.isStreamed, soundSource.Power), soundSource.Volume, playLooped, fadeIn, minDefferedStart, maxDefferedStart);
        }

        public ISoundVoice StartPlay2D(string soundAlias, float volume, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            return StartPlay2D(AddSoundSourceFromFile(null, soundAlias, Category), volume, playLooped, fadeIn, minDefferedStart, maxDefferedStart);
        }

        public ISoundVoice StartPlay2D(string FilePath, string soundAlias, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0, int priority = 0)
        {
            return StartPlay2D(AddSoundSourceFromFile(FilePath, soundAlias, Category), playLooped, fadeIn, minDefferedStart, maxDefferedStart);
        }

        public ISoundVoice StartPlay2D(string soundAlias, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            return StartPlay2D(AddSoundSourceFromFile(null, soundAlias, Category), playLooped, fadeIn, minDefferedStart, maxDefferedStart);
        }

        #endregion

        #region 3d Sound playing

        public ISoundVoice StartPlay3D(ISoundDataSource soundSource, Vector3 position, float volume, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            if (soundSource == null) throw new ArgumentNullException();

            ISoundVoice soundVoice = null;
            if (GetVoice(soundSource, out soundVoice))
            {
                soundVoice.Emitter.Position = position;
                soundVoice.Emitter.OrientTop = Vector3.UnitY;
                soundVoice.Emitter.Velocity = Vector3.Zero;

                soundVoice.is3DSound = true;
                soundVoice.IsLooping = playLooped;
                soundVoice.PlayingDataSource = soundSource;
                if (maxDefferedStart == 0) soundVoice.PushDataSourceForPlaying();
                soundVoice.RefreshVoices();
                soundVoice.MaxDefferedStart = maxDefferedStart;
                soundVoice.MinDefferedStart = minDefferedStart;
                soundVoice.Start(volume);
                if (playLooped || maxDefferedStart > 0)
                {
                    AddSoundWatching(soundVoice);
                }
            }
            else
            {
                logger.Warn("Sound playing skipped because no sound channel are IDLE : {0}", soundSource.Alias);
            }

            return soundVoice;
        }

        public ISoundVoice StartPlay3D(ISoundDataSource soundSource, Vector3 position, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            if (soundSource != null) return StartPlay3D(soundSource, position, soundSource.Volume, playLooped, minDefferedStart, maxDefferedStart);
            return null;
        }

        public ISoundVoice StartPlay3D(ISoundDataSourceBase soundSource, Vector3 position, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            return StartPlay3D(AddSoundSourceFromFile(soundSource.FilePath, soundSource.Alias, soundSource.Category, soundSource.isStreamed, soundSource.Power, soundSource.Priority), position, soundSource.Volume, playLooped, minDefferedStart, maxDefferedStart);
        }

        public ISoundVoice StartPlay3D(string soundAlias, float volume, Vector3 position, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            return StartPlay3D(AddSoundSourceFromFile(null, soundAlias, Category), position, volume, playLooped, minDefferedStart, maxDefferedStart);
        }

        public ISoundVoice StartPlay3D(string soundAlias, Vector3 position, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0)
        {
            return StartPlay3D(AddSoundSourceFromFile(null, soundAlias, Category), position, playLooped, minDefferedStart, maxDefferedStart);
        }

        public ISoundVoice StartPlay3D(string FilePath, string soundAlias, Vector3 position, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0, int priority = 0)
        {
            return StartPlay3D(AddSoundSourceFromFile(FilePath, soundAlias, Category, priority: priority), position, playLooped, minDefferedStart, maxDefferedStart);
        }

        #endregion

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

            if (SoundDeviceName == null) _masteringVoice = ToDispose(new MasteringVoice(_xaudio2, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate, 0));
            else _masteringVoice = ToDispose(new MasteringVoice(_xaudio2, _deviceDetail.OutputFormat.Channels, _deviceDetail.OutputFormat.SampleRate, customDeviceId));

            //Default state values =============
            _maxVoicePoolPerFileType = maxVoicesNbr;

            _soundDataSources = new Dictionary<string, ISoundDataSource>();
            _soundVoices = new Dictionary<int, ISoundVoice[]>();
            _soundProcessingQueue = new List<ISoundVoice>();

            _listener = new Listener();

            //Start Sound voice processing thread
            _syncro = new ManualResetEvent(false);
            _d3dEngine.RunningThreadedWork.Add("SoundEngine");
            _d3dEngine.OnShuttingDown += d3dEngine_OnShuttingDown;
            _thread = new Thread(DataSoundPocessingAsync) { Name = "SoundEngine" }; //Start the main loop
            _stopThreading = false;
            _thread.Start();

            GeneralSoundVolume = 1.0f;

            GlobalMusicVolume = 1;
            GlobalFXVolume = 1;

            _xaudio2.StartEngine();
        }

        private void RefreshMusicSoundVolume()
        {
            foreach (var soundVoicesQueue in _soundVoices.Values)
            {
                foreach (var soundVoice in soundVoicesQueue.Where(x => x != null && x.is3DSound == false && x.IsPlaying && x.PlayingDataSource.Category == SourceCategory.Music))
                {
                    soundVoice.RefreshVoices();
                }
            }
        }

        private void RefreshFXSoundVolume()
        {
            foreach (var soundVoicesQueue in _soundVoices.Values)
            {
                foreach (var soundVoice in soundVoicesQueue.Where(x => x != null && x.is3DSound == false && x.IsPlaying && x.PlayingDataSource.Category == SourceCategory.FX))
                {
                    soundVoice.RefreshVoices();
                }
            }
        }

        private bool GetVoice(ISoundDataSource dataSource2Bplayed, out ISoundVoice soundVoice)
        {
            //Get the soundqueue following SoundFormatCategory
            ISoundVoice[] voiceQueue;
            if (!_soundVoices.TryGetValue(dataSource2Bplayed.GetSoundFormatCategory(), out voiceQueue))
            {
                _soundVoices.Add(dataSource2Bplayed.GetSoundFormatCategory(), voiceQueue = new ISoundVoice[_maxVoicePoolPerFileType]);
            }

            //Check for a voice IDLE in the voice pool for this type of sound file.
            long oldestTimerTick = -1;
            ISoundVoice oldestSoundVoice = null;
            for (int i = 0; i < _maxVoicePoolPerFileType; i++)
            {
                soundVoice = voiceQueue[i];
                if (soundVoice == null)
                {
                    //logger.Info("NEW Voice Id : {0}, for queue {1} song playing {2}", i, dataSource2Bplayed.WaveFormat.ToString(), dataSource2Bplayed.SoundAlias);
                    soundVoice = voiceQueue[i] = ToDispose(new SoundVoice(this, dataSource2Bplayed.WaveFormat, Voice_BufferEnd));
                    soundVoice.Id = i + "  dataSource2Bplayed.WaveFormat.ToString()";
                    soundVoice.Emitter = new Emitter()
                    {
                        ChannelCount = 1,
                        CurveDistanceScaler = float.MinValue
                    };
                    return true; //Return a newly created voice 
                }
                if (soundVoice.IsPlaying == false)
                {                    
                    //logger.Info("Reuse Voice Id {0}, for queue {1} song playing {2}", i, dataSource2Bplayed.WaveFormat.ToString(), dataSource2Bplayed.SoundAlias);

                    return true;  //Return an already created voice, that was waiting to play a sound
                }
                else
                {
                    //If a priority is given, then try to find the oldest sound with lower or equal priority
                    if (dataSource2Bplayed.Priority > 0 && soundVoice.Priority <= dataSource2Bplayed.Priority)
                    {
                        if (oldestTimerTick < soundVoice.PlayingTime.ElapsedTicks)
                        {
                            oldestSoundVoice = soundVoice;
                            oldestTimerTick = soundVoice.PlayingTime.ElapsedTicks;
                        }
                    }
                }
            }

            if (oldestSoundVoice != null)
            {
                oldestSoundVoice.Stop();
                soundVoice = oldestSoundVoice;
                return true;
            }
            else
            {
                soundVoice = null;
                return false;
            }
        }


        private void d3dEngine_OnShuttingDown(object sender, EventArgs e)
        {
            _syncro.Set();
        }

        //Voice end sound reading call back
        private void Voice_BufferEnd(IntPtr obj)
        {
            try
            {
                if (!_stopThreading) _syncro.Set();
            }
            catch (Exception)
            {
            }
        }


        //ASYNC called methods responsible to do actions against WatchSound added (looping, fadding) =======================================

        //Function that is running its own thread responsible to do background stuff concerning sound
        private void DataSoundPocessingAsync()
        {
            while (!IsDisposed && !_stopThreading && !_d3dEngine.IsShuttingDownRequested)
            {
                WatchingSoundRefresh();
                //Reset only if no more fading sound in processing Voice List
                lock (_soundQueueSync)
                {
                    if (_soundProcessingQueue.Count(x => x.IsFadingMode == true || x.IsLooping) == 0)
                    {
                        _syncro.Reset();
                    }
                }

                _syncro.WaitOne();

                Thread.Sleep(10);
            }
            _d3dEngine.RunningThreadedWork.Remove("SoundEngine");
        }

        private void WatchingSoundRefresh()
        {
            ISoundVoice soundVoice;
            int soundQueueCount;
            lock (_soundQueueSync)
            {
                soundQueueCount = _soundProcessingQueue.Count;
            }
            for (int i = soundQueueCount - 1; i >= 0; i--)
            {
                soundVoice = _soundProcessingQueue[i];

                if (soundVoice.IsLooping || soundVoice.MaxDefferedStart > 0) processLoopingSoundState(soundVoice);
                if (soundVoice.IsFadingMode) processFadingSoundState(soundVoice);

                //CleanUp Queue if needed
                if (soundVoice.IsPlaying == false)
                {
                    lock (_soundQueueSync)
                    {
                        _soundProcessingQueue.RemoveAt(i);
                    }
                }
            }
        }

        private void processLoopingSoundState(ISoundVoice soundVoice)
        {
            if (soundVoice.State.BuffersQueued == 0)
            {
                    soundVoice.PushDataSourceForPlaying();
            }
        }

        private void processFadingSoundState(ISoundVoice soundVoice)
        {
            soundVoice.RefreshVoices();
        }
        
        #endregion
    }
}
