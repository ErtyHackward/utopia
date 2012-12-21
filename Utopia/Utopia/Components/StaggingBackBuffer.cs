using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using SharpDX.Direct3D;

namespace Utopia.Components
{
    public class StaggingBackBuffer : DrawableGameComponent
    {
        #region Private variables
        private readonly D3DEngine _engine;
        private Texture2D _solidBackBuffer;
        private ShaderResourceView _backBuffer;
        #endregion

        #region Public variables/properties
        public delegate void StaggingBackBufferChanged(ShaderResourceView newStaggingBackBuffer);
        public event StaggingBackBufferChanged OnStaggingBackBufferChanged;

        public ShaderResourceView BackBuffer
        {
            get
            {
                return _backBuffer;
            }
        }
        public Vector2 SolidStaggingBackBufferSize;
        #endregion

        public StaggingBackBuffer(D3DEngine engine, string Name)
        {
            _engine = engine;
            _engine.ViewPort_Updated += engine_ViewPort_Updated;
            this.DrawOrders.UpdateIndex(0, 999, Name); //This should be call After all SOLID object have been draw on screen.
            this.Name = Name;

            AutoStateEnabled = false; //Need to activate the component myself.
        }

        public override void BeforeDispose()
        {
            //Force removal of event linked objects
            if (OnStaggingBackBufferChanged != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in OnStaggingBackBufferChanged.GetInvocationList())
                {
                    OnStaggingBackBufferChanged -= (StaggingBackBufferChanged)d;
                }
            }

            if (_solidBackBuffer != null) _solidBackBuffer.Dispose();
            if (_backBuffer != null) _backBuffer.Dispose();
            _engine.ViewPort_Updated -= engine_ViewPort_Updated;
        }

        #region Public Methods
        public override void Initialize()
        {
            CreateSolidBackBuffer(_engine.BackBufferTex.Description);
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            if (_engine.CurrentMSAASampling.Count <= 1)
            {
                context.CopyResource(_engine.BackBufferTex, _solidBackBuffer);
            }
            else
            {
                context.ResolveSubresource(_engine.BackBufferTex, 0, _solidBackBuffer, 0, Format.R8G8B8A8_UNorm);
            }
        }
        #endregion

        #region Private methods
        private void engine_ViewPort_Updated(ViewportF viewport, Texture2DDescription newBackBuffer)
        {
            CreateSolidBackBuffer(newBackBuffer);

            if (OnStaggingBackBufferChanged != null) OnStaggingBackBufferChanged(_backBuffer);
        }

        private void CreateSolidBackBuffer(Texture2DDescription newBackBuffer)
        {
            if (_backBuffer != null)
            {
                _backBuffer.Dispose();
                _solidBackBuffer.Dispose();
            }

            Texture2DDescription StaggingBackBufferDescr = new Texture2DDescription()
            {
                Width = _engine.BackBufferTex.Description.Width,
                Height = _engine.BackBufferTex.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1,0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None                
            };

            SolidStaggingBackBufferSize = new Vector2(StaggingBackBufferDescr.Width, StaggingBackBufferDescr.Height);
            _solidBackBuffer = new Texture2D(_engine.Device, StaggingBackBufferDescr);

            _backBuffer = new ShaderResourceView(_engine.Device, _solidBackBuffer);
        }
        #endregion
    }
}
