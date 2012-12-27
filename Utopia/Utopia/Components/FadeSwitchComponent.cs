using System;
using S33M3CoreComponents.States.Interfaces;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3Resources.Effects.Basics;
using SharpDX;
using S33M3Resources.Structs.Vertex;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using S33M3DXEngine.RenderStates;
using Utopia.Shared.GameDXStates;
using S33M3CoreComponents.States;

namespace Utopia.Components
{
    public class FadeSwitchComponent : SwitchComponent
    {
        #region Private variables
        private readonly D3DEngine _engine;
        private IndexBuffer<short> _iBuffer;
        private VertexBuffer<VertexPosition2> _vBuffer;
        HLSLScreenSpaceRect _effect;
        private Color4 _color;
        private float _fadeTimeS = 0.2f;
        private float _targetAlpha;
        #endregion

        #region Public variables/properties
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
        #endregion

        /// <summary>
        /// Occurs when screen is completely opaque. It is a time to change active components
        /// </summary>
        public override event EventHandler SwitchMoment;

        protected void OnSwitchMoment()
        {
            var handler = SwitchMoment;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when effect is completed and can be removed
        /// </summary>
        public override event EventHandler EffectComplete;

        protected void OnEffectComplete()
        {
            var handler = EffectComplete;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public FadeSwitchComponent(D3DEngine engine)
        {
            IsDefferedLoadContent = true;

            _engine = engine;

            DrawOrders.UpdateIndex(0, int.MaxValue); //Must ne draw the last
        }

        #region Public methods
        public override void Initialize()
        {
            _effect = ToDispose(new HLSLScreenSpaceRect(_engine.Device));

            // Create the vertex buffer => Thread safe, can be done here
            _vBuffer = ToDispose(new VertexBuffer<VertexPosition2>(_engine.Device, 4, VertexPosition2.VertexDeclaration, PrimitiveTopology.TriangleList, "Fade_vBuffer", ResourceUsage.Immutable));

            // Create the index buffer => Thread safe, can be done here
            _iBuffer = ToDispose(new IndexBuffer<short>(_engine.Device, 6, SharpDX.DXGI.Format.R16_UInt, "Fade_iBuffer"));
        }

        public override void LoadContent(DeviceContext context)
        {
            //Load data into the VB  => NOT Thread safe, MUST be done in the loadcontent
            VertexPosition2[] vertices = { 
                                          new VertexPosition2(new Vector2(-1.00f, -1.00f)),
                                          new VertexPosition2(new Vector2(1.00f, -1.00f)),
                                          new VertexPosition2(new Vector2(1.00f, 1.00f)),
                                          new VertexPosition2(new Vector2(-1.00f, 1.00f))
                                      };
            _vBuffer.SetData(context, vertices);

            //Load data into the IB => NOT Thread safe, MUST be done in the loadcontent
            short[] indices = { 3, 0, 2, 0, 1, 2 };
            _iBuffer.SetData(context, indices);

            base.LoadContent(context);
        }

        public override void FTSUpdate(S33M3DXEngine.Main.GameTime timeSpent)
        {
            if (_targetAlpha != _color.Alpha)
            {
                if (_targetAlpha > _color.Alpha)
                {
                    _color.Alpha += timeSpent.ElapsedGameTimeInS_LD / FadeTimeS;
                    if (_targetAlpha <= _color.Alpha)
                    {
                        _color.Alpha = _targetAlpha;
                        OnSwitchMoment();
                    }
                }
                else
                {
                    _color.Alpha -= timeSpent.ElapsedGameTimeInS_LD / FadeTimeS;
                    if (_targetAlpha >= _color.Alpha)
                    {
                        _color.Alpha = _targetAlpha;
                        OnEffectComplete();
                    }
                }
            }
        }

        public override void Draw(DeviceContext context, int index)
        {
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.CullNone, DXStates.Blenders.Enabled);

            _vBuffer.SetToDevice(context, 0);
            _iBuffer.SetToDevice(context, 0);

            _effect.Begin(context);

            _effect.CBPerDraw.Values.Color = _color;
            _effect.CBPerDraw.IsDirty = true;

            _effect.Apply(context);

            context.DrawIndexed(6, 0, 0);
        }

        public override void BeginSwitch()
        {
            _color.Alpha = 0;
            _targetAlpha = 1;
        }

        public override void FinishSwitch()
        {
            _color.Alpha = 1;
            _targetAlpha = 0;
        }
        #endregion
    }
}
