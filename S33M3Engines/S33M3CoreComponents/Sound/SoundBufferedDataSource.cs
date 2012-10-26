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
        public float SoundPower { get; set; }

        public WaveFormat WaveFormat { get; set; }
        public uint[] DecodedPacketsInfo { get; set; }
        public AudioBuffer AudioBuffer { get; set; }

        public int GetSoundFormatCategory()
        {
            return ((int)WaveFormat.Encoding << 16) ^ WaveFormat.Channels;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
