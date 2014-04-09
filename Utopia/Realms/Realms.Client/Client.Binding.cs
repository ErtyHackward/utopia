using System;
using System.Drawing;
using System.Windows.Forms;
using Ninject;
using Realms.Client.Components;
using Realms.Client.Components.GUI;
using Realms.Client.Components.GUI.Inventory;
using Realms.Client.Components.GUI.Settings;
using Realms.Client.Components.GUI.SinglePlayer;
using Realms.Client.States;
using Utopia.Components;
using Utopia.Entities.Voxel;
using Utopia.GUI.CharacterSelection;
using Utopia.GUI.Crafting;
using Utopia.Shared.Net.Web;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Weather;
using Utopia.Worlds.SkyDomes.SharedComp;
using Utopia.Worlds.SkyDomes;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Shared.Interfaces;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Cubes;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Entities;
using Utopia.Worlds.Storage;
using Utopia.Network;
using Utopia.Entities.Managers;
using Utopia.Entities.Renderer;
using Utopia.GUI;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer.Interfaces;
using S33M3DXEngine;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.States;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Timers;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.GUI.Nuclex;
using Utopia.GUI.Inventory;
using S33M3DXEngine.Main.Interfaces;
using Utopia.Action;
using System.Collections.Generic;
using System.Reflection;
using S33M3CoreComponents.Debug;
using Utopia.Shared.Entities;
using S33M3Resources.Structs;
using Utopia.Shared.Settings;
using Utopia.Shared.Net.Messages;
using S33M3CoreComponents.Sound;
using S33M3CoreComponents.Particules;
using Utopia.Particules;
using Utopia.Sounds;
using Utopia.Shared.LandscapeEntities;
using Utopia.Shared;
using Utopia.Worlds.Shadows;
using Utopia.PostEffects;
using Utopia.GUI.WindRose;
using Utopia.Shared.GraphicManagers;

namespace Realms.Client
{
    public partial class GameClient
    {
        public void IocBinding(string WindowsCaption, Size windowStartingSize, Size resolutionSize = default(Size))
        {
            if (_iocContainer != null || _d3dEngine != null)
                throw new InvalidOperationException();

            _iocContainer = new StandardKernel();

            // =============================================================================================================================================================
            // Application LifeTime Binding configuration = Singleton Scope ================================================================================================
            // =============================================================================================================================================================
            _d3dEngine = new D3DEngine(windowStartingSize, WindowsCaption, ClientSettings.Current.Settings.GraphicalParameters.MSAA.SampleDescription, resolutionSize);

            _d3dEngine.IsFullScreen = ClientSettings.Current.Settings.GraphicalParameters.Fullscreen;

            if (!ClientSettings.Current.Settings.GraphicalParameters.WindowPos.IsEmpty)
            {
                _d3dEngine.GameWindow.StartPosition = FormStartPosition.Manual;
                _d3dEngine.GameWindow.Location = ClientSettings.Current.Settings.GraphicalParameters.WindowPos;
            }

            _iocContainer.Bind<D3DEngine>().ToConstant(_d3dEngine).InSingletonScope();
            _iocContainer.Bind<GameStatesManager>().ToSelf().InSingletonScope().WithConstructorArgument("allocatedThreadPool", 3); //Application shared states
            _iocContainer.Bind<SpriteRenderer>().ToSelf().InSingletonScope();
            _iocContainer.Bind<WorldParameters>().ToSelf().InSingletonScope();
            _iocContainer.Bind<SandboxCommonResources>().ToSelf().InSingletonScope();

            // Game states ================================================
            _iocContainer.Bind<RealmRuntimeVariables>().ToSelf().InSingletonScope();
            //Force RuntimeVariables to use the same singleton as RealmRuntimeVariables !
            _iocContainer.Bind<RuntimeVariables>().ToMethod(x => x.Kernel.Get<RealmRuntimeVariables>());

            _iocContainer.Bind<LoginState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<LoginComponent>().ToSelf().InSingletonScope();

            _iocContainer.Bind<SystemComponentsState>().ToSelf().InSingletonScope();

            _iocContainer.Bind<StartUpState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<StartUpComponent>().ToSelf().InSingletonScope();

            _iocContainer.Bind<CreditsState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<CreditsComponent>().ToSelf().InSingletonScope();

            _iocContainer.Bind<MainMenuState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<MainMenuComponent>().ToSelf().InSingletonScope();

            _iocContainer.Bind<LoadingGameState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<LoadingComponent>().ToSelf().InSingletonScope();

            _iocContainer.Bind<GamePlayState>().ToSelf().InSingletonScope();

            _iocContainer.Bind<ServerSelectionComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<SelectServerGameState>().ToSelf().InSingletonScope();

            _iocContainer.Bind<SettingsComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<SettingsState>().ToSelf().InSingletonScope();

            _iocContainer.Bind<InGameMenuComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<InGameMenuState>().ToSelf().InSingletonScope();

            _iocContainer.Bind<SinglePlayerComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<SinglePlayerMenuState>().ToSelf().InSingletonScope();

            _iocContainer.Bind<FadeSwitchComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<CubeTexturesManager>().ToSelf().InSingletonScope();

            //Network Related =============================================
            _iocContainer.Bind<ClientWebApi>().ToSelf().InSingletonScope();
            //=============================================================

            //User Input Handling ======================================
            _iocContainer.Bind<InputsManager>().ToSelf().InSingletonScope().WithConstructorArgument("actionType", typeof(UtopiaActions));     //Input management
            //==========================================================

            //GUI =========================================================
            //Create a list of assembly where GUI components will be looked into.
            List<Assembly> componentsAssemblies = new List<Assembly>();
            componentsAssemblies.Add(typeof(Program).Assembly); //Add all components from Sanbox.Client Assembly
            componentsAssemblies.Add(typeof(Utopia.UtopiaRender).Assembly); //Check inside Utopia namespace assembly
            _iocContainer.Bind<GuiManager>().ToSelf().InSingletonScope().WithConstructorArgument("skinPath", @"GUI\Skins\SandBox\SandBox.skin.xml")
                                                                        .WithConstructorArgument("plugInComponentAssemblies", componentsAssemblies);        //Gui base class
            _iocContainer.Bind<MainScreen>().ToSelf().InSingletonScope();
            //=============================================================
            _iocContainer.Bind<VersionWatermark>().ToSelf().InSingletonScope();

            _iocContainer.Bind<ModelEditorComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<BlackBgComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<ISoundEngine>().To<SoundEngine>().InSingletonScope();
            _iocContainer.Bind<GeneralSoundManager>().To<RealmGeneralSoundManager>().InSingletonScope();

            //Debug Components ===========================================
            _iocContainer.Bind<DebugComponent>().ToSelf().InSingletonScope().WithConstructorArgument("LeftPanelColor", new ByteColor(44, 51, 59));             
            //=============================================================


            // =============================================================================================================================================================
            // Game related LifeTime Binding configuration = "Current Game Scope" ==========================================================================================
            // =============================================================================================================================================================

            //DirectX layer & Helper ===================================
            _iocContainer.Bind<WorldFocusManager>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            //==========================================================

            //Inventory ===================================================
            _iocContainer.Bind<IconFactory>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            //=============================================================

            //Parameters ===============================================
            _iocContainer.Bind<VisualWorldParameters>().ToSelf().InScope(x => GameScope.CurrentGameScope);

            _iocContainer.Bind<ICameraFocused>().To<FirstPersonCameraWithFocus>().InScope(x => GameScope.CurrentGameScope).Named("FirstPCamera").WithConstructorArgument("nearPlane", 0.1f).WithConstructorArgument("farPlane", 2800f); //Type of camera used
            _iocContainer.Bind<ICameraFocused>().To<ThirdPersonCameraWithFocus>().InScope(x => GameScope.CurrentGameScope).Named("ThirdPCamera").WithConstructorArgument("nearPlane", 0.1f).WithConstructorArgument("farPlane", 2800f); //Type of camera used
            //Force ICamera to use the same singleton as ICameraFocused !
            _iocContainer.Bind<ICamera>().ToMethod(x => x.Kernel.Get<ICameraFocused>()).InScope(x => GameScope.CurrentGameScope);

            _iocContainer.Bind<CameraManager<ICameraFocused>>().ToSelf().InScope(x => GameScope.CurrentGameScope);//Camera manager
            _iocContainer.Bind<ICameraManager>().ToMethod(x => x.Kernel.Get<CameraManager<ICameraFocused>>()).InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<TimerManager>().ToSelf().InScope(x => GameScope.CurrentGameScope);      //Ingame based Timer class
            _iocContainer.Bind<StaggingBackBuffer>().ToSelf().InScope(x => GameScope.CurrentGameScope).Named("SkyBuffer").WithConstructorArgument("Name", "SkyBuffer");
            _iocContainer.Bind<PostEffectComponent>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<SharedFrameCB>().ToSelf().InScope(x => GameScope.CurrentGameScope);     //Ingame based Timer class

            //Network Related =============================================
            _iocContainer.Bind<IChunkEntityImpactManager>().To<ChunkEntityImpactManager>().InScope(x => GameScope.CurrentGameScope); //Impact on player action (From server events)
            _iocContainer.Bind<ILandscapeManager>().ToMethod(x => x.Kernel.Get<IChunkEntityImpactManager>()).InScope(x => GameScope.CurrentGameScope);

            _iocContainer.Bind<EntityFactory>().ToSelf().InScope(x => GameScope.CurrentGameScope).Named("Client");

            _iocContainer.Bind<EntityMessageTranslator>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<ItemMessageTranslator>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<SyncManager>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            //=============================================================

            //Game Componenents =========================================
            _iocContainer.Bind<ServerComponent>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<IClock>().To<ClientClock>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<PlayerInventory>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<InventoryComponent>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<WindRoseComponent>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<ContainerInventory>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<CraftingComponent>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<CraftingWindow>().To<CraftingInventory>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<CharacterSelectionComponent>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<CharacterSelectionWindow>().To<SelectionInventory>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<InventoryEventComponent>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<ChatComponent>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<Hud>().To<RealmsHud>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<IDrawableComponent>().To<SkyStars>().InScope(x => GameScope.CurrentGameScope).Named("Stars");
            _iocContainer.Bind<ISkyDome>().To<RegularSkyDome>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<IWeather>().To<Weather>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<IDrawableComponent>().To<Clouds>().InScope(x => GameScope.CurrentGameScope).Named("Clouds");
            _iocContainer.Bind<VoxelModelManager>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<LocalServer>().ToSelf().InScope(x => GameScope.CurrentGameScope);

            //Landscape Creation/Acces/Management ====================================
            _iocContainer.Bind<IChunkStorageManager>().To<SQLiteWorldStorageManager>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<ICubeMeshFactory>().To<SolidCubeMeshFactory>().InScope(x => GameScope.CurrentGameScope).Named("SolidCubeMeshFactory");
            _iocContainer.Bind<ICubeMeshFactory>().To<LiquidCubeMeshFactory>().InScope(x => GameScope.CurrentGameScope).Named("LiquidCubeMeshFactory");
            _iocContainer.Bind<SingleArrayChunkContainer>().ToSelf().InScope(x => GameScope.CurrentGameScope);         //The client  "Big" Array
            _iocContainer.Bind<ILandscapeManager2D>().To<LandscapeManager>().InScope(x => GameScope.CurrentGameScope);   //Interface betwee the big array and landscape processors
            _iocContainer.Bind<ILightingManager>().To<LightingManager>().InScope(x => GameScope.CurrentGameScope);     //Landscape lightings
            _iocContainer.Bind<IChunkMeshManager>().To<ChunkMeshManager>().InScope(x => GameScope.CurrentGameScope);   //Chunk Mesh + Entities creation
            _iocContainer.Bind<IWorldChunks>().To<WorldChunks>().InScope(x => GameScope.CurrentGameScope);             //Chunk Management (Update/Draw)
            _iocContainer.Bind<WorldShadowMap>().ToSelf().InScope(x => GameScope.CurrentGameScope);                    //Shadow Map Drawing Component
            _iocContainer.Bind<IChunksWrapper>().To<WorldChunksWrapper>().InScope(x => GameScope.CurrentGameScope);    //Chunk "Wrapping" inside the big Array
            _iocContainer.Bind<LandscapeBufferManager>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<CracksRenderer>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            //=============================================================

            //Entities related stuff ====================================================
            _iocContainer.Bind<IPickingRenderer>().To<PickingRenderer>().InScope(x => GameScope.CurrentGameScope);         // Use to display the picking cursor on block
            _iocContainer.Bind<SelectedBlocksRenderer>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<IEntityPickingManager>().To<EntityPickAndCollisManager>().InScope(x => GameScope.CurrentGameScope);   //Entites picking and collision handling vs player
            _iocContainer.Bind<IVisualDynamicEntityManager>().To<DynamicEntityManager>().InScope(x => GameScope.CurrentGameScope);         //Dynamic Entity manager
            _iocContainer.Bind<IDynamicEntityManager>().To<DynamicEntityManager>().InScope(x => GameScope.CurrentGameScope);         //Dynamic Entity manager
            _iocContainer.Bind<IGlobalStateManager>().To<GlobalStateManager>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<IPlayerManager,PlayerEntityManager>().To<PlayerEntityManager>().InScope(x => GameScope.CurrentGameScope);                             //The player manager
            //_iocContainer.Bind<IPlayerManager>().To<GodEntityManager>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<PickingManager>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            //Register the Player Against IDynamicEntity and PlayerCharacter
            _iocContainer.Bind<VoxelMeshFactory>().ToSelf().InScope(x => GameScope.CurrentGameScope);  //Voxel Factory
            _iocContainer.Bind<FirstPersonToolRenderer>().ToSelf().InScope(x => GameScope.CurrentGameScope); // draw active tool in first person mode
            _iocContainer.Bind<GhostedEntityRenderer>().ToSelf().InScope(x => GameScope.CurrentGameScope); // draws a blue semi-transparent model
            //=============================================================
            _iocContainer.Bind<GameSoundManager>().To<RealmGameSoundManager>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<ToolBarUi>().To<SandboxToolBar>().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<FadeComponent>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<AdminConsole>().ToSelf().InScope(x => GameScope.CurrentGameScope);
            _iocContainer.Bind<UtopiaParticuleEngine>().ToSelf().InScope(x => GameScope.CurrentGameScope);
        }
    }
}
