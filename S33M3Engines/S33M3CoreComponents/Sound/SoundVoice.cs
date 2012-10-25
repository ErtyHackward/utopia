using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;

namespace S33M3CoreComponents.Sound
{
    public class SoundVoice : ISoundVoice, IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private SourceVoice _voice;
        private WaveFormat _linkedWaveFormat;
        private XAudio2 _xAudio2;
        private ISoundDataSource _playingDataSource;
        private Action<IntPtr> _callback;
        private Listener _listener;
        private X3DAudio _x3DAudio;
        private DeviceDetails _deviceDetail;
        #endregion

        #region Public Properties
        public Emitter Emitter { get; set; }
        public bool is3DSound { get; set; }
        public SourceVoice Voice { get { return _voice; } }
        public ISoundDataSource PlayingDataSource
        {
            get { return _playingDataSource; }
            set { _playingDataSource = value; CheckSourceVoice(value); }
        }
        public Vector3 Position
        {
            get { return Emitter.Position; }
            set { Emitter.Position = Position; Refresh3DParameters(); }
        }
        public bool IsLooping{ get; set; }
        #endregion

        public SoundVoice(XAudio2 xAudio2, WaveFormat linkedWaveFormat, Listener listener, X3DAudio x3DAudio, DeviceDetails deviceDetail,  Action<IntPtr> callBack)
        {
            if (linkedWaveFormat == null) throw new ArgumentNullException();
            _callback = callBack;
            _deviceDetail = deviceDetail;
            _x3DAudio = x3DAudio;
            _listener = listener;
            _xAudio2 = xAudio2;
            _linkedWaveFormat = linkedWaveFormat;
            CheckSourceVoice(_linkedWaveFormat);
        }

        #region Public Methods
        public void Dispose()
        {
            if (_voice != null && _voice.IsDisposed == false) _voice.Dispose();
        }

        public void Refresh3DParameters()
        {
            DspSettings settings3D = _x3DAudio.Calculate(_listener, Emitter, CalculateFlags.Matrix, 1, _deviceDetail.OutputFormat.Channels);
            //Adapt output channel volume in 3D
            //Voice.SetOutputMatrix(1, _deviceDetail.OutputFormat.Channels, settings3D.MatrixCoefficients);
        }

        public void Stop()
        {
            IsLooping = false;
            _voice.Stop();
            _voice.FlushSourceBuffers();
        }
        #endregion

        #region Private Methods
        private void CheckSourceVoice(ISoundDataSource playingDataSource)
        {
            CheckSourceVoice(playingDataSource.WaveFormat);
        }

        private void CheckSourceVoice(WaveFormat format)
        {
            if (_voice == null)
            {
                _linkedWaveFormat = format;
                _voice = new SourceVoice(_xAudio2, _linkedWaveFormat, true);
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
