using Utopia;
using Ninject;
using Utopia.Settings;
using Utopia.Action;
using Utopia.Shared.Config;
using System.Windows.Forms;
using Utopia.Shared.Settings;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using S33M3CoreComponents.Inputs;

namespace Sandbox.Client
{
    public partial class GameClient
    {
        public UtopiaRender CreateNewGameEngine(IKernel iocContainer)
        {
            GameSystemSettings.Current = new XmlSettingsManager<GameSystemSetting>(@"GameSystemSettings.xml", SettingsStorage.CustomPath) { CustomSettingsFolderPath = @"Config\" };
            GameSystemSettings.Current.Load();

            var utopiaRenderer = new UtopiaRender(iocContainer.Get<D3DEngine>(), iocContainer.Get<InputsManager>(), true);
            
            BindActions(iocContainer.Get<InputsManager>());    //Bind the various actions

            return utopiaRenderer;
        }

        private void BindActions(InputsManager inputsManager)
        {

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Forward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Forward
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Forward,
                TriggerType = MouseTriggerMode.ButtonDown,
                Binding = MouseButton.LeftAndRightButton
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Backward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Backward
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_StrafeLeft,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeLeft
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_StrafeRight,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeRight
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Down,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Down
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Up,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Up
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Jump,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Jump,
                WithTimeElapsed = true,
                MaxTimeElapsedInS = 1
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Mode,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Mode
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Run,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Run
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EngineFullScreen,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.FullScreen
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.MouseCapture,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.LockMouseCursor
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_Left,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_Right,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_LeftWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = true
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_RightWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton,
                WithCursorLocked = true
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_LeftWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = false
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_RightWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton,
                WithCursorLocked = false
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.ToolBar_SelectNext,
                TriggerType = MouseTriggerMode.ScrollWheelForward,
                Binding = MouseButton.ScrollWheel
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.ToolBar_SelectPrevious,
                TriggerType = MouseTriggerMode.ScrollWheelBackWard,
                Binding = MouseButton.ScrollWheel
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EngineVSync,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.VSync
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EngineExit,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = new KeyWithModifier() { MainKey = Keys.Escape }
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.Toggle_Chat,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Chat
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.Exit_Chat,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = new KeyWithModifier() { MainKey = Keys.Escape }
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.EntityUse,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Use
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.EntityThrow,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Throw
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.OpenInventory,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Inventory
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.OpenMap,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Map
            });

            
        }
    }
}
