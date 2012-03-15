using Ninject;
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

namespace Sandbox.Client.States
{
    public class GamePlayState : GameState
    {
        private readonly IKernel _ioc;

        public override string Name
        {
            get { return "Gameplay"; }
        }

        public GamePlayState(GameStatesManager stateManager, IKernel ioc)
            :base(stateManager)
        {
            _ioc = ioc;
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
            var map = _ioc.Get<MapComponent>();
            var hud = _ioc.Get<Hud>();
            var skyDome = _ioc.Get<ISkyDome>();
            var weather = _ioc.Get<IWeather>();
            var worldChunks = _ioc.Get<IWorldChunks>();
            var pickingRenderer = _ioc.Get<IPickingRenderer>();
            var dynamicEntityManager = _ioc.Get<IDynamicEntityManager>();
            var playerEntityManager = _ioc.Get<PlayerEntityManager>();
            var sharedFrameCB = _ioc.Get<SharedFrameCB>();
            var soundManager = _ioc.Get<SoundManager>();
            var staggingBackBuffer = _ioc.Get<StaggingBackBuffer>();

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
            //AddComponent(entityEditor);
            //AddComponent(carvingEditor);
            AddComponent(skyDome);
            AddComponent(gameClock);
            AddComponent(weather);
            AddComponent(worldChunks);
            AddComponent(sharedFrameCB);
            AddComponent(staggingBackBuffer);
            AddComponent(soundManager);

            base.Initialize(context);
        }

        public override void OnEnabled(GameState previousState)
        {
            base.OnEnabled(previousState);
        }
    }
}
