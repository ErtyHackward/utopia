using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Size = System.Drawing.Size;
using SharpDX;
using S33M3Engines.Cameras;
using S33M3Engines.StatesManager;
using System.Threading;
using S33M3Engines.D3D.Effects;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using System.Diagnostics;
using S33M3Engines.InputHandler;
using System.Windows.Forms;
using S33M3Engines.Struct;
using S33M3Engines.D3D.DebugTools;
using Utopia.Shared.Landscaping;

namespace S33M3Engines.D3D
{
    public class Game : IDisposable
    {
        #region Public Properties
        //protected D3DEngine _d3dEngine;
        public bool DebugActif { get { return _debugActif; } set { _debugActif = value; } }
        public int DebugDisplay { get { return _debugDisplay; } set { _debugDisplay = value; } }
        public Color4 BackBufferColor { get { return _backBufferColor; } set { _backBufferColor = value; } }
        public List<IGameComponent> GameComponents { get { return _gameComponents; } }
        public bool VSync { get { return _vSync == 1; } set { _vSync = value == true ? 1 : 0; } }
        public bool FixedTimeSteps { get { return S33M3Engines.D3DEngine.FIXED_TIMESTEP_ENABLED; } set { ResetGamePendingUpdate(value); S33M3Engines.D3DEngine.FIXED_TIMESTEP_ENABLED = value; } }

        public LandscapeBuilder LandscapeBuilder { get; set; }
        protected InputHandlerManager InputHandler { get; set; }

        #endregion

        #region Private Variable
        protected D3DEngine _d3dEngine;
        public static int TargetedGameUpdatePerSecond = 40;                                         //Number of targeted update per seconds
        public static long GameUpdateDelta = Stopwatch.Frequency / TargetedGameUpdatePerSecond;   //Compute the number of Ticks/s per Update
        private long _giveUp = GameUpdateDelta * 5;                                                //Compute the number of Ticks/s per Update
        private int _maxRenderFrameSkip = 5;                                                        //Maximum frame rendering skipping

        int _updateWithoutrenderingCount;
        double _interpolation_hd;
        float _interpolation_ld;
        long _next_game_update = Stopwatch.GetTimestamp();

        protected bool _isFormClosed = false;
        int _debugDisplay = 0;
        bool _debugActif = false;
        List<IGameComponent> _gameComponents = new List<IGameComponent>();
        Color4 _backBufferColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        GameTime _gameTime = new GameTime();
        int _vSync = 1;

        #endregion

        public Game()
        {
            
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
                    while (Stopwatch.GetTimestamp() > _next_game_update && _updateWithoutrenderingCount < _maxRenderFrameSkip)
                    {
                        _gameTime.Update(FixedTimeSteps);
                        Update(ref _gameTime);

                        _next_game_update += GameUpdateDelta;
                        _updateWithoutrenderingCount++;
                    }

                    _interpolation_hd = (double)(Stopwatch.GetTimestamp() + GameUpdateDelta - _next_game_update) / GameUpdateDelta;
                    _interpolation_ld = (float)_interpolation_hd;
                    Interpolation(ref _interpolation_hd, ref _interpolation_ld); //Call before each Draw !
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

            for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].CallUpdate) GameComponents[i].Update(ref TimeSpend);
            }

        }

        public virtual void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            systemInputStates();

            for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].CallUpdate) GameComponents[i].Interpolation(ref interpolation_hd, ref interpolation_ld);
            }
        }

        public virtual void Draw()
        {
            //Depth 0 drawing
            for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].CallDraw) GameComponents[i].DrawDepth0();
            }

            //Take a snapshot of the backbuffer before beginning to draw seethrough polygons !
            _d3dEngine.RefreshBackBufferAsTexture();

            //Depth 1 drawing
            for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].CallDraw) GameComponents[i].DrawDepth1();
            }
        }

        public void DrawInterfaces()
        {
            //Depth 2 drawing
            for (int i = 0; i < GameComponents.Count; i++)
            {
                if (GameComponents[i].CallDraw) GameComponents[i].DrawDepth2();
            }
        }

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
            InputHandler.ReshreshStates();
        }


        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            InputHandler.CleanUp();
        }

        #endregion
    }
}
