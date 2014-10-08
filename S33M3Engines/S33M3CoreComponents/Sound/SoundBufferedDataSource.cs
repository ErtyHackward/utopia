using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace S33M3CoreComponents.Sound
{
    public class SoundBufferedDataSource : ISoundDataSource, IDisposable
    {
        #region Private Variables
        #endregion

        #region Public Properties
        public DataSourcePlayMode PlayMode { get { return DataSourcePlayMode.Buffered; } }

        public string Alias { get; set; }
        public string FilePath { get; set; }
        public float Volume { get; set; }
        public float Power { get; set; }
        public bool isStreamed { get; set; }
        public int Priority { get; set; }

        public WaveFormat WaveFormat { get; set; }
        public AudioBuffer AudioBuffer { get; private set; }

        public SourceCategory Category { get; set; }

        public int GetSoundFormatCategory()
        {
            return ((int)WaveFormat.Encoding << 16) ^ WaveFormat.Channels;
        }
        #endregion

        public SoundBufferedDataSource(FileInfo FileName)
        {
            //Extract the data from the sound file, and create a buffer with them

            //Creating the source, was not existing
            Volume = 1.0f;

            SoundStream soundstream;
            switch (FileName.Extension)
            {
                case ".wav":
                    //Load the sound and bufferize it
                    soundstream = new SoundStream(File.OpenRead(FileName.FullName));
                    WaveFormat = soundstream.Format;
                    AudioBuffer = new AudioBuffer()
                    {
                        Stream = soundstream.ToDataStream(),
                        AudioBytes = (int)soundstream.Length,
                        Flags = BufferFlags.EndOfStream
                    };

                    soundstream.Close();
                    soundstream.Dispose();
                    break;
                case ".wma": // NOT good idea this can be HUGE buffer, better streaming a WMA file !
                    //New data stream
                    using (FileStream fileStream = new FileStream(FileName.FullName, FileMode.Open, FileAccess.Read))
                    {
                        var audioDecoder = new AudioDecoder(fileStream);
                        var outputWavStream = new MemoryStream();

                        var wavWriter = new WavWriter(outputWavStream);

                        // Write the WAV file
                        wavWriter.Begin(audioDecoder.WaveFormat);
                        // Decode the samples from the input file and output PCM raw data to the WAV stream.
                        wavWriter.AppendData(audioDecoder.GetSamples());
                        // Close the wav writer.
                        wavWriter.End();

                        outputWavStream.Position = 0;
                        soundstream = new SoundStream(outputWavStream);

                        WaveFormat = soundstream.Format;
                        AudioBuffer = new AudioBuffer()
                        {
                            Stream = soundstream.ToDataStream(),
                            AudioBytes = (int)soundstream.Length,
                            Flags = BufferFlags.EndOfStream
                        };

                        soundstream.Close();
                        soundstream.Dispose();
                        outputWavStream.Dispose();
                        audioDecoder.Dispose();
                    }

                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            if (AudioBuffer != null)
            {
                AudioBuffer.Stream.Dispose();
            }
        }
        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
