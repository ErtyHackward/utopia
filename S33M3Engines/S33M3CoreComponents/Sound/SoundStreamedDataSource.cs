using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace S33M3CoreComponents.Sound
{
    public class SoundStreamedDataSource : ISoundDataSource, IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private readonly object _syncRoot = new object();
        private static int bufferRing = 8;
        private DataPointer[] _memBuffers;
        private AudioBuffer[] _audioBuffersRing;
        private MemoryStream _bufferedCompressedFile;
        private AudioDecoder _audioDecoder;
        private AutoResetEvent _sleep;
        private ISoundVoice _linkedVoice;
        private bool _stopThread;
        private Task _fetchingThread;
        #endregion

        #region Public Properties
        public SourceCategory Category { get; set; }
        public DataSourcePlayMode PlayMode { get { return DataSourcePlayMode.Streamed; } }
        public string FilePath { get; set; }
        public string Alias { get; set; }
        public float Volume { get; set; }
        public float Power { get; set; }
        public bool isStreamed { get; set; }
        public int Priority { get; set; }
        public WaveFormat WaveFormat { get; set; }
        public AudioBuffer AudioBuffer
        {
            get { return null; }
        }
        #endregion

        public SoundStreamedDataSource(FileInfo fi)
        {
            //To avoid Disk activity, preload the sound file into memoryStream
            using (FileStream fileStream = File.OpenRead(fi.FullName))
            {
                _bufferedCompressedFile = new MemoryStream();
                _bufferedCompressedFile.SetLength(fileStream.Length);
                fileStream.Read(_bufferedCompressedFile.GetBuffer(), 0, (int)fileStream.Length);
            }

            //Pre allocate buffers
            _audioBuffersRing = new AudioBuffer[bufferRing];
            _memBuffers = new DataPointer[bufferRing];
            for (int i = 0; i < bufferRing; i++)
            {
                _audioBuffersRing[i] = new AudioBuffer();
                _memBuffers[i].Size = 32 * 1024; // default size 32Kb
                _memBuffers[i].Pointer = Utilities.AllocateMemory(_memBuffers[i].Size);
            }

            Volume = 1;
            //Get File metaData
            _audioDecoder = new AudioDecoder(_bufferedCompressedFile);
            WaveFormat = _audioDecoder.WaveFormat;

            _sleep = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            _stopThread = true;
            if(_fetchingThread!=null) _fetchingThread.Wait();
            _bufferedCompressedFile.Dispose();
            foreach (var buffer in _audioBuffersRing) if(buffer.Stream != null) buffer.Stream.Dispose();
            foreach (var buffer in _memBuffers) Utilities.FreeMemory(buffer.Pointer);
            _audioDecoder.Dispose();
        }

        #region Public Methods
        public int GetSoundFormatCategory()
        {
            return ((int)WaveFormat.Encoding << 16) ^ WaveFormat.Channels;
        }

        public void StartVoiceDataFetching(ISoundVoice voice)
        {
            if (_fetchingThread != null &&
                _fetchingThread.Status != TaskStatus.RanToCompletion &&
                _fetchingThread.Status != TaskStatus.Faulted &&
                _fetchingThread.Status != TaskStatus.Canceled &&
                _fetchingThread.Status != TaskStatus.Created
                )
            {
                //logger.Warn("Cannot stream the same resource as streaming source for 2 differents voices !! Previously playing sound will be stopped");
                lock (_syncRoot)
                {
                    if(_linkedVoice != null) _linkedVoice.Stop();
                }
                _sleep.Set();
                _fetchingThread.Wait();
            }
            _linkedVoice = voice;
            _linkedVoice.IsPlaying = true;
            //Start voice Fetching with data !
            _fetchingThread = Task.Factory.StartNew(asyncVoiceFetching, TaskCreationOptions.LongRunning);
        }
        #endregion

        #region Private Methods
        //Async
        private void asyncVoiceFetching()
        {
            int nextBuffer = 0;
            while (!_stopThread && _linkedVoice != null && _linkedVoice.IsPlaying)
            {
                // Get the decoded samples from the specified starting position.
                var sampleIterator = _audioDecoder.GetSamples().GetEnumerator();

                // Playing all the samples
                while (true)
                {
                    // If ring buffer queued is full, wait for the end of a buffer.
                    while (_linkedVoice.State.BuffersQueued == bufferRing && !_stopThread && _linkedVoice.IsPlaying)
                        _sleep.WaitOne(1);

                    // If the player is stopped or disposed, then break of this loop
                    if (_stopThread || !_linkedVoice.IsPlaying)
                    {
                        break;
                    }

                    // Check that there is a next sample
                    if (!sampleIterator.MoveNext())
                    {
                        break;
                    }

                    // Retrieve a pointer to the sample data
                    var bufferPointer = sampleIterator.Current;

                    // Check that our ring buffer has enough space to store the audio buffer.
                    if (bufferPointer.Size > _memBuffers[nextBuffer].Size)
                    {
                        if (_memBuffers[nextBuffer].Pointer != IntPtr.Zero)
                            Utilities.FreeMemory(_memBuffers[nextBuffer].Pointer);

                        _memBuffers[nextBuffer].Pointer = Utilities.AllocateMemory(bufferPointer.Size);
                        _memBuffers[nextBuffer].Size = bufferPointer.Size;
                    }

                    // Copy the memory from MediaFoundation AudioDecoder to the buffer that is going to be played.
                    Utilities.CopyMemory(_memBuffers[nextBuffer].Pointer, bufferPointer.Pointer, bufferPointer.Size);

                    // Set the pointer to the data.
                    _audioBuffersRing[nextBuffer].AudioDataPointer = _memBuffers[nextBuffer].Pointer;
                    _audioBuffersRing[nextBuffer].AudioBytes = bufferPointer.Size;

                    // Submit the audio buffer to xaudio2
                    _linkedVoice.Voice.SubmitSourceBuffer(_audioBuffersRing[nextBuffer], null);

                    // Go to next entry in the ringg audio buffer
                    nextBuffer = ++nextBuffer % bufferRing;
                }

                // If the song is not looping go out the while
                if (!_linkedVoice.IsLooping)
                {
                    break;
                }
            }

            lock (_syncRoot)
            {
                _linkedVoice.Stop();
                _linkedVoice = null;
            }
        }

        #endregion
    }
}
