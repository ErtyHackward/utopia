using Utopia.Shared.World;
using Utopia;
using Ninject;
using Utopia.Network;
using Utopia.Settings;
using Utopia.Shared.Structs;
using Utopia.Worlds.Cubes;
using Utopia.Shared.Structs.Landscape;
using S33M3Engines.D3D;
using Utopia.Action;
using Utopia.Shared.Config;
using S33M3Engines.D3D.DebugTools;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Chunks;
using Utopia.Entities.Managers;
using S33M3Engines;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.GUI;
using Utopia.GUI.D3D.Map;
using Utopia.Editor;
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;
using S33M3Engines.Cameras;
using S33M3Engines.Timers;
using Utopia.InputManager;
using Utopia.GUI.D3D;
using Utopia.Entities;
using Utopia.Worlds.Weather;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.Storage;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Shared.Interfaces;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.Chunks.Entities;
using Utopia.Entities.Voxel;
using Ninject.Parameters;
using System.Windows.Forms;
using System.Drawing;

namespace LostIslandHD.Client
{
    public partial class GameClient
    {
        public UtopiaRender CreateNewGameEngine(IKernel iocContainer)
        {
            //Prapare the world parameter variable from server sources ==================================
            WorldParameters worldParam = new WorldParameters()
            {
                IsInfinite = true,
                Seed = iocContainer.Get<Server>().WorldSeed,
                SeaLevel = iocContainer.Get<Server>().SeaLevel,
                WorldChunkSize = new Location2<int>(ClientSettings.Current.Settings.GraphicalParameters.WorldSize,   //Define the visible Client chunk size
                                                ClientSettings.Current.Settings.GraphicalParameters.WorldSize)
            };
            //===========================================================================================
            //Doing components bindings
            UtopiaRender utopiaRenderer; // Need to create it there, the "system" component will be binded at creation time.
            LateBinding(iocContainer, worldParam);            // Bind various Components against concrete class.

            //=======================================================================================================================
            //Create the various Concrete classe Binded, forwarding appropriate value. ==============================================
            //=======================================================================================================================
            //Init Block Profiles
            VisualCubeProfile.InitCubeProfiles(iocContainer.Get<ICubeMeshFactory>("SolidCubeMeshFactory"),     //The default binded Solid Cube Mesh Factory
                                               iocContainer.Get<ICubeMeshFactory>("LiquidCubeMeshFactory"),    //The default binded Water Cube Mesh Factory
                                               @"Config\CubesProfile.xml");                                    //The path to the Cubes Profiles descriptions
            CubeProfile.InitCubeProfiles(@"Config\CubesProfile.xml");                                          // Init the cube profiles use by shared application (Similar than VisualCubeProfile, but without visual char.)


            utopiaRenderer = new UtopiaRender(new UtopiaRenderStates()
            {
                engine = iocContainer.Get<D3DEngine>(new ConstructorArgument("startingSize", new Size(1024, 600)),
                                                    new ConstructorArgument("windowCaption", "LostIsland Client")),
                server = iocContainer.Get<Server>(),
                worldFocusManager = iocContainer.Get<WorldFocusManager>(),
                worldParameters = iocContainer.Get<WorldParameters>(),
                visualWorldParameters = iocContainer.Get<VisualWorldParameters>(),
                gameStatesManager = iocContainer.Get<GameStatesManager>(),
                firstPersonCamera = iocContainer.Get<ICamera>(),
                cameraManager = iocContainer.Get<CameraManager>(),
                timerManager = iocContainer.Get<TimerManager>(),
                entityMessageTranslator = iocContainer.Get<EntityMessageTranslator>(),
                itemMessageTranslator = iocContainer.Get<ItemMessageTranslator>(),
                inputsManager = iocContainer.Get<InputsManager>(),
                actionsManager = iocContainer.Get<ActionsManager>(),
                guiManager = iocContainer.Get<GuiManager>(),
                screen = iocContainer.Get<Nuclex.UserInterface.Screen>(),
                iconFactory = iocContainer.Get<IconFactory>(),
                fps = iocContainer.Get<FPS>(),
                gameClock = iocContainer.Get<IClock>(),
                chatComponent = iocContainer.Get<ChatComponent>(),
                mapComponent = iocContainer.Get<MapComponent>(),
                hud = iocContainer.Get<Hud>(),
                entityEditor = iocContainer.Get<EntityEditor>(),
                stars = iocContainer.Get<IDrawableComponent>("Stars"),
                skydome = iocContainer.Get<ISkyDome>(),
                weather = iocContainer.Get<IWeather>(),
                clouds = iocContainer.Get<IDrawableComponent>("Clouds"),
                chunkStorageManager = iocContainer.Get<IChunkStorageManager>(new ConstructorArgument("forceNew", false),
                                                                              new ConstructorArgument("UserName", _server.ServerConnection.Login)),
                solidCubeMeshFactory = iocContainer.Get<ICubeMeshFactory>("SolidCubeMeshFactory"),
                liquidCubeMeshFactory = iocContainer.Get<ICubeMeshFactory>("LiquidCubeMeshFactory"),
                singleArrayChunkContainer = iocContainer.Get<SingleArrayChunkContainer>(),
                landscapeManager = iocContainer.Get<ILandscapeManager>(),
                lightingManager = iocContainer.Get<ILightingManager>(),
                chunkMeshManager = iocContainer.Get<IChunkMeshManager>(),
                worldChunks = iocContainer.Get<IWorldChunks>(),
                chunksWrapper = iocContainer.Get<IChunksWrapper>(),
                worldGenerator = iocContainer.Get<WorldGenerator>(),
                worldProcessorConfig = iocContainer.Get<IWorldProcessorConfig>(),
                pickingRenderer = iocContainer.Get<IPickingRenderer>(),
                chunkEntityImpactManager = iocContainer.Get<IChunkEntityImpactManager>(),
                entityPickingManager = iocContainer.Get<IEntityPickingManager>(),
                dynamicEntityManager = iocContainer.Get<IDynamicEntityManager>(),
                playerEntityManager = iocContainer.Get<PlayerEntityManager>(),
                playerCharacter = iocContainer.Get<PlayerCharacter>(),
                playerEntityRenderer = iocContainer.Get<IEntitiesRenderer>("PlayerEntityRenderer"),
                defaultEntityRenderer = iocContainer.Get<IEntitiesRenderer>("DefaultEntityRenderer"),
                voxelMeshFactory = iocContainer.Get<VoxelMeshFactory>()
            }
            );
            
            BindActions(iocContainer.Get<ActionsManager>());    //Bind the various actions

            //Create a debug displayer component =====
            DebugInfo debugInfo = new DebugInfo(iocContainer.Get<D3DEngine>());
            debugInfo.Activated = true;
            debugInfo.SetComponants(iocContainer.Get<FPS>(), iocContainer.Get<IClock>(), iocContainer.Get<IWorldChunks>(), iocContainer.Get<PlayerEntityManager>(), _server);
            utopiaRenderer.GameComponents.Add(debugInfo);

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
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Jump
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
                Action = Actions.Block_SelectNext,
                TriggerType = MouseTriggerMode.ScrollWheelForward,
                Binding = MouseButton.ScrollWheel
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Block_SelectPrevious,
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
