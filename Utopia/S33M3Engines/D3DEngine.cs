using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using S33M3Engines.Windows;
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
        public static bool FULLDEBUGMODE = false;
        #endregion

        #region Private variables
        int _mouseHideCount = 0;
        bool _isResizing = false;
        bool _mouseCapture = true;
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

        public event EventHandler<D3DEngineMouseCaptureChangedEventArgs> MouseCaptureChanged;

        private void OnMouseCaptureChanged(D3DEngineMouseCaptureChangedEventArgs e)
        {
            var handler = MouseCaptureChanged;
            if (handler != null) handler(this, e);
        }

        #region Public properties
        public Device Device;

        public D3DEngineDelegates.ViewPortUpdated ViewPort_Updated;
        public ShaderFlags ShaderFlags { get { return _shaderFlags; } }
        public Viewport ViewPort { get { return _viewPort; } set { _viewPort = value; } }
        public RenderForm GameWindow { get { return _renderForm; } }
        public SwapChain SwapChain { get { return _swapChain; } }

        public RenderTargetView RenderTarget { get { return _renderTarget; } }
        public DepthStencilView DepthStencilTarget { get { return _depthStencil; } }
        public ShaderResourceView StaggingBackBuffer;

        public bool B8G8R8A8_UNormSupport { get; set; }

        public DeviceContext Context;

        public bool HasFocus;

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

        /// <summary>
        /// Gets or sets whether the mouse is captured by the engine
        /// </summary>
        public bool MouseCapture
        {
            get { return _mouseCapture; }
            set
            {
                if (value != _mouseCapture)
                {
                    if (_mouseCapture)
                    {
                        ShowMouseCursor();
                    }
                    else
                    {
                        HideMouseCursor();
                    }
                    _mouseCapture = value;

                    OnMouseCaptureChanged(new D3DEngineMouseCaptureChangedEventArgs { MouseCaptured = _mouseCapture });
                }
            }
        }

        #endregion

        //Constructor
        public D3DEngine(Size startingSize, string windowCaption)
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

            //Link the mouse to the windows handle
            S33M3Engines.InputHandler.Mouse.SetMouseMessageHooker(D3DEngine.WindowHandle);

            Initialize();
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

            B8G8R8A8_UNormSupport = false;
            foreach (var mode in _dx11factory.GetAdapter(0).GetOutput(0).GetDisplayModeList(Format.B8G8R8A8_UNorm, DisplayModeEnumerationFlags.Interlaced))
            {
                B8G8R8A8_UNormSupport = true;
            }

            CreateSwapChain();
            CreateRenderTarget();
            CreateDepthStencil();
            CreateViewPort();
            CreateStaggingBackBuffer();

            //Set Viewport and rendertarget to the device
            Context.OutputMerger.SetTargets(_depthStencil, _renderTarget);

            //Remove the some built-in fonctionnality of DXGI
            _dx11factory.MakeWindowAssociation(_renderForm.Handle, WindowAssociationFlags.IgnoreAll | WindowAssociationFlags.IgnoreAltEnter);

            _renderForm.ResizeBegin += _renderForm_ResizeBegin;
            _renderForm.ResizeEnd += _renderForm_ResizeEnd;
            _renderForm.Resize += _renderForm_Resize;
            _renderForm.LostFocus += GameWindow_LostFocus;
            _renderForm.GotFocus += GameWindow_GotFocus;
            _renderForm.Closed += _renderForm_Closed;

            _renderForm.Show();
            _renderForm.Focus();
            _renderForm.TopMost = true;
            HasFocus = true;
            _renderForm.TopMost = false;
        }

        public void ResetDefaultRenderTargets()
        {
            Context.OutputMerger.SetTargets(_depthStencil, _renderTarget);
        }

        public void ResetDefaultRenderTargetsAndViewPort()
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

        public SharpDX.Rectangle ScissorRectangle
        {
            get { return Context.Rasterizer.GetScissorRectangles()[0]; }
            set { Context.Rasterizer.SetScissorRectangles(new SharpDX.Rectangle[] { value }); }
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
            if (MouseCapture) HideMouseCursor();
        }

        void GameWindow_LostFocus(object sender, EventArgs e)
        {
            HasFocus = false;
            ShowMouseCursor();            
        }

        void _renderForm_Closed(object sender, EventArgs e)
        {
            ResetMouseCursor();
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
                    Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, SwapDesc, out Device, out _swapChain);
                }
                else
                {
                    try
                    {
                        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, SwapDesc, out Device,
                                                   out _swapChain);
                    }
                    catch (SharpDXException ex)
                    {
                        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, SwapDesc, out Device, out _swapChain);
                    }
                }

                //Create the threaded contexts
                Context = Device.ImmediateContext;
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
            _renderTarget = new RenderTargetView(Device, _backBuffer);
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
            Texture2D DepthStencilBuffer = new Texture2D(Device, DepthStencilDescr);

            //Create the Depth Stencil View + View
            DepthStencilViewDescription DepthStencilViewDescr = new DepthStencilViewDescription()
            {
                Format = DepthStencilDescr.Format,
                Dimension = DepthStencilViewDimension.Texture2D,
                Texture2D = new DepthStencilViewDescription.Texture2DResource() { MipSlice = 0 }
            };
            //Create the Depth Stencil view
            _depthStencil = new DepthStencilView(Device, DepthStencilBuffer, DepthStencilViewDescr);

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

            _staggingBackBufferTexture = new Texture2D(Device, StaggingBackBufferDescr);
            StaggingBackBuffer = new ShaderResourceView(Device, _staggingBackBufferTexture);
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

        public void HideMouseCursor()
        {
            while (_mouseHideCount >= 0)
            {
                System.Windows.Forms.Cursor.Hide();
                _mouseHideCount--;
            }
        }

        public void ShowMouseCursor()
        {
            while (_mouseHideCount < 0)
            {
                System.Windows.Forms.Cursor.Show();
                _mouseHideCount++;
            }
        }

        public void ResetMouseCursor()
        {
            //System.Windows.Forms.Cursor.Hide();

            while (_mouseHideCount < 0)
            {
                System.Windows.Forms.Cursor.Show();
                _mouseHideCount++;
            }

            while (_mouseHideCount > 0)
            {
                System.Windows.Forms.Cursor.Hide();
                _mouseHideCount--;
            }
        }

        private IntPtr _cursorPtr;

        public Cursor AssignMouseCursor(String text, Brush color = null)
        {
            if (color == null) color = Brushes.Red;

            Bitmap bitmap = new Bitmap(140, 25);
            Graphics g = Graphics.FromImage(bitmap);
            using (Font f = new Font(FontFamily.GenericSansSerif, 10))
                g.DrawString(text, f, color, 0, 0);
            Cursor old = GameWindow.Cursor;
            GameWindow.Cursor = BuildMouseCursor(bitmap, 3, 3);
            bitmap.Dispose();
            return old;
        }

        public Cursor AssignMouseCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            Cursor old = GameWindow.Cursor;
            GameWindow.Cursor = BuildMouseCursor(bmp, xHotSpot, xHotSpot);
            return old;
        }

        private Cursor BuildMouseCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            if (_cursorPtr != IntPtr.Zero) UnsafeNativeMethods.DestroyIcon(_cursorPtr);

            IntPtr ptr = bmp.GetHicon();
            IconInfo tmp = new IconInfo();
            UnsafeNativeMethods.GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            _cursorPtr = UnsafeNativeMethods.CreateIconIndirect(ref tmp);

            if (tmp.hbmColor != IntPtr.Zero) UnsafeNativeMethods.DeleteObject(tmp.hbmColor);
            if (tmp.hbmMask != IntPtr.Zero) UnsafeNativeMethods.DeleteObject(tmp.hbmMask);
            if (ptr != IntPtr.Zero) UnsafeNativeMethods.DestroyIcon(ptr);

            return new Cursor(_cursorPtr);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Context.ClearState();
            Context.Flush();

            _renderForm.ResizeBegin -= _renderForm_ResizeBegin;
            _renderForm.ResizeEnd -= _renderForm_ResizeEnd;
            _renderForm.Resize -= _renderForm_Resize;
            _renderForm.LostFocus -= GameWindow_LostFocus;
            _renderForm.GotFocus -= GameWindow_GotFocus;
            _renderForm.Closed -= _renderForm_Closed;

            //Dispose the created states
            _backBuffer.Dispose();
            _dx11factory.Dispose();
            _depthStencil.Dispose();
            _renderTarget.Dispose();
            _swapChain.Dispose();
            _staggingBackBufferTexture.Dispose();
            StaggingBackBuffer.Dispose();
            Device.Dispose();

            S33M3Engines.InputHandler.Mouse.CleanUp();

            S33M3Engines.D3D.Tools.Resource.CleanUpSetNames();
        }

        #endregion
    }

    public class D3DEngineMouseCaptureChangedEventArgs : EventArgs
    {
        public bool MouseCaptured { get; set; }
    }
}
