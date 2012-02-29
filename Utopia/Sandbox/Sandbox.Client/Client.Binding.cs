using System;
using System.Drawing;
using Ninject;
using Nuclex.UserInterface;
using S33M3Engines.Sprites;
using Sandbox.Client.Components;
using Sandbox.Client.States;
using Sandbox.Shared.Web;
using Utopia;
using Utopia.Components;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.GUI.D3D.Inventory;
using Utopia.GUI.D3D.Map;
using Utopia.Server;
using Utopia.Server.Managers;
using Utopia.Shared.Config;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Weather;
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;
using S33M3Engines;
using S33M3Engines.Cameras;
using Utopia.Worlds.SkyDomes.SharedComp;
using S33M3Engines.D3D;
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
using Utopia.Action;
using Utopia.Network;
using Utopia.InputManager;
using Utopia.Entities.Managers;
using Utopia.Entities.Renderer;
using Utopia.GUI;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Managers.Interfaces;
using S33M3Engines.Timers;
using Utopia.Entities.Renderer.Interfaces;
using S33M3Engines.D3D.DebugTools;
using Utopia.Effects.Shared;

namespace Sandbox.Client
{
    public partial class GameClient
    {
        public void EarlyBinding(IKernel iocContainer)
        {
            
        }

        public void IocBinding()
        {
            if (_iocContainer != null)
                throw new InvalidOperationException();

            _iocContainer = new StandardKernel();

            _iocContainer.Bind<IKernel>().ToConstant(_iocContainer).InSingletonScope();

            //DirectX layer & Helper ===================================
            _iocContainer.Bind<D3DEngine>().ToSelf().InSingletonScope().WithConstructorArgument("startingSize", new Size(1024, 600)).WithConstructorArgument("windowCaption", "Utopia Sandbox Client");         //DirectX Engine
            _iocContainer.Bind<WorldFocusManager>().ToSelf().InSingletonScope(); //Focus
            //==========================================================

            //Parameters ===============================================
            //iocContainer.Bind<WorldParameters>().ToConstant(worldParam).InSingletonScope();
            _iocContainer.Bind<VisualWorldParameters>().ToSelf().InSingletonScope();

            //System Objects Management ================================
            _iocContainer.Bind<StatesManager>().ToSelf().InSingletonScope(); 
            _iocContainer.Bind<GameStatesManager>().ToSelf().InSingletonScope(); //Application shared states
            _iocContainer.Bind<ICamera>().To<FirstPersonCamera>().InSingletonScope(); //Type of camera used
            _iocContainer.Bind<CameraManager>().ToSelf().InSingletonScope();     //Camera manager
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
            _iocContainer.Bind<InputsManager>().ToSelf().InSingletonScope();     //Input management
            _iocContainer.Bind<ActionsManager>().ToSelf().InSingletonScope();    //Action management
            //==========================================================

            //GUI =========================================================
            _iocContainer.Bind<GuiManager>().ToSelf().InSingletonScope();        //Gui base class
            _iocContainer.Bind<Screen>().ToSelf().InSingletonScope();
            //=============================================================

            //Inventory ===================================================
            _iocContainer.Bind<IconFactory>().ToSelf().InSingletonScope();       //Icon Factory
            //=============================================================

            //Debug displayer component ===================================
            _iocContainer.Bind<FPS>().ToSelf().InSingletonScope();
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
            _iocContainer.Bind<FadeComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<DebugInfo>().ToSelf().InSingletonScope();
            _iocContainer.Bind<VoxelModelManager>().ToSelf().InSingletonScope();
            _iocContainer.Bind<ModelEditorComponent>().ToSelf().InSingletonScope();
            _iocContainer.Bind<BlackBgComponent>().ToSelf().InSingletonScope();

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
