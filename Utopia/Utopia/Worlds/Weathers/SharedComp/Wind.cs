using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Worlds.Weathers.SharedComp;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Maths;

namespace Utopia.Worlds.Weather
{
    public class Wind : GameComponent, IWind
    {
        #region Private Variables
        private Random _rndWindFlowing;
        private Random _rndWindFlowChange;
        private bool _randomFlow;
        private float _animationStep = 0.01f;

        private FTSValue<double> _keyFrameAnimation;
        private FTSValue<Vector3> _flatWindFlowNormalizedWithNoise;

        private FastRandom _rnd;
        #endregion

        #region Public Variables
        public Vector3 WindFlow { get; set; }
        public Vector3 WindFlowFlat { get; set; }

        public Vector3 FlatWindFlowNormalizedWithNoise
        {
            get { return _flatWindFlowNormalizedWithNoise.ValueInterp; }
            set { _flatWindFlowNormalizedWithNoise.Value = value; }
        }
        public double KeyFrameAnimation
        {
            get { return _keyFrameAnimation.ValueInterp; }
        }
        #endregion

        public Wind(bool randomFlow = false)
        {
            _randomFlow = randomFlow;
        }

        #region Public methods
        public override void Initialize()
        {
            _rndWindFlowing = new Random();
            _rndWindFlowChange = new Random(_rndWindFlowing.Next());

            WindFlow = new Vector3(GetFlowRnd(), GetFlowRnd(), GetFlowRnd());

            _keyFrameAnimation = new FTSValue<double>();
            _keyFrameAnimation.Value = 0;
            _keyFrameAnimation.ValuePrev = 0;

            _flatWindFlowNormalizedWithNoise = new FTSValue<Vector3>();
            _flatWindFlowNormalizedWithNoise.Value = Vector3.Normalize(new Vector3(WindFlow.X, 0, WindFlow.Z));
            _flatWindFlowNormalizedWithNoise.ValuePrev = _flatWindFlowNormalizedWithNoise.Value;

            WindFlowFlat = Vector3.Normalize(new Vector3(WindFlow.X, 0, WindFlow.Z));

            _rnd = new FastRandom();
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            _keyFrameAnimation.ValueInterp = MathHelper.Lerp(_keyFrameAnimation.ValuePrev, _keyFrameAnimation.Value, interpolationHd);
            Vector3.Lerp(ref _flatWindFlowNormalizedWithNoise.ValuePrev, ref _flatWindFlowNormalizedWithNoise.Value, interpolationLd, out _flatWindFlowNormalizedWithNoise.ValueInterp);
        }

        public override void FTSUpdate( GameTime timeSpend)
        {
            _keyFrameAnimation.BackUpValue();
            _flatWindFlowNormalizedWithNoise.BackUpValue();

            UpdateKeyAnimation();

            if (!_randomFlow) return;

            if (_rndWindFlowing.Next(0, 10000) == 0)
            {
                WindFlow = new Vector3(GetFlowRnd(), GetFlowRnd(), GetFlowRnd());
                FlatWindFlowNormalizedWithNoise = Vector3.Normalize(new Vector3(WindFlow.X, 0, WindFlow.Z));
                WindFlowFlat = FlatWindFlowNormalizedWithNoise;
            }
        }

        #endregion

        #region Private Methods
        private float GetFlowRnd()
        {
            return (float)(_rndWindFlowing.NextDouble() * 2) - 1;
        }

        //Create the wind simulation flow
        private bool _standBy;
        private int _standbyLimit = 10;
        private void UpdateKeyAnimation()
        {
            if (_rnd.Next(0, 500) == 0)
            {
                _standBy = false;
                _animationStep = MathHelper.FullLerp(0.02f, 0.05f, 0, 1, _rnd.NextDouble());
                FlatWindFlowNormalizedWithNoise = Vector3.Normalize(new Vector3(WindFlow.X, 0, WindFlow.Z));
            }

            _keyFrameAnimation.Value += (_animationStep);

            if (_standBy)
            {
                if (_keyFrameAnimation.Value > MathHelper.PiOver2 / _standbyLimit)
                {
                    _animationStep = -1f * MathHelper.FullLerp(0.003f, 0.005f, 0, 1, _rnd.NextDouble());
                    _standbyLimit = _rnd.Next(10, 11);
                    _keyFrameAnimation.Value = MathHelper.PiOver2 / _standbyLimit;
                }

                if (_keyFrameAnimation.Value > -0.002 && _keyFrameAnimation.Value < 0.002)
                {
                    _flatWindFlowNormalizedWithNoise.Value = Vector3.Normalize(new Vector3((float)(WindFlow.X + _rnd.NextDouble()), 0, (float)(WindFlow.Z + _rnd.NextDouble())));
                }

                if (_keyFrameAnimation.Value < -MathHelper.PiOver2 / _standbyLimit)
                {
                    _animationStep = MathHelper.FullLerp(0.003f, 0.005f, 0, 1, _rnd.NextDouble());
                    _standbyLimit = _rnd.Next(10, 11);
                   _keyFrameAnimation.Value = -MathHelper.PiOver2 / _standbyLimit;
                }
            }
            else
            {
                if (_keyFrameAnimation.Value > MathHelper.PiOver2 / 2)
                {
                    _animationStep = -0.01f;
                    _keyFrameAnimation.Value = MathHelper.PiOver2 / 2;
                }
                if (_keyFrameAnimation.Value < -MathHelper.PiOver2 / 8)
                {
                    _standBy = true;
                    _animationStep = 0.008f;
                    _keyFrameAnimation.Value = -MathHelper.PiOver2 / 8;
                    _standbyLimit = 8;
                }
            }
        }
        #endregion

    }
}
