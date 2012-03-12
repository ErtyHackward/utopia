using System;
using System.Drawing;
using Ninject;
using Sandbox.Client.Components;
using Sandbox.Client.States;
using Sandbox.Shared.Web;
using Utopia;
using Utopia.Components;
using Utopia.Entities.Voxel;
using Utopia.Server;
using Utopia.Server.Managers;
using Utopia.Shared.Config;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
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
using Utopia.Effects.Shared;
using S33M3_DXEngine;
using S33M3_CoreComponents.WorldFocus;
using S33M3_CoreComponents.States;
using S33M3_CoreComponents.Cameras.Interfaces;
using S33M3_CoreComponents.Cameras;
using S33M3_CoreComponents.Timers;
using S33M3_CoreComponents.Sprites;
using S33M3_CoreComponents.Inputs;
using S33M3_CoreComponents.Inputs.Actions;
using S33M3_CoreComponents.GUI;
using S33M3_CoreComponents.GUI.Nuclex;
using Utopia.GUI.Inventory;
using Utopia.GUI.Map;
using S33M3_DXEngine.Main.Interfaces;
using Utopia.Action;

namespace Sandbox.Client
{
    public partial class GameClient
    {
        public void EarlyBinding(IKernel iocContainer)
        {
            
        }

        public void IocBinding(string WindowsCaption, Size windowStartingSize, Size resolutionSize = default(Size))
        {
            if (_iocContainer != null)
                throw new InvalidOperationException();

            _iocContainer = new StandardKernel();

            _iocContainer.Bind<IKernel>().ToConstant(_iocContainer).InSingletonScope();

            //DirectX layer & Helper ===================================
            _iocContainer.Bind<D3DEngine>().ToSelf().InSingletonScope().WithConstructorArgument("startingSize", windowStartingSize).WithConstructorArgument("windowCaption", WindowsCaption).WithConstructorArgument("renderResolution", resolutionSize);         //DirectX Engine
            _iocContainer.Bind<WorldFocusManager>().ToSelf().InSingletonScope(); //Focus
            //==========================================================

            //Parameters ===============================================
            //iocContainer.Bind<WorldParameters>().ToConstant(worldParam).InSingletonScope();
            _iocContainer.Bind<VisualWorldParameters>().ToSelf().InSingletonScope();

            //System Objects Management ================================
            _iocContainer.Bind<GameStatesManager>().ToSelf().InSingletonScope().WithConstructorArgument("allocatedThreadPool", 3); //Application shared states
            _iocContainer.Bind<ICameraFocused>().To<FirstPersonCameraWithFocus>().InSingletonScope().WithConstructorArgument("nearPlane", 0.5f).WithConstructorArgument("farPlane", 3000f); //Type of camera used
            _iocContainer.Bind<ICamera>().To<FirstPersonCameraWithFocus>().InSingletonScope().WithConstructorArgument("nearPlane", 0.5f).WithConstructorArgument("farPlane", 3000f); //Type of camera used
            _iocContainer.Bind<CameraManager<ICameraFocused>>().ToSelf().InSingletonScope();     //Camera manager
            _iocContainer.Bind<TimerManager>().ToSelf().InSingletonScope();      //Ingame based Timer class
            _iocContainer.Bind<SharedFrameCB>().ToSelf().InSingletonScope();      //Ingame based Timer class
            _iocContainer.Bind<SpriteRenderer>().ToSelf().InSingletonScope();

            // Game states ================================================
            _iocContainer.Bind<RuntimeVariables>().ToSelf().InSingletonScope();
            _iocContainer.Bind<LoginState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<CreditsState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<MainMenuState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<LoadingGameState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<GamePlayState>().ToSelf().InSingletonScope();
            _iocContainer.Bind<ServerSelectionComponent>().ToSelf().InSingletonScope();
            
            //Network Related =============================================
            _iocContainer.Bind<IChunkEntityImpactManager>().To<ChunkEntityImpactManager>().InSingletonScope(); //Impact on player action (From server events)
            _iocContainer.Bind<EntityMessageTranslator>().ToSelf().InSingletonScope();
            _iocContainer.Bind<ItemMessageTranslator>().ToSelf().InSingletonScope();
            _iocContainer.Bind<ClientWebApi>().ToSelf().InSingletonScope();
            //=============================================================

            //User Input Handling ======================================
            _iocContainer.Bind<InputsManager>().ToSelf().InSingletonScope().WithConstructorArgument("actionType", typeof(UtopiaActions));     //Input management
            //==========================================================

            //GUI =========================================================
            _iocContainer.Bind<GuiManager>().ToSelf().InSingletonScope().WithConstructorArgument("skinPath", @"GUI\Skins\Default\Default.skin.xml");        //Gui base class
            _iocContainer.Bind<MainScreen>().ToSelf().InSingletonScope();
            //=============================================================

            //Inventory ===================================================
            _iocContainer.Bind<IconFactory>().ToSelf().InSingletonScope();       //Icon Factory
            //=============================================================

            //Game Componenents =========================================
            _iocContainer.Bind<ServerComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<IClock>().To<WorldClock>().InSingletonScope();
            _iocContainer.Bind<InventoryComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<ChatComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<MapComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<Hud>().ToSelf().InSingletonScope();
            //_iocContainer.Bind<EntityEditor>().ToSelf().InSingletonScope();
            //_iocContainer.Bind<CarvingEditor>().ToSelf().InSingletonScope();            
            _iocContainer.Bind<IDrawableComponent>().To<SkyStars>().InSingletonScope().Named("Stars");
            _iocContainer.Bind<ISkyDome>().To<RegularSkyDome>().InSingletonScope();
            _iocContainer.Bind<IWeather>().To<Weather>().InSingletonScope();
            _iocContainer.Bind<IDrawableComponent>().To<Clouds>().InSingletonScope().Named("Clouds_flat");
            _iocContainer.Bind<IDrawableComponent>().To<Clouds3D>().InSingletonScope().Named("Clouds_3D");
            _iocContainer.Bind<BepuPhysicsComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<LoadingComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<LoginComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<FadeSwitchComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<VoxelModelManager>().ToSelf().InSingletonScope();
            _iocContainer.Bind<ModelEditorComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<BlackBgComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<SoundManager>().To<SandboxSoundManager>().InSingletonScope();

            //Landscape Creation/Acces/Management ====================================
            _iocContainer.Bind<IChunkStorageManager>().To<SQLiteWorldStorageManager>().InSingletonScope();
            _iocContainer.Bind<ICubeMeshFactory>().To<SolidCubeMeshFactory>().InSingletonScope().Named("SolidCubeMeshFactory");
            _iocContainer.Bind<ICubeMeshFactory>().To<LiquidCubeMeshFactory>().InSingletonScope().Named("LiquidCubeMeshFactory");
            _iocContainer.Bind<SingleArrayChunkContainer>().ToSelf().InSingletonScope();         //The client  "Big" Array
            _iocContainer.Bind<ILandscapeManager>().To<LandscapeManager>().InSingletonScope();   //Interface betwee the big array and landscape processors
            _iocContainer.Bind<ILightingManager>().To<LightingManager>().InSingletonScope();     //Landscape lightings
            _iocContainer.Bind<IChunkMeshManager>().To<ChunkMeshManager>().InSingletonScope();   //Chunk Mesh + Entities creation
            _iocContainer.Bind<IWorldChunks>().To<WorldChunks>().InSingletonScope();             //Chunk Management (Update/Draw)
            _iocContainer.Bind<IChunksWrapper>().To<WorldChunksWrapper>().InSingletonScope();    //Chunk "Wrapping" inside the big Array
            
            //_iocContainer.Bind<WorldGenerator>().ToSelf().InSingletonScope();                    //World Generator Class
            //_iocContainer.Bind<IWorldProcessorConfig>().To<ErtyHackwardWorldConfig>().InSingletonScope();
            //_iocContainer.Bind<IWorldProcessor>().To<PlanWorldProcessor>().Named("ErtyHackwardPlanWorldProcessor");

            _iocContainer.Bind<IGameStateToolManager>().To<GameStateToolManager>().InSingletonScope();

            //iocContainer.Bind<IWorldProcessorConfig>().To<s33m3WorldConfig>().InSingletonScope().Named("s33m3World");
            //iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("s33m3WorldProcessor");
            //iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");
            //=============================================================

            //Entities related stuff ====================================================
            _iocContainer.Bind<IPickingRenderer>().To<PickingRenderer>().InSingletonScope();         // Use to display the picking cursor on block
            _iocContainer.Bind<IEntityPickingManager>().To<EntityPickAndCollisManager>().InSingletonScope();   //Entites picking and collision handling vs player
            _iocContainer.Bind<IDynamicEntityManager>().To<DynamicEntityManager>().InSingletonScope();         //Dynamic Entity manager
            _iocContainer.Bind<PlayerEntityManager>().ToSelf().InSingletonScope();                             //The player manager
            //Register the Player Against IDynamicEntity and PlayerCharacter
            _iocContainer.Bind<PlayerCharacter>().ToConstant(null).InSingletonScope(); //Register the current Player.
            _iocContainer.Bind<IDynamicEntity>().ToConstant(null).InSingletonScope().Named("Player"); //Register the current Player.
            _iocContainer.Bind<IEntitiesRenderer>().To<PlayerEntityRenderer>().InSingletonScope().Named("PlayerEntityRenderer");    //Rendering Player
            _iocContainer.Bind<IEntitiesRenderer>().To<DynamicEntityRenderer>().InSingletonScope().Named("DefaultEntityRenderer");  //Rendering Dynamic Entities
            _iocContainer.Bind<VoxelMeshFactory>().ToSelf().InSingletonScope();  //Voxel Factory
            _iocContainer.Bind<IVoxelModelStorage>().To<ModelSQLiteStorage>().InSingletonScope();
            //=============================================================

            // Server components ==========================================
            _iocContainer.Bind<XmlSettingsManager<ServerSettings>>().ToSelf().InSingletonScope().WithConstructorArgument("fileName", "localServer.config");
            _iocContainer.Bind<SQLiteStorageManager>().ToSelf().InSingletonScope();
            
        }
    }
}
