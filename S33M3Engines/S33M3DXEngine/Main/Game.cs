﻿using System;
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
    public class Game : Component
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

        #endregion

        #region Private Variable

        private delegate void WinformInvockCallBack();
        protected D3DEngine Engine;

        //public static int TargetedGameUpdatePerSecond = 40; //Number of targeted update per seconds
        //public static long GameUpdateDelta = Stopwatch.Frequency / TargetedGameUpdatePerSecond;
        //Compute the number of Ticks/s per Update

        private int _maxRenderFrameSkip = 5; //Maximum frame rendering skipping

        private int _updateWithoutrenderingCount;
        private double _interpolation_hd;
        private float _interpolation_ld;
        private long _nextGameUpdateTime = Stopwatch.GetTimestamp();

        protected bool _isFormClosed = false;
        private int _debugDisplay = 0;
        private bool _debugActif = false;
        private readonly GameComponentCollection _gameComponents;
        private Color4 _backBufferColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        private GameTime _gameTime = new GameTime();
        private int _vSync = 1;
        #endregion

        //Constructed Engine
        public Game(Size startingWindowsSize, string WindowsCaption, SampleDescription sampleDescription, Size ResolutionSize = default(Size), bool withDebugObjectTracking = false)
        {
            Engine = ToDispose(new D3DEngine(startingWindowsSize, WindowsCaption, sampleDescription, ResolutionSize));

            Engine.GameWindow.FormClosing += GameWindow_FormClosing;

            _visibleDrawable = new List<DrawableComponentHolder>();
            _enabledUpdatable = new List<IUpdatableComponent>();
            _gameComponents = ToDispose(new GameComponentCollection());

            gameInitialize(withDebugObjectTracking);
        }

        //Injected Engine
        public Game(D3DEngine engine, bool withDebugObjectTracking = false)
        {
            Engine = engine;
            Engine.GameWindow.FormClosing += GameWindow_FormClosing;
            _visibleDrawable = new List<DrawableComponentHolder>();
            _enabledUpdatable = new List<IUpdatableComponent>();
            _gameComponents = ToDispose(new GameComponentCollection());

            gameInitialize(withDebugObjectTracking);
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
        }

        #region Public Methods

        //Init + Start the pump !
        public virtual void Run()
        {
            //Check if the Threading engine has been initialize or not
            SmartThread.CheckInit();

            //Call the game Initialize !
            Initialize();

            //Call components Load Content
            LoadContent();

            ResetTimers();

            FixedTimeStepLoop();
            //FixedTimeStepLoopWithPrediction();

            this.Exit(false);
        }

        private void FixedTimeStepLoop()
        {
            //The Pump !
            RenderLoop.Run(Engine.GameWindow, () =>
            {
                if (_isFormClosed) return;

#if DEBUG
                //In case, if too much time has passed, Skip the update for this time period
                //(Help in case of a break point placed in inline working)
                //Default is a "pause of more then 1 seconde".
                if (Stopwatch.GetTimestamp() - _nextGameUpdateTime > Stopwatch.Frequency)
                {
                    ResetTimers();
                }
#endif
                _updateWithoutrenderingCount = 0;
                while (Stopwatch.GetTimestamp() > _nextGameUpdateTime && _updateWithoutrenderingCount < _maxRenderFrameSkip && _isFormClosed == false)
                {
                    if (_updateWithoutrenderingCount == 1 && ComponentsPerfMonitor.Updatable)
                    {
                        logger.Debug("Frame skipped because too late for : {0}, Updt maximum Delta {1}", Stopwatch.GetTimestamp() - _nextGameUpdateTime, _gameTime.GameUpdateDelta);
                        logger.Debug("Last     Update time {0}, Draw time {1}, Present wait for {2}", ComponentsPerfMonitor.PerfTimer.GetLastUpdateTime, ComponentsPerfMonitor.PerfTimer.GetLastDrawTime, ComponentsPerfMonitor.PerfTimer.GetLastPresentTime);
                        logger.Debug("Previous Update time {0}, Draw time {1}, Present wait for {2}", ComponentsPerfMonitor.PerfTimer.GetPrevUpdateTime, ComponentsPerfMonitor.PerfTimer.GetPrevDrawTime, ComponentsPerfMonitor.PerfTimer.GetPrevPresentTime);

                        foreach (var result in ComponentsPerfMonitor.PerfTimer.GetComponentByDeltaPerf(5))
                        {
                            logger.Debug("Perf problem could caused by {0}, Last duration {1}, previous duration {2}", result.Name, result.LastValue, result.PrevValue);
                        }
                    }

                    Update(_gameTime);

                    _nextGameUpdateTime += _gameTime.GameUpdateDelta;
                    _updateWithoutrenderingCount++;
                }

                _interpolation_hd = (double)(Stopwatch.GetTimestamp() + _gameTime.GameUpdateDelta - _nextGameUpdateTime) / _gameTime.GameUpdateDelta;
                _interpolation_ld = (float)_interpolation_hd;
                Interpolation(_interpolation_hd, _interpolation_ld, _gameTime.GetElapsedTime());

                Draw();
            });
        }

        private void FixedTimeStepLoopWithPrediction()
        {
            //Wait for a VSync signal
            Present();

            long num = 0;
            long last_swap = Stopwatch.GetTimestamp();
            long start = Stopwatch.GetTimestamp();
            long next_swap_time = start;
            long swap_time;

            //The Pump !
            RenderLoop.Run(Engine.GameWindow, () =>
            {
                if (_isFormClosed) return;

                while (num * _gameTime.GameUpdateDelta < (next_swap_time - start) && _isFormClosed == false)
                {
                    Update(_gameTime);
                    num += 1;
                }

                _interpolation_hd = (next_swap_time - start) / _gameTime.GameUpdateDelta - (num - 1);
                _interpolation_ld = (float)_interpolation_hd;
                Interpolation(_interpolation_hd, _interpolation_ld, _gameTime.GetElapsedTime());
                Draw();

                swap_time = Stopwatch.GetTimestamp() - last_swap;
                if (swap_time < _gameTime.GameUpdateDelta / 2) swap_time = _gameTime.GameUpdateDelta;
                last_swap = Stopwatch.GetTimestamp();
                next_swap_time = Stopwatch.GetTimestamp() + swap_time;

            });

        }

        private void ResetTimers()
        {
            _nextGameUpdateTime = Stopwatch.GetTimestamp();
            _gameTime.ResetElapsedTimeCounter();
        }

        //Close Window to stop the Window Pump !
        //HACK [DebuggerStepThrough on EXIT] To avoid breaking inside while debugging => Remove it to give the possibility for the debugger to stop inside this function
        //[DebuggerStepThrough()]
        public void Exit(bool forced)
        {
            if (_isFormClosed) return;
            try
            {
                Threading.SmartThread.ThreadPool.Shutdown(forced, 0);
                logger.Info("Engine shutDown requested, active background threads are being forced to close : {0}", Threading.SmartThread.ThreadPool.ActiveThreads);
                while (Threading.SmartThread.ThreadPool.ActiveThreads > 0) { }
                logger.Info("Engine shutDown requested,  all background thread are closed");

            }
            catch (Exception)
            {
            }
            finally
            {
                if (Engine.isFullScreen) Engine.isFullScreen = false;
                CloseWinform();
            }
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
                //Create the Single Player NEW world data message
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

        public virtual void Update(GameTime TimeSpend)
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
                    _currentlyUpdatingComponents[i].Update(TimeSpend);
                    ComponentsPerfMonitor.StopMesure(_currentlyUpdatingComponents[i], "Update");
                }
                else
                {
                    _currentlyUpdatingComponents[i].Update(TimeSpend);
                }
            }
        }

        public virtual void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            for (int i = 0; i < _currentlyUpdatingComponents.Count; i++)
            {
                _currentlyUpdatingComponents[i].Interpolation(interpolationHd, interpolationLd, elapsedTime);
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
                logger.Info("SharpDX Object Tracking result : {0}", string.IsNullOrEmpty(info) ? "Nothing, all directX COM components have been disposed" : info);
            }
#endif
        } 
        #endregion
    }
}
