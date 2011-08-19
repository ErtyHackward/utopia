using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DXGI;
using S33M3Engines.D3D;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.Direct3D;

namespace S33M3Engines.Textures
{
    //Texture where we can render into !
    public class DrawableTex2D : IDisposable
    {
        #region Private Variable
        private int _width;
        private int _height;
        private Format _colorMapFormat;
        private D3DEngine _d3dEngine;
        private RenderTargetView _colorMapRTV;
        private ShaderResourceView _colorMapSRV;
        private DepthStencilView _depthMapDSV;
        private ShaderResourceView _depthMapSRV;
        private Viewport _viewport;
        private Color4 _whiteColor = new Color4(1, 0, 0, 0);
        private Texture2DDescription _colorMaptexDesc;
        private Texture2DDescription _depthMapDesc;
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
        }


        public void Dispose()
        {
            if (_colorMapRTV != null) _colorMapRTV.Dispose();
            if (_colorMapSRV != null) _colorMapSRV.Dispose();
            if (_depthMapDSV != null) _depthMapDSV.Dispose();
            if (_depthMapSRV != null) _depthMapSRV.Dispose();
        }

        #region Public Methods
        public void Init(int width, int height, bool isColorMap, Format colorFormat)
        {
            _width = width;
            _height = height;
            _colorMapFormat = colorFormat;
            buildDepthMap();
            if (isColorMap) buildColorMap();

            _viewport.Height = height;
            _viewport.Width = width;
            _viewport.TopLeftX = 0;
            _viewport.TopLeftY = 0;
            _viewport.MinDepth = 0.0f;
            _viewport.MaxDepth = 1.0f;
        }

        public void Begin()
        {
            //Set the Depth Buffer and Render texture target to the outputMerger
            _d3dEngine.Context.OutputMerger.SetTargets(_depthMapDSV, _colorMapRTV);
            //Set the viewport associated to the Texture renderer
            _d3dEngine.Context.Rasterizer.SetViewports(_viewport);

            if (_colorMapRTV != null) _d3dEngine.Context.ClearRenderTargetView(_colorMapRTV, _whiteColor);
            _d3dEngine.Context.ClearDepthStencilView(_depthMapDSV, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        public void End()
        {
            if (_colorMapSRV != null) _d3dEngine.Context.GenerateMips(_colorMapSRV);
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

            _depthMapDSV = new DepthStencilView(_d3dEngine.Device, depthMap, dsvDesc);

            ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription()
            {
                Format = Format.R32_Float,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = _depthMapDesc.MipLevels, MostDetailedMip = 0 }
            };


            _depthMapSRV = new ShaderResourceView(_d3dEngine.Device, depthMap, srvDesc);

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

            _colorMapRTV = new RenderTargetView(_d3dEngine.Device, colorMap);
            _colorMapSRV = new ShaderResourceView(_d3dEngine.Device, colorMap);

            colorMap.Dispose();
        }
        #endregion

    }
}
