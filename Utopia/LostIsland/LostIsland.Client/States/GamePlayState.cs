using System;
using System.IO;
using LostIsland.Client.Components;
using LostIsland.Shared;
using Ninject;
using Ninject.Parameters;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.Shared.Math;
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
using Utopia.Settings;
using Utopia.Shared.Chunks;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Config;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors;
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
        private ServerComponent _serverComponent;

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
            AddComponent(_ioc.Get<ServerComponent>());

            var engine = _ioc.Get<D3DEngine>();

            engine.MouseCapture = true;


        }

        private void GameplayInitialize()
        {
            if (_vars.SinglePlayer)
            {
                if(_server == null)
                {
                    _serverFactory = new LostIslandEntityFactory(null);
                    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Utopia\\local.db");
                    var sqliteStorage = _ioc.Get<SQLiteStorageManager>(new[] { new ConstructorArgument("filePath", dbPath), new ConstructorArgument("factory", _serverFactory) });

                    sqliteStorage.Register("local", "qwe123".GetMd5Hash(), Utopia.Shared.Structs.UserRole.Administrator);
                    
                    var settings = _ioc.Get<XmlSettingsManager<ServerSettings>>();

                    var wp = _ioc.Get<WorldParameters>();
                    var planProcessor = new PlanWorldProcessor(wp, _serverFactory);
                    var worldGenerator = new WorldGenerator(wp, planProcessor);

                    _server = new Server(settings, worldGenerator, sqliteStorage, sqliteStorage, sqliteStorage, _serverFactory);
                    _serverFactory.LandscapeManager = _server.LandscapeManager;
                    _server.ConnectionManager.LocalMode = true;
                    _server.ConnectionManager.Listen();
                    _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;
                    _server.LoginManager.GenerationParameters = planProcessor.WorldPlan.Parameters;

                    // client world generator
                    var clientGeneratpr = new WorldGenerator(wp, new PlanWorldProcessor(wp, _ioc.Get<EntityFactory>("Client")));
                    _ioc.Bind<WorldGenerator>().ToConstant(clientGeneratpr).InSingletonScope();
                }

                if (_serverComponent == null)
                {
                    _serverComponent = _ioc.Get<ServerComponent>();
                    _serverComponent.ConnectionInitialized += ServerComponentConnectionInitialized;
                }
                _serverComponent.BindingServer("127.0.0.1");
                _serverComponent.ConnectToServer("local", "qwe123", false);
            }
        }

        void LoginManagerPlayerEntityNeeded(object sender, NewPlayerEntityNeededEventArgs e)
        {
            var dEntity = new PlayerCharacter();
            dEntity.DynamicId = e.EntityId;
            dEntity.DisplacementMode = EntityDisplacementModes.Walking;
            dEntity.Position = _server.LandscapeManager.GetHighestPoint(new Vector3D(10, 0, 10));
            dEntity.CharacterName = "Local player";
            ContainedSlot outItem;
            //dEntity.Equipment.Equip(EquipmentSlotType.LeftHand, new EquipmentSlot<ITool> { Item = (ITool)EntityFactory.Instance.CreateEntity(LostIslandEntityClassId.Annihilator) }, out outItem);

            var adder = (CubeResource)_server.EntityFactory.CreateEntity(EntityClassId.CubeResource);
            adder.CubeId = CubeId.HalfWoodPlank;//looting a terraincube will create a new blockadder instance or add to the stack

            dEntity.Equipment.Equip(EquipmentSlotType.LeftHand, new EquipmentSlot<ITool> { Item = adder }, out outItem);

            foreach (var cubeId in CubeId.All())
            {
                if (cubeId == CubeId.Air)
                    continue;

                var item3 = (CubeResource)_server.EntityFactory.CreateEntity((EntityClassId.CubeResource));
                item3.CubeId = cubeId;
                dEntity.Inventory.PutItem(item3);
            }
            e.PlayerEntity = dEntity;
        }

        void ServerComponentConnectionInitialized(object sender, ServerComponentConnectionInitializeEventArgs e)
        {
            if (e.ServerConnection != null)
            {
                e.ServerConnection.MessageEntityIn += ServerConnectionMessageEntityIn;
                
            }
        }

        void ServerConnectionMessageEntityIn(object sender, Utopia.Shared.Net.Connections.ProtocolMessageEventArgs<Utopia.Shared.Net.Messages.EntityInMessage> e)
        {
            var player = (PlayerCharacter)e.Message.Entity;
            _ioc.Rebind<PlayerCharacter>().ToConstant(player).InSingletonScope(); //Register the current Player.
            _ioc.Rebind<IDynamicEntity>().ToConstant(player).InSingletonScope().Named("Player"); //Register the current Player.
            _serverComponent.ServerConnection.MessageEntityIn -= ServerConnectionMessageEntityIn;
            GameplayComponentsCreation();
        }

        private void GameplayComponentsCreation()
        {
            //GameComponents.Add(new DebugComponent(this, _d3dEngine, _renderStates.screen, _renderStates.gameStatesManager, _renderStates.actionsManager, _renderStates.playerEntityManager));

            var worldParam = new WorldParameters
            {
                IsInfinite = true,
                Seed = _ioc.Get<ServerComponent>().GameInformations.WorldSeed,
                SeaLevel = _ioc.Get<ServerComponent>().GameInformations.WaterLevel,
                WorldChunkSize = new Location2<int>(ClientSettings.Current.Settings.GraphicalParameters.WorldSize,   //Define the visible Client chunk size
                                                ClientSettings.Current.Settings.GraphicalParameters.WorldSize)
            };

            _ioc.Rebind<WorldParameters>().ToConstant(worldParam).InSingletonScope();

            // be careful with initilization order
            var serverComponent = _ioc.Get<ServerComponent>();
            var worldFocusManager = _ioc.Get<WorldFocusManager>();
            var wordParameters = _ioc.Get<WorldParameters>();
            //var visualWorldParameters = _ioc.Get<VisualWorldParameters>();
            var firstPersonCamera = _ioc.Get<ICamera>();
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
            //var worldProcessorConfig = _ioc.Get<IWorldProcessorConfig>();
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
            worldFocusManager.WorldFocus = (IWorldFocus)firstPersonCamera;
            chunkEntityImpactManager.LateInitialization(serverComponent, singleArrayChunkContainer, worldChunks, chunkStorageManager, lightingManager);

            AddComponent(cameraManager);
            //AddComponent(_ioc.Get<ServerComponent>());
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

            StatesManager.UpdateCurrentStateComponents();
            
            var debugInfo = _ioc.Get<DebugInfo>();
            debugInfo.Activated = true;
            debugInfo.SetComponants(
                _ioc.Get<FPS>(),
                _ioc.Get<IClock>(),
                _ioc.Get<IWorldChunks>(),
                _ioc.Get<PlayerEntityManager>(),
                _ioc.Get<GuiManager>()
                );

            playerEntityManager.Enabled = false;
            worldChunks.LoadComplete += worldChunks_LoadComplete;
        }

        void worldChunks_LoadComplete(object sender, EventArgs e)
        {
            var loading = _ioc.Get<LoadingComponent>();
            loading.Enabled = false;
            loading.Visible = false;

            var playerEntityManager = _ioc.Get<PlayerEntityManager>();
            playerEntityManager.Enabled = true;
        }

    }
}
