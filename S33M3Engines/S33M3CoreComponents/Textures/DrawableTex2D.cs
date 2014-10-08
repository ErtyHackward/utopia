using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using S33M3DXEngine.Main;

namespace S33M3DXEngine.Textures
{
    //Texture where we can render into !
    public class DrawableTex2D : BaseComponent
    {
        #region Private Variable
        private int _width;
        private int _height;
        private Vector2 _size;
        private Format _colorMapFormat;
        private D3DEngine _d3dEngine;
        private RenderTargetView _colorMapRTV;
        private ShaderResourceView _colorMapSRV;
        private DepthStencilView _depthMapDSV;
        private ShaderResourceView _depthMapSRV;
        private ViewportF _viewport;
        private Color4 _whiteColor = new Color4(0, 0, 0, 1);
        private Texture2DDescription _colorMaptexDesc;
        private Texture2DDescription _depthMapDesc;

        private SpriteRenderer _depthBufferRender;
        private SpriteTexture _depthStencilSprite;
        #endregion

        #region Public Properties
        public Texture2DDescription ColorMapTexDesc
        {
            get { return _colorMaptexDesc; }
        }

        public ShaderResourceView ColorMap
        {
            get { return _colorMapSRV; }
        }

        public ShaderResourceView DepthMap
        {
            get { return _depthMapSRV; }
        }

        public Texture2DDescription DepthMapDesc
        {
            get { return _depthMapDesc; }
        }
        #endregion

        public DrawableTex2D(D3DEngine d3dEngine)
        {
            _d3dEngine = d3dEngine;
            _depthBufferRender = ToDispose(new SpriteRenderer(d3dEngine, false));
        }

        #region Public Methods
        public void Init(int width, int height, bool isColorMap, Format colorFormat)
        {
            _width = width;
            _height = height;
            _size = new Vector2I(_width, _height);

            _colorMapFormat = colorFormat;
            buildDepthMap();
            if (isColorMap) buildColorMap();

            _viewport.Height = height;
            _viewport.Width = width;
            _viewport.X = 0;
            _viewport.Y = 0;
            _viewport.MinDepth = 0.0f;
            _viewport.MaxDepth = 1.0f;
        }

        public void Begin()
        {
            //Set the Depth Buffer and Render texture target to the outputMerger
            if (_colorMapRTV != null)
            {
                _d3dEngine.ImmediateContext.OutputMerger.SetTargets(_depthMapDSV, _colorMapRTV);
            }
            else
            {
                _d3dEngine.ImmediateContext.OutputMerger.SetTargets(_depthMapDSV);
            }

            //Set the viewport associated to the Texture renderer
            _d3dEngine.ImmediateContext.Rasterizer.SetViewport(_viewport);

            if (_colorMapRTV != null) _d3dEngine.ImmediateContext.ClearRenderTargetView(_colorMapRTV, _whiteColor);
            _d3dEngine.ImmediateContext.ClearDepthStencilView(_depthMapDSV, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        public void End()
        {
            if (_colorMapSRV != null) _d3dEngine.ImmediateContext.GenerateMips(_colorMapSRV);
        }

        private Vector2 _posi = new Vector2(0, 20);
        private ByteColor _color = Color.Wheat;
        public void DrawDepthBuffer(DeviceContext context, ref Vector2 size)
        {
            if (_depthStencilSprite == null)
            {
                _depthStencilSprite = new SpriteTexture(_width, _height, _depthMapSRV, new Vector2I(0, 0));
            }

            // uncomment to draw the depth buffer on the screen
            //
            //_depthBufferRender.Begin(false, context);
            //_depthBufferRender.Draw(_depthStencilSprite, ref _posi, ref size, ref _color);
            //_depthBufferRender.End(context);

        }

        #endregion

        #region Private Methods
        private void buildDepthMap()
        {
            _depthMapDesc = new Texture2DDescription()
            {
                Width = _width,
                Height = _height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R32_Typeless,
                SampleDescription = new SampleDescription() { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            Texture2D depthMap = new Texture2D(_d3dEngine.Device, _depthMapDesc);

            DepthStencilViewDescription dsvDesc = new DepthStencilViewDescription()
            {
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
                Texture2D = new DepthStencilViewDescription.Texture2DResource() { MipSlice = 0 }
            };

            _depthMapDSV = ToDispose(new DepthStencilView(_d3dEngine.Device, depthMap, dsvDesc));

            ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription()
            {
                Format = Format.R32_Float,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = _depthMapDesc.MipLevels, MostDetailedMip = 0 }
            };


            _depthMapSRV = ToDispose(new ShaderResourceView(_d3dEngine.Device, depthMap, srvDesc));

            //Can be disposed because a reference to it is still applied by the 2 view created on it !

        }

        private void buildColorMap()
        {
            _colorMaptexDesc = new Texture2DDescription()
            {
                Width = _width,
                Height = _height,
                MipLevels = 0,
                ArraySize = 1,
                Format = _colorMapFormat,
                SampleDescription = new SampleDescription() { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.GenerateMipMaps
            };

            Texture2D colorMap = new Texture2D(_d3dEngine.Device, _colorMaptexDesc);

            _colorMapRTV = ToDispose(new RenderTargetView(_d3dEngine.Device, colorMap));
            _colorMapSRV = ToDispose(new ShaderResourceView(_d3dEngine.Device, colorMap));

            colorMap.Dispose();
        }
        #endregion

    }
}
