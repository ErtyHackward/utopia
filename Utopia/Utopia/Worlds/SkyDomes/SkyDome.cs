using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.GameClocks;
using SharpDX;
using Utopia.Worlds.Weather;
using Utopia.Shared.World;
using S33M3DXEngine;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;

namespace Utopia.Worlds.SkyDomes
{
    public abstract class SkyDome : DrawableGameComponent, ISkyDome
    {
        #region Private variable
        protected D3DEngine _d3dEngine;
        protected IClock _clock;
        protected IWeather _weather;
        protected FTSValue<Vector3> _lightDirection;
        protected float _fPhi = 0.0f;
        protected Color3 _sunColor = new Color3(1, 1, 1);
        #endregion

        #region Public Variables
        public Vector3 LightDirection { get { return _lightDirection.ValueInterp; } }
        public Color3 SunColor { get { return _sunColor; } }
        #endregion

        public SkyDome(D3DEngine d3dEngine, IClock clock, IWeather weather)
        {
            _d3dEngine = d3dEngine;
            _clock = clock;
            _weather = weather;
        }

        #region Public methods

        public override void Initialize()
        {
            _lightDirection = new FTSValue<Vector3>() { Value = new Vector3(100.0f, 100.0f, 100.0f) };
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            _lightDirection.BackUpValue();
            _lightDirection.Value = this.GetDirection();
            _lightDirection.Value.Normalize();
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            _lightDirection.ValueInterp = Vector3.Lerp(_lightDirection.ValuePrev, _lightDirection.Value, interpolationLd);
            _lightDirection.ValueInterp.Normalize();
        }

        #endregion

        #region Private Methods

        //Get sunray direction
        protected Vector3 GetDirection()
        {
            float y = (float)Math.Cos(_clock.ClockTime.ClockTimeNormalized);
            float x = (float)(Math.Sin(_clock.ClockTime.ClockTimeNormalized) * Math.Cos(this._fPhi));
            float z = (float)(Math.Sin(_clock.ClockTime.ClockTimeNormalized) * Math.Sin(this._fPhi));

            return new Vector3(x, y, z);
        }

        #endregion

    }
}
