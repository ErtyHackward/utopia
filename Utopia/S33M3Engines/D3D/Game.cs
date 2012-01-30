using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using S33M3Engines.Buffers;
using S33M3Engines.D3D.Effects;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Windows;
using S33M3Engines.Threading;
using S33M3Engines.D3D.DebugTools;

namespace S33M3Engines.D3D
{
    public class Game : IDisposable
    {
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

        /// <summary>
        /// Gets collection of game components
        /// </summary>
        public GameComponentCollection GameComponents
        {
            get { return _gameComponents; }
        }

        //two lists are necessary for having two sortings : one is sorted by draworder and the other by updateorder 
        private readonly List<DrawableComponentHolder> _visibleDrawable;
        private readonly List<IUpdateableComponent> _enabledUpdateable;
        //this is for having the possibility to remove components at runtime, while iterating on the list
        //exemple : enabling the editor disables the normal Hud. without the _currentlyxxx collections you get invalid operation modify list while iterating 
        private readonly List<IUpdateableComponent> _currentlyUpdatingComponents = new List<IUpdateableComponent>();
        //private readonly List<IDrawableComponent> _currentlyDrawingComponents = new List<IDrawableComponent>();

        public bool VSync
        {
            get { return _vSync == 1; }
            set { _vSync = value == true ? 1 : 0; }
        }

        /// <summary>
        /// Indicates if game was started
        /// </summary>
        public bool IsStarted
        {
            get { return _gameStarted; }
        }

        public GameExitReasonMessage GameExitReason;

        public PerfMonitor ComponentsPerfMonitor { get; set; }
        #endregion

        #region Private Variable

        private delegate void WinformInvockCallBack();
        protected D3DEngine _d3dEngine;
        public static int TargetedGameUpdatePerSecond = 40; //Number of targeted update per seconds

        public static long GameUpdateDelta = Stopwatch.Frequency/TargetedGameUpdatePerSecond;
                           //Compute the number of Ticks/s per Update

        private long _giveUp = GameUpdateDelta*5; //Compute the number of Ticks/s per Update
        private int _maxRenderFrameSkip = 5; //Maximum frame rendering skipping

        private int _updateWithoutrenderingCount;
        private double _interpolation_hd;
        private float _interpolation_ld;
        private long _next_game_update = Stopwatch.GetTimestamp();

        protected bool _isFormClosed = false;
        private int _debugDisplay = 0;
        private bool _debugActif = false;
        private readonly GameComponentCollection _gameComponents;
        private Color4 _backBufferColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        private GameTime _gameTime = new GameTime();
        private int _vSync = 0;
        private bool _gameStarted;
        internal static object DxLock = new object();
        #endregion

        public Game(D3DEngine engine)
        {
            _d3dEngine = engine;
            _visibleDrawable = new List<DrawableComponentHolder>();
            _enabledUpdateable = new List<IUpdateableComponent>();

            _gameComponents = new GameComponentCollection();
            _gameComponents.ComponentAdded += GameComponentAdded;
            _gameComponents.ComponentRemoved += GameComponentRemoved;

            ComponentsPerfMonitor = new PerfMonitor();
        }

        #region Public Methods

        //Init + Start the pump !
        public void Run()
        {

            ////Call the game Initialize !
            //WorkQueue.DoWorkSingleThreadedGroup(new Amib.Threading.Action(Initialize));

            ////Call components Load Content
            //WorkQueue.DoWorkSingleThreadedGroup(new Amib.Threading.Action(LoadContent));

            _gameStarted = true;

            //Call the game Initialize !
            Initialize();

            //Call components Load Content
            LoadContent();

            _next_game_update = Stopwatch.GetTimestamp();

            var lastTimestamp = _next_game_update;
            //The Pump !
            RenderLoop.Run(_d3dEngine.GameWindow, () =>
            {
                if (_isFormClosed) return;

                _updateWithoutrenderingCount = 0;
                while (Stopwatch.GetTimestamp() > _next_game_update && _updateWithoutrenderingCount < _maxRenderFrameSkip && _isFormClosed == false)
                {
                    Update(ref _gameTime);

                    _next_game_update += GameUpdateDelta;
                    _updateWithoutrenderingCount++;
                }

                var timestamp = Stopwatch.GetTimestamp();

                var passed = timestamp - lastTimestamp;
                lastTimestamp = timestamp;
                _interpolation_hd = (double)(timestamp + GameUpdateDelta - _next_game_update) / GameUpdateDelta;
                _interpolation_ld = (float)_interpolation_hd;

                Interpolation(ref _interpolation_hd, ref _interpolation_ld, ref passed);

                Draw();
            });

            _d3dEngine.MouseCapture = true;

            UnloadContent();
        }

        //Close Window to stop the Window Pump !
        [DebuggerStepThrough()]
        public void Exit(GameExitReasonMessage msg)
        {
            try
            {
                GameExitReason = msg;
                Threading.WorkQueue.ThreadPool.Shutdown(false, 0);
                Thread.Sleep(100);
                Threading.WorkQueue.ThreadPool.Shutdown(true, 0);
            }
            catch (Exception)
            {
            }
            finally
            {
                if (_d3dEngine.isFullScreen) _d3dEngine.isFullScreen = false;
                CloseWinform();
            }

        }

        private void CloseWinform()
        {
            if (_d3dEngine.GameWindow.InvokeRequired)
            {
                var d = new WinformInvockCallBack(CloseWinform);
                _d3dEngine.GameWindow.Invoke(d);
            }
            else
            {
                //Create the Single Player NEW world data message
                _d3dEngine.GameWindow.Close();
            }
        }

        //Game Initialize
        public virtual void Initialize()
        {
            for (int i = 0; i < GameComponents.Count; i++)
            {
                WorkQueue.DoWorkInThreadedGroup(GameComponents[i].InternalInitialize);
                //GameComponents[i].Initialize();
            }

            //Wait for end of all jobs
            WorkQueue.ThreadPoolGrp.WaitForIdle();
        }

        public virtual void LoadContent()
        {
            for (int i = 0; i < GameComponents.Count; i++)
            {
                WorkQueue.DoWorkInThreadedGroup(GameComponents[i].InternalLoadContent);
                //GameComponents[i].LoadContent();
            }

            //Wait for end of all jobs
            WorkQueue.ThreadPoolGrp.WaitForIdle();
        }

        public virtual void UnloadContent()
        {
            for (int i = 0; i < GameComponents.Count; i++)
            {
                GameComponents[i].UnloadContent();
            }
        }

        /// <summary>
        /// Performs game logic update. Always called with fixed time step
        /// </summary>
        /// <param name="timeSpent">Gives an amount of seconds passed since last call [runtime constant]</param>
        public virtual void Update(ref GameTime timeSpent)
        {
            _currentlyUpdatingComponents.Clear();
            for (int i = 0; i < _enabledUpdateable.Count; i++) _currentlyUpdatingComponents.Add(_enabledUpdateable[i]);

            for (int i = 0; i < _currentlyUpdatingComponents.Count; i++)
            {
                if (ComponentsPerfMonitor.isActivated)
                {
                    ComponentsPerfMonitor.StartMesure(_currentlyUpdatingComponents[i], "Update");
                    _currentlyUpdatingComponents[i].Update(ref timeSpent);
                    ComponentsPerfMonitor.StopMesure(_currentlyUpdatingComponents[i], "Update");
                }
                else
                {
                    _currentlyUpdatingComponents[i].Update(ref timeSpent);
                }
            }
        }

        /// <summary>
        /// Performs gamecomponents interpolation. This method called right before each Draw call.
        /// Only visual interpolation should be done here.
        /// </summary>
        /// <param name="interpolationHd">Value in range [0;1] that indicates factor of interpolation between Update calls</param>
        /// <param name="interpolationLd">Value in range [0;1] that indicates factor of interpolation between Update calls</param>
        /// <param name="timePassed">Time passed since last method call [milliseconds]</param>
        public virtual void Interpolation(ref double interpolationHd, ref float interpolationLd, ref long timePassed)
        {
            for (int i = 0; i < _currentlyUpdatingComponents.Count; i++)
            {
                _currentlyUpdatingComponents[i].Interpolation(ref interpolationHd, ref interpolationLd, ref timePassed);
            }
        }

        public virtual void Draw()
        {
            for (int i = 0; i < _visibleDrawable.Count; i++)
            {
                DrawableComponentHolder drawComponent = _visibleDrawable[i];
                lock (DxLock)
                {
                    if (ComponentsPerfMonitor.isActivated)
                    {
                        ComponentsPerfMonitor.StartMesure(drawComponent.DrawableComponent, "Draw");
                        drawComponent.DrawableComponent.Draw(drawComponent.DrawOrder.DrawID);
                        ComponentsPerfMonitor.StopMesure(drawComponent.DrawableComponent, "Draw");
                    }
                    else
                    {
                        drawComponent.DrawableComponent.Draw(drawComponent.DrawOrder.DrawID);
                    }
                }
            }
        }

        public void Present()
        {
            if (ComponentsPerfMonitor.isActivated)
            {
                ComponentsPerfMonitor.StartMesure("GC Present", "Draw");
                _d3dEngine.SwapChain.Present(_vSync, PresentFlags.None); // Send BackBuffer to screen
                ComponentsPerfMonitor.StopMesure("GC Present", "Draw");
            }
            else
            {
                _d3dEngine.SwapChain.Present(_vSync, PresentFlags.None); // Send BackBuffer to screen
            }

            HLSLShaderWrap.ResetEffectStateTracker();
            VertexBuffer.ResetVertexStateTracker();
        }

        #region Game Component Collection Methods

        private void GameComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            if (_gameStarted)
            {
                // we need to initialize it properly before add

                var component = e.GameComponent;

                // catch load complete events to continue the load
                component.Initialized += GameComponentInitialized;
                component.ContentLoaded += GameComponentContentLoaded;

                WorkQueue.ThreadPool.QueueWorkItem(component.InternalInitialize);

                return;
            }

            // we not yet started, so simple add it
            AddGameComponentToProcessing(e.GameComponent);
        }

        private void AddGameComponentToProcessing(GameComponent gc)
        {
            var d = gc as IDrawableComponent;
            if (d != null)
            {
                d.DrawOrderChanged += DrawableDrawOrderChanged;
                d.VisibleChanged += DrawableVisibleChanged;

                if (d.Visible)
                    AddDrawable(d);
            }

            var u = gc as IUpdateableComponent;
            if (u != null)
            {
                u.UpdateOrderChanged += UpdatableUpdateOrderChanged;
                u.EnabledChanged += UpdatableEnabledChanged;

                if (u.Enabled)
                    AddUpdatable(u);
            }
        }

        void GameComponentInitialized(object sender, EventArgs e)
        {
            // continue to initialize
            var component = (GameComponent)sender;

            component.Initialized -= GameComponentInitialized;

            WorkQueue.ThreadPoolGrp.QueueWorkItem(component.InternalLoadContent);
        }

        void GameComponentContentLoaded(object sender, EventArgs e)
        {
            // initilization complete
            var component = (GameComponent)sender;

            component.ContentLoaded -= GameComponentContentLoaded;

            AddGameComponentToProcessing(component);
        }
        
        private void GameComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            var d = e.GameComponent as IDrawableComponent;
            if (d != null)
            {
                d.DrawOrderChanged -= DrawableDrawOrderChanged;
                d.VisibleChanged -= DrawableVisibleChanged;

                if (d.Visible)
                {
                    _visibleDrawable.RemoveAll(x => x.DrawOrder.GetHashCode() == d.GetHashCode());
                }
            }

            var u = e.GameComponent as IUpdateableComponent;
            if (u != null)
            {
                u.UpdateOrderChanged -= UpdatableUpdateOrderChanged;
                u.EnabledChanged -= UpdatableEnabledChanged;

                if (u.Enabled)
                    _enabledUpdateable.Remove(u);
            }
        }

        private void GameComponentCleaning()
        {
        //            private readonly List<DrawableComponentHolder> _visibleDrawable;
        //private readonly List<IUpdateableComponent> _enabledUpdateable;
            foreach (var component in _enabledUpdateable)
            {
                var u = component as IUpdateableComponent;
                if (u != null)
                {
                    u.UpdateOrderChanged -= UpdatableUpdateOrderChanged;
                    u.EnabledChanged -= UpdatableEnabledChanged;
                }
            }

            foreach (var component in _visibleDrawable)
            {
                var d = component as IDrawableComponent;
                if (d != null)
                {
                    d.DrawOrderChanged -= DrawableDrawOrderChanged;
                    d.VisibleChanged -= DrawableVisibleChanged;
                }
            }

        }

        #region Updatable Methods

        private void AddUpdatable(IUpdateableComponent u)
        {
            _enabledUpdateable.Add(u);
            _enabledUpdateable.Sort(UpdatableComparison);
        }

        private void UpdatableEnabledChanged(object sender, EventArgs e)
        {
            IUpdateableComponent u = (IUpdateableComponent) sender;
            if (u.Enabled)
                AddUpdatable(u);
            else
                _enabledUpdateable.Remove(u);
        }

        private void UpdatableUpdateOrderChanged(object sender, EventArgs e)
        {
            _enabledUpdateable.Sort(UpdatableComparison);
        }

        private static int UpdatableComparison(IUpdateableComponent x, IUpdateableComponent y)
        {
            return x.UpdateOrder.CompareTo(y.UpdateOrder);
        }

        #endregion Updatable Methods

        #region Drawable Methods

        private void AddDrawable(IDrawableComponent d)
        {
            //Add all Draw call linked to this Component
            foreach (var drawOrder in d.DrawOrders.GetAllDrawOrder())
            {
                _visibleDrawable.Add(new DrawableComponentHolder(d, drawOrder));
            }
            _visibleDrawable.Sort(DrawableComparison);
        }

        private void DrawableVisibleChanged(object sender, EventArgs e)
        {
            var d = (IDrawableComponent) sender;
            if (d.Visible)
            {
                foreach (var drawOrder in d.DrawOrders.GetAllDrawOrder())
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

        public virtual void Dispose()
        {
            GameComponentCleaning();
        }

        #endregion
    }
}