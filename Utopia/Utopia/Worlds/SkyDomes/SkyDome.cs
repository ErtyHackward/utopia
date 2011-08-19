using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.GameClock;
using Utopia.Worlds.GameClocks;
using S33M3Engines.Struct;
using SharpDX;
using Utopia.Worlds.Weather;
using Utopia.Shared.World;
using S33M3Engines;

namespace Utopia.Worlds.SkyDomes
{
    public abstract class SkyDome : ISkyDome
    {
        #region Private variable
        protected D3DEngine _d3dEngine;
        protected IClock _clock;
        protected IWeather _weather;
        protected FTSValue<Vector3> _lightDirection;
        protected float _fPhi = 0.0f;
        #endregion

        #region Public Variables
        public Vector3 LightDirection { get { return _lightDirection.ActualValue; } }
        #endregion

        public SkyDome(D3DEngine d3dEngine, IClock clock, IWeather weather)
        {
            _d3dEngine = d3dEngine;
            _clock = clock;
            _weather = weather;
        }

        #region Public methods

        public virtual void Draw()
        {
        }

        public virtual void Initialize()
        {
            _lightDirection = new FTSValue<Vector3>() { Value = new Vector3(100.0f, 100.0f, 100.0f) };
        }

        public virtual void Update(ref S33M3Engines.D3D.GameTime TimeSpend)
        {
            _lightDirection.BackUpValue();
            _lightDirection.Value = this.GetDirection();
            _lightDirection.Value.Normalize();
        }

        public virtual void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _lightDirection.ValueInterp = Vector3.Lerp(_lightDirection.ValuePrev, _lightDirection.Value, interpolation_ld);
            _lightDirection.ValueInterp.Normalize();
        }

        public virtual void Dispose()
        {
            _clock.Dispose();
        }

        #endregion

        #region Private Methods

        //Get sunray direction
        protected Vector3 GetDirection()
        {
            float y = (float)Math.Cos(_clock.ClockTime.Time);
            float x = (float)(Math.Sin(_clock.ClockTime.Time) * Math.Cos(this._fPhi));
            float z = (float)(Math.Sin(_clock.ClockTime.Time) * Math.Sin(this._fPhi));

            return new Vector3(x, y, z);
        }

        #endregion
    }
}
