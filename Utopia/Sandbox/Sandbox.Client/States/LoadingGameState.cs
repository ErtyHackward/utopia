﻿using System;
using System.IO;
using Ninject;
using Ninject.Parameters;
using Sandbox.Client.Components;
using Sandbox.Shared;
using Sandbox.Shared.Items;
using Utopia;
using Utopia.Action;
using Utopia.Effects.Shared;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.GUI;
using Utopia.Network;
using Utopia.Server;
using Utopia.Server.Managers;
using Utopia.Shared.Chunks;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
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
using S33M3CoreComponents.States;
using S33M3DXEngine.Threading;
using S33M3Resources.Structs;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Timers;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.GUI;
using Utopia.GUI.Inventory;
using Utopia.GUI.Map;
using S33M3DXEngine.Main.Interfaces;
using S33M3CoreComponents.WorldFocus.Interfaces;
using S33M3DXEngine;
using S33M3CoreComponents.Debug;
using Utopia.Components;
using Utopia.Shared.Interfaces;
using Sandbox.Client.Components.GUI;
using Utopia.Shared.Settings;
using Utopia.Worlds.SkyDomes.SharedComp;
using S33M3CoreComponents.Config;
using Utopia.Shared.World.Processors.Utopia;

namespace Sandbox.Client.States
{
    /// <summary>
    /// Main gameplay stuff. Displaying the chunks, an entities, handling an input
    /// </summary>
    public class LoadingGameState : GameState
    {
        private readonly IKernel _ioc;
        private RuntimeVariables _vars;
        private Server _server;
        private SandboxEntityFactory _serverFactory;
        private SharpDX.Direct3D11.DeviceContext _context;
        private SQLiteStorageManager _serverSqliteStorageSinglePlayer;

        public override string Name
        {
            get { return "LoadingGame"; }
        }

        public LoadingGameState(GameStatesManager stateManager, IKernel ioc)
            :base(stateManager)
        {
            _ioc = ioc;
        }

        //State Initialization =>
        //Add Loading screen animation, and ServerComponent
        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            if (this.PreviousGameState != this) this.GameComponents.Clear();

            var loading = _ioc.Get<LoadingComponent>();
            _vars = _ioc.Get<RuntimeVariables>();
            _context = context;

            AddComponent(loading); //Will "Mask" the Components being loaded.
            AddComponent(_ioc.Get<ServerComponent>());
            AddComponent(_ioc.Get<GuiManager>());

            base.Initialize(context);
        }

        //The state is enabled, start loading other components in background while the Loading is shown
        public override void OnEnabled(GameState previousState)
        {
            if (this.PreviousGameState != this) SmartThread.ThreadPool.QueueWorkItem(GameplayInitializeAsync);

            base.OnEnabled(previousState);
        }

        private void GameplayInitializeAsync()
        {
            ServerComponent serverComponent = _ioc.Get<ServerComponent>();

            if (_vars.SinglePlayer)
            {
                var wp = _ioc.Get<WorldParameters>();

                //wp not initialized ==> We are in "SandBox" mode, load from the "Utopia SandBox" Default WorldParameters
                if (wp.SeedName == null)
                {
                    wp.WorldName = "SandBox World";
                    wp.SeedName = "Utopia SandBox";
                    wp.SeaLevel = Utopia.Shared.Chunks.AbstractChunk.ChunkSize.Y / 2;
                }

                InitSinglePlayerServer(wp);

                if (serverComponent.ServerConnection == null ||
                    serverComponent.ServerConnection.ConnectionStatus != Utopia.Shared.Net.Connections.ConnectionStatus.Connected)
                {
                    serverComponent.MessageEntityIn += ServerConnectionMessageEntityIn;
                    serverComponent.BindingServer("127.0.0.1");
                    serverComponent.ConnectToServer("local", _vars.DisplayName, "qwe123".GetSHA1Hash());
                    _vars.LocalDataBasePath = Path.Combine(_vars.ApplicationDataPath, "Client", "Singleplayer", wp.WorldName, "ClientWorldCache.db");
                }
            }
            else
            {
                if (serverComponent.ServerConnection == null ||
                    serverComponent.ServerConnection.ConnectionStatus != Utopia.Shared.Net.Connections.ConnectionStatus.Connected)
                {
                    serverComponent.MessageEntityIn += ServerConnectionMessageEntityIn;
                    serverComponent.BindingServer(_vars.CurrentServerAddress);
                    serverComponent.ConnectToServer(_vars.Login, _vars.DisplayName, _vars.PasswordHash);
                    _vars.LocalDataBasePath = Path.Combine(_vars.ApplicationDataPath, _vars.Login, "Client", "Multiplayer", _vars.CurrentServerAddress.Replace(':', '_'), "ClientWorldCache.db");
                }
            }
        }

        private void InitSinglePlayerServer(WorldParameters worldParam)
        {
            if (_server != null)
            {
                _server.Dispose();
            }

            _serverFactory = new SandboxEntityFactory(null);
            var dbPath = Path.Combine(_vars.ApplicationDataPath, "Server", "Singleplayer", worldParam.WorldName, "ServerWorld.db");

            _serverSqliteStorageSinglePlayer = new SQLiteStorageManager(dbPath,_serverFactory, worldParam);
            _serverSqliteStorageSinglePlayer.Register("local", "qwe123".GetSHA1Hash(), UserRole.Administrator);

            var settings = new XmlSettingsManager<ServerSettings>(@"Server\localServer.config");
            settings.Load();
            settings.Save();

            //Utopia New Landscape Test
            var utopiaProcessor = new UtopiaProcessor(worldParam);
            var worldGenerator = new WorldGenerator(worldParam, utopiaProcessor);

            //Old s33m3 landscape
            //IWorldProcessor processor1 = new s33m3WorldProcessor(worldParam);
            //IWorldProcessor processor2 = new LandscapeLayersProcessor(worldParam, _serverFactory);
            //var worldGenerator = new WorldGenerator(worldParam, processor1, processor2);

            //Vlad Generator
            //var planProcessor = new PlanWorldProcessor(wp, _serverFactory);
            //var worldGenerator = new WorldGenerator(wp, planProcessor);
            settings.Settings.ChunksCountLimit = 1024 * 3; // better use viewRange * viewRange * 3

            _server = new Server(settings, worldGenerator, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverFactory);
            _serverFactory.LandscapeManager = _server.LandscapeManager;
            _server.ConnectionManager.LocalMode = true;
            _server.ConnectionManager.Listen();
            _server.ConnectionManager.ConnectionRemoved += ConnectionManager_ConnectionRemoved;
            _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;
            _server.LoginManager.GenerationParameters = default(Utopia.Shared.World.PlanGenerator.GenerationParameters); // planProcessor.WorldPlan.Parameters;
            _server.Clock.SetCurrentTimeOfDay(TimeSpan.FromHours(12));

        }

        //The event will only be called when the client is removed from the local Single Player server
        //I need to dispose manually the SQLlite connection to the database server, in order to remove the lock
        void ConnectionManager_ConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            _server.ConnectionManager.ConnectionRemoved -= ConnectionManager_ConnectionRemoved;
            _serverSqliteStorageSinglePlayer.Dispose();
        }

        void LoginManagerPlayerEntityNeeded(object sender, NewPlayerEntityNeededEventArgs e)
        {
            var dEntity = new PlayerCharacter();
            dEntity.DynamicId = e.EntityId;
            dEntity.DisplacementMode = EntityDisplacementModes.Walking;
            dEntity.Position = _server.LandscapeManager.GetHighestPoint(new Vector3D(10, 0, 10));
            dEntity.CharacterName = "Local player";
            ContainedSlot outItem;
            //dEntity.Equipment.Equip(EquipmentSlotType.LeftHand, new EquipmentSlot<ITool> { Item = (ITool)EntityFactory.Instance.CreateEntity(SandboxEntityClassId.Annihilator) }, out outItem);

            var adder = _server.EntityFactory.CreateEntity<CubeResource>();
            adder.CubeId = CubeId.HalfWoodPlank;//looting a terraincube will create a new blockadder instance or add to the stack

            dEntity.Equipment.Equip(EquipmentSlotType.LeftHand, new EquipmentSlot<ITool> { Item = adder }, out outItem);

            foreach (var cubeId in CubeId.All())
            {
                if (cubeId == CubeId.Air)
                    continue;

                var item3 = _server.EntityFactory.CreateEntity<CubeResource>();
                item3.CubeId = cubeId;
                dEntity.Inventory.PutItem(item3);
            }
            var goldCoins = _server.EntityFactory.CreateEntity<GoldCoin>();
            dEntity.Inventory.PutItem(goldCoins);
            //var Torch = _server.EntityFactory.CreateEntity<Sandbox.Shared.Items.Torch>();
            //dEntity.Inventory.PutItem(Torch);

            e.PlayerEntity = dEntity;
        }

        void ServerConnectionMessageEntityIn(object sender, Utopia.Shared.Net.Connections.ProtocolMessageEventArgs<Utopia.Shared.Net.Messages.EntityInMessage> e)
        {
            ServerComponent serverComponent = _ioc.Get<ServerComponent>();

            var player = (PlayerCharacter)e.Message.Entity;

            _ioc.Rebind<PlayerCharacter>().ToConstant(player).InScope(x => GameScope.CurrentGameScope); //Register the current Player.
            _ioc.Rebind<IDynamicEntity>().ToConstant(player).InScope(x => GameScope.CurrentGameScope).Named("Player"); //Register the current Player.

            serverComponent.MessageEntityIn -= ServerConnectionMessageEntityIn;
            serverComponent.Player = player;

            GameplayComponentsCreation();
        }

        private void GameplayComponentsCreation()
        {
            //_ioc.Get<ServerComponent>().GameInformations was set by the MessageGameInformation received by the server
            WorldParameters clientSideworldParam = _ioc.Get<WorldParameters>();

            clientSideworldParam.SeedName = _ioc.Get<ServerComponent>().GameInformations.WorldSeed;
            clientSideworldParam.SeaLevel = _ioc.Get<ServerComponent>().GameInformations.WaterLevel;


            //IWorldProcessor processor1 = new s33m3WorldProcessor(clientSideworldParam);
            //IWorldProcessor processor2 = new LandscapeLayersProcessor(clientSideworldParam, _ioc.Get<EntityFactory>("Client"));
            //var worldGenerator = new WorldGenerator(clientSideworldParam, processor1, processor2);
            //_ioc.Rebind<WorldGenerator>().ToConstant(worldGenerator).InSingletonScope();

            IWorldProcessor processor = new UtopiaProcessor(clientSideworldParam);
            var worldGenerator = new WorldGenerator(clientSideworldParam, processor);
            _ioc.Rebind<WorldGenerator>().ToConstant(worldGenerator).InSingletonScope();

            var commonResources = _ioc.Get<SandboxCommonResources>();
            commonResources.LoadInventoryImages(_ioc.Get<D3DEngine>());

            // be careful with initialization order
            var serverComponent = _ioc.Get<ServerComponent>();
            var worldFocusManager = _ioc.Get<WorldFocusManager>();
            var wordParameters = _ioc.Get<WorldParameters>();
            var visualWorldParameters = _ioc.Get<VisualWorldParameters>(new ConstructorArgument("visibleChunkInWorld", new Vector2I(ClientSettings.Current.Settings.GraphicalParameters.WorldSize, ClientSettings.Current.Settings.GraphicalParameters.WorldSize)));
            var firstPersonCamera = _ioc.Get<ICamera>();
            var cameraManager = _ioc.Get<CameraManager<ICameraFocused>>();
            var timerManager = _ioc.Get<TimerManager>();
            var inputsManager = _ioc.Get<InputsManager>();
            var guiManager = _ioc.Get<GuiManager>();
            var iconFactory = _ioc.Get<IconFactory>();
            var gameClock = _ioc.Get<IClock>();
            var inventory = _ioc.Get<InventoryComponent>();
            inventory.PlayerInventoryWindow = _ioc.Get<PlayerInventory>();

            var chat = _ioc.Get<ChatComponent>();
            var hud = _ioc.Get<Hud>();
            var stars = _ioc.Get<IDrawableComponent>("Stars");
            var clouds = _ioc.Get<IDrawableComponent>("Clouds");
            var skyDome = _ioc.Get<ISkyDome>();
            var weather = _ioc.Get<IWeather>();


            var chunkStorageManager = _ioc.Get<IChunkStorageManager>(new ConstructorArgument("forceNew", false), new ConstructorArgument("fileName", _vars.LocalDataBasePath));
            var solidCubeMeshFactory = _ioc.Get<ICubeMeshFactory>("SolidCubeMeshFactory");
            var liquidCubeMeshFactory = _ioc.Get<ICubeMeshFactory>("LiquidCubeMeshFactory");
            var singleArrayChunkContainer = _ioc.Get<SingleArrayChunkContainer>();
            var landscapeManager = _ioc.Get<ILandscapeManager>();
            var lightingManager = _ioc.Get<ILightingManager>();
            var chunkMeshManager = _ioc.Get<IChunkMeshManager>();
            var worldChunks = _ioc.Get<IWorldChunks>();
            var chunksWrapper = _ioc.Get<IChunksWrapper>();
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
            var soundManager = _ioc.Get<GameSoundManager>();
            var staggingBackBuffer = _ioc.Get<StaggingBackBuffer>();

            landscapeManager.EntityFactory = _ioc.Get<EntityFactory>();
            playerEntityManager.HasMouseFocus = true;
            firstPersonCamera.CameraPlugin = playerEntityManager;
            worldFocusManager.WorldFocus = (IWorldFocus)firstPersonCamera;
            chunkEntityImpactManager.LateInitialization(serverComponent, singleArrayChunkContainer, worldChunks, chunkStorageManager, lightingManager);

            //Late Inject PlayerCharacter into VisualWorldParameters
            var c = clouds as Clouds;
            if (c != null) c.LateInitialization(sharedFrameCB);
            

            AddComponent(cameraManager);
            AddComponent(serverComponent);
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
            AddComponent(skyDome);
            AddComponent(gameClock);
            AddComponent(weather);
            AddComponent(worldChunks);
            AddComponent(sharedFrameCB);
            AddComponent(soundManager);
            AddComponent(staggingBackBuffer);

            //Will start the initialization of the newly added Components on the states, and Activate them
            StatesManager.ActivateGameStateAsync(this);           

            worldChunks.LoadComplete += worldChunks_LoadComplete;

            var engine = _ioc.Get<D3DEngine>();
            inputsManager.MouseManager.MouseCapture = true;
        }

        void worldChunks_LoadComplete(object sender, EventArgs e)
        {
            _ioc.Get<IWorldChunks>().LoadComplete -= worldChunks_LoadComplete;
            StatesManager.ActivateGameStateAsync("Gameplay");
        }

    }
}
