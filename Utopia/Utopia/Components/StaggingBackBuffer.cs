using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;

namespace Utopia.Components
{
    public class StaggingBackBuffer : DrawableGameComponent
    {
        #region Private variables
        private readonly D3DEngine _engine;
        private Texture2D _solidBackBuffer;
        private ShaderResourceView _solidStaggingBackBuffer;
        #endregion

        #region Public variables/properties
        public ShaderResourceView SolidStaggingBackBuffer
        {
            get
            {
                return _solidStaggingBackBuffer;
            }
        }
        public Vector2 SolidStaggingBackBufferSize;
        #endregion

        public StaggingBackBuffer(D3DEngine engine)
        {
            _engine = engine;
            _engine.ViewPort_Updated += engine_ViewPort_Updated;
            this.DrawOrders.UpdateIndex(0, 999, "SolidBackBuffer"); //This should be call After all SOLID object have been draw on screen.
        }

        public override void Dispose()
        {
            if (_solidBackBuffer != null) _solidBackBuffer.Dispose();
            if (_solidStaggingBackBuffer != null) _solidStaggingBackBuffer.Dispose();
            _engine.ViewPort_Updated -= engine_ViewPort_Updated;

            base.Dispose();
        }

        #region Public Methods
        public override void Initialize()
        {
            CreateSolidBackBuffer(_engine.BackBufferTex.Description);
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            context.CopyResource(_engine.BackBufferTex, _solidBackBuffer);
        }
        #endregion

        #region Private methods
        private void engine_ViewPort_Updated(Viewport viewport, Texture2DDescription newBackBuffer)
        {
            CreateSolidBackBuffer(newBackBuffer);
        }

        private void CreateSolidBackBuffer(Texture2DDescription newBackBuffer)
        {
            if (_solidStaggingBackBuffer != null)
            {
                _solidStaggingBackBuffer.Dispose();
                _solidBackBuffer.Dispose();
            }

            Texture2DDescription StaggingBackBufferDescr = new Texture2DDescription()
            {
                Width = _engine.BackBufferTex.Description.Width,
                Height = _engine.BackBufferTex.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription() { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };

            SolidStaggingBackBufferSize = new Vector2(StaggingBackBufferDescr.Width, StaggingBackBufferDescr.Height);
            _solidBackBuffer = new Texture2D(_engine.Device, StaggingBackBufferDescr);
            _solidStaggingBackBuffer = new ShaderResourceView(_engine.Device, _solidBackBuffer);
        }
        #endregion
    }
}
