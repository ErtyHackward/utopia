using Utopia;
using Ninject;
using Utopia.Settings;
using Utopia.Action;
using Utopia.Shared.Config;
using S33M3Engines;
using System.Windows.Forms;
using Utopia.Shared.Settings;

namespace Sandbox.Client
{
    public partial class GameClient
    {
        public D3DEngine Engine { get; private set; }


        public UtopiaRender CreateNewGameEngine(IKernel iocContainer)
        {
            //Prapare the world parameter variable from server sources ==================================
            //var worldParam = new WorldParameters
            //                                 {
            //    IsInfinite = true,
            //    Seed = iocContainer.Get<Server>().GameInformations.WorldSeed,
            //    SeaLevel = iocContainer.Get<Server>().GameInformations.WaterLevel,
            //    WorldChunkSize = new Location2<int>(ClientSettings.Current.Settings.GraphicalParameters.WorldSize,   //Define the visible Client chunk size
            //                                    ClientSettings.Current.Settings.GraphicalParameters.WorldSize)
            //};
            //===========================================================================================
            //Doing components bindings
            
            //LateBinding(iocContainer, worldParam);            // Bind various Components against concrete class.

            //=======================================================================================================================
            //Create the various Concrete classe Binded, forwarding appropriate value. ==============================================
            //=======================================================================================================================
            //Init Block Profiles
            GameSystemSettings.Current = new XmlSettingsManager<GameSystemSetting>(@"GameSystemSettings.xml", SettingsStorage.CustomPath) { CustomSettingsFolderPath = @"Config\" };
            GameSystemSettings.Current.Load();

            //var plan = new WorldPlan
            //               {
            //                   RenderMapTemplate = Resources.mapbg,
            //                   RenderContinentTemplate = Resources.brush,
            //                   RenderWavePatterns = new[] {Resources.wavePattern, Resources.wavePattern1, Resources.wavePattern2},
            //                   RenderForest = Resources.forest,
            //                   RenderTropicalForest = Resources.tropicForest
            //               };

            Engine = iocContainer.Get<D3DEngine>();

            Engine.MouseCapture = false;

            //var states = new UtopiaRenderStates
            //{
            //    server = iocContainer.Get<Server>(),
            //    worldFocusManager = iocContainer.Get<WorldFocusManager>(),
            //    worldParameters = iocContainer.Get<WorldParameters>(),
            //    visualWorldParameters = iocContainer.Get<VisualWorldParameters>(),
            //    gameStatesManager = iocContainer.Get<GameStatesManager>(),
            //    firstPersonCamera = iocContainer.Get<ICamera>(),
            //    cameraManager = iocContainer.Get<CameraManager>(),
            //    timerManager = iocContainer.Get<TimerManager>(),
            //    inputsManager = iocContainer.Get<InputsManager>(),
            //    actionsManager = iocContainer.Get<ActionsManager>(),
            //    guiManager = iocContainer.Get<GuiManager>(),
            //    screen = iocContainer.Get<Nuclex.UserInterface.Screen>(),
            //    iconFactory = iocContainer.Get<IconFactory>(),
            //    fps = iocContainer.Get<FPS>(),
            //    gameClock = iocContainer.Get<IClock>(),
            //    inventoryComponent = iocContainer.Get<InventoryComponent>(),
            //    chatComponent = iocContainer.Get<ChatComponent>(),
            //    mapComponent = iocContainer.Get<MapComponent>(new ConstructorArgument("plan", plan)),
            //    hud = iocContainer.Get<Hud>(),
            //    entityEditor = iocContainer.Get<EntityEditor>(),
            //    carvingEditor = iocContainer.Get<CarvingEditor>(),
            //    stars = iocContainer.Get<IDrawableComponent>("Stars"),
            //    skydome = iocContainer.Get<ISkyDome>(),
            //    weather = iocContainer.Get<IWeather>(),
            //    clouds = iocContainer.Get<IDrawableComponent>("Clouds"),
            //    chunkStorageManager = iocContainer.Get<IChunkStorageManager>(new ConstructorArgument("forceNew", false),
            //                                                                  new ConstructorArgument("UserName", _server.ServerConnection.Login)),
            //    solidCubeMeshFactory = iocContainer.Get<ICubeMeshFactory>("SolidCubeMeshFactory"),
            //    liquidCubeMeshFactory = iocContainer.Get<ICubeMeshFactory>("LiquidCubeMeshFactory"),
            //    singleArrayChunkContainer = iocContainer.Get<SingleArrayChunkContainer>(),
            //    landscapeManager = iocContainer.Get<ILandscapeManager>(),
            //    lightingManager = iocContainer.Get<ILightingManager>(),
            //    chunkMeshManager = iocContainer.Get<IChunkMeshManager>(),
            //    worldChunks = iocContainer.Get<IWorldChunks>(),
            //    chunksWrapper = iocContainer.Get<IChunksWrapper>(),
            //    worldGenerator = iocContainer.Get<WorldGenerator>(),
            //    worldProcessorConfig = iocContainer.Get<IWorldProcessorConfig>(),
            //    pickingRenderer = iocContainer.Get<IPickingRenderer>(),
            //    chunkEntityImpactManager = iocContainer.Get<IChunkEntityImpactManager>(),
            //    entityPickingManager = iocContainer.Get<IEntityPickingManager>(),
            //    dynamicEntityManager = iocContainer.Get<IDynamicEntityManager>(),
            //    playerEntityManager = iocContainer.Get<PlayerEntityManager>(),
            //    playerCharacter = iocContainer.Get<PlayerCharacter>(),
            //    playerEntityRenderer = iocContainer.Get<IEntitiesRenderer>("PlayerEntityRenderer"),
            //    defaultEntityRenderer = iocContainer.Get<IEntitiesRenderer>("DefaultEntityRenderer"),
            //    voxelMeshFactory = iocContainer.Get<VoxelMeshFactory>(),
            //    sharedFrameCB = iocContainer.Get<SharedFrameCB>(),
            //    itemMessageTranslator = iocContainer.Get<ItemMessageTranslator>(),
            //    entityMessageTranslator = iocContainer.Get<EntityMessageTranslator>()
            //};

            //states.playerEntityManager.Enabled = false;
            //states.worldChunks.LoadComplete += worldChunks_LoadComplete;
            
            var utopiaRenderer = new UtopiaRender(Engine, iocContainer.Get<ActionsManager>());
            //// needed to display an nuclex forms
            //utopiaRenderer.GameComponents.Add(iocContainer.Get<GuiManager>());
            //// handles login dialog
            //utopiaRenderer.GameComponents.Add(iocContainer.Get<LoginComponent>());

            //utopiaRenderer.GameComponents.Add(iocContainer.Get<BepuPhysicsComponent>());
            //utopiaRenderer.GameComponents.Add(iocContainer.Get<LoadingComponent>());
            //utopiaRenderer.GameComponents.Add(iocContainer.Get<LoginComponent>());
            
            BindActions(iocContainer.Get<ActionsManager>());    //Bind the various actions

            ////Create a debug displayer component =====
            //DebugInfo debugInfo = new DebugInfo(iocContainer.Get<D3DEngine>());
            //debugInfo.Activated = true;
            //debugInfo.SetComponants(
            //    iocContainer.Get<FPS>(), 
            //    iocContainer.Get<IClock>(), 
            //    iocContainer.Get<IWorldChunks>(), 
            //    iocContainer.Get<PlayerEntityManager>(), 
            //    _server,
            //    iocContainer.Get<GuiManager>()
            //    );
            //utopiaRenderer.GameComponents.Add(debugInfo);

            return utopiaRenderer;
        }

        private void BindActions(ActionsManager actionManager)
        {
            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Forward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Forward
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Move_Forward,
                TriggerType = MouseTriggerMode.ButtonDown,
                Binding = MouseButton.LeftAndRightButton
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Backward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Backward
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_StrafeLeft,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeLeft
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_StrafeRight,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeRight
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Down,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Down
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Up,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Up
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Jump,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Jump,
                WithTimeElapsed = true,
                MaxTimeElapsedInS = 1
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Mode,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Mode
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Run,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Run
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_FullScreen,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.FullScreen
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_LockMouseCursor,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.LockMouseCursor
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_Left,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_Right,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_LeftWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = true
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_RightWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton,
                WithCursorLocked = true
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_LeftWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = false
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_RightWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton,
                WithCursorLocked = false
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.ToolBar_SelectNext,
                TriggerType = MouseTriggerMode.ScrollWheelForward,
                Binding = MouseButton.ScrollWheel
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.ToolBar_SelectPrevious,
                TriggerType = MouseTriggerMode.ScrollWheelBackWard,
                Binding = MouseButton.ScrollWheel
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.World_FreezeTime,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.FreezeTime
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_VSync,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.VSync
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_ShowDebugUI,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.F12 }
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_Exit,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.Escape }
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_TogglePerfMonitor,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.F10 }
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_ToggleDebugInfo,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.F9 }
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.DebugUI_Insert,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.Insert }
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.Toggle_Chat,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Chat
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.EntityUse,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Use
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.EntityThrow,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Throw
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.OpenInventory,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Inventory
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.OpenMap,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Map
            });
        }
    }
}
