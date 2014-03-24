using System;
using System.Drawing;
using S33M3DXEngine.RenderStates;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;
using System.Collections.Generic;
using S33M3DXEngine.Threading;

namespace S33M3DXEngine
{
    //3D Engine based on SharpDX - target microsoft D3D10
    public class D3DEngine : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Static Variables
        public static IntPtr WindowHandle;
        public static List<SampleDescription> MSAAList = new List<SampleDescription>();
        public static string MainAdapter;

        public SampleDescription CurrentMSAASampling = new SampleDescription(1, 0);
        //Trick to avoid VertexBuffer PrimitiveTopology change when not needed
        //CANNOT be use in a multithreaded buffer approch !
        public static bool SingleThreadRenderingOptimization = true;

        //public static bool FULLDEBUGMODE = true;
        public static bool FULLDEBUGMODE = false;
        #endregion

        #region Private variables
        private bool _isResizing = false;
        private RenderForm _renderForm;
        private SwapChain _swapChain;
        private RenderTargetView _renderTarget;
        private DepthStencilView _depthStencil;
        private ViewportF _viewPort;
        private Factory1 _dx11factory;

        private List<DeviceContext> _registeredDC = new List<DeviceContext>();

#if DEBUG
        public static readonly ShaderFlags ShaderFlags = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#else
        public static readonly ShaderFlags ShaderFlags = ShaderFlags.OptimizationLevel3;
#endif
        #endregion

        #region Public properties
        public Device Device;

        public delegate void ScreenSizeUpdated(ViewportF viewport, Texture2DDescription newBackBuffer);
        public event ScreenSizeUpdated ScreenSize_Updated;

        public ViewportF ViewPort { get { return _viewPort; } set { _viewPort = value; } }
        public RenderForm GameWindow { get { return _renderForm; } }
        public SwapChain SwapChain { get { return _swapChain; } }

        public bool IsShuttingDownRequested;
        public bool IsShuttingDownSafe;
        public event EventHandler OnShuttingDown;

        public List<string> RunningThreadedWork = new List<string>();

        public RenderTargetView RenderTarget { get { return _renderTarget; } }
        public DepthStencilView DepthStencilTarget { get { return _depthStencil; } }

        public Vector2 BackBufferSize;
        public Texture2D BackBufferTex;
        public bool isInitialized;

        public Matrix Projection2D;
        /// <summary>
        /// Leave at 0:0 to use optimal resolution display.
        /// </summary>
        public Size RenderResolution { get; set; }

        public bool IsB8G8R8A8_UNormSupport { get; set; }

        public DeviceContext ImmediateContext;

        public bool HasFocus;

        public bool IsFullScreen
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

        /// <summary>
        /// Creating a new D3DEngine with an associated window for rendering
        /// </summary>
        /// <param name="startingSize">Windows starting size</param>
        /// <param name="windowCaption">Window Caption</param>
        /// <param name="RenderResolution">if not passed or equal to 0;0 then the resolution will be the one from the Windows Size</param>
        public D3DEngine(Size startingSize, string windowCaption, SampleDescription samplingMode, Size renderResolution = default(Size))
        {
            IsShuttingDownRequested = false;
            //Create the MainRendering Form
            _renderForm = new RenderForm()
            {
                Text = windowCaption,
                ClientSize = startingSize,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            };
            WindowHandle = _renderForm.Handle;
            _renderForm.KeyUp += new System.Windows.Forms.KeyEventHandler(_renderForm_KeyUp);

            this.RenderResolution = renderResolution;
            this.CurrentMSAASampling = samplingMode;

            string errorInit;
            isInitialized = false;
            if (Initialize(out errorInit) == false)
            {
                errorInit = "DirectX initialization error, cannot start the game : " + errorInit;
                System.Windows.Forms.MessageBox.Show(errorInit);
                logger.Error(errorInit);
                return;
            }
            isInitialized = true;

            //Init State repo
            RenderStatesRepo.Initialize(this);
        }

        public D3DEngine()
        {
            if (!FULLDEBUGMODE)
            {
                Device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
                logger.Info("Device created in Release mode");
            }
            else
            {
                try
                {
                    Device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug);
                    logger.Info("Device et swapchain created in FULLDEBUGMODE mode");
                }
                catch (SharpDXException ex)
                {
                    logger.Warn("Error Creating SwapChain or Device in debug mode : {0}", ex.Message);
                    Device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
                    logger.Info("Device et swapchain created in Release mode");
                }
            }

            //Create the threaded contexts
            ImmediateContext = Device.ImmediateContext;
        }

        public DeviceContext CreateDeviceContext()
        {
            DeviceContext dc = new DeviceContext(Device);
            _registeredDC.Add(dc);
            return dc;
        }

        //Check if the shuttingDownCan be done safely
        public void ShuttingDownProcess()
        {
            //Check for fullscreen state
            if (IsFullScreen)
            {
                IsFullScreen = false;
                return;
            }

            if (OnShuttingDown != null) OnShuttingDown(this, null);

            //Independant Threaded work are still running, waiting them for shutting down.
            if (RunningThreadedWork.Count > 0) return;

            ThreadsManager.IsShuttingDown = true;

            if (ThreadsManager.RunningThreads > 0) return;

            ThreadsManager.Dispose();

            IsShuttingDownSafe = true;
        }

        //Remove default F10 (Open menu) Form key push !
        void _renderForm_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.F10) e.Handled = true;
        }

        #region Private methods
        #endregion

        #region Public Methods
        public bool Initialize(out string errorMsg)
        {
            try
            {
            errorMsg = null;

            List<ModeDescription> adapterModes = new List<ModeDescription>();
            _dx11factory = new Factory1();

            using (Adapter1 adapter = _dx11factory.GetAdapter1(0))
            {
                using (Output output = adapter.Outputs[0])
                {
                    IsB8G8R8A8_UNormSupport = false;
                    foreach (var mode in output.GetDisplayModeList(Format.B8G8R8A8_UNorm, DisplayModeEnumerationFlags.Interlaced))
                    {
                        IsB8G8R8A8_UNormSupport = true;
                        adapterModes.Add(mode);
                    }

                    MainAdapter = adapter.Description.Description;
                    logger.Info("GPU found : {0}", MainAdapter);
                    //GetResource Level            
                    FeatureLevel maxSupportLevel = Device.GetSupportedFeatureLevel(adapter);
                    logger.Info("Maximum supported DirectX11 level = {0}", maxSupportLevel.ToString());
                    
                    if (maxSupportLevel == FeatureLevel.Level_9_1 || 
                        maxSupportLevel == FeatureLevel.Level_9_2 || 
                        maxSupportLevel == FeatureLevel.Level_9_3)
                    {
                        errorMsg = "Your graphical card doesn't support at minimum DirectX 10 feature, current feature : " + maxSupportLevel.ToString();
                        return false;
                    }

                    int DedicatedGPU = adapter.Description.DedicatedVideoMemory / (1024 * 1024);
                    if (DedicatedGPU < 0) DedicatedGPU = 0;
                    int DedicatedSystem = adapter.Description.DedicatedSystemMemory / (1024 * 1024);
                    if (DedicatedSystem < 0) DedicatedSystem = 0;
                    int SharedSystem = adapter.Description.SharedSystemMemory / (1024 * 1024);
                    if (SharedSystem < 0) SharedSystem = 0;

                    logger.Info("GPU Memory : Dedicated from GPU : {0}MB, Shared : {1}MB, Dedicated from System : {2}MB. Total : {3}MB", DedicatedGPU, DedicatedSystem, SharedSystem, DedicatedGPU + DedicatedSystem + SharedSystem);
                    logger.Info("B8G8R8A8_UNormSupport compatibility = {0}", IsB8G8R8A8_UNormSupport);

#if DEBUG
                    foreach (var mode in adapterModes)
                    {
                        logger.Trace("[{1}:{2}], format : {0}, RefreshRate : {3}hz, Scaling : {4}, ScanlineMode : {5}", mode.Format, mode.Width, mode.Height, (float)mode.RefreshRate.Numerator / mode.RefreshRate.Denominator, mode.Scaling, mode.ScanlineOrdering);
                    }
#endif
                }
            }

            RefreshResources();

            GetMSAAQualities(Format.B8G8R8A8_UNorm);

            //Remove the some built-in fonctionnality of DXGI
            _dx11factory.MakeWindowAssociation(_renderForm.Handle, WindowAssociationFlags.IgnoreAll | WindowAssociationFlags.IgnoreAltEnter);

            _renderForm.ResizeBegin += _renderForm_ResizeBegin;
            _renderForm.ResizeEnd += _renderForm_ResizeEnd;
            _renderForm.Resize += _renderForm_Resize;
            _renderForm.LostFocus += GameWindow_LostFocus;
            _renderForm.GotFocus += GameWindow_GotFocus;

            _renderForm.Show();
            _renderForm.Focus();
            _renderForm.TopMost = true;
            HasFocus = true;
            _renderForm.TopMost = false;

            return true;

            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return false;
            }
        }

        public void SetRenderTargets(DeviceContext context)
        {
            context.OutputMerger.SetTargets(_depthStencil, _renderTarget);
        }

        public void SetRenderTargetsAndViewPort(DeviceContext context)
        {
            SetRenderTargets(context);
            context.Rasterizer.SetViewport(_viewPort);
        }

        public void SetCustomViewPort(ViewportF customViewPort)
        {
            ImmediateContext.Rasterizer.SetViewport(customViewPort);
        }

        public void SetScreenViewPort()
        {
            ImmediateContext.Rasterizer.SetViewport(_viewPort);
        }

        public SharpDX.Rectangle[] ScissorRectangles
        {
            get
            {
                return ImmediateContext.Rasterizer.GetScissorRectangles();
            }
            set
            {
                ImmediateContext.Rasterizer.SetScissorRectangles(value);
            }
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

        void GameWindow_GotFocus(object sender, EventArgs e)
        {
            HasFocus = true;
        }

        void GameWindow_LostFocus(object sender, EventArgs e)
        {
            HasFocus = false;
        }

        private void CreateSwapChain()
        {
            if (_swapChain == null)
            {
                //Create the SwapChain Param object
                SwapChainDescription SwapDesc = new SwapChainDescription()
                {
                    BufferCount = 1,
                    Usage = Usage.RenderTargetOutput,
                    OutputHandle = _renderForm.Handle,
                    IsWindowed = true,
                    ModeDescription = RenderResolution == default(Size) ? new ModeDescription() { Format = Format.R8G8B8A8_UNorm, Width = _renderForm.ClientSize.Width, Height = _renderForm.ClientSize.Height }
                                                                        : new ModeDescription() { Format = Format.R8G8B8A8_UNorm, Width = RenderResolution.Width, Height = RenderResolution.Height },
                    SampleDescription = CurrentMSAASampling,
                    SwapEffect = SwapEffect.Discard
                };

                if (!FULLDEBUGMODE)
                {
                    Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, SwapDesc, out Device, out _swapChain);
                    logger.Info("Device and swapchain created in Release mode");
                }
                else
                {
                    try
                    {
                        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, SwapDesc, out Device, out _swapChain);
                        logger.Info("Device et swapchain created in FULLDEBUGMODE mode");
                    }
                    catch (SharpDXException ex)
                    {
                        logger.Warn("Error Creating SwapChain or Device in debug mode : {0}", ex.Message);
                        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, SwapDesc, out Device, out _swapChain);
                        logger.Info("Device et swapchain created in Release mode");
                    }
                }

                //Create the threaded contexts
                ImmediateContext = Device.ImmediateContext;

#if DEBUG
                //Set resource Name, will only be done at debug time.
                Device.DebugName = "Main Created DX11 device";
                _swapChain.DebugName = "Main Swap Chain";
                ImmediateContext.DebugName = "Immediat Context";
#endif
            }
            else
            {
                if (RenderResolution == default(Size) || IsFullScreen)
                {
                    _swapChain.ResizeBuffers(1, _renderForm.ClientSize.Width, _renderForm.ClientSize.Height, Format.R8G8B8A8_UNorm, (int)SwapChainFlags.None);
                }
                else
                {
                    _swapChain.ResizeBuffers(1, RenderResolution.Width, RenderResolution.Height, Format.R8G8B8A8_UNorm, (int)SwapChainFlags.None);
                }

            }
            
            logger.Info("SwapChain is using the GPU in mode : [{1}px * {2}px], format : {0}, RefreshRate : {3}hz, Scaling : {4}, ScanlineMode : {5}", _swapChain.Description.ModeDescription.Format, _swapChain.Description.ModeDescription.Width, _swapChain.Description.ModeDescription.Height, (float)_swapChain.Description.ModeDescription.RefreshRate.Numerator / _swapChain.Description.ModeDescription.RefreshRate.Denominator, _swapChain.Description.ModeDescription.Scaling, _swapChain.Description.ModeDescription.ScanlineOrdering);
            // Get the created BackBuffer
            BackBufferTex = Resource.FromSwapChain<Texture2D>(_swapChain, 0);

#if DEBUG
            //Set resource Name, will only be done at debug time.
            BackBufferTex.DebugName = "Device BackBuffer";
#endif
        }

        private void GetMSAAQualities(Format format)
        {
            for (int SamplingCount = 0; SamplingCount <= SharpDX.Direct3D11.Device.MultisampleCountMaximum; SamplingCount++)
            {
                int Quality = Device.CheckMultisampleQualityLevels(format, SamplingCount);
                if (Quality > 0)
                {
                    //Add Base Quality
                    MSAAList.Add(new SampleDescription() { Count = SamplingCount, Quality = 0 });

                    if (MainAdapter.ToUpper().Contains("NVIDIA"))
                    {
                        //Add CSAA 8x
                        if (Quality >= 9)
                        {
                            MSAAList.Add(new SampleDescription() { Count = SamplingCount, Quality = 8 });
                        }

                        //Add CSAA 16x
                        if (Quality >= 17)
                        {
                            MSAAList.Add(new SampleDescription() { Count = SamplingCount, Quality = 16 });
                        }

                        //Add CSAA 32x
                        if (Quality >= 33)
                        {
                            MSAAList.Add(new SampleDescription() { Count = SamplingCount, Quality = 32 });
                        }
                    }
                }
            }

#if DEBUG
            foreach (var mode in MSAAList)
            {
                logger.Trace("Compatible Device MSAA : {0}x", mode.Count);
            }
#endif
        }

        private void CreateRenderTarget()
        {
            //Create the Depth Stencil View + View
            RenderTargetViewDescription renderTargetViewDescription = new RenderTargetViewDescription()
            {
                Format = BackBufferTex.Description.Format,
                Dimension = CurrentMSAASampling.Count <= 1 ? RenderTargetViewDimension.Texture2D : RenderTargetViewDimension.Texture2DMultisampled
            };
            //Create RenderTargetView 
            _renderTarget = new RenderTargetView(Device, BackBufferTex, renderTargetViewDescription);
        }

        private void CreateDepthStencil()
        {
            //Create the Depth Stencil Texture
            Texture2DDescription DepthStencilDescr = new Texture2DDescription()
            {
                Width = BackBufferTex.Description.Width,
                Height = BackBufferTex.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R32_Typeless,
                SampleDescription = CurrentMSAASampling,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };
            Texture2D DepthStencilBuffer = new Texture2D(Device, DepthStencilDescr);

#if DEBUG
            //Set resource Name, will only be done at debug time.
            DepthStencilBuffer.DebugName = "DepthStencilBuffer Texture";
#endif

            //Create the Depth Stencil View + View
            DepthStencilViewDescription DepthStencilViewDescr = new DepthStencilViewDescription()
            {
                Format = Format.D32_Float,
                Dimension = CurrentMSAASampling.Count <= 1 ? DepthStencilViewDimension.Texture2D : DepthStencilViewDimension.Texture2DMultisampled,
                Texture2D = new DepthStencilViewDescription.Texture2DResource() { MipSlice = 0 }
            };
            //Create the Depth Stencil view
            _depthStencil = new DepthStencilView(Device, DepthStencilBuffer, DepthStencilViewDescr);

            DepthStencilBuffer.Dispose();
        }

        private void CreateViewPort()
        {
            //Create ViewPort
            _viewPort = new ViewportF(0, 0, BackBufferTex.Description.Width, BackBufferTex.Description.Height, 0, 1);
            BackBufferSize = new Vector2(_viewPort.Width, _viewPort.Height);

            //Refresh the Projection2D Matrix (Doesn't a camera to set it up !)
            Matrix.OrthoOffCenterLH(0, _viewPort.Width, _viewPort.Height, 0, 0, 1, out Projection2D); // Make the 0,0 bottom/left, 1,1 Up/right

            if (ScreenSize_Updated != null) ScreenSize_Updated(_viewPort, BackBufferTex.Description);
            ImmediateContext.Rasterizer.SetViewport(_viewPort);

            logger.Debug("ViewPort Updated new size Width : {0}px Height : {1}px", BackBufferTex.Description.Width, BackBufferTex.Description.Height);
        }

        private void ReleaseBackBufferLinkedResources()
        {
            if (BackBufferTex != null)
            {
                BackBufferTex.Dispose(); BackBufferTex = null;
            }
            if (_renderTarget != null)
            {
                _renderTarget.Dispose(); _renderTarget = null;
            }
            if (_depthStencil != null)
            {
                _depthStencil.Dispose(); _depthStencil = null;
            }
        }

        private void WindowSizeChanged()
        {
            logger.Debug("Window size changed new size = Width : {0}px Height : {1}px", _renderForm.ClientSize.Width, _renderForm.ClientSize.Height);

            ReleaseBackBufferLinkedResources();
            RefreshResources();
        }

        private void RefreshResources()
        {
            //Resize renderTarget based on the new backbuffer size
            CreateSwapChain();
            CreateRenderTarget();
            CreateDepthStencil();
            CreateViewPort();

            SetRenderTargets(Device.ImmediateContext);

            logger.Debug("SwapChain, RenderTarget, DepthStencil, StaggingBackBuffer and viewport recreated");
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                //Clean Up event Delegates
                if (ScreenSize_Updated != null)
                {
                    //Remove all Events associated to this control (That haven't been unsubscribed !)
                    foreach (Delegate d in ScreenSize_Updated.GetInvocationList())
                    {
                        ScreenSize_Updated -= (ScreenSizeUpdated)d;
                    }
                }

                //Dispo State repo
                RenderStatesRepo.Dispose();

                if (_renderForm != null)
                {
                    _renderForm.ResizeBegin -= _renderForm_ResizeBegin;
                    _renderForm.ResizeEnd -= _renderForm_ResizeEnd;
                    _renderForm.Resize -= _renderForm_Resize;
                    _renderForm.LostFocus -= GameWindow_LostFocus;
                    _renderForm.GotFocus -= GameWindow_GotFocus;
                }

                //Dispose the created states
                if (BackBufferTex != null) BackBufferTex.Dispose();
                if (_dx11factory != null) _dx11factory.Dispose();
                if (_depthStencil != null) _depthStencil.Dispose();
                if (_renderTarget != null) _renderTarget.Dispose();
                if (_swapChain != null) _swapChain.Dispose();

                foreach (var dc in _registeredDC) dc.Dispose();

                //The ImmediatContext is automaticaly disposed by the Device
                Device.Dispose();
            }
            catch (Exception e)
            {
                logger.Error("Error logged at Engine disposal time {0}", e.Message);
            }
        }

        #endregion
    }
}
