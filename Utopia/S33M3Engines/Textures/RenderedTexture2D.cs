﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using S33M3Engines.Shared.Sprites;

namespace S33M3Engines.Textures
{
    /// <summary>
    /// Render to Texture2D class.
    /// It handle the needed operation to collection the Draw calls into its internal texture.
    /// </summary>
    public class RenderedTexture2D : IDisposable
    {
        #region Private variables
        private Texture2DDescription _textureDesc;
        private RenderTargetView _renderTargetView;
        private DepthStencilView _depthStencilView;
        private D3DEngine _d3dEngine;
        private Viewport _viewport;

        private Format _textureFormat;
        private int _textureWidth;
        private int _textureHeight;
        private Color4 _backGroundColor = new Color4(1, 0, 0, 0);
        #endregion

        #region Public variables
        public Texture2D RenderTargetTexture;
        public ShaderResourceView ShaderResourceView;

        public Color4 BackGroundColor
        {
            get { return _backGroundColor; }
            set { _backGroundColor = value; }
        }
        #endregion
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="engine">D3d engine Wrapper</param>
        /// <param name="textureWidth">Created texture Width</param>
        /// <param name="textureHeight">Created texture Height</param>
        public RenderedTexture2D(D3DEngine d3dEngine, int textureWidth, int textureHeight, Format textureFormat = Format.R32G32B32A32_Float)
        {
            _d3dEngine = d3dEngine;
            _textureWidth = textureWidth;
            _textureHeight = textureHeight;
            _textureFormat = textureFormat;
            Initialize();
        }

        public void Dispose()
        {
            if(ShaderResourceView != null) ShaderResourceView.Dispose();
            if (_renderTargetView != null) _renderTargetView.Dispose();
            if (RenderTargetTexture != null) RenderTargetTexture.Dispose();
            if (_depthStencilView != null) _depthStencilView.Dispose();
        }

        #region Private methods
        private void Initialize()
        {
            //ViewPort Initialization
            _viewport.Height = _textureHeight;
            _viewport.Width = _textureWidth;
            _viewport.TopLeftX = 0;
            _viewport.TopLeftY = 0;
            _viewport.MinDepth = 0.0f;
            _viewport.MaxDepth = 1.0f;

            // Setup the render target texture description.
            _textureDesc = new Texture2DDescription()
            {
                Width = _textureWidth,
                Height = _textureHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = _textureFormat,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription() { Count = 1, Quality = 0 }
            };

            // Create the render target texture.
            RenderTargetTexture = new Texture2D(_d3dEngine.Device, _textureDesc);

            Texture2DDescription depthMapDesc = new Texture2DDescription()
            {
                Width = _textureWidth,
                Height = _textureHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R32_Typeless,
                SampleDescription = new SampleDescription() { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            Texture2D depthMap = new Texture2D(_d3dEngine.Device, depthMapDesc);

            DepthStencilViewDescription depthStencilViewDesc = new DepthStencilViewDescription()
            {
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
                Texture2D = new DepthStencilViewDescription.Texture2DResource() { MipSlice = 0 }
            };
            _depthStencilView = new DepthStencilView(_d3dEngine.Device, depthMap, depthStencilViewDesc);
            depthMap.Dispose();

            // Setup the description of the render target view.
            RenderTargetViewDescription renderTargetViewDesc = new RenderTargetViewDescription()
            {
                Format = _textureFormat,
                Dimension = RenderTargetViewDimension.Texture2D,
                Texture2D = new RenderTargetViewDescription.Texture2DResource() { MipSlice = 0 }
            };
            // Create the render target view for using the Texture define.
            _renderTargetView = new RenderTargetView(_d3dEngine.Device, RenderTargetTexture, renderTargetViewDesc);

            // Setup the description of the shader resource view.
            ShaderResourceViewDescription shaderResourceViewDesc = new ShaderResourceViewDescription()
            {
                Format = _textureFormat,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = 1, MostDetailedMip = 0 }
            };
            ShaderResourceView = new ShaderResourceView(_d3dEngine.Device, RenderTargetTexture, shaderResourceViewDesc);
        }

        /// <summary>
        /// If we won't use the Texture for rendering anymore, we can freeUp Resources
        /// </summary>
        private void ReleaseDrawingResources()
        {
            if (_renderTargetView != null) _renderTargetView.Dispose();
            if (RenderTargetTexture != null) RenderTargetTexture.Dispose();
            if (_depthStencilView != null) _depthStencilView.Dispose();
        }
        #endregion

        #region Public methods
        public void Begin()
        {
            _d3dEngine.Context.OutputMerger.SetTargets(_depthStencilView, _renderTargetView);
            //Set the viewport associated to the Texture renderer
            _d3dEngine.Context.Rasterizer.SetViewports(_viewport);

            // Clear the back buffer.
            _d3dEngine.Context.ClearRenderTargetView(_renderTargetView, _backGroundColor);

            // Clear the depth buffer.
            _d3dEngine.Context.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        public void End(bool releaseDrawingResources ,bool generateMips = false)
        {
            if (generateMips) _d3dEngine.Context.GenerateMips(ShaderResourceView);
            if (releaseDrawingResources) ReleaseDrawingResources();
        }

        public SpriteTexture CloneToSpriteTexture()
        {
            Texture2D clonedTexture = new Texture2D(_d3dEngine.Device, _textureDesc);
            _d3dEngine.Context.CopyResource(RenderTargetTexture, clonedTexture);
            return new SpriteTexture(_d3dEngine.Device, clonedTexture, Vector2.Zero);
        }

        public Texture2D CloneTexture(ResourceUsage defaultResourceUsage)
        {
            // Setup the render target texture description.
            Texture2DDescription clonetextureDesc = new Texture2DDescription()
            {
                Width = _textureWidth,
                Height = _textureHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = _textureFormat,
                Usage = defaultResourceUsage,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription() { Count = 1, Quality = 0 }
            };

            if (defaultResourceUsage == ResourceUsage.Staging)
            {
                clonetextureDesc.BindFlags = BindFlags.None;
                clonetextureDesc.CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write;
            }
            else
            {
                clonetextureDesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
                clonetextureDesc.CpuAccessFlags = CpuAccessFlags.None;
            }

            Texture2D clonedTexture = new Texture2D(_d3dEngine.Device, clonetextureDesc);

            _d3dEngine.Context.CopyResource(RenderTargetTexture, clonedTexture);
            return clonedTexture;
        }
        #endregion
    }
}
