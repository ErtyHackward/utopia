using Ninject;
using S33M3DXEngine;
using Sandbox.Client.Components;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.GUI;
using Utopia.Network;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.Weather;
using S33M3CoreComponents.States;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Timers;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.GUI;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.GUI.Inventory;
using Utopia.Components;
using Utopia.Shared.Settings;
using System.Linq;
using Utopia.Shared.Configuration;
using S33M3CoreComponents.Particules;
using Utopia.Particules;
using Utopia.Sounds;
using Utopia.Shared.World;

namespace Sandbox.Client.States
{
    public class GamePlayState : GameState
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private SandboxGameSoundManager _sandboxGameSoundManager;

        private readonly IKernel _ioc;

        public override string Name
        {
            get { return "Gameplay"; }
        }

        public GamePlayState(GameStatesManager stateManager, IKernel ioc)
            :base(stateManager)
        {
            _ioc = ioc;
            AllowMouseCaptureChange = true;
        }

        public override void Initialize(DeviceContext context)
        {
            var cameraManager = _ioc.Get<CameraManager<ICameraFocused>>();
            var timerManager = _ioc.Get<TimerManager>();
            var inputsManager = _ioc.Get<InputsManager>();
            var guiManager = _ioc.Get<GuiManager>();
            var iconFactory = _ioc.Get<IconFactory>();
            var gameClock = _ioc.Get<IClock>();
            var inventory = _ioc.Get<InventoryComponent>();
            inventory.SwitchInventory += InventorySwitchInventory;
            var chat = _ioc.Get<ChatComponent>();
            var hud = _ioc.Get<Hud>();

            var skyBackBuffer = _ioc.Get<StaggingBackBuffer>("SkyBuffer");
            skyBackBuffer.DrawOrders.UpdateIndex(0, 50, "SkyBuffer");
            if (ClientSettings.Current.Settings.GraphicalParameters.LandscapeFog == "SkyFog")
            {
                skyBackBuffer.EnableComponent(true);
            }

            var skyDome = _ioc.Get<ISkyDome>();

            //Rendering time changed depending on landscape fog option, TRUE = Faster drawing (Because we are actively using depth testing)
            if (ClientSettings.Current.Settings.GraphicalParameters.LandscapeFog == "SkyFog")
            {
                skyDome.DrawOrders.UpdateIndex(0, 40);
            }
            else
            {
                skyDome.DrawOrders.UpdateIndex(0, 990);
            }

            var weather = _ioc.Get<IWeather>();
            var worldChunks = _ioc.Get<IWorldChunks>();
            var pickingRenderer = _ioc.Get<IPickingRenderer>();
            var dynamicEntityManager = _ioc.Get<IVisualDynamicEntityManager>();
            var playerEntityManager = _ioc.Get<PlayerEntityManager>();
            var sharedFrameCB = _ioc.Get<SharedFrameCB>();

            _sandboxGameSoundManager = (SandboxGameSoundManager)_ioc.Get<GameSoundManager>();
            var serverComponent = _ioc.Get<ServerComponent>();
            var fadeComponent = _ioc.Get<FadeComponent>();
            fadeComponent.Visible = false;
            var voxelModelManager = _ioc.Get<VoxelModelManager>();
            var toolRenderer = _ioc.Get<FirstPersonToolRenderer>();
            var particuleEngine = _ioc.Get<UtopiaParticuleEngine>();
            var ghostedRenderer = _ioc.Get<GhostedEntityRenderer>();
            var landscapeEntityManager = _ioc.Get<LandscapeBufferManager>();

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
            AddComponent(_sandboxGameSoundManager);
            AddComponent(fadeComponent);
            AddComponent(voxelModelManager);
            AddComponent(toolRenderer);
            AddComponent(particuleEngine);
            AddComponent(ghostedRenderer);

#if DEBUG
            //Check if the GamePlay Components equal those that have been loaded inside the LoadingGameState
            foreach (var gc in _ioc.Get<LoadingGameState>().GameComponents.Except(GameComponents))
            {
                if (gc.GetType() != typeof(Sandbox.Client.Components.GUI.LoadingComponent))
                {
                    logger.Warn("Missing GamePlayState component, present inside LoadingGameState : {0}", gc.GetType().ToString());
                }
            }

            //Check if the GamePlay Components equal those that have been loaded inside the LoadingGameState
            foreach (var gc in GameComponents.Except(_ioc.Get<LoadingGameState>().GameComponents))
            {
                if (gc.GetType() != typeof(Sandbox.Client.Components.GUI.LoadingComponent))
                {
                    logger.Warn("Missing LoadingGameState component, present inside GamePlayState : {0}", gc.GetType().ToString());
                }
            }

#endif
            base.Initialize(context);
        }
        

        void InventorySwitchInventory(object sender, InventorySwitchEventArgs e)
        {
            var inventory = _ioc.Get<InventoryComponent>();
            var fadeComponent = _ioc.Get<FadeComponent>();
            fadeComponent.Color = new SharpDX.Color4(0, 0, 0, 0.85f);
            if (e.Closing)
            {
                fadeComponent.Visible = false;
            }
            else
            {
                fadeComponent.Visible = true;
            }
        }

        public override void OnEnabled(GameState previousState)
        {
            var guiManager = _ioc.Get<GuiManager>();
            guiManager.Screen.ShowAll();
            var fadeComponent = _ioc.Get<FadeComponent>();
            var inventory = _ioc.Get<InventoryComponent>();
            fadeComponent.Visible = inventory.IsActive;

            base.OnEnabled(previousState);
        }
    }
}
