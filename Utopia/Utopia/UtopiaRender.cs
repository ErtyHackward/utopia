using System;
using S33M3Engines.D3D;
using SharpDX.Direct3D11;
using Utopia.Shared.Net.Connections;
using Utopia.GameDXStates;
using Utopia.Settings;
using S33M3Engines.WorldFocus;
using Utopia.Worlds.Cubes;
using Utopia.Shared.Chunks;
using Utopia.Action;
using S33M3Engines.D3D.Effects.Basics;
using Utopia.Shared.Settings;
using Utopia.Shared.Config;

namespace Utopia
{
    public partial class UtopiaRender : Game
    {
        #region Private Variables
        private UtopiaRenderStates _renderStates;
        #endregion

        public UtopiaRender(UtopiaRenderStates renderStates)
        {
            _renderStates = renderStates;
            VSync = true;                                              // Vsync ON (default)
        }

        public override void Initialize()
        {
            //Initialize the Thread Pool manager
            S33M3Engines.Threading.WorkQueue.Initialize(ClientSettings.Current.Settings.GraphicalParameters.AllocatedThreadsModifier);

            Init();

            base.Initialize();
        }

        //Default Utopia Init method.
        private void Init()
        {
            _renderStates.server.ServerConnection.ConnectionStatusChanged += ServerConnection_ConnectionStatusChanged;

            if (AbstractChunk.ChunkSize != _renderStates.server.GameInformations.ChunkSize)
            {
                throw new Exception("Client chunkSize is different from server !");
            }
            //Change Visible WorldSize if client parameter > Server !
            if (ClientSettings.Current.Settings.GraphicalParameters.WorldSize > _renderStates.server.GameInformations.MaxViewRange)
            {
                ClientSettings.Current.Settings.GraphicalParameters.WorldSize = _renderStates.server.GameInformations.MaxViewRange;
            }

            _d3dEngine = _renderStates.engine;
            _d3dEngine.GameWindow.Closed += GameWindow_Closed;
            _d3dEngine.HideMouseCursor();   //Hide the mouse by default !
            DXStates.CreateStates(_d3dEngine);  //Create all States that could by used by the game.

            //-- Get Camera --
            _renderStates.firstPersonCamera.CameraPlugin = _renderStates.playerEntityManager;

            //-- Get World focus --
            _renderStates.worldFocusManager.WorldFocus = (IWorldFocus)_renderStates.firstPersonCamera; // Use the camera as a the world focus

            //Do the ChunkEntityImpactManager late initialization
            _renderStates.chunkEntityImpactManager.LateInitialization(_renderStates.server,
                                                                      _renderStates.singleArrayChunkContainer,
                                                                      _renderStates.worldChunks,
                                                                      _renderStates.chunkStorageManager,
                                                                      _renderStates.lightingManager);

            //Add Components to the main game Loop !
            GameComponents.Add(_renderStates.server);
            GameComponents.Add(_renderStates.inputsManager);
            GameComponents.Add(_renderStates.iconFactory);
            GameComponents.Add(_renderStates.timerManager);
            GameComponents.Add(_renderStates.playerEntityManager);
            GameComponents.Add(_renderStates.dynamicEntityManager);
            GameComponents.Add(_renderStates.cameraManager);
            GameComponents.Add(_renderStates.hud);
            GameComponents.Add(_renderStates.guiManager);
            GameComponents.Add(_renderStates.pickingRenderer);
            GameComponents.Add(_renderStates.inventoryComponent);
            GameComponents.Add(_renderStates.chatComponent);
            GameComponents.Add(_renderStates.mapComponent);
            GameComponents.Add(new DebugComponent(this, _d3dEngine, _renderStates.screen, _renderStates.gameStatesManager, _renderStates.actionsManager, _renderStates.playerEntityManager));
            GameComponents.Add(_renderStates.fps);
            GameComponents.Add(_renderStates.entityEditor);
            GameComponents.Add(_renderStates.skydome);
            GameComponents.Add(_renderStates.gameClock);
            GameComponents.Add(_renderStates.weather);
            GameComponents.Add(_renderStates.worldChunks);
            GameComponents.Add(_renderStates.sharedFrameCB);
            GameComponents.Add(_renderStates.bepuPhysicsComponent);

            if (_renderStates.customComponents != null)
            {
                foreach (var customComponent in _renderStates.customComponents)
                {
                    GameComponents.Add(customComponent);
                }
            }

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
            _renderStates.actionsManager.FetchInputs();
            _renderStates.actionsManager.Update();
            base.Update(ref TimeSpend);

            //After all update are done, Check against "System" keys like Exit, ...
            InputHandling();
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _renderStates.actionsManager.FetchInputs();
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
            if (_renderStates.actionsManager.isTriggered(Actions.Engine_Exit))
            {
                GameExitReasonMessage msg = new GameExitReasonMessage()
                {
                    GameExitReason = ExitReason.UserRequest,
                    MainMessage = "User Requested exit"
                };
            
                Exit(msg);
            }
            if (_renderStates.actionsManager.isTriggered(Actions.Engine_LockMouseCursor)) _renderStates.engine.MouseCapture = !_renderStates.engine.MouseCapture;
            if (_renderStates.actionsManager.isTriggered(Actions.Engine_FullScreen)) _renderStates.engine.isFullScreen = !_renderStates.engine.isFullScreen;
        }

        public override void Dispose()
        {
#if DEBUG
            DebugEffect.Dispose();
#endif
            _d3dEngine.GameWindow.Closed -= GameWindow_Closed; //Subscribe to Close event

            _renderStates.server.ServerConnection.ConnectionStatusChanged -= ServerConnection_ConnectionStatusChanged;
            GameSystemSettings.Current.Settings.CleanUp();
            base.Dispose();
        }
    }
}
