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
        private Texture2D _renderTexture;
        private ShaderResourceView _renderTextureView;
        #endregion

        #region Public variables/properties
        public delegate void StaggingBackBufferChanged(ShaderResourceView newStaggingBackBuffer);
        public event StaggingBackBufferChanged OnStaggingBackBufferChanged;

        public ShaderResourceView RenderTextureView
        {
            get
            {
                return _renderTextureView;
            }
        }
        public Vector2 SolidStaggingBackBufferSize;

        public Texture2D OffSreenRenderTarget = null;
        #endregion

        public StaggingBackBuffer(D3DEngine engine, string Name)
        {
            _engine = engine;
            _engine.ScreenSize_Updated += engine_ScreenSize_Updated;
            this.DrawOrders.UpdateIndex(0, 999, Name);
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

            if (_renderTexture != null) _renderTexture.Dispose();
            if (_renderTextureView != null) _renderTextureView.Dispose();
            _engine.ScreenSize_Updated -= engine_ScreenSize_Updated;
        }

        #region Public Methods
        public override void Initialize()
        {
            CreateRenderTargets(_engine.BackBufferTex.Description);
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            if (_engine.CurrentMSAASampling.Count <= 1)
            {
                if (OffSreenRenderTarget == null)
                {
                    context.CopyResource(_engine.BackBufferTex, _renderTexture);
                }
                else
                {
                    context.CopyResource(OffSreenRenderTarget, _renderTexture);
                }
            }
            else
            {
                if (OffSreenRenderTarget == null)
                {
                    context.ResolveSubresource(_engine.BackBufferTex, 0, _renderTexture, 0, Format.R8G8B8A8_UNorm);
                }
                else
                {
                    context.ResolveSubresource(OffSreenRenderTarget, 0, _renderTexture, 0, Format.R8G8B8A8_UNorm);
                }
            }
        }
        #endregion

        #region Private methods
        private void engine_ScreenSize_Updated(ViewportF viewport, Texture2DDescription newBackBuffer)
        {
            CreateRenderTargets(newBackBuffer);

            if (OnStaggingBackBufferChanged != null) OnStaggingBackBufferChanged(_renderTextureView);
        }

        private void CreateRenderTargets(Texture2DDescription newBackBuffer)
        {
            if (_renderTextureView != null)
            {
                _renderTextureView.Dispose();
                _renderTexture.Dispose();
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
            _renderTexture = new Texture2D(_engine.Device, StaggingBackBufferDescr);

            _renderTextureView = new ShaderResourceView(_engine.Device, _renderTexture);
        }
        #endregion
    }
}
