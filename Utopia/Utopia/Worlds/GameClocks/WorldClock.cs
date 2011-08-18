using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Settings;

namespace Utopia.Worlds.GameClocks
{
    public class WorldClock : Clock
    {
        #region Private variables
        private float _startTime;
        private float _deltaAngleTime;
        private bool _frozenTime;
        #endregion

        #region Public variable/properties
        public float ClockSpeed { get; set; }
        #endregion

        /// <summary>
        /// Control the Game time of teh world
        /// </summary>
        /// <param name="game">Base tools class</param>
        /// <param name="clockSpeed">The Ingame time speed in "Nbr of second ingame for each realtime seconds"; ex : 1 = Real time, 60 = 60times faster than realtime</param>
        /// <param name="gameTimeStatus">The startup time</param>
        public WorldClock(Game game, float clockSpeed, float startTime)
            :base(game)
        {
            _startTime = startTime;
            ClockSpeed = clockSpeed;
        }

        #region Public methods
        public override void Initialize()
        {
            _frozenTime = false;
            base._clockTime.Value = _startTime;
            base.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(ref S33M3Engines.D3D.GameTime TimeSpend)
        {
            InputHandler(false);

            if (_frozenTime) return;

            _deltaAngleTime = ClockSpeed * TimeSpend.ElapsedGameTimeInS_LD * (float)Math.PI / 10800.0f;

            //Back UP previous values
            _clockTime.BackUpValue();
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

            _visualClockTime.Time = _clockTime.ActualValue;
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            InputHandler(true);

            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override string GetInfo()
        {
            return base.GetInfo();
        }
        #endregion

        #region Private methods
        bool _freezeTimeBuffer;
        private void InputHandler(bool bufferMode)
        {
            if (_game.InputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.FreezeTime) || _freezeTimeBuffer)
            {
                if (bufferMode) { _freezeTimeBuffer = true; return; } else _freezeTimeBuffer = false;

                _frozenTime = !_frozenTime;
            }
        }
        #endregion


    }
}
