using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler;
using S33M3Engines.Maths;
using S33M3Engines.Struct;
using S33M3Engines;
using System.Windows.Forms;
using Utopia.Shared;
using Utopia.Settings;
using S33M3Engines.Shared.Math;

namespace Utopia.GameClock
{

    //Gestion du temps
    public enum GameTimeMode
    {
        Manual = 0,
        Automatic = 1,
        RealTime = 2
    }


    public class Clock : GameComponent,IDebugInfo
    {
        #region Private variables
        FTSValue<float> _clockTime = new FTSValue<float>(); // Radian angle representing the Period of time inside a day. 0 = Midi, Pi = SunSleep, 2Pi = Midnight, 3/2Pi : Sunrise Morning
        GameTimeMode _gameTimeStatus;
        float _clockSpeed, _originalSpeed;
        InputHandlerManager _input;
        float _deltaAngleTime;

        int _seconds;
        int _heures;
        int _minutes;
        #endregion

        #region Public porperties
        public int Heures
        {
            get { return _heures; }
        }

        public int Minutes
        {
            get { return _minutes; }
        }

        public int Seconds
        {
            get { return _seconds; }
        }
        public float ClockTime { get { return _clockTime.ActualValue; } set { _clockTime.Value = value; } }
        public float ClockTimeNormalized { get { return (ClockTime / MathHelper.TwoPi); } }
        public float ClockTimeNormalized2
        {
            get
            {
                if (_clockTime.ActualValue <= MathHelper.Pi)
                {
                    return MathHelper.FullLerp(0, 1, 0, MathHelper.Pi, _clockTime.ActualValue);
                }
                else
                {
                    return 1 - MathHelper.FullLerp(0, 1, MathHelper.Pi, MathHelper.TwoPi, _clockTime.ActualValue);
                }
            }
        }
        public float ClockSpeed
        {
            get { return _clockSpeed; }
            set
            {
                _clockSpeed = value;
            }
        }
        #endregion

        /// <summary>
        /// Control the Game time
        /// </summary>
        /// <param name="game">Base tools class</param>
        /// <param name="clockSpeed">The Ingame time speed in "Nbr of second ingame for each realtime seconds"; ex : 1 = Real time, 60 = 60times faster than realtime</param>
        /// <param name="gameTimeStatus">The mode use to make the time progress</param>
        public Clock(float clockSpeed, GameTimeMode gameTimeStatus, float startTime, InputHandlerManager input)
        {
            _gameTimeStatus = gameTimeStatus;
            ClockSpeed = clockSpeed;
            _originalSpeed = clockSpeed;
            _input = input;
            _clockTime.Value = startTime;

            base.CallDraw = false;
        }

        //Automaticaly change the current game time !
        public override void Update(ref GameTime TimeSpend)
        {

            InputHandler(false);

            _deltaAngleTime = _clockSpeed * TimeSpend.ElapsedGameTimeInS_LD * (float)Math.PI / 10800.0f;

            //Back UP previous values
            _clockTime.BackUpValue();

            switch (_gameTimeStatus)
            {
                case GameTimeMode.Automatic:
                    _clockTime.Value += _deltaAngleTime;

                    if (_clockTime.Value > Math.PI * 2)
                    {
                        //+1 Day
                        _clockTime.Value = _clockTime.Value - (2 * (float)Math.PI);
                    }
                    if (_clockTime.Value < Math.PI * -2)
                    {
                        //-1 Day
                        _clockTime.Value = _clockTime.Value + (2 * (float)Math.PI);
                    }

                    break;
                case GameTimeMode.RealTime:
                    _clockTime.Value = (float)(DateTime.Now.Hour * 60 + DateTime.Now.Minute) * (float)(Math.PI) / 12.0f / 60.0f;
                    break;
            }

            _seconds = (int)(86400.0f / MathHelper.TwoPi * _clockTime.Value);
            _heures = (int)(_seconds / 3600.0f);
            _minutes = (int)((_seconds % 3600.0f) / 60);
        }

        bool _freezeTimeBuffer;
        private void InputHandler(bool bufferMode)
        {
            if (_input.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.FreezeTime)|| _freezeTimeBuffer)
            {
                if (bufferMode) { _freezeTimeBuffer = true; return; } else _freezeTimeBuffer = false;

                if (ClockSpeed == 0) ClockSpeed = _originalSpeed;
                else ClockSpeed = 0;
            }
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            InputHandler(true);

            float recomputedClock = _clockTime.Value;
            if (_clockTime.Value < _clockTime.ValuePrev)
            {
                recomputedClock = _clockTime.Value + MathHelper.TwoPi;
            }

            _clockTime.ValueInterp = MathHelper.Lerp(_clockTime.ValuePrev, recomputedClock, interpolation_ld);
            if (_clockTime.ValueInterp > Math.PI * 2)
            {
                //+1 Day
                _clockTime.ValueInterp = _clockTime.ValueInterp - (2 * (float)Math.PI);
            }
            if (_clockTime.ValueInterp < Math.PI * -2)
            {
                //-1 Day
                _clockTime.ValueInterp = _clockTime.ValueInterp + (2 * (float)Math.PI);
            }

            _seconds = (int)(86400.0f / MathHelper.TwoPi * _clockTime.ValueInterp);
            _heures = (int)(_seconds / 3600.0f);
            _minutes = (int)((_seconds % 3600.0f) / 60);
        }
    
        #region IDebugInfo Members

        public string  GetInfo()
        {
            return "<Clock Mod> Current time : " + Heures.ToString("00:") + Minutes.ToString("00") + " normalized : " + ClockTimeNormalized2 + " ; " + ClockTimeNormalized + " ; " + ClockTime;
        }

        #endregion
}
}
