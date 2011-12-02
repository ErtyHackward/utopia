using System;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.D3D.Effects;
using SharpDX;

namespace Utopia.Components
{
    /// <summary>
    /// Allows to fade screen to color specified
    /// </summary>
    public class FadeComponent : DrawableGameComponent, ISwitchComponent
    {
        private readonly D3DEngine _engine;
        FadeEffect _effect;
        private Color4 _color;
        private float _fadeTimeS = 0.2f;
        private float _targetAlpha;
        
        /// <summary>
        /// Gets or sets fade time in seconds 
        /// </summary>
        public float FadeTimeS
        {
            get { return _fadeTimeS; }
            set { _fadeTimeS = value; }
        }
        
        /// <summary>
        /// Gets or sets required fade color
        /// </summary>
        public Color4 Color
        {
            get { return _color; }
            set { _color = value; }
        }
        
        /// <summary>
        /// Occurs when screen is completely opaque. It is a time to change active components
        /// </summary>
        public event EventHandler SwitchMoment;

        private void OnSwitchMoment()
        {
            var handler = SwitchMoment;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when effect is completed and can be removed
        /// </summary>
        public event EventHandler EffectComplete;

        private void OnEffectComplete()
        {
            var handler = EffectComplete;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        
        public FadeComponent(D3DEngine engine)
        {
            _engine = engine;

            DrawOrders.UpdateIndex(0, int.MaxValue);
        }

        public override void LoadContent()
        {
            _effect = new FadeEffect(_engine);
        }

        public override void UnloadContent()
        {
            _effect.Dispose();
        }

        public override void Dispose()
        {
            _effect = null;
            base.Dispose();
        }

        public override void Update(ref GameTime timeSpend)
        {
            if (_targetAlpha != _color.Alpha)
            {
                if (_targetAlpha > _color.Alpha)
                {
                    _color.Alpha += timeSpend.ElapsedGameTimeInS_LD / FadeTimeS;
                    if (_targetAlpha <= _color.Alpha)
                    {
                        _color.Alpha = _targetAlpha;
                        OnSwitchMoment();
                    }
                }
                else
                {
                    _color.Alpha -= timeSpend.ElapsedGameTimeInS_LD / FadeTimeS;
                    if (_targetAlpha >= _color.Alpha)
                    {
                        _color.Alpha = _targetAlpha;
                        OnEffectComplete();
                    }
                }
            }
        }

        public override void Draw(int index)
        {
            _effect.Draw(_color);
        }

        /// <summary>
        /// Begins the switch effect
        /// </summary>
        public void BeginSwitch()
        {
            _color.Alpha = 0;
            _targetAlpha = 1;
        }

        /// <summary>
        /// Begins the second stage of the switch effect
        /// </summary>
        public void FinishSwitch()
        {
            _color.Alpha = 1;
            _targetAlpha = 0;
        }

    }
}
