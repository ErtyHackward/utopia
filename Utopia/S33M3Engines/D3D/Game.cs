using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using S33M3Engines.Buffers;
using S33M3Engines.D3D.Effects;
using S33M3Engines.InputHandler;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Windows;

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

        public GameComponentCollection GameComponents
        {
            get { return _gameComponents; }
        }

        //two lists are necessary for having two sortings : one is sorted by draworder and the other by updateorder 
        private readonly List<IDrawableComponent> _visibleDrawable;
        private readonly List<IUpdateableComponent> _enabledUpdateable;
        //this is for having the possibility to remove components at runtime, while iterating on the list
        //exemple : enabling the editor disables the normal Hud. without the _currentlyxxx collections you get invalid operation modify list while iterating 
        private readonly List<IUpdateableComponent> _currentlyUpdatingComponents = new List<IUpdateableComponent>();
        private readonly List<IDrawableComponent> _currentlyDrawingComponents = new List<IDrawableComponent>();

        public bool VSync
        {
            get { return _vSync == 1; }
            set { _vSync = value == true ? 1 : 0; }
        }

        public bool FixedTimeSteps
        {
            get { return S33M3Engines.D3DEngine.FIXED_TIMESTEP_ENABLED; }
            set
            {
                ResetGamePendingUpdate(value);
                S33M3Engines.D3DEngine.FIXED_TIMESTEP_ENABLED = value;
            }
        }

        protected InputHandlerManager _inputHandler { get; set; }

        #endregion

        #region Private Variable

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
        private int _vSync = 1;

        #endregion

        public Game()
        {
            _visibleDrawable = new List<IDrawableComponent>();
            _enabledUpdateable = new List<IUpdateableComponent>();

            _gameComponents = new GameComponentCollection();
            _gameComponents.ComponentAdded += new EventHandler<GameComponentCollectionEventArgs>(GameComponentAdded);
            _gameComponents.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(GameComponentRemoved);
        }

        #region Public Methods

        //Init + Start the pump !
        public void Run()
        {
            //Call the game Initialize !
            Initialize();

            //Call components Load Content
            LoadContent();

            ResetGamePendingUpdate(FixedTimeSteps);

            //The Pump !
            RenderLoop.Run(_d3dEngine.GameWindow, () =>
                                                      {
                                                          if (_isFormClosed) return;

                                                          if (FixedTimeSteps)
                                                          {
                                                              _updateWithoutrenderingCount = 0;
                                                              while (Stopwatch.GetTimestamp() > _next_game_update &&
                                                                     _updateWithoutrenderingCount < _maxRenderFrameSkip)
                                                              {
                                                                  _gameTime.Update(FixedTimeSteps);
                                                                  Update(ref _gameTime);

                                                                  _next_game_update += GameUpdateDelta;
                                                                  _updateWithoutrenderingCount++;
                                                              }

                                                              _interpolation_hd =
                                                                  (double)
                                                                  (Stopwatch.GetTimestamp() + GameUpdateDelta -
                                                                   _next_game_update)/GameUpdateDelta;
                                                              _interpolation_ld = (float) _interpolation_hd;
                                                              Interpolation(ref _interpolation_hd, ref _interpolation_ld);
                                                                  //Call before each Draw !
                                                          }
                                                          else
                                                          {
                                                              _gameTime.Update(FixedTimeSteps);
                                                              Update(ref _gameTime);
                                                          }

                                                          Draw();
                                                      });

            UnloadContent();
            //Dispose();
        }

        public void ResetGamePendingUpdate(bool fixedTimeSteps)
        {
            _gameTime.Update(fixedTimeSteps);
            _gameTime.Update(fixedTimeSteps);
            _next_game_update = Stopwatch.GetTimestamp();
        }

        //Close Window to stop the Window Pump !
        public void Exit()
        {
            Threading.WorkQueue.ThreadPool.Shutdown(true, 1000);
            while (Threading.WorkQueue.ThreadPool.InUseThreads > 0)
            {
                Thread.Sleep(100);
            }

            if (_d3dEngine.isFullScreen) _d3dEngine.isFullScreen = false;
            _d3dEngine.GameWindow.Close();
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
                GameComponents[i].LoadContent();
            }
        }

        public virtual void UnloadContent()
        {
            for (int i = 0; i < GameComponents.Count; i++)
            {
                GameComponents[i].UnloadContent();
            }
        }

        public virtual void Update(ref GameTime TimeSpend)
        {
            systemInputStates();
            
            foreach (var updateableComponent in _enabledUpdateable)
                _currentlyUpdatingComponents.Add(updateableComponent);

            foreach (var updateableComponent in _currentlyUpdatingComponents)
            {
                updateableComponent.Update(ref TimeSpend);
            }
            _currentlyUpdatingComponents.Clear();
            /*
            for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].Enabled) GameComponents[i].Update(ref TimeSpend);
            }*/
        }

        public virtual void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            systemInputStates();

            /*  for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].Enabled) GameComponents[i].Interpolation(ref interpolation_hd, ref interpolation_ld);
            }*/
           
            foreach (var updateableComponent in _enabledUpdateable)
                _currentlyUpdatingComponents.Add(updateableComponent);
           
            foreach (var updateableComponent in _enabledUpdateable)
            {
                updateableComponent.Interpolation(ref interpolationHd, ref interpolationLd);
            }
            _currentlyUpdatingComponents.Clear();
        }

        public virtual void Draw()
        {
            foreach (IDrawableComponent drawable in _visibleDrawable)
                _currentlyDrawingComponents.Add(drawable);

            foreach (IDrawableComponent drawable in _currentlyDrawingComponents)
            {
                drawable.Draw();
            }

             _currentlyDrawingComponents.Clear();
            _d3dEngine.RefreshBackBufferAsTexture();

            /*
            //Depth 0 drawing
            for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].Visible) GameComponents[i].Draw();
            }

            //TODO FIXME Take a snapshot of the backbuffer before beginning to draw seethrough polygons !
            _d3dEngine.RefreshBackBufferAsTexture();

            //Depth 1 drawing
            for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].CallDraw) GameComponents[i].Draw();
            }*/
        }

        /*   public void DrawInterfaces()
        {
            //Depth 2 drawing
            for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].CallDraw) GameComponents[i].Draw();
            }
        }*/

        public void Present()
        {
            _d3dEngine.SwapChain.Present(_vSync, PresentFlags.None); // Send BackBuffer to screen

            HLSLShaderWrap.ResetEffectStateTracker();
            VertexBuffer.ResetVertexStateTracker();
            //StatesMnger.ApplyStates(DefaultRenderStates.All);
        }

        //Keyboard and Mouse system watch up
        private void systemInputStates()
        {
            _inputHandler.ReshreshStates();
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

            IUpdateableComponent u = e.GameComponent as IUpdateableComponent;
            if (u != null)
            {
                u.UpdateOrderChanged += UpdatableUpdateOrderChanged;
                u.EnabledChanged += UpdatableEnabledChanged;

                if (u.Enabled)
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
                    _visibleDrawable.Remove(d);
            }

            IUpdateableComponent u = e.GameComponent as IUpdateableComponent;
            if (u != null)
            {
                u.UpdateOrderChanged -= UpdatableUpdateOrderChanged;
                u.EnabledChanged -= UpdatableEnabledChanged;

                if (u.Enabled)
                    _enabledUpdateable.Remove(u);
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
            _visibleDrawable.Add(d);
            _visibleDrawable.Sort(DrawableComparison);
        }

        private void DrawableVisibleChanged(object sender, EventArgs e)
        {
            IDrawableComponent d = (IDrawableComponent) sender;
            if (d.Visible)
                AddDrawable(d);
            else
                _visibleDrawable.Remove(d);
        }

        private void DrawableDrawOrderChanged(object sender, EventArgs e)
        {
            _visibleDrawable.Sort(DrawableComparison);
        }

        private static int DrawableComparison(IDrawableComponent x, IDrawableComponent y)
        {
            return x.DrawOrder - y.DrawOrder;
        }

        #endregion Drawable Methods

        #endregion Game Component Collection Methods

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            _inputHandler.CleanUp();
        }

        #endregion
    }
}