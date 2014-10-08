using Utopia;
using Ninject;
using Utopia.Action;
using System.Windows.Forms;
using Utopia.Shared.Settings;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Config;

namespace Realms.Client
{
    public partial class GameClient
    {
        public UtopiaRender CreateNewGameEngine(IKernel iocContainer, bool VSync)
        {
            var utopiaRenderer = new UtopiaRender(iocContainer.Get<D3DEngine>(), iocContainer.Get<InputsManager>(), false);

            utopiaRenderer.VSync = VSync;
            
            BindActions(iocContainer.Get<InputsManager>(), false);    //Bind the various actions

            return utopiaRenderer;
        }

        public void BindActions(InputsManager inputsManager, bool rebindSettingsBasedAction)
        {
            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Forward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Forward
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EndMoveForward,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Forward
            }, rebindSettingsBasedAction);

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
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EndMoveBackward,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Backward
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_StrafeLeft,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeLeft
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EndMoveStrafeLeft,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeLeft
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_StrafeRight,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeRight
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EndMoveStrafeRight,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeRight
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Down,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Down
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Up,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Up
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Jump,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Jump,
                WithTimeElapsed = true,
                MaxTimeElapsedInS = 1
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Mode,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Mode
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.Move_Run,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Run
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EngineFullScreen,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.System.FullScreen
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.MouseCapture,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.System.LockMouseCursor
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.UseLeft,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton,
                WithAutoResetButtonPressed = true,
                AutoResetTimeInS = 0.2f
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.RightDown,
                TriggerType = MouseTriggerMode.ButtonDown,
                Binding = MouseButton.RightButton
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.UseRight,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.UseLeftWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = true
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.UseRightWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton,
                WithCursorLocked = true
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.UseLeftWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = false
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.UseRightWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton,
                WithCursorLocked = false
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.ToolBarSelectNext,
                TriggerType = MouseTriggerMode.ScrollWheelForward,
                Binding = MouseButton.ScrollWheel
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.ToolBarSelectPrevious,
                TriggerType = MouseTriggerMode.ScrollWheelBackWard,
                Binding = MouseButton.ScrollWheel
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.LandscapeSlicerDown,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = new KeyWithModifier() { MainKey = Keys.PageDown }
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.LandscapeSlicerUp,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = new KeyWithModifier() { MainKey = Keys.PageUp }
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.LandscapeSlicerOff,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = new KeyWithModifier() { MainKey = Keys.Home }
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EngineVSync,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.System.VSync
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = UtopiaActions.EngineExit,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = new KeyWithModifier() { MainKey = Keys.Escape }
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.ToggleChat,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Game.Chat
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.ChangeCameraType,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Game.CameraType
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.ExitChat,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = new KeyWithModifier() { MainKey = Keys.Escape }
            });

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.EntityUse,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Game.Use
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.EntityThrow,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Game.Throw
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.OpenInventory,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Game.Inventory
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.ToggleInterface,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Game.ToggleInterface
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.DropMode,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Game.Throw
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.OpenCrafting,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Game.Crafting
            }, rebindSettingsBasedAction);

            inputsManager.ActionsManager.AddActions(new KeyboardTriggeredAction
            {
                ActionId = UtopiaActions.SelectCharacter,
                TriggerType = KeyboardTriggerMode.KeyReleased,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Game.CharSelect
            }, rebindSettingsBasedAction);
        }
    }
}
