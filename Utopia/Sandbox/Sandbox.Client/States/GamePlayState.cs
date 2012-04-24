using Ninject;
using S33M3DXEngine;
using Sandbox.Client.Components;
using Utopia;
using Utopia.Effects.Shared;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.GUI;
using Utopia.Network;
using Utopia.Worlds.Chunks;
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
using Utopia.GUI.Map;
using S33M3CoreComponents.Debug;
using Utopia.Components;
using Utopia.Shared.Settings;
using System.Linq;
using Sandbox.Client.Components.GUI;

namespace Sandbox.Client.States
{
    public class GamePlayState : GameState
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
            var chat = _ioc.Get<ChatComponent>();
            var hud = _ioc.Get<Hud>();
            var skyDome = _ioc.Get<ISkyDome>();
            var weather = _ioc.Get<IWeather>();
            var worldChunks = _ioc.Get<IWorldChunks>();
            var pickingRenderer = _ioc.Get<IPickingRenderer>();
            var dynamicEntityManager = _ioc.Get<IDynamicEntityManager>();
            var playerEntityManager = _ioc.Get<PlayerEntityManager>();
            var sharedFrameCB = _ioc.Get<SharedFrameCB>();
            var staggingBackBuffer = _ioc.Get<StaggingBackBuffer>();
            var soundManager = _ioc.Get<GameSoundManager>();
            var serverComponent = _ioc.Get<ServerComponent>();

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

            chat.MessageOut += ChatMessageOut;

            base.Initialize(context);
        }

        void ChatMessageOut(object sender, ChatMessageEventArgs e)
        {
            if (e.Message == "/reloadtex")
            {
                e.DoNotSend = true;
                var worldChunks = _ioc.Get<IWorldChunks>();

                //Refresh the texture pack values
                TexturePackConfig.Current.Load();

                worldChunks.InitDrawComponents(_ioc.Get<D3DEngine>().ImmediateContext);
            }
        }

        public override void OnEnabled(GameState previousState)
        {
            var guiManager = _ioc.Get<GuiManager>();
            guiManager.Screen.ShowAll();
            base.OnEnabled(previousState);
        }
    }
}
