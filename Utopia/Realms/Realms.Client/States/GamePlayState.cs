using System;
using System.Linq;
using Ninject;
using Realms.Client.Components;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Sound;
using Utopia.Action;
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
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Net.Connections;
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
using Utopia.Worlds.Shadows;
using Utopia.PostEffects;
using Utopia.GUI.WindRose;

namespace Realms.Client.States
{
    public class GamePlayState : GameState
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private RealmGameSoundManager _sandboxGameSoundManager;

        private readonly IKernel _ioc;
        private InputsManager _inputsManager;
        private RealmsHud _hud;
        private PlayerEntityManager _playerEntityManager;
        private ServerComponent _serverComponent;

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
            GameScope.CurrentGameScope.Disposed += CurrentGameScope_Disposed;

            var cameraManager = _ioc.Get<CameraManager<ICameraFocused>>();
            var timerManager = _ioc.Get<TimerManager>();
            _inputsManager = _ioc.Get<InputsManager>();
            _inputsManager.ActionsManager.KeyboardAction += ActionsManager_KeyboardAction;

            var guiManager = _ioc.Get<GuiManager>();
            var iconFactory = _ioc.Get<IconFactory>();
            var gameClock = _ioc.Get<IClock>();
            var inventory = _ioc.Get<InventoryComponent>();
            var windRose = _ioc.Get<WindRoseComponent>();
            
            var chat = _ioc.Get<ChatComponent>();
            chat.ActivatedChanged += chat_ActivatedChanged;


            _hud = (RealmsHud)_ioc.Get<Hud>();
            _hud.CraftingButton.Pressed += CraftingButton_Pressed;
            _hud.InventoryButton.Pressed += InventoryButton_Pressed;

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
            var worldChunks = _ioc.Get<IWorldChunks2D>();
            var worldShadowMap = ClientSettings.Current.Settings.GraphicalParameters.ShadowMap ? _ioc.Get<WorldShadowMap>() : null;
            var pickingRenderer = _ioc.Get<IPickingRenderer>();
            var dynamicEntityManager = _ioc.Get<IVisualDynamicEntityManager>();

            _playerEntityManager = (PlayerEntityManager)_ioc.Get<IPlayerManager>();
            _playerEntityManager.PlayerCharacter.Inventory.ItemPut += InventoryOnItemPut;
            _playerEntityManager.PlayerCharacter.Inventory.ItemTaken += InventoryOnItemTaken;
            _playerEntityManager.NeedToShowInventory += playerEntityManager_NeedToShowInventory;
            
            var sharedFrameCB = _ioc.Get<SharedFrameCB>();

            _sandboxGameSoundManager = (RealmGameSoundManager)_ioc.Get<GameSoundManager>();
            _serverComponent = _ioc.Get<ServerComponent>();
            _serverComponent.ConnectionStatusChanged += ServerComponentOnConnectionStausChanged;
            
            var fadeComponent = _ioc.Get<FadeComponent>();
            var voxelModelManager = _ioc.Get<VoxelModelManager>();
            var adminConsole = _ioc.Get<AdminConsole>();
            var toolRenderer = _ioc.Get<FirstPersonToolRenderer>();
            var particuleEngine = _ioc.Get<UtopiaParticuleEngine>();
            var ghostedRenderer = _ioc.Get<GhostedEntityRenderer>();
            var crafting = _ioc.Get<CraftingComponent>();
            var charSelection = _ioc.Get<CharacterSelectionComponent>();
            var inventoryEvents = _ioc.Get<InventoryEventComponent>();
            var pickingManager = _ioc.Get<PickingManager>();
            var cracksRenderer = _ioc.Get<CracksRenderer>();
            var postEffectComponent = _ioc.Get<PostEffectComponent>();

            AddComponent(cameraManager);
            AddComponent(_serverComponent);
            AddComponent(_inputsManager);
            AddComponent(iconFactory);
            AddComponent(timerManager);
            AddComponent(skyBackBuffer);
            AddComponent(_playerEntityManager);
            AddComponent(dynamicEntityManager);
            AddComponent(_hud);
            AddComponent(guiManager);
            AddComponent(pickingRenderer);
            AddComponent(inventory);
            AddComponent(windRose);
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
            AddComponent(charSelection);
            AddComponent(postEffectComponent);

            if (worldShadowMap != null)
                AddComponent(worldShadowMap);
            
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
                if (gc.GetType() != typeof(LoadingComponent))
                {
                    logger.Warn("Missing LoadingGameState component, present inside GamePlayState : {0}", gc.GetType().ToString());
                }
            }

#endif
            base.Initialize(context);
        }

        void chat_ActivatedChanged(object sender, EventArgs e)
        {
            var chat = _ioc.Get<ChatComponent>();
            _hud.DisableNumbersHandling = chat.Activated;
        }

        void CurrentGameScope_Disposed(object sender, EventArgs e)
        {
            _inputsManager.ActionsManager.KeyboardAction -= ActionsManager_KeyboardAction;
            _inputsManager = null;

            _serverComponent.ConnectionStatusChanged -= ServerComponentOnConnectionStausChanged;
            _serverComponent = null;
            
            _hud.CraftingButton.Pressed -= CraftingButton_Pressed;
            _hud.InventoryButton.Pressed -= InventoryButton_Pressed;
            _hud = null;
            
            _playerEntityManager.NeedToShowInventory -= playerEntityManager_NeedToShowInventory;
            _playerEntityManager = null;

            var chat = _ioc.Get<ChatComponent>();
            chat.ActivatedChanged -= chat_ActivatedChanged;
        }

        private void ServerComponentOnConnectionStausChanged(object sender, TcpConnectionStatusEventArgs e)
        {
            if (e.Status == TcpConnectionStatus.Disconnected)
            {
                var vars = _ioc.Get<RealmRuntimeVariables>();
                vars.MessageOnExit = "Server connection was interrupted. " + _serverComponent.LastErrorText;
                StatesManager.ActivateGameState("MainMenu");
            }
        }

        void playerEntityManager_NeedToShowInventory(object sender, InventoryEventArgs e)
        {
            if (StatesManager.CurrentState.Name != "Inventory")
            {
                StatesManager.ActivateGameStateAsync("Inventory", true);
            }
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
            var inventory = _ioc.Get<InventoryComponent>();
            if (inventory.IsToolbarSwitching)
                return;

            var iec = _ioc.Get<InventoryEventComponent>();
            iec.Notify(e.Slot.Item, e.Slot.Item.Name + " removed", false, e.Slot.ItemsCount);
        }

        private void InventoryOnItemPut(object sender, EntityContainerEventArgs<ContainedSlot> e)
        {
            var inventory = _ioc.Get<InventoryComponent>();
            if (inventory.IsToolbarSwitching)
                return;

            var iec = _ioc.Get<InventoryEventComponent>();
            iec.Notify(e.Slot.Item, e.Slot.Item.Name + " added", true, e.Slot.ItemsCount);
        }

        void ActionsManager_KeyboardAction(object sender, ActionsManagerEventArgs e)
        {
            var guiManager = _ioc.Get<GuiManager>();

            if (StatesManager.CurrentState.Name == "Settings") 
                return;

            if (e.Action.ActionId == Actions.EngineExit)
            {
                if (StatesManager.CurrentState.Name == "Gameplay")
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

            if (guiManager.ScreenIsLocked == false)
            {
                switch (e.Action.ActionId)
                {
                    case UtopiaActions.OpenInventory:
                        if (StatesManager.CurrentState.Name != "Inventory")
                        {
                            StatesManager.ActivateGameStateAsync("Inventory", true);
                        }
                        else
                        {
                            StatesManager.ActivateGameStateAsync("Gameplay");
                        }
                        break;
                    case UtopiaActions.OpenCrafting:
                        if (StatesManager.CurrentState.Name != "Crafting")
                        {
                            StatesManager.ActivateGameStateAsync("Crafting", true);
                        }
                        else
                        {
                            StatesManager.ActivateGameStateAsync("Gameplay");
                        }
                        break;
                    case UtopiaActions.SelectCharacter:
                        if (StatesManager.CurrentState.Name != "CharSelection")
                        {
                            StatesManager.ActivateGameStateAsync("CharSelection", true);
                        }
                        else
                        {
                            StatesManager.ActivateGameStateAsync("Gameplay");
                        }
                        break;
                    default:
                        if (e.Action.ActionId == UtopiaActions.EntityUse && StatesManager.CurrentState.Name == "Inventory")
                        {
                            StatesManager.ActivateGameStateAsync("Gameplay");
                        }
                        break;
                }
            }
        }

        public override void OnEnabled(GameState previousState)
        {
            var guiManager = _ioc.Get<GuiManager>();
            guiManager.Screen.ShowAll();

            var playerEntityManager = _ioc.Get<IPlayerManager>();
            playerEntityManager.EnableComponent();

            var fadeComponent = _ioc.Get<FadeComponent>();
            fadeComponent.Visible = false;

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
