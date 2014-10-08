using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Diagnostics;
using System.Threading;
using SharpDX.DXGI;
using S33M3DXEngine.Threading;
using S33M3DXEngine.Main.Interfaces;
using SharpDX.Windows;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.Buffers;
using SharpDX.Direct3D11;
using System.Drawing;
using S33M3DXEngine.Debug;

namespace S33M3DXEngine.Main
{
    public class Game : BaseComponent
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public delegate void RenderLoopFrozen(float frozenTime);
        public event RenderLoopFrozen OnRenderLoopFrozen;

        #region Public Properties

        //protected D3DEngine _d3dEngine;
        public bool DebugActif
        {
            get { return _debugActif; }
            set { _debugActif = value; }
        }

        public int DebugDisplay
        {
            get { return _debugDisplay; }
            set { _debugDisplay = value; }
        }

        public Color4 BackBufferColor
        {
            get { return _backBufferColor; }
            set { _backBufferColor = value; }
        }

        public GameComponentCollection GameComponents
        {
            get { return _gameComponents; }
        }

        //two lists are necessary for having two sortings : one is sorted by draworder and the other by updateorder 
        private readonly List<DrawableComponentHolder> _visibleDrawable;
        private readonly List<IUpdatableComponent> _enabledUpdatable;
        //this is for having the possibility to remove components at runtime, while iterating on the list
        //exemple : enabling the editor disables the normal Hud. without the _currentlyxxx collections you get invalid operation modify list while iterating
        private bool isCurrentlyUpdatingComponentsDirty;
        private readonly List<IUpdatableComponent> _currentlyUpdatingComponents = new List<IUpdatableComponent>();

        public bool VSync
        {
            get { return _vSync == 1; }
            set { _vSync = value == true ? 1 : 0; }
        }
        public PerfMonitor ComponentsPerfMonitor { get; set; }

        public long FramelimiterTime
        {
            get { return _framelimiterTime; }
            set { _framelimiterTime = value; }
        } 

        #endregion

        #region Private Variable

        private delegate void WinformInvockCallBack();
        public D3DEngine Engine;
        Stopwatch _framelimiter = new Stopwatch();
        long _framelimiterTime = 0;

        private int _maxRenderFrameSkip = 5; //Maximum frame rendering skipping

        private int _updateWithoutrenderingCount;
        private double _interpolation_hd;
        private float _interpolation_ld;
        private long _nextGameUpdateTime = Stopwatch.GetTimestamp();

        private int _debugDisplay = 0;
        private bool _debugActif = false;
        private readonly GameComponentCollection _gameComponents;
        private Color4 _backBufferColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        public static readonly GameTime GameTime = new GameTime();
        private int _vSync = 1;

        #endregion

        //Constructed Engine
        public Game(Size startingWindowsSize, string WindowsCaption, SampleDescription sampleDescription, Size ResolutionSize = default(Size), bool withDebugObjectTracking = false)
        {
            Engine = ToDispose(new D3DEngine(startingWindowsSize, WindowsCaption, sampleDescription, ResolutionSize));
            if (Engine.isInitialized)
            {
                Engine.GameWindow.FormClosing += GameWindow_FormClosing;

                _visibleDrawable = new List<DrawableComponentHolder>();
                _enabledUpdatable = new List<IUpdatableComponent>();
                _gameComponents = ToDispose(new GameComponentCollection());

                gameInitialize(withDebugObjectTracking);
            }
        }

        //Injected Engine
        public Game(D3DEngine engine, bool withDebugObjectTracking = false)
        {
            Engine = engine;
            if (Engine.isInitialized)
            {
                Engine.GameWindow.FormClosing += GameWindow_FormClosing;
                _visibleDrawable = new List<DrawableComponentHolder>();
                _enabledUpdatable = new List<IUpdatableComponent>();
                _gameComponents = ToDispose(new GameComponentCollection());

                gameInitialize(withDebugObjectTracking);
            }
        }

        protected virtual void GameWindow_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
        }

        private void gameInitialize(bool withDebugObjectTracking = false)
        {
#if DEBUG
            Configuration.EnableObjectTracking = withDebugObjectTracking;
            logger.Info("Configuration.EnableObjectTracking : {0}", Configuration.EnableObjectTracking);
#endif
            _gameComponents.ComponentAdded += new EventHandler<GameComponentCollectionEventArgs>(GameComponentAdded);
            _gameComponents.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(GameComponentRemoved);

            ComponentsPerfMonitor = ToDispose(new PerfMonitor());
            _framelimiter.Start();
        }

        #region Public Methods

        //Init + Start the pump !
        public virtual void Run()
        {
            //Check if the Threading engine has been initialize or not
            ThreadsManager.CheckInit();

            //Call the game Initialize !
            Initialize();

            //Call components Load Content
            LoadContent();

            ResetTimers();

            FixedTimeStepLoop();
        }

        private void FixedTimeStepLoop()
        {
            //The Pump !
            RenderLoop.Run(Engine.GameWindow, () =>
            {
                if (Engine.IsShuttingDownRequested)
                {
                    Engine.ShuttingDownProcess();
                    if (Engine.IsShuttingDownSafe) CloseWinform();
                }

                //In case, if too much time has passed, Skip the update for this time period
                //(Help in case of a break point placed in inline working)
                //Default is a "pause of more then 10 seconde".
                if (Stopwatch.GetTimestamp() - _nextGameUpdateTime > GameTime.FTSSafeGuard)
                {
                    ResetTimers();
                }

                _updateWithoutrenderingCount = 0;
                while (Stopwatch.GetTimestamp() > _nextGameUpdateTime && _updateWithoutrenderingCount < _maxRenderFrameSkip)
                {
#if DEBUG
                    if (_updateWithoutrenderingCount == 1 && ComponentsPerfMonitor.Updatable)
                    {
                        logger.Debug("Frame skipped because too late for : {0:0.000}, Updt maximum Delta {1:0.000}", GameTime.Tick2Ms(Stopwatch.GetTimestamp() - _nextGameUpdateTime), GameTime.Tick2Ms(GameTime.GameUpdateDelta));
                        logger.Debug("Last     Update time {0:0.000}, Draw time {1:0.000}, Present wait for {2:0.000}", GameTime.Tick2Ms(ComponentsPerfMonitor.PerfTimer.GetLastUpdateTime), GameTime.Tick2Ms(ComponentsPerfMonitor.PerfTimer.GetLastDrawTime), GameTime.Tick2Ms(ComponentsPerfMonitor.PerfTimer.GetLastPresentTime));
                        logger.Debug("Previous Update time {0:0.000}, Draw time {1:0.000}, Present wait for {2:0.000}", GameTime.Tick2Ms(ComponentsPerfMonitor.PerfTimer.GetPrevUpdateTime), GameTime.Tick2Ms(ComponentsPerfMonitor.PerfTimer.GetPrevDrawTime), GameTime.Tick2Ms(ComponentsPerfMonitor.PerfTimer.GetPrevPresentTime));

                        foreach (var result in ComponentsPerfMonitor.PerfTimer.GetComponentByDeltaPerf(5))
                        {
                            logger.Debug("Perf problem could caused by {0:0.000}, Last duration {1:0.000}, previous duration {2:0.000}", result.Name,  GameTime.Tick2Ms(result.LastValue), GameTime.Tick2Ms(result.PrevValue));
                        }
                        logger.Debug("=================================================================================");

                    }
#endif
                    if (_updateWithoutrenderingCount > 0)
                    {
                        //In case, if too much time has passed, Skip the update for this time period
                        //(Help in case of a break point placed in inline working)
                        //Default is a "pause of more then 10 seconde".
                        if (Stopwatch.GetTimestamp() - _nextGameUpdateTime > GameTime.FTSSafeGuard)
                        {
                            logger.Info("SafeGuard for FTS triggered");
                            if (OnRenderLoopFrozen != null) OnRenderLoopFrozen(GameTime.QueryElapsedTime());
                            ResetTimers();
                        }
                    }

                    FTSUpdate(GameTime);

                    _nextGameUpdateTime += GameTime.GameUpdateDelta;
                    _updateWithoutrenderingCount++;
                }

                _interpolation_hd = (double)(Stopwatch.GetTimestamp() + GameTime.GameUpdateDelta - _nextGameUpdateTime) / GameTime.GameUpdateDelta;
                _interpolation_ld = (float)_interpolation_hd;
                VTSUpdate(_interpolation_hd, _interpolation_ld, GameTime.GetElapsedTime());
                Draw();
            });
        }

        private void ResetTimers()
        {
            //Reset the fixed time step to avoid flickering
            _nextGameUpdateTime = Stopwatch.GetTimestamp();
            GameTime.ResetElapsedTimeCounter();
        }

        //Close Window to stop the Window Pump !
        //HACK [DebuggerStepThrough on EXIT] To avoid breaking inside while debugging => Remove it to give the possibility for the debugger to stop inside this function
        //[DebuggerStepThrough()]
        public void Exit()
        {
            Engine.IsShuttingDownRequested = true;
        }

        private void CloseWinform()
        {
            if (Engine.GameWindow.InvokeRequired)
            {
                WinformInvockCallBack d = new WinformInvockCallBack(CloseWinform);
                Engine.GameWindow.Invoke(d);
            }
            else
            {
                Engine.GameWindow.Close();
            }
        }

        //Game Initialize
        public virtual void Initialize()
        {
            for (int i = 0; i < GameComponents.Count; i++)
            {
                GameComponents[i].Initialize();
            }
        }

        public virtual void LoadContent()
        {
            for (int i = 0; i < GameComponents.Count; i++)
            {
                GameComponents[i].LoadContent(Engine.ImmediateContext);
            }
        }

        /// <summary>
        /// Fixed Time Step Update
        /// </summary>
        /// <param name="TimeSpend">The fixed amount of time between 2 FTSUpdate call</param>
        public virtual void FTSUpdate(GameTime TimeSpend)
        {
            _currentlyUpdatingComponents.Clear();
            for (int i = 0; i < _enabledUpdatable.Count; i++) _currentlyUpdatingComponents.Add(_enabledUpdatable[i]);

            for (int i = 0; i < _currentlyUpdatingComponents.Count; i++)
            {
                if (isCurrentlyUpdatingComponentsDirty)
                {
                    isCurrentlyUpdatingComponentsDirty = false;
                    break;
                }

                if (ComponentsPerfMonitor.Updatable)
                {
                    ComponentsPerfMonitor.StartMesure(_currentlyUpdatingComponents[i], "Update");
                    _currentlyUpdatingComponents[i].FTSUpdate(TimeSpend);
                    ComponentsPerfMonitor.StopMesure(_currentlyUpdatingComponents[i], "Update");
                }
                else
                {
                    _currentlyUpdatingComponents[i].FTSUpdate(TimeSpend);
                }
            }
        }

        /// <summary>
        /// The Variable Time Step Update
        /// </summary>
        /// <param name="interpolationHd">FTSUpdate interpolation variables</param>
        /// <param name="interpolationLd">FTSUpdate interpolation variables</param>
        /// <param name="elapsedTime">Amount if time elpased since last call in ms</param>
        public virtual void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            for (int i = 0; i < _currentlyUpdatingComponents.Count; i++)
            {
                _currentlyUpdatingComponents[i].VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
            }
        }

        public virtual void Draw()
        {
            //Init New Frame before drawing components
            Engine.ImmediateContext.ClearRenderTargetView(Engine.RenderTarget, BackBufferColor);
            Engine.ImmediateContext.ClearDepthStencilView(Engine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);

            //Draw everything
            for (int i = 0; i < _visibleDrawable.Count; i++)
            {
                DrawableComponentHolder drawComponent = _visibleDrawable[i];
                if (ComponentsPerfMonitor.Updatable)
                {
                    ComponentsPerfMonitor.StartMesure(drawComponent.DrawableComponent, "Draw", drawComponent.DrawOrder.DrawID);
                    drawComponent.DrawableComponent.Draw(Engine.ImmediateContext, drawComponent.DrawOrder.DrawID);
                    ComponentsPerfMonitor.StopMesure(drawComponent.DrawableComponent, "Draw", drawComponent.DrawOrder.DrawID);
                }
                else
                {
                    drawComponent.DrawableComponent.Draw(Engine.ImmediateContext, drawComponent.DrawOrder.DrawID);
                }
            }
            Present();
        }

        public void Present()
        {
            if (ComponentsPerfMonitor.Updatable)
            {
                ComponentsPerfMonitor.StartMesure("GPU Rendering", "Draw");
                Engine.SwapChain.Present(_vSync, PresentFlags.None); // Send BackBuffer to screen
                ComponentsPerfMonitor.StopMesure("GPU Rendering", "Draw");
            }
            else
            {
                Engine.SwapChain.Present(_vSync, PresentFlags.None); // Send BackBuffer to screen
            }

            HLSLShaderWrap.ResetEffectStateTracker();
            VertexBuffer.ResetVertexStateTracker();

            if (_framelimiterTime > 0)
            {
                while (_framelimiter.ElapsedMilliseconds <= _framelimiterTime)
                {
                    Thread.Sleep(1);
                }
                _framelimiter.Restart();
            }
            //Frame limiter, will hold the frame for a minimum amount of time
        }

        #region Game Component Collection Methods

        private void GameComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            IDrawableComponent d = e.GameComponent as IDrawableComponent;
            if (d != null)
            {
                d.DrawOrderChanged += DrawableDrawOrderChanged;
                d.VisibleChanged += DrawableVisibleChanged;

                if (d.Visible)
                    AddDrawable(d);
            }

            IUpdatableComponent u = e.GameComponent as IUpdatableComponent;
            if (u != null)
            {
                u.UpdateOrderChanged += UpdatableUpdateOrderChanged;
                u.UpdatableChanged += UpdatableEnabledChanged;

                if (u.Updatable)
                    AddUpdatable(u);
            }
        }

        private void GameComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            IDrawableComponent d = e.GameComponent as IDrawableComponent;
            if (d != null)
            {
                d.DrawOrderChanged -= DrawableDrawOrderChanged;
                d.VisibleChanged -= DrawableVisibleChanged;

                if (d.Visible)
                {
                    _visibleDrawable.RemoveAll(x => x.DrawOrder.GetHashCode() == d.GetHashCode());
                }
            }

            IUpdatableComponent u = e.GameComponent as IUpdatableComponent;
            if (u != null)
            {
                u.UpdateOrderChanged -= UpdatableUpdateOrderChanged;
                u.UpdatableChanged -= UpdatableEnabledChanged;

                if (u.Updatable)
                    _enabledUpdatable.Remove(u);

                isCurrentlyUpdatingComponentsDirty = true; //Need to clear the currently Updating collection, that will need to be refreshed.
            }
        }

        private void GameComponentCleaning()
        {
            foreach (var component in _enabledUpdatable)
            {
                IUpdatableComponent u = component as IUpdatableComponent;
                if (u != null)
                {
                    u.UpdateOrderChanged -= UpdatableUpdateOrderChanged;
                    u.UpdatableChanged -= UpdatableEnabledChanged;
                }
            }

            foreach (var component in _visibleDrawable)
            {
                IDrawableComponent d = component as IDrawableComponent;
                if (d != null)
                {
                    d.DrawOrderChanged -= DrawableDrawOrderChanged;
                    d.VisibleChanged -= DrawableVisibleChanged;
                }
            }

            if (OnRenderLoopFrozen != null)
            {
                //Remove all Events associated (That haven't been unsubscribed !)
                foreach (Delegate d in OnRenderLoopFrozen.GetInvocationList())
                {
                    OnRenderLoopFrozen -= (RenderLoopFrozen)d;
                }
            }

        }

        #region Updatable Methods

        private void AddUpdatable(IUpdatableComponent u)
        {
            _enabledUpdatable.Add(u);
            _enabledUpdatable.Sort(UpdatableComparison);
        }

        private void UpdatableEnabledChanged(object sender, EventArgs e)
        {
            IUpdatableComponent u = (IUpdatableComponent)sender;
            if (u.Updatable)
                AddUpdatable(u);
            else
                _enabledUpdatable.Remove(u);
        }

        private void UpdatableUpdateOrderChanged(object sender, EventArgs e)
        {
            _enabledUpdatable.Sort(UpdatableComparison);
        }

        private static int UpdatableComparison(IUpdatableComponent x, IUpdatableComponent y)
        {
            return x.UpdateOrder.CompareTo(y.UpdateOrder);
        }

        #endregion Updatable Methods

        #region Drawable Methods

        private void AddDrawable(IDrawableComponent d)
        {
            //Add all Draw call linked to this Component
            foreach (var drawOrder in d.DrawOrders.DrawOrdersCollection)
            {
                _visibleDrawable.Add(new DrawableComponentHolder(d, drawOrder));
            }
            _visibleDrawable.Sort(DrawableComparison);
        }

        private void DrawableVisibleChanged(object sender, EventArgs e)
        {
            IDrawableComponent d = (IDrawableComponent)sender;
            if (d.Visible)
            {
                foreach (var drawOrder in d.DrawOrders.DrawOrdersCollection)
                {
                    _visibleDrawable.Add(new DrawableComponentHolder(d, drawOrder));
                }
                _visibleDrawable.Sort(DrawableComparison);
            }
            else
                _visibleDrawable.RemoveAll(x => x.DrawOrder.GetHashCode() == d.GetHashCode());
        }

        private void DrawableDrawOrderChanged(object sender, EventArgs e)
        {
            _visibleDrawable.Sort(DrawableComparison);
        }

        private static int DrawableComparison(DrawableComponentHolder x, DrawableComponentHolder y)
        {
            return x.DrawOrder.Order - y.DrawOrder.Order;
        }

        #endregion Drawable Methods

        #endregion Game Component Collection Methods

        #endregion

        #region IDisposable Members
        public override void BeforeDispose()
        {
            GameComponentCleaning();
        }

        public override void AfterDispose()
        {
#if DEBUG
            if (Configuration.EnableObjectTracking)
            {
                string info = SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects();
                if (logger.IsEnabled(NLog.LogLevel.Info))
                {
                    logger.Info("SharpDX Object Tracking result : {0}", string.IsNullOrEmpty(info) ? "Nothing, all directX COM components have been disposed" : info);
                }
                else
                {
                    Console.WriteLine("SharpDX Object Tracking result : {0}", string.IsNullOrEmpty(info) ? "Nothing, all directX COM components have been disposed" : info);
                }
            }
#endif
        } 
        #endregion
    }
}
