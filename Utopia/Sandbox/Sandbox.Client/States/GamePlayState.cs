﻿using Ninject;
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
using S33M3_CoreComponents.States;
using S33M3_CoreComponents.Cameras;
using S33M3_CoreComponents.Timers;
using S33M3_CoreComponents.Inputs;
using S33M3_CoreComponents.GUI;

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

        public override void Initialize()
        {
            var cameraManager = _ioc.Get<CameraManager>();
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
            //AddComponent(entityEditor);
            //AddComponent(carvingEditor);
            AddComponent(skyDome);
            AddComponent(gameClock);
            AddComponent(weather);
            AddComponent(worldChunks);
            AddComponent(sharedFrameCB);
            AddComponent(debugInfo);
        }

        public override void OnEnabled(GameState previousState)
        {
            var playerEntityManager = _ioc.Get<PlayerEntityManager>();
            playerEntityManager.EnableComponent();

            var debugInfo = _ioc.Get<DebugInfo>();
            debugInfo.Activated = true;
            debugInfo.SetComponants(
                _ioc.Get<FPS>(),
                _ioc.Get<IClock>(),
                _ioc.Get<IWorldChunks>(),
                _ioc.Get<PlayerEntityManager>(),
                _ioc.Get<GuiManager>()
                );

            base.OnEnabled(previousState);
        }
    }
}
