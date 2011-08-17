using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Windows;
using S33M3Engines.Struct;
using System.Drawing;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.D3DCompiler;
using S33M3Engines.StatesManager;
using SharpDX.Direct3D;
using S33M3Engines.Shared.Delegates;

namespace S33M3Engines
{
    //3D Engine based on SharpDX - target microsoft D3D10
    public class D3DEngine : IDisposable
    {
        #region Static Variables
        public static IntPtr WindowHandle;
        public static bool FIXED_TIMESTEP_ENABLED = true;
        public static bool FULLDEBUGMODE = false;
        #endregion

        #region Private variables
        bool _isResizing = false;
        RenderForm _renderForm;
        SwapChain _swapChain;
        RenderTargetView _renderTarget;
        DepthStencilView _depthStencil;
        Viewport _viewPort;
        Factory _dx11factory;
        Texture2D _backBuffer, _staggingBackBufferTexture;
#if DEBUG
        ShaderFlags _shaderFlags = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#else
        ShaderFlags _shaderFlags = ShaderFlags.OptimizationLevel3;
#endif
        #endregion

        #region Public properties
        public Device GraphicsDevice;

        public D3DEngineDelegates.ViewPortUpdated ViewPort_Updated;
        public ShaderFlags ShaderFlags { get { return _shaderFlags; } }
        public Viewport ViewPort { get { return _viewPort; } set { _viewPort = value; } }
        public RenderForm GameWindow { get { return _renderForm; } }
        public SwapChain SwapChain { get { return _swapChain; } }

        public RenderTargetView RenderTarget { get { return _renderTarget; } }
        public DepthStencilView DepthStencilTarget { get { return _depthStencil; } }
        public ShaderResourceView StaggingBackBuffer;

        public DeviceContext Context;

        public bool isFullScreen
        {
            get
            {
                return !_swapChain.Description.IsWindowed;
            }
            set
            {
                if (value != !_swapChain.Description.IsWindowed)
                {
                    _swapChain.SetFullscreenState(value, null);
                    WindowSizeChanged();
                }
            }
        }

        #endregion

        //Constructor
        public D3DEngine(System.Drawing.Size startingSize, string windowCaption, int MaxNbrThreads)
        {
            //Create the MainRendering Form
            _renderForm = new RenderForm()
            {
                Text = windowCaption,
                ClientSize = startingSize,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            };
            WindowHandle = _renderForm.Handle;
            _renderForm.KeyUp += new System.Windows.Forms.KeyEventHandler(_renderForm_KeyUp);
        }

        //Remove default F10 (Open menu) Form key push !
        void _renderForm_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.F10) e.Handled = true;
        }

        #region Private methods
        #endregion

        #region Public Methods
        public void Initialize()
        {
            _dx11factory = new Factory();

            //foreach (var mode in dx10factory.GetAdapter(0).GetOutput(0).GetDisplayModeList(Format.B8G8R8A8_UNorm, DisplayModeEnumerationFlags.Interlaced))
            //{
            //    if (mode.Width == dx10factory.GetAdapter(0).GetOutput(0).Description.DesktopBounds.Width && 
            //        mode.Height == dx10factory.GetAdapter(0).GetOutput(0).Description.DesktopBounds.Height)
            //    {
            //        refreshRate = mode.RefreshRate;
            //        break;
            //    }
            //}

            CreateSwapChain();
            CreateRenderTarget();
            CreateDepthStencil();
            CreateViewPort();
            CreateStaggingBackBuffer();

            //Set Viewport and rendertarget to the device
            Context.OutputMerger.SetTargets(_depthStencil, _renderTarget);

            //Remove the some built-in fonctionnality of DXGI
            _dx11factory.MakeWindowAssociation(_renderForm.Handle, WindowAssociationFlags.IgnoreAll | WindowAssociationFlags.IgnoreAltEnter);

            _renderForm.ResizeBegin += new EventHandler(_renderForm_ResizeBegin);
            _renderForm.ResizeEnd += new EventHandler(_renderForm_ResizeEnd);
            _renderForm.Resize += new EventHandler(_renderForm_Resize);

            _renderForm.Show();
        }

        public void ResetRenderTargetsAndViewPort()
        {
            Context.OutputMerger.SetTargets(_depthStencil, _renderTarget);
            Context.Rasterizer.SetViewports(_viewPort);
        }

        public void RefreshBackBufferAsTexture()
        {
            Context.CopyResource(_backBuffer, _staggingBackBufferTexture);

            //if(StaggingBackBuffer != null) StaggingBackBuffer.Dispose();
            //StaggingBackBuffer = new ShaderResourceView(GraphicsDevice, _staggingBackBufferTexture);
            //Texture2D.SaveTextureToFile(Context, _staggingBackBufferTexture, ImageFileFormat.Png, @"e:\Img.png");
        }

        void _renderForm_ResizeBegin(object sender, EventArgs e)
        {
            _isResizing = true;
        }

        void _renderForm_Resize(object sender, EventArgs e)
        {
            //Only If maximized !
            if (!_isResizing)
            {
                WindowSizeChanged();
            }
        }

        void _renderForm_ResizeEnd(object sender, EventArgs e)
        {
            if (_isResizing)
            {
                WindowSizeChanged();
                _isResizing = false;
            }
        }


        private void CreateSwapChain()
        {
            if (_swapChain == null)
            {
                //Create the SwapChain Param object
                SwapChainDescription SwapDesc = new SwapChainDescription()
                //{
                //    BufferCount = 1,
                //    Usage = Usage.RenderTargetOutput | Usage.ShaderInput,
                //    OutputHandle = _renderForm.Handle,
                //    IsWindowed = true,
                //    ModeDescription = new ModeDescription(_renderForm.ClientSize.Width, _renderForm.ClientSize.Height, new Rational(120, 1), Format.R8G8B8A8_UNorm),
                //    SampleDescription = new SampleDescription(1, 0),
                //    Flags = SwapChainFlags.None,
                //    SwapEffect = SwapEffect.Discard
                //};
                {
                    BufferCount = 1,
                    Usage = Usage.RenderTargetOutput | Usage.ShaderInput,
                    OutputHandle = _renderForm.Handle,
                    IsWindowed = true,
                    ModeDescription = new ModeDescription() { Format = Format.R8G8B8A8_UNorm },
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard
                };

                if (!FULLDEBUGMODE)
                {
                    Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, SwapDesc, out GraphicsDevice, out _swapChain);
                }
                else
                {
                    Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, SwapDesc, out GraphicsDevice, out _swapChain);
                }

                //Create the threaded contexts
                Context = GraphicsDevice.ImmediateContext;
            }
            else
            {
                //_swapChain.ResizeBuffers(1, _renderForm.ClientSize.Width, _renderForm.ClientSize.Height, Format.R8G8B8A8_UNorm, (int)SwapChainFlags.None);
                _swapChain.ResizeBuffers(0, 0, 0, Format.Unknown, 0);
            }
            // Get the created BackBuffer
            _backBuffer = Resource.FromSwapChain<Texture2D>(_swapChain, 0);
        }

        private void CreateRenderTarget()
        {
            //Create RenderTargetView 
            _renderTarget = new RenderTargetView(GraphicsDevice, _backBuffer);
        }

        private void CreateDepthStencil()
        {
            //Create the Depth Stencil Texture
            Texture2DDescription DepthStencilDescr = new Texture2DDescription()
            {
                Width = _backBuffer.Description.Width,
                Height = _backBuffer.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D32_Float,
                SampleDescription = new SampleDescription() { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };
            Texture2D DepthStencilBuffer = new Texture2D(GraphicsDevice, DepthStencilDescr);

            //Create the Depth Stencil View + View
            DepthStencilViewDescription DepthStencilViewDescr = new DepthStencilViewDescription()
            {
                Format = DepthStencilDescr.Format,
                Dimension = DepthStencilViewDimension.Texture2D,
                Texture2D = new DepthStencilViewDescription.Texture2DResource() { MipSlice = 0 }
            };
            //Create the Depth Stencil view
            _depthStencil = new DepthStencilView(GraphicsDevice, DepthStencilBuffer, DepthStencilViewDescr);

            DepthStencilBuffer.Dispose();
        }

        private void CreateViewPort()
        {
            //Create ViewPort
            _viewPort = new Viewport(0, 0, _backBuffer.Description.Width, _backBuffer.Description.Height, 0, 1);
            if (ViewPort_Updated != null) ViewPort_Updated(_viewPort);
            Context.Rasterizer.SetViewports(_viewPort);
        }

        private void CreateStaggingBackBuffer()
        {
            Texture2DDescription StaggingBackBufferDescr = new Texture2DDescription()
            {
                Width = _backBuffer.Description.Width,
                Height = _backBuffer.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription() { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };

            _staggingBackBufferTexture = new Texture2D(GraphicsDevice, StaggingBackBufferDescr);
            StaggingBackBuffer = new ShaderResourceView(GraphicsDevice, _staggingBackBufferTexture);
        }

        private void ReleaseBackBufferLinkedResources()
        {
            if (_backBuffer != null)
            {
                _backBuffer.Dispose(); _backBuffer = null;
            }
            if (_renderTarget != null)
            {
                _renderTarget.Dispose(); _renderTarget = null;
            }
            if (_depthStencil != null)
            {
                _depthStencil.Dispose(); _depthStencil = null;
            }
            if (StaggingBackBuffer != null)
            {
                if (_staggingBackBufferTexture != null) _staggingBackBufferTexture.Dispose();
                StaggingBackBuffer.Dispose(); StaggingBackBuffer = null;
            }
        }


        private void WindowSizeChanged()
        {
            ReleaseBackBufferLinkedResources();

            //Resize renderTarget based on the new backbuffer size
            CreateSwapChain();
            CreateRenderTarget();
            CreateDepthStencil();
            CreateViewPort();
            CreateStaggingBackBuffer();

            Context.OutputMerger.SetTargets(_depthStencil, _renderTarget);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //Dispose the created states
            _backBuffer.Dispose();
            _dx11factory.Dispose();
            _depthStencil.Dispose();
            _renderTarget.Dispose();
            _swapChain.Dispose();
            _staggingBackBufferTexture.Dispose();
            StaggingBackBuffer.Dispose();
            Context.Dispose();
            GraphicsDevice.Dispose();
        }

        #endregion
    }
}
