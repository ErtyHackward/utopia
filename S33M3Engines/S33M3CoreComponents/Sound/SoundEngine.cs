using S33M3DXEngine.Main.Interfaces;
using SharpDX.IO;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using S33M3_DXEngine.Main;

namespace S33M3CoreComponents.Sound
{

    public class SoundEngine : BaseComponent, ISoundEngine
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private static readonly int _maxVoicesNbr = 8;
        private SourceVoiceAndMetaData[] _soundQueues = new SourceVoiceAndMetaData[_maxVoicesNbr]; //By default create the possibility to play 8 songs at the same time.
        private Dictionary<string, AudioBufferAndMetaData> AudioBuffers = new Dictionary<string, AudioBufferAndMetaData>();
        private MasteringVoice _masteringVoice;

        private XAudio2 _soundDevice;
        private X3DAudio _x3DAudio;

        private ManualResetEvent _syncro;
        private Thread _thread;
        private List<string> _soundDevices;

        private sealed class AudioBufferAndMetaData : AudioBuffer
        {
            public WaveFormat WaveFormat { get; set; }
            public uint[] DecodedPacketsInfo { get; set; }
        }

        private sealed class SourceVoiceAndMetaData : SourceVoice
        {
            public bool IsLooping { get; set; }
            public string LoopingSourceFile { get; set; }
            public AudioBufferAndMetaData Buffer { get; set; }
            public Stopwatch LoopTimer { get; set; }
            public int LoopDelay { get; set; }

            public SourceVoiceAndMetaData(XAudio2 device, WaveFormat sourceFormat, bool withCallBack = false)
                : base(device, sourceFormat, withCallBack)
            {
            }

            public void SetLoopState(string fileName, AudioBufferAndMetaData buffer, int loopDelay)
            {
                LoopingSourceFile = fileName;
                IsLooping = true;
                Buffer = buffer;
                LoopDelay = loopDelay;
                if (LoopTimer == null) LoopTimer = new Stopwatch();
                LoopTimer.Reset();
            }

            public void UnSetLoopState()
            {
                LoopingSourceFile = null;
                IsLooping = false;
                LoopDelay = 0;
                Buffer = null;
                LoopTimer.Reset();
            }
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

        public List<string> SoundDevices
        {
            get { return _soundDevices; }
            set { _soundDevices = value; }
        }
        #endregion

        public SoundEngine()
        {
            Initialize();
        }

        public override void AfterDispose()
        {
            _syncro.Set();
            while (_thread.IsAlive) { }
            _syncro.Dispose();
        }

        #region Public Methods

        /// <summary>
        /// Play a sound Once
        /// </summary>
        /// <param name="soundfile"></param>
        /// <param name="volume"></param>
        /// <param name="forcedVoiceId"></param>
        public void PlaySound(string soundfile, float volume = 1, int forcedVoiceId = -1)
        {
            var buffer = GetBuffer(soundfile);
            SourceVoiceAndMetaData sourceVoice;
            if (GetVoice(buffer.WaveFormat, out sourceVoice, forcedVoiceId))
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

        //private void PlayStreamedAsync()
        //{
        //    int currentPlayCounter = 0;
        //    int nextBuffer = 0;

        //    try
        //    {
        //        while (true)
        //        {
        //            // Check that this instanced is not disposed
        //            while (!IsDisposed)
        //            {
        //                if (playEvent.WaitOne(WaitPrecision))
        //                    break;
        //            }

        //            if (IsDisposed)
        //                break;

        //            clock.Restart();
        //            playPositionStart = nextPlayPosition;
        //            playPosition = playPositionStart;
        //            currentPlayCounter = playCounter;

        //            // Get the decoded samples from the specified starting position.
        //            var sampleIterator = audioDecoder.GetSamples(playPositionStart).GetEnumerator();

        //            bool isFirstTime = true;

        //            bool endOfSong = false;

        //            // Playing all the samples
        //            while (true)
        //            {
        //                // If the player is stopped or disposed, then break of this loop
        //                while (!IsDisposed && State != AudioPlayerState.Stopped)
        //                {
        //                    if (playEvent.WaitOne(WaitPrecision))
        //                        break;
        //                }

        //                // If the player is stopped or disposed, then break of this loop
        //                if (IsDisposed || State == AudioPlayerState.Stopped)
        //                {
        //                    nextPlayPosition = TimeSpan.Zero;
        //                    break;
        //                }

        //                // If there was a change in the play position, restart the sample iterator.
        //                if (currentPlayCounter != playCounter)
        //                    break;

        //                // If ring buffer queued is full, wait for the end of a buffer.
        //                while (sourceVoice.State.BuffersQueued == audioBuffersRing.Length && !IsDisposed && State != AudioPlayerState.Stopped)
        //                    bufferEndEvent.WaitOne(WaitPrecision);

        //                // If the player is stopped or disposed, then break of this loop
        //                if (IsDisposed || State == AudioPlayerState.Stopped)
        //                {
        //                    nextPlayPosition = TimeSpan.Zero;
        //                    break;
        //                }

        //                // Check that there is a next sample
        //                if (!sampleIterator.MoveNext())
        //                {
        //                    endOfSong = true;
        //                    break;
        //                }

        //                // Retrieve a pointer to the sample data
        //                var bufferPointer = sampleIterator.Current;

        //                // If there was a change in the play position, restart the sample iterator.
        //                if (currentPlayCounter != playCounter)
        //                    break;

        //                // Check that our ring buffer has enough space to store the audio buffer.
        //                if (bufferPointer.Size > memBuffers[nextBuffer].Size)
        //                {
        //                    if (memBuffers[nextBuffer].Pointer != IntPtr.Zero)
        //                        Utilities.FreeMemory(memBuffers[nextBuffer].Pointer);

        //                    memBuffers[nextBuffer].Pointer = Utilities.AllocateMemory(bufferPointer.Size);
        //                    memBuffers[nextBuffer].Size = bufferPointer.Size;
        //                }

        //                // Copy the memory from MediaFoundation AudioDecoder to the buffer that is going to be played.
        //                Utilities.CopyMemory(memBuffers[nextBuffer].Pointer, bufferPointer.Pointer, bufferPointer.Size);

        //                // Set the pointer to the data.
        //                audioBuffersRing[nextBuffer].AudioDataPointer = memBuffers[nextBuffer].Pointer;
        //                audioBuffersRing[nextBuffer].AudioBytes = bufferPointer.Size;

        //                // If this is a first play, restart the clock and notify play method.
        //                if (isFirstTime)
        //                {
        //                    clock.Restart();
        //                    isFirstTime = false;

        //                    waitForPlayToOutput.Set();
        //                }

        //                // Update the current position used for sync
        //                playPosition = new TimeSpan(playPositionStart.Ticks + clock.Elapsed.Ticks);

        //                // Submit the audio buffer to xaudio2
        //                sourceVoice.SubmitSourceBuffer(audioBuffersRing[nextBuffer], null);

        //                // Go to next entry in the ringg audio buffer
        //                nextBuffer = ++nextBuffer % audioBuffersRing.Length;
        //            }

        //            // If the song is not looping (by default), then stop the audio player.
        //            if (endOfSong && !IsRepeating && State == AudioPlayerState.Playing)
        //            {
        //                Stop();
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        DisposePlayer();
        //    }
        //}

        /// <summary>
        /// Play a song in Loop
        /// </summary>
        /// <param name="soundfile">The sound file</param>
        /// <param name="volume"></param>
        /// <param name="delay">A delay between the repeating of the sound</param>
        public void StartPlayingSound(string soundfile, float volume = 1, int delay = 0)
        {
            //Get the sound buffer
            var buffer = GetBuffer(soundfile);
            //Get the first free Voice
            SourceVoiceAndMetaData sourceVoiceForLooping;
            if (GetVoice(buffer.WaveFormat, out sourceVoiceForLooping))
            {
                sourceVoiceForLooping.SetLoopState(soundfile, GetBuffer(soundfile), delay);

                //Get sound buffer
                sourceVoiceForLooping.Buffer = sourceVoiceForLooping.Buffer;
                sourceVoiceForLooping.SetVolume(volume, XAudio2.CommitNow);
                sourceVoiceForLooping.SubmitSourceBuffer(buffer, buffer.DecodedPacketsInfo);
                sourceVoiceForLooping.Start(); //Play the song                
            }
        }

        /// <summary>
        /// Stop Playing sound before it ends (Loop or single)
        /// </summary>
        /// <param name="soundfile">the sound file Id</param>
        public void StopPlayingSound(string soundfile)
        {
            for (int i = 0; i < _maxVoicesNbr; i++)
            {
                SourceVoiceAndMetaData voice = _soundQueues[i];
                if (voice != null && voice.IsLooping && voice.LoopingSourceFile == soundfile)
                {
                    voice.Stop();
                    voice.UnSetLoopState();
                }
            }
        }

        #endregion

        #region Private Methods
        private void Initialize()
        {
            //Create new Xaudio2 engine
            _soundDevice = ToDispose(new XAudio2());

            _soundDevices = new List<string>();
            //Get all sound devices
            for (int i = 0; i < _soundDevice.DeviceCount; i++)
            {
                _soundDevices.Add(_soundDevice.GetDeviceDetails(i).DisplayName);
            }

            DeviceDetails deviceDetail = _soundDevice.GetDeviceDetails(0); //_soundDevice.DeviceCount;
            logger.Info("s33m3 sound engine started for device : " + deviceDetail.DisplayName);
            _x3DAudio = new X3DAudio(deviceDetail.OutputFormat.ChannelMask);
            //Create a Mastering Voice
            _masteringVoice = ToDispose(new MasteringVoice(_soundDevice));                       //Default interface sending sound stream to Hardware
            _masteringVoice.SetVolume(1, 0);
            _soundDevice.StartEngine();

            _syncro = new ManualResetEvent(false);

            //Start Sound voice processing thread
            _thread = new Thread(DataSoundPocessingAsync) { Name = "SoundEngine" }; //Start the main loop
            _thread.Start();
        }

        /// <summary>
        /// Get the Audio File memory buffer from FileName
        /// </summary>
        /// <param name="soundfile"></param>
        /// <returns></returns>
        private AudioBufferAndMetaData GetBuffer(string soundfile)
        {
            AudioBufferAndMetaData buffer;

            if (AudioBuffers.TryGetValue(soundfile, out buffer) == false)
            {
                //Load the sound and bufferize it
                var soundstream = new SoundStream(File.OpenRead(soundfile));

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

        /// <summary>
        /// Get a voice from the pool
        /// By default it retrieve a voice doing nothing, but you can ask a specific voice by its ID
        /// In this case, the voice may be currently rendereing a sound
        /// </summary>
        /// <param name="waveFormat">The format asked for the voice to play</param>
        /// <param name="source"></param>
        /// <param name="forcedVoiceId"></param>
        /// <returns></returns>
        private bool GetVoice(WaveFormat waveFormat, out SourceVoiceAndMetaData source, int forcedVoiceId = -1)
        {
            if (forcedVoiceId > -1)
            {
                if (_soundQueues[forcedVoiceId] == null)
                {
                    _soundQueues[forcedVoiceId] = ToDispose(new SourceVoiceAndMetaData(_soundDevice, waveFormat, true));
                    _soundQueues[forcedVoiceId].BufferEnd += SoundEngine_BufferEnd;
                }
                source = _soundQueues[forcedVoiceId];
                return true;
            }

            for (int i = 0; i < _maxVoicesNbr; i++)
            {
                source = _soundQueues[i];
                if (source == null)
                {
                    source = _soundQueues[i] = ToDispose(new SourceVoiceAndMetaData(_soundDevice, waveFormat, true));
                    _soundQueues[i].BufferEnd += SoundEngine_BufferEnd;
                    return true;
                }
                if (source.State.BuffersQueued == 0 && source.IsLooping == false)
                {
                    return true;
                }
            }

            //By default, if all channel are playing a sound, skip Sound playing
            source = null;
            return false;
        }

        //A sound finished playing
        void SoundEngine_BufferEnd(IntPtr obj)
        {
            _syncro.Set();
        }

        //Function that is running its own thread responsible to do background stuff concerning sound
        private void DataSoundPocessingAsync()
        {
            while (!IsDisposed)
            {
                if (LoopingSoundRefresh() == false) _syncro.Reset();
                _syncro.WaitOne();
            }
        }

        /// <summary>
        /// Logic to implement "Looping" sound.
        /// </summary>
        /// <returns></returns>
        private bool LoopingSoundRefresh()
        {
            for (int i = 0; i < _maxVoicesNbr; i++)
            {
                SourceVoiceAndMetaData voice = _soundQueues[i];
                if (voice != null && voice.IsLooping)
                {
                    //Loop without delay
                    if (voice.LoopDelay == 0)
                    {
                        //Check the qt of buffer in the looping voice queue
                        if (voice.State.BuffersQueued == 0)
                        {
                            //Add a new sound in the queue the next sound !
                            voice.SubmitSourceBuffer(voice.Buffer, voice.Buffer.DecodedPacketsInfo);
                            return false;
                        }
                    }
                    else
                    {
                        //Loop with delay
                        if (voice.State.BuffersQueued == 0)
                        {
                            if (voice.LoopTimer.IsRunning == false) voice.LoopTimer.Restart();

                            if (voice.LoopTimer.ElapsedMilliseconds >= voice.LoopDelay)
                            {
                                voice.LoopTimer.Stop();
                                voice.SubmitSourceBuffer(voice.Buffer, voice.Buffer.DecodedPacketsInfo);
                                return false;
                            }
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

    }
}
