using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace S33M3CoreComponents.Sound
{
    public class SoundBufferedDataSource : ISoundDataSource
    {
        #region Private Variables
        #endregion

        #region Public Properties
        public string SoundAlias { get; set; }

        public float SoundVolume { get; set; }
        public float MaxDistance { get; set; }
        public float MinDistance { get; set; }

        public WaveFormat WaveFormat { get; set; }
        public uint[] DecodedPacketsInfo { get; set; }
        public AudioBuffer AudioBuffer { get; set; }
        public SoundBufferedDataSource.FileFormatType FormatType { get; set; }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public enum FileFormatType
        {
            Wav,
            Adpcm
        }
    }
}
