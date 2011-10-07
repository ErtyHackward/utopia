using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.KeyboardHelper;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using S33M3Engines.Cameras;
using Utopia.Editor;
using Utopia.GUI;
using Utopia.GUI.D3D;
using Utopia.GUI.D3D.Map;
using Utopia.Shared.Net.Connections;
using UtopiaContent.ModelComp;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Maths;
using S33M3Engines.D3D.Effects.Basics;
using Utopia.GameDXStates;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared;
using Utopia.Settings;
using Utopia.Shared.Config;
using S33M3Engines;
using Ninject;
using S33M3Engines.GameStates;
using S33M3Engines.WorldFocus;
using Utopia.Worlds.Cubes;
using Utopia.Worlds.Weather;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds;
using Utopia.Entities;
using Utopia.Shared.Chunks;
using Ninject.Parameters;
using S33M3Engines.Threading;
using Size = System.Drawing.Size;
using Utopia.Shared.World;
using Utopia.Shared.Interfaces;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Network;
using Utopia.Worlds.Storage;
using Utopia.Action;
using Utopia.InputManager;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Entities.Managers;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Renderer;
using Utopia.Entities.Managers.Interfaces;
using S33M3Engines.Timers;
using Utopia.Entities.Renderer.Interfaces;

namespace Utopia
{
    public partial class UtopiaRender : Game
    {
        private WorldFocusManager _worldFocusManager;
        private ActionsManager _actionManager;
        private Server _server;
        private D3DEngine _engine;
        private IKernel _iocContainer;

        public UtopiaRender(IKernel iocContainer)
        {
            _iocContainer = iocContainer;
            S33M3Engines.Threading.WorkQueue.ThreadingActif = true;    // Activate the threading Mode (Default : true, false used mainly to debug purpose)
            S33M3Engines.D3DEngine.FULLDEBUGMODE = false;
            VSync = true;                                              // Vsync ON (default)

            //Bind System objects
            SystemBinding(iocContainer);

            //Initialize the Thread Pool manager
            S33M3Engines.Threading.WorkQueue.Initialize(ClientSettings.Current.Settings.GraphicalParameters.AllocatedThreadsModifier);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        //Default Utopia Init method.
        public void Init(IKernel IoCContainer, 
                         string WindowsCaption, 
                         Size startupWindowsSize,
                         bool resetClientCache)
        {
            _server = IoCContainer.Get<Server>();
            _server.ServerConnection.ConnectionStatusChanged += ServerConnection_ConnectionStatusChanged;

            if (AbstractChunk.ChunkSize != _server.ChunkSize)
            {
                throw new Exception("Client chunkSize is different from server !");
            }
            //Change Visible WorldSize if client parameter > Server !
            if (ClientSettings.Current.Settings.GraphicalParameters.WorldSize > _server.MaxServerViewRange)
            {
                ClientSettings.Current.Settings.GraphicalParameters.WorldSize = _server.MaxServerViewRange;
            }

            //-- Get the Main D3dEngine --
            _d3dEngine = IoCContainer.Get<D3DEngine>(new ConstructorArgument("startingSize", startupWindowsSize),
                                                     new ConstructorArgument("windowCaption", WindowsCaption),
                                                     new ConstructorArgument("MaxNbrThreads", WorkQueue.ThreadPool.Concurrency));
            _d3dEngine.Initialize(); //Init the 3d Engine
            _d3dEngine.GameWindow.Closed += GameWindow_Closed;
            _d3dEngine.HideMouseCursor();   //Hide the mouse by default !
            DXStates.CreateStates(_d3dEngine);  //Create all States that could by used by the game.

            //-- Get Camera --
            ICamera camera = IoCContainer.Get<ICamera>();
            camera.CameraPlugin = IoCContainer.Get<PlayerEntityManager>();

            //-- Get World focus --
            _worldFocusManager = IoCContainer.Get<WorldFocusManager>();
            _worldFocusManager.WorldFocus = (IWorldFocus)camera; // Use the camera as a the world focus

            //Create the EntityMessageTranslator to active them. (Will subscribe to server events, ...)
            IoCContainer.Get<IChunkStorageManager>(new ConstructorArgument("forceNew", resetClientCache), new ConstructorArgument("UserName", _server.ServerConnection.Login));
            IoCContainer.Get<EntityMessageTranslator>();
            IoCContainer.Get<ItemMessageTranslator>();
            IoCContainer.Get<IChunkEntityImpactManager>();
            IoCContainer.Get<GameStatesManager>();

            _actionManager = IoCContainer.Get<ActionsManager>();
            _engine = IoCContainer.Get<D3DEngine>();

            GameComponents.Add(_server);
            GameComponents.Add(IoCContainer.Get<InputsManager>());
            GameComponents.Add(IoCContainer.Get<IconFactory>());
            GameComponents.Add(IoCContainer.Get<TimerManager>());
            GameComponents.Add(IoCContainer.Get<PlayerEntityManager>());
            GameComponents.Add(IoCContainer.Get<IDynamicEntityManager>());
            GameComponents.Add(IoCContainer.Get<CameraManager>());
            GameComponents.Add(IoCContainer.Get<Hud>());
            GameComponents.Add(IoCContainer.Get<GuiManager>());

            #region Debug Components
#if DEBUG
            DebugEffect.Init(_d3dEngine);             // Default Effect used by debug componant (will be shared)
#endif
            #endregion

        }

        //Windows state management

        void GameWindow_Closed(object sender, EventArgs e)
        {
            _isFormClosed = true; //Subscribe to Close event
        }

        //State management

        /// <summary>
        /// Check server connection change state !!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ServerConnection_ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            if (e.Status == ConnectionStatus.Disconnected && e.Exception != null)
            {
                GameExitReasonMessage msg = new GameExitReasonMessage()
                {
                    GameExitReason = ExitReason.Error,
                    MainMessage = "Server connection lost",
                    DetailedMessage = "Reason : " + e.Reason.ToString() + Environment.NewLine + "Detail : " + e.Exception.Message
                };
                Exit(msg);
            }
        }

        

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            DXStates.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            _actionManager.FetchInputs();
            _actionManager.Update();
            base.Update(ref TimeSpend);

            //After all update are done, Check against "System" keys like Exit, ...
            InputHandling();
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _actionManager.FetchInputs();
            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override void Draw()
        {
            _d3dEngine.Context.ClearRenderTargetView(_d3dEngine.RenderTarget, BackBufferColor);
            _d3dEngine.Context.ClearDepthStencilView(_d3dEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);
            base.Draw();
            base.Present();
        }


        private void InputHandling()
        {
            //Exit application
            if (_actionManager.isTriggered(Actions.Engine_Exit))
            {
                GameExitReasonMessage msg = new GameExitReasonMessage()
                {
                    GameExitReason = ExitReason.UserRequest,
                    MainMessage = "User Requested exit"
                };
            
                Exit(msg);
            }
            if (_actionManager.isTriggered(Actions.Engine_LockMouseCursor)) _engine.UnlockedMouse = !_engine.UnlockedMouse;
            if (_actionManager.isTriggered(Actions.Engine_FullScreen)) _engine.isFullScreen = !_engine.isFullScreen;
        }

        public override void Dispose()
        {
#if DEBUG
            DebugEffect.Dispose();
#endif
            _d3dEngine.GameWindow.Closed -= GameWindow_Closed; //Subscribe to Close event

            _server.ServerConnection.ConnectionStatusChanged -= ServerConnection_ConnectionStatusChanged;
            VisualCubeProfile.CleanUp();
            base.Dispose();
        }
    }
}
