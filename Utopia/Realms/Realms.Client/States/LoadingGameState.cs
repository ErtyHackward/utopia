using System;
using System.IO;
using Ninject;
using Ninject.Parameters;
using Realms.Client.Components;
using Realms.Client.Components.GUI;
using Realms.Client.Components.GUI.Inventory;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.GUI;
using Utopia.GUI.CharacterSelection;
using Utopia.GUI.Crafting;
using Utopia.Network;
using Utopia.Shared.Chunks;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Net.Connections;
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
using S33M3CoreComponents.States;
using S33M3DXEngine.Threading;
using S33M3Resources.Structs;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Timers;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.GUI;
using Utopia.GUI.Inventory;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine;
using Utopia.Components;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Worlds.SkyDomes.SharedComp;
using Utopia.Shared.World.Processors.Utopia;
using Utopia.Shared.World.Processors;
using S33M3CoreComponents.Particules;
using Utopia.Particules;
using Utopia.Sounds;
using Utopia.Shared.LandscapeEntities;
using Utopia.Worlds.Shadows;
using Utopia.PostEffects;
using Utopia.GUI.WindRose;

namespace Realms.Client.States
{
    /// <summary>
    /// Main gameplay stuff. Displaying the chunks, an entities, handling an input
    /// </summary>
    public class LoadingGameState : GameState
    {
        private readonly IKernel _ioc;
        private RealmRuntimeVariables _vars;
        
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
            if (PreviousGameState != this) GameComponents.Clear();

            var loading = _ioc.Get<LoadingComponent>();
            _vars = _ioc.Get<RealmRuntimeVariables>();
            
            AddComponent(loading); //Will "Mask" the Components being loaded.
            AddComponent(_ioc.Get<ServerComponent>());
            AddComponent(_ioc.Get<GuiManager>());

            base.Initialize(context);
        }

        //The state is enabled, start loading other components in background while the Loading is shown
        public override void OnEnabled(GameState previousState)
        {
            if (PreviousGameState != this) 
                ThreadsManager.RunAsync(GameplayInitializeAsync);

            var serverComponent = _ioc.Get<ServerComponent>();
            serverComponent.ConnectionStatusChanged += serverComponent_ConnectionStausChanged;

            base.OnEnabled(previousState);
        }

        public override void OnDisabled(GameState nextState)
        {
            var serverComponent = _ioc.Get<ServerComponent>();
            serverComponent.ConnectionStatusChanged -= serverComponent_ConnectionStausChanged;

            base.OnDisabled(nextState);
        }

        void serverComponent_ConnectionStausChanged(object sender, ServerConnectionStatusEventArgs e)
        {
            if (e.Status == TcpConnectionStatus.Disconnected && e.Final)
            {
                var serverComponent = _ioc.Get<ServerComponent>();
                var guiManager = _ioc.Get<GuiManager>();
                guiManager.MessageBox("Can't connect to the server. " + serverComponent.LastErrorText, "error");
                StatesManager.ActivateGameStateAsync("MainMenu");
            }
        }

        private void GameplayInitializeAsync()
        {
            var serverComponent = _ioc.Get<ServerComponent>();

            if (_vars.SinglePlayer)
            {
                var wp = _ioc.Get<WorldParameters>();

                AbstractChunk.SetChunkHeight(wp.Configuration.WorldHeight);

                //wp not initialized ==> We are in "SandBox" mode, load from the "Utopia SandBox" Default WorldParameters
                if (wp.SeedName == null)
                {
                    wp.WorldName = "SandBox World";
                    wp.SeedName = "Utopia SandBox";
                }

                //Create a local server for single player purpose
                if (_vars.LocalServer == null || _vars.LocalServer.IsDisposed) 
                    _vars.LocalServer = _ioc.Get<LocalServer>();

                //Passed the WorldParameters to the server for single use purpose mode
                _vars.LocalServer.InitSinglePlayerServer(wp);

                if (serverComponent.ServerConnection == null ||
                    serverComponent.ServerConnection.Status != TcpConnectionStatus.Connected)
                {
                    serverComponent.MessageEntityIn += ServerConnectionMessageEntityIn;
                    var port = _vars.LocalServer.Server.SettingsManager.Settings.ServerPort;
                    serverComponent.BindingServer("127.0.0.1" + (port == 4815 ? "" : ":" + port), null);
                    serverComponent.ConnectToServer("local", _vars.DisplayName, "qwe123".GetSHA1Hash());
                    _vars.LocalDataBasePath = Path.Combine(_vars.ApplicationDataPath, "Client", "Singleplayer", wp.WorldName, "ClientWorldCache.db");
                }
            }
            else
            {
                if (serverComponent.ServerConnection == null ||
                    serverComponent.ServerConnection.Status != TcpConnectionStatus.Connected)
                {
                    serverComponent.MessageEntityIn += ServerConnectionMessageEntityIn;
                    serverComponent.BindingServer(_vars.CurrentServerAddress, _vars.CurrentServerLocalAddress);

                    // take server address without port to create server password hash
                    var srvAddr = _vars.CurrentServerAddress.Contains(":")
                                      ? _vars.CurrentServerAddress.Substring(0, _vars.CurrentServerAddress.IndexOf(':'))
                                      : _vars.CurrentServerAddress;

                    var userHash = ( _vars.PasswordHash + _vars.Login.ToLower() ).GetSHA1Hash();

                    serverComponent.ConnectToServer(_vars.Login, _vars.DisplayName, (srvAddr + userHash).GetSHA1Hash());
                    _vars.LocalDataBasePath = Path.Combine(_vars.ApplicationDataPath, _vars.Login, "Client", "Multiplayer", _vars.CurrentServerAddress.Replace(':', '_'), "ClientWorldCache.db");
                }
            }
        }
        
        void ServerConnectionMessageEntityIn(object sender, ProtocolMessageEventArgs<Utopia.Shared.Net.Messages.EntityInMessage> e)
        {
            var serverComponent = _ioc.Get<ServerComponent>();
            
            serverComponent.MessageEntityIn -= ServerConnectionMessageEntityIn;
            serverComponent.Player = (IDynamicEntity)e.Message.Entity;

            var factory = _ioc.Get<EntityFactory>();
            factory.PrepareEntity(serverComponent.Player);

            GameplayComponentsCreation();
        }

        private void GameplayComponentsCreation()
        {
            _vars.DisposeGameComponents = true;

            var clientSideworldParam = _ioc.Get<ServerComponent>().GameInformations.WorldParameter;

            var clientFactory = _ioc.Get<EntityFactory>("Client");
            clientFactory.Config = clientSideworldParam.Configuration;
            
            var landscapeEntityManager = _ioc.Get<LandscapeBufferManager>();
            FileInfo fi = new FileInfo(_vars.LocalDataBasePath);
            string bufferPath = Path.Combine(fi.Directory.FullName, "LandscapeBuffer.proto");
            landscapeEntityManager.SetBufferPath(bufferPath);
            landscapeEntityManager.LoadBuffer();

            IWorldProcessor processor = null;
            switch (clientSideworldParam.Configuration.WorldProcessor)
            {
                case WorldConfiguration.WorldProcessors.Flat:
                    processor = new FlatWorldProcessor();
                    break;
                case WorldConfiguration.WorldProcessors.Utopia:
                    processor = new UtopiaProcessor(clientSideworldParam, _ioc.Get<EntityFactory>("Client"), landscapeEntityManager);
                    break;
                default:
                    break;
            }

            _ioc.Rebind<WorldConfiguration>().ToConstant(clientSideworldParam.Configuration);

            var worldGenerator = new WorldGenerator(clientSideworldParam, processor);
            _ioc.Rebind<WorldGenerator>().ToConstant(worldGenerator).InSingletonScope();
            _ioc.Rebind<WorldParameters>().ToConstant(clientSideworldParam);

            var commonResources = _ioc.Get<SandboxCommonResources>();
            commonResources.LoadInventoryImages(_ioc.Get<D3DEngine>());

            // be careful with initialization order
            var serverComponent = _ioc.Get<ServerComponent>();
            var worldFocusManager = _ioc.Get<WorldFocusManager>();
            var wordParameters = _ioc.Get<WorldParameters>();
            var visualWorldParameters = _ioc.Get<VisualWorldParameters>(
                new ConstructorArgument("visibleChunkInWorld", new Vector2I(ClientSettings.Current.Settings.GraphicalParameters.WorldSize, ClientSettings.Current.Settings.GraphicalParameters.WorldSize)),
                new ConstructorArgument("player", serverComponent.Player));
            
            var firstPersonCamera = _ioc.Get<ICameraFocused>("FirstPCamera");
            var thirdPersonCamera = _ioc.Get<ICameraFocused>("ThirdPCamera");
            var cameraManager = _ioc.Get<CameraManager<ICameraFocused>>(new ConstructorArgument("camera", firstPersonCamera));
            cameraManager.RegisterNewCamera(thirdPersonCamera);

            var timerManager = _ioc.Get<TimerManager>();
            var inputsManager = _ioc.Get<InputsManager>();
            var guiManager = _ioc.Get<GuiManager>();
            var iconFactory = _ioc.Get<IconFactory>();
            var gameClock = _ioc.Get<IClock>();
            var chunkStorageManager = _ioc.Get<IChunkStorageManager>(new ConstructorArgument("forceNew", false), new ConstructorArgument("fileName", _vars.LocalDataBasePath));
            
            var inventory = _ioc.Get<InventoryComponent>();
            var windrose = _ioc.Get<WindRoseComponent>();
            inventory.PlayerInventoryWindow = _ioc.Get<PlayerInventory>();
            inventory.ContainerInventoryWindow = _ioc.Get<ContainerInventory>();
            
            var skyBackBuffer = _ioc.Get<StaggingBackBuffer>("SkyBuffer");
            skyBackBuffer.DrawOrders.UpdateIndex(0, 50, "SkyBuffer");

            var chat = _ioc.Get<ChatComponent>();
            var hud = _ioc.Get<Hud>();
            var stars = _ioc.Get<IDrawableComponent>("Stars");
            var clouds = _ioc.Get<IDrawableComponent>("Clouds");
            var skyDome = _ioc.Get<ISkyDome>();
            var weather = _ioc.Get<IWeather>();
            
            var solidCubeMeshFactory = _ioc.Get<ICubeMeshFactory>("SolidCubeMeshFactory");
            var liquidCubeMeshFactory = _ioc.Get<ICubeMeshFactory>("LiquidCubeMeshFactory");
            var singleArrayChunkContainer = _ioc.Get<SingleArrayChunkContainer>();
            var landscapeManager = _ioc.Get<ILandscapeManager2D>();
            var lightingManager = _ioc.Get<ILightingManager>();
            var chunkMeshManager = _ioc.Get<IChunkMeshManager>();
            var worldChunks = _ioc.Get<IWorldChunks>();
            var worldShadowMap = ClientSettings.Current.Settings.GraphicalParameters.ShadowMap ? _ioc.Get<WorldShadowMap>() : null;
            var chunksWrapper = _ioc.Get<IChunksWrapper>();
            var fadeComponent = _ioc.Get<FadeComponent>();
            var pickingRenderer = _ioc.Get<IPickingRenderer>();
            var playerEntityManager = (PlayerEntityManager)_ioc.Get<IPlayerManager>();
            var selectedBlocksRenderer = _ioc.Get<SelectedBlocksRenderer>();
            var chunkEntityImpactManager = _ioc.Get<IChunkEntityImpactManager>();
            var entityPickingManager = _ioc.Get<IEntityPickingManager>();
            var dynamicEntityManager = _ioc.Get<IVisualDynamicEntityManager>();
            var voxelMeshFactory = _ioc.Get<VoxelMeshFactory>();
            var sharedFrameCB = _ioc.Get<SharedFrameCB>();
            var itemMessageTranslator = _ioc.Get<ItemMessageTranslator>();
            var entityMessageTranslator = _ioc.Get<EntityMessageTranslator>();
            var soundManager = _ioc.Get<GameSoundManager>();
            var voxelModelManager = _ioc.Get<VoxelModelManager>();
            var toolRenderer = _ioc.Get<FirstPersonToolRenderer>();
            var particuleEngine = _ioc.Get<UtopiaParticuleEngine>();
            var ghostedRenderer = _ioc.Get<GhostedEntityRenderer>();
            var crafting = _ioc.Get<CraftingComponent>();
            var charSelection = _ioc.Get<CharacterSelectionComponent>();
            var inventoryEvents = _ioc.Get<InventoryEventComponent>();
            var cracksRenderer = _ioc.Get<CracksRenderer>();
            var postEffectComponent = _ioc.Get<PostEffectComponent>();

            //Assign the various Post Processing effect to the component
            //Ghost PostEffect
            IPostEffect ghost = new PostEffectGhost() { Name = "Dead"};
            postEffectComponent.RegisteredEffects.Add(ghost.Name, ghost);

            landscapeManager.EntityFactory = clientFactory;
            playerEntityManager.HasMouseFocus = true;
            cameraManager.SetCamerasPlugin(playerEntityManager);
            ((ThirdPersonCameraWithFocus)thirdPersonCamera).CheckCamera += worldChunks.ValidatePosition;
            chunkEntityImpactManager.LateInitialization(serverComponent, singleArrayChunkContainer, worldChunks, chunkStorageManager, lightingManager, visualWorldParameters);
            
            clientFactory.DynamicEntityManager = _ioc.Get<IVisualDynamicEntityManager>();
            clientFactory.GlobalStateManager = _ioc.Get<IGlobalStateManager>();

            //Late Inject PlayerCharacter into VisualWorldParameters
            var c = clouds as Clouds;
            if (c != null) c.LateInitialization(sharedFrameCB);

            AddComponent(cameraManager);
            AddComponent(serverComponent);
            AddComponent(inputsManager);
            AddComponent(iconFactory);
            AddComponent(timerManager);
            AddComponent(skyBackBuffer);
            AddComponent(playerEntityManager);
            AddComponent(dynamicEntityManager);
            AddComponent(hud);
            AddComponent(guiManager);
            AddComponent(pickingRenderer);
            AddComponent(inventory);
            AddComponent(windrose);
            AddComponent(chat);
            AddComponent(skyDome);
            AddComponent(gameClock);
            AddComponent(weather);
            AddComponent(worldChunks);
            AddComponent(sharedFrameCB);
            AddComponent(soundManager);
            AddComponent(voxelModelManager);
            AddComponent(toolRenderer);
            AddComponent(fadeComponent);
            AddComponent(particuleEngine);
            AddComponent(ghostedRenderer);
            AddComponent(crafting);
            AddComponent(inventoryEvents);
            AddComponent(cracksRenderer);
            AddComponent(charSelection);
            AddComponent(postEffectComponent);

            if (ClientSettings.Current.Settings.GraphicalParameters.ShadowMap)
                AddComponent(worldShadowMap);

            //Will start the initialization of the newly added Components on the states, and Activate them
            StatesManager.ActivateGameStateAsync(this);           

            worldChunks.LoadComplete += worldChunks_LoadComplete;

            var engine = _ioc.Get<D3DEngine>();
            inputsManager.MouseManager.MouseCapture = true;
        }

        //All chunks have been created on the client (They can be rendered)
        void worldChunks_LoadComplete(object sender, EventArgs e)
        {
            _ioc.Get<IWorldChunks>().LoadComplete -= worldChunks_LoadComplete;
            StatesManager.ActivateGameStateAsync("Gameplay");

            //Say to server that the loading phase is finished inside the client
            _ioc.Get<ServerComponent>().EnterTheWorld();

            //Start a client chunk resync phase.
            _ioc.Get<IWorldChunks>().ResyncClientChunks();
        }

    }
}
