using Utopia;
using Ninject;
using Utopia.Settings;
using Utopia.Action;
using Utopia.Shared.Config;
using System.Windows.Forms;
using Utopia.Shared.Settings;
using S33M3_DXEngine;
using S33M3_CoreComponents.Inputs.Actions;
using S33M3_CoreComponents.Inputs.KeyboardHandler;
using S33M3_CoreComponents.Inputs;

namespace Sandbox.Client
{
    public partial class GameClient
    {
        public UtopiaRender CreateNewGameEngine(IKernel iocContainer)
        {
            GameSystemSettings.Current = new XmlSettingsManager<GameSystemSetting>(@"GameSystemSettings.xml", SettingsStorage.CustomPath) { CustomSettingsFolderPath = @"Config\" };
            GameSystemSettings.Current.Load();

            var utopiaRenderer = new UtopiaRender(iocContainer.Get<D3DEngine>(), iocContainer.Get<InputsManager>(), false);
            
            BindActions(iocContainer.Get<ActionsManager>());    //Bind the various actions

            return utopiaRenderer;
        }

        private void BindActions(ActionsManager actionManager)
        {
            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Forward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Forward
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Forward,
                TriggerType = MouseTriggerMode.ButtonDown,
                Binding = MouseButton.LeftAndRightButton
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Backward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Backward
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_StrafeLeft,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeLeft
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_StrafeRight,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeRight
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Down,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Down
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Up,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Up
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Jump,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Jump,
                WithTimeElapsed = true,
                MaxTimeElapsedInS = 1
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Mode,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Mode
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Run,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Run
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EngineFullScreen,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.FullScreen
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.MouseCapture,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.LockMouseCursor
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_Left,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_Right,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_LeftWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = true
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_RightWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton,
                WithCursorLocked = true
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_LeftWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = false
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.Use_RightWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton,
                WithCursorLocked = false
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.ToolBar_SelectNext,
                TriggerType = MouseTriggerMode.ScrollWheelForward,
                Binding = MouseButton.ScrollWheel
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.ToolBar_SelectPrevious,
                TriggerType = MouseTriggerMode.ScrollWheelBackWard,
                Binding = MouseButton.ScrollWheel
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EngineVSync,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.VSync
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EngineExit,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = new KeyWithModifier() { MainKey = Keys.Escape }
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.Toggle_Chat,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Chat
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.EntityUse,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Use
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.EntityThrow,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Throw
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.OpenInventory,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Inventory
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.OpenMap,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Map
            });
        }
    }
}
