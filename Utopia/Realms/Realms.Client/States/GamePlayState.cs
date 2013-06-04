using System;
using System.Linq;
using Ninject;
using Realms.Client.Components;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.Inputs.Actions;
using Utopia.Action;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.GUI;
using Utopia.GUI.Crafting;
using Utopia.Network;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
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
using Utopia.Components;
using Utopia.Shared.Settings;
using Utopia.Particules;
using Utopia.Sounds;
using Utopia.Shared.World;

namespace Realms.Client.States
{
    public class GamePlayState : GameState
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private RealmGameSoundManager _sandboxGameSoundManager;

        private readonly IKernel _ioc;

        public override string Name
        {
            get { return "Gameplay"; }
        }

        public GamePlayState(GameStatesManager stateManager, IKernel ioc)
            :base(stateManager)
        {
            _ioc = ioc;
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(DeviceContext context)
        {
            var cameraManager = _ioc.Get<CameraManager<ICameraFocused>>();
            var timerManager = _ioc.Get<TimerManager>();
            var inputsManager = _ioc.Get<InputsManager>();
            inputsManager.ActionsManager.KeyboardAction += ActionsManager_KeyboardAction;

            var guiManager = _ioc.Get<GuiManager>();
            var iconFactory = _ioc.Get<IconFactory>();
            var gameClock = _ioc.Get<IClock>();
            //var inventory = _ioc.Get<InventoryComponent>();
            
            
            var chat = _ioc.Get<ChatComponent>();

            var hud = (RealmsHud)_ioc.Get<Hud>();
            //hud.CraftingButton.Pressed += CraftingButton_Pressed;
            //hud.InventoryButton.Pressed += InventoryButton_Pressed;

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
            var selectedBlocksRenderer = _ioc.Get<SelectedBlocksRenderer>();
            var dynamicEntityManager = _ioc.Get<IVisualDynamicEntityManager>();
            //var playerEntityManager = _ioc.Get<PlayerEntityManager>();
            //playerEntityManager.Player.Inventory.ItemPut += InventoryOnItemPut;
            //playerEntityManager.Player.Inventory.ItemTaken += InventoryOnItemTaken;
            var playerEntityManager = _ioc.Get<IPlayerManager>();

            var sharedFrameCB = _ioc.Get<SharedFrameCB>();

            _sandboxGameSoundManager = (RealmGameSoundManager)_ioc.Get<GameSoundManager>();
            var serverComponent = _ioc.Get<ServerComponent>();
            var fadeComponent = _ioc.Get<FadeComponent>();
            fadeComponent.Visible = false;
            var voxelModelManager = _ioc.Get<VoxelModelManager>();
            var adminConsole = _ioc.Get<AdminConsole>();
            var toolRenderer = _ioc.Get<FirstPersonToolRenderer>();
            var particuleEngine = _ioc.Get<UtopiaParticuleEngine>();
            var ghostedRenderer = _ioc.Get<GhostedEntityRenderer>();
            var crafting = _ioc.Get<CraftingComponent>();
            var inventoryEvents = _ioc.Get<InventoryEventComponent>();
            var pickingManager = _ioc.Get<PickingManager>();
            var cracksRenderer = _ioc.Get<CracksRenderer>();
            
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
            AddComponent(selectedBlocksRenderer);
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
            AddComponent(crafting);
            AddComponent(inventoryEvents);
            AddComponent(pickingManager);
            AddComponent(cracksRenderer);
            
            inputsManager.MouseManager.StrategyMode = true;

#if DEBUG
            //Check if the GamePlay Components equal those that have been loaded inside the LoadingGameState
            foreach (var gc in _ioc.Get<LoadingGameState>().GameComponents.Except(GameComponents))
            {
                if (gc.GetType() != typeof(Realms.Client.Components.GUI.LoadingComponent))
                {
                    logger.Warn("Missing GamePlayState component, present inside LoadingGameState : {0}", gc.GetType().ToString());
                }
            }

            //Check if the GamePlay Components equal those that have been loaded inside the LoadingGameState
            foreach (var gc in GameComponents.Except(_ioc.Get<LoadingGameState>().GameComponents))
            {
                if (gc.GetType() != typeof(Realms.Client.Components.GUI.LoadingComponent))
                {
                    logger.Warn("Missing LoadingGameState component, present inside GamePlayState : {0}", gc.GetType().ToString());
                }
            }

#endif
            base.Initialize(context);
        }

        void InventoryButton_Pressed(object sender, EventArgs e)
        {
            ActionsManager_KeyboardAction(null, new ActionsManagerEventArgs { Action = new KeyboardTriggeredAction { ActionId = UtopiaActions.OpenInventory } });
        }

        void CraftingButton_Pressed(object sender, EventArgs e)
        {
            ActionsManager_KeyboardAction(null, new ActionsManagerEventArgs { Action = new KeyboardTriggeredAction { ActionId = UtopiaActions.OpenCrafting } });
        }

        private void InventoryOnItemTaken(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            var iec = _ioc.Get<InventoryEventComponent>();
            iec.Notify(e.Slot.Item, e.Slot.Item.Name + " removed " + (e.Slot.ItemsCount > 1 ? "x" + e.Slot.ItemsCount : ""), false);
        }

        private void InventoryOnItemPut(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            var iec = _ioc.Get<InventoryEventComponent>();
            iec.Notify(e.Slot.Item, e.Slot.Item.Name + " added " + (e.Slot.ItemsCount > 1 ? "x" + e.Slot.ItemsCount : ""), true);
        }

        void ActionsManager_KeyboardAction(object sender, S33M3CoreComponents.Inputs.Actions.ActionsManagerEventArgs e)
        {
            if (StatesManager.CurrentState.Name == "Settings") 
                return;

            if (e.Action.ActionId == Actions.EngineExit)
            {
                if (StatesManager.CurrentState.Name != "InGameMenu")
                {
                    StatesManager.ActivateGameStateAsync("InGameMenu", true);
                }
                else
                {
                    StatesManager.ActivateGameStateAsync("Gameplay");
                }
                return;
            }

            if (StatesManager.CurrentState.Name == "InGameMenu") 
                return;

            if (e.Action.ActionId == UtopiaActions.OpenInventory)
            {
                if (StatesManager.CurrentState.Name != "Inventory")
                {
                    StatesManager.ActivateGameStateAsync("Inventory", true);
                }
                else
                {
                    StatesManager.ActivateGameStateAsync("Gameplay");
                }
            }

            if (e.Action.ActionId == UtopiaActions.OpenCrafting)
            {
                if (StatesManager.CurrentState.Name != "Crafting")
                {
                    StatesManager.ActivateGameStateAsync("Crafting", true);
                }
                else
                {
                    StatesManager.ActivateGameStateAsync("Gameplay");
                }
            }
        }

        public override void OnEnabled(GameState previousState)
        {
            var guiManager = _ioc.Get<GuiManager>();
            guiManager.Screen.ShowAll();

            var playerEntityManager = _ioc.Get<IPlayerManager>();
            playerEntityManager.EnableComponent();

            base.OnEnabled(previousState);
        }

        public override void OnDisabled(GameState nextState)
        {
            var playerEntityManager = _ioc.Get<IPlayerManager>();
            playerEntityManager.DisableComponent();

            base.OnDisabled(nextState);
        }

    }
}
