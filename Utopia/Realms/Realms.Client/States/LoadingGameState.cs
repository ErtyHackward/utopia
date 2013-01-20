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
using Utopia.GUI.Crafting;
using Utopia.Network;
using Utopia.Shared.Chunks;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
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

namespace Realms.Client.States
{
    /// <summary>
    /// Main gameplay stuff. Displaying the chunks, an entities, handling an input
    /// </summary>
    public class LoadingGameState : GameState
    {
        private readonly IKernel _ioc;
        private RuntimeVariables _vars;
        
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
            _vars = _ioc.Get<RuntimeVariables>();
            
            AddComponent(loading); //Will "Mask" the Components being loaded.
            AddComponent(_ioc.Get<ServerComponent>());
            AddComponent(_ioc.Get<GuiManager>());

            base.Initialize(context);
        }

        //The state is enabled, start loading other components in background while the Loading is shown
        public override void OnEnabled(GameState previousState)
        {
            if (this.PreviousGameState != this) ThreadsManager.RunAsync(GameplayInitializeAsync);

            base.OnEnabled(previousState);
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
                    serverComponent.ServerConnection.Status != Utopia.Shared.Net.Connections.TcpConnectionStatus.Connected)
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
                    serverComponent.ServerConnection.Status != Utopia.Shared.Net.Connections.TcpConnectionStatus.Connected)
                {
                    serverComponent.MessageEntityIn += ServerConnectionMessageEntityIn;
                    serverComponent.BindingServer(_vars.CurrentServerAddress);
                    serverComponent.ConnectToServer(_vars.Login, _vars.DisplayName, _vars.PasswordHash);
                    _vars.LocalDataBasePath = Path.Combine(_vars.ApplicationDataPath, _vars.Login, "Client", "Multiplayer", _vars.CurrentServerAddress.Replace(':', '_'), "ClientWorldCache.db");
                }
            }
        }
        
        void ServerConnectionMessageEntityIn(object sender, Utopia.Shared.Net.Connections.ProtocolMessageEventArgs<Utopia.Shared.Net.Messages.EntityInMessage> e)
        {
            ServerComponent serverComponent = _ioc.Get<ServerComponent>();

            var player = (PlayerCharacter)e.Message.Entity;

            var factory = _ioc.Get<EntityFactory>();
            factory.PrepareEntity(player);

            _ioc.Rebind<PlayerCharacter>().ToConstant(player).InScope(x => GameScope.CurrentGameScope); //Register the current Player.
            _ioc.Rebind<IDynamicEntity>().ToConstant(player).InScope(x => GameScope.CurrentGameScope).Named("Player"); //Register the current Player.

            serverComponent.MessageEntityIn -= ServerConnectionMessageEntityIn;
            serverComponent.Player = player;

            GameplayComponentsCreation();
        }

        private void GameplayComponentsCreation()
        {
            //_ioc.Get<ServerComponent>().GameInformations is set by the MessageGameInformation received by the server
            WorldParameters clientSideworldParam = _ioc.Get<WorldParameters>();

            clientSideworldParam = _ioc.Get<ServerComponent>().GameInformations.WorldParameter;
            _ioc.Get<EntityFactory>("Client").Config = clientSideworldParam.Configuration;

            IWorldProcessor processor = null;
            switch (clientSideworldParam.Configuration.WorldProcessor)
            {
                case Utopia.Shared.Configuration.WorldConfiguration.WorldProcessors.Flat:
                    processor = new FlatWorldProcessor();
                    break;
                case Utopia.Shared.Configuration.WorldConfiguration.WorldProcessors.Utopia:
                    processor = new UtopiaProcessor(clientSideworldParam, _ioc.Get<EntityFactory>("Client"));
                    break;
                default:
                    break;
            }

            _ioc.Rebind<WorldConfiguration>().ToConstant(clientSideworldParam.Configuration);

            var worldGenerator = new WorldGenerator(clientSideworldParam, processor);
            _ioc.Rebind<WorldGenerator>().ToConstant(worldGenerator).InSingletonScope();

            var commonResources = _ioc.Get<SandboxCommonResources>();
            commonResources.LoadInventoryImages(_ioc.Get<D3DEngine>());

            // be careful with initialization order
            var serverComponent = _ioc.Get<ServerComponent>();
            var worldFocusManager = _ioc.Get<WorldFocusManager>();
            var wordParameters = _ioc.Get<WorldParameters>();
            var visualWorldParameters = _ioc.Get<VisualWorldParameters>(new ConstructorArgument("visibleChunkInWorld", new Vector2I(ClientSettings.Current.Settings.GraphicalParameters.WorldSize, ClientSettings.Current.Settings.GraphicalParameters.WorldSize)));
            
            var firstPersonCamera = _ioc.Get<ICameraFocused>("FirstPCamera");
            var thirdPersonCamera = _ioc.Get<ICameraFocused>("ThirdPCamera");
            var cameraManager = _ioc.Get<CameraManager<ICameraFocused>>();
            cameraManager.RegisterNewCamera(firstPersonCamera);
            cameraManager.RegisterNewCamera(thirdPersonCamera);

            var timerManager = _ioc.Get<TimerManager>();
            var inputsManager = _ioc.Get<InputsManager>();
            var guiManager = _ioc.Get<GuiManager>();
            var iconFactory = _ioc.Get<IconFactory>();
            var gameClock = _ioc.Get<IClock>();
            var chunkStorageManager = _ioc.Get<IChunkStorageManager>(new ConstructorArgument("forceNew", false), new ConstructorArgument("fileName", _vars.LocalDataBasePath));
            var inventory = _ioc.Get<InventoryComponent>();
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
            var landscapeManager = _ioc.Get<ILandscapeManager>();
            var lightingManager = _ioc.Get<ILightingManager>();
            var chunkMeshManager = _ioc.Get<IChunkMeshManager>();
            var worldChunks = _ioc.Get<IWorldChunks>();
            var chunksWrapper = _ioc.Get<IChunksWrapper>();
            var fadeComponent = _ioc.Get<FadeComponent>();
            fadeComponent.Visible = false;
            var pickingRenderer = _ioc.Get<IPickingRenderer>();
            var chunkEntityImpactManager = _ioc.Get<IChunkEntityImpactManager>();
            var entityPickingManager = _ioc.Get<IEntityPickingManager>();
            var dynamicEntityManager = _ioc.Get<IDynamicEntityManager>();
            var playerEntityManager = _ioc.Get<PlayerEntityManager>();
            var playerCharacter = _ioc.Get<PlayerCharacter>();
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

            landscapeManager.EntityFactory = _ioc.Get<EntityFactory>();
            playerEntityManager.HasMouseFocus = true;
            cameraManager.SetCamerasPlugin(playerEntityManager);
            ((ThirdPersonCameraWithFocus)thirdPersonCamera).CheckCamera += worldChunks.ValidatePosition;
            chunkEntityImpactManager.LateInitialization(serverComponent, singleArrayChunkContainer, worldChunks, chunkStorageManager, lightingManager, visualWorldParameters);

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
