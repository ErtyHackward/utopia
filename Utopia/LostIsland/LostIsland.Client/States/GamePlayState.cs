using LostIsland.Client.Components;
using LostIsland.Shared;
using Ninject;
using Ninject.Parameters;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.Threading;
using S33M3Engines.Timers;
using S33M3Engines.WorldFocus;
using Utopia;
using Utopia.Action;
using Utopia.Editor;
using Utopia.Effects.Shared;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.GUI;
using Utopia.GUI.D3D;
using Utopia.GUI.D3D.Inventory;
using Utopia.GUI.D3D.Map;
using Utopia.InputManager;
using Utopia.Network;
using Utopia.Server;
using Utopia.Server.Managers;
using Utopia.Shared.Chunks;
using Utopia.Shared.Config;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Worlds.Cubes;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.Storage;
using Utopia.Worlds.Weather;

namespace LostIsland.Client.States
{
    /// <summary>
    /// Main gameplay stuff. Displaying the chunks, an entities, handling an input
    /// </summary>
    public class GamePlayState : GameState
    {
        private readonly IKernel _ioc;
        private RuntimeVariables _vars;
        private Server _server;
        private LostIslandEntityFactory _serverFactory;

        public override string Name
        {
            get { return "Gameplay"; }
        }

        public GamePlayState(IKernel ioc)
        {
            _ioc = ioc;
        }

        public override void OnEnabled(GameState previousState)
        {
            WorkQueue.DoWorkInThread(GameplayInitialize);
            
            base.OnEnabled(previousState);
        }

        public override void Initialize()
        {
            var loading = _ioc.Get<LoadingComponent>();
            _vars = _ioc.Get<RuntimeVariables>();

            AddComponent(loading);
        }

        private void GameplayInitialize()
        {
            if (_vars.SinglePlayer)
            {
                if(_server == null)
                {
                    var sqliteStorage = _ioc.Get<SQLiteStorageManager>();
                    _serverFactory = new LostIslandEntityFactory(null);
                    _server = new Server(_ioc.Get<XmlSettingsManager<ServerSettings>>(), _ioc.Get<WorldGenerator>(), sqliteStorage, sqliteStorage, sqliteStorage, _serverFactory);
                    _serverFactory.LandscapeManager = _server.LandscapeManager;
                }
            }
        }

        private void GameplayComponentsCreation()
        {
            //GameComponents.Add(new DebugComponent(this, _d3dEngine, _renderStates.screen, _renderStates.gameStatesManager, _renderStates.actionsManager, _renderStates.playerEntityManager));
            
            // be carefull with initilization order
            var serverComponent = _ioc.Get<ServerComponent>();
            var worldFocusManager = _ioc.Get<WorldFocusManager>();
            var wordParameters = _ioc.Get<WorldParameters>();
            //var visualWorldParameters = _ioc.Get<VisualWorldParameters>();
            var firstPersonCamera = _ioc.Get<FirstPersonCamera>();
            var cameraManager = _ioc.Get<CameraManager>();
            var timerManager = _ioc.Get<TimerManager>();
            var inputsManager = _ioc.Get<InputsManager>();
            var actionsManager = _ioc.Get<ActionsManager>();
            var guiManager = _ioc.Get<GuiManager>();
            var iconFactory = _ioc.Get<IconFactory>();
            var fps = _ioc.Get<FPS>();
            var gameClock = _ioc.Get<IClock>();
            var inventory = _ioc.Get<InventoryComponent>();
            var chat = _ioc.Get<ChatComponent>();
            var map = _ioc.Get<MapComponent>();
            var hud = _ioc.Get<Hud>();
            var entityEditor = _ioc.Get<EntityEditor>();
            var carvingEditor = _ioc.Get<CarvingEditor>();
            var stars = _ioc.Get<IDrawableComponent>("Stars");
            var skyDome = _ioc.Get<ISkyDome>();
            var weather = _ioc.Get<IWeather>();
            var clouds = _ioc.Get<IDrawableComponent>("Clouds");
            var chunkStorageManager = _ioc.Get<IChunkStorageManager>(new ConstructorArgument("forceNew", false), new ConstructorArgument("UserName", "change_this_login"));
            var solidCubeMeshFactory = _ioc.Get<ICubeMeshFactory>("SolidCubeMeshFactory");

            var liquidCubeMeshFactory = _ioc.Get<ICubeMeshFactory>("LiquidCubeMeshFactory");
            var singleArrayChunkContainer = _ioc.Get<SingleArrayChunkContainer>();
            var landscapeManager = _ioc.Get<ILandscapeManager>();
            var lightingManager = _ioc.Get<ILightingManager>();
            var chunkMeshManager = _ioc.Get<IChunkMeshManager>();
            var worldChunks = _ioc.Get<IWorldChunks>();
            var chunksWrapper = _ioc.Get<IChunksWrapper>();
            var worldGenerator = _ioc.Get<WorldGenerator>();
            var worldProcessorConfig = _ioc.Get<IWorldProcessorConfig>();
            var pickingRenderer = _ioc.Get<IPickingRenderer>();
            var chunkEntityImpactManager = _ioc.Get<IChunkEntityImpactManager>();
            var entityPickingManager = _ioc.Get<IEntityPickingManager>();
            var dynamicEntityManager = _ioc.Get<IDynamicEntityManager>();
            var playerEntityManager = _ioc.Get<PlayerEntityManager>();
            var playerCharacter = _ioc.Get<PlayerCharacter>();
            var playerEntityRenderer = _ioc.Get<IEntitiesRenderer>("PlayerEntityRenderer");
            var defaultEntityRenderer = _ioc.Get<IEntitiesRenderer>("DefaultEntityRenderer");
            var voxelMeshFactory = _ioc.Get<VoxelMeshFactory>();
            var sharedFrameCB = _ioc.Get<SharedFrameCB>();
            var itemMessageTranslator = _ioc.Get<ItemMessageTranslator>();
            var entityMessageTranslator = _ioc.Get<EntityMessageTranslator>();

            firstPersonCamera.CameraPlugin = playerEntityManager;
            worldFocusManager.WorldFocus = firstPersonCamera;
            chunkEntityImpactManager.LateInitialization(serverComponent, singleArrayChunkContainer, worldChunks, chunkStorageManager, lightingManager);

            AddComponent(cameraManager);
            AddComponent(_ioc.Get<ServerComponent>());
            AddComponent(inputsManager);
            AddComponent(iconFactory);
            AddComponent(timerManager);
            AddComponent(playerEntityManager);
            AddComponent(dynamicEntityManager);
            AddComponent(hud);
            AddComponent(guiManager);
            AddComponent(pickingRenderer);
            AddComponent(inventory);
            AddComponent(chat);
            AddComponent(map);
            AddComponent(fps);
            AddComponent(entityEditor);
            AddComponent(carvingEditor);
            AddComponent(skyDome);
            AddComponent(gameClock);
            AddComponent(weather);
            AddComponent(worldChunks);
            AddComponent(sharedFrameCB);

            var debugInfo = _ioc.Get<DebugInfo>();
            debugInfo.Activated = true;
            debugInfo.SetComponants(
                _ioc.Get<FPS>(),
                _ioc.Get<IClock>(),
                _ioc.Get<IWorldChunks>(),
                _ioc.Get<PlayerEntityManager>(),
                _ioc.Get<GuiManager>()
                );
        }

    }
}
