using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace S33M3CoreComponents.Sound
{
    public class SoundVoice : ISoundVoice, IDisposable
    {
        #region Private Variables
        private SourceVoice _voice;
        private WaveFormat _linkedWaveFormat;
        private XAudio2 _xAudio2;
        private ISoundDataSource _playingDataSource;
        private Action<IntPtr> _callback;
        #endregion

        #region Public Properties
        public SourceVoice Voice { get { return _voice; } }
        public ISoundDataSource PlayingDataSource
        {
            get { return _playingDataSource; }
            set { _playingDataSource = value; CheckSourceVoice(value); }
        }
        public bool IsLooping{ get; set; }
        #endregion

        public SoundVoice(XAudio2 xAudio2, WaveFormat linkedWaveFormat, Action<IntPtr> callBack)
        {
            _callback = callBack;
            _xAudio2 = xAudio2;
            _linkedWaveFormat = linkedWaveFormat;
            CheckSourceVoice(_linkedWaveFormat);
        }

        #region Public Methods
        public void Dispose()
        {
            if (_voice != null && _voice.IsDisposed == false) _voice.Dispose();
        }
        #endregion

        #region Private Methods
        private void CheckSourceVoice(ISoundDataSource playingDataSource)
        {
            CheckSourceVoice(playingDataSource.WaveFormat);
        }

        private void CheckSourceVoice(WaveFormat format)
        {
            if (_voice == null || _linkedWaveFormat != format)
            {
                if (_voice != null)
                {
                    ///UnRegister all delegates
                    _voice.BufferEnd -= _callback;
                    _voice.Dispose();
                }
                _linkedWaveFormat = format;
                _voice = new SourceVoice(_xAudio2, _linkedWaveFormat, true);
                _voice.BufferEnd += _callback;
            }
        }
        #endregion
    }
}
