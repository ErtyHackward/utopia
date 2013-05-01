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
                ActionId = UtopiaActions.EndMove_Forward,
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
                ActionId = UtopiaActions.EndMove_Backward,
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
                ActionId = UtopiaActions.EndMove_StrafeLeft,
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
                ActionId = UtopiaActions.EndMove_StrafeRight,
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
                ActionId = UtopiaActions.Use_Left,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.RightDown,
                TriggerType = MouseTriggerMode.ButtonDown,
                Binding = MouseButton.RightButton
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

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.LandscapeSlicerDown,
                TriggerType = MouseTriggerMode.ScrollWheelForward,
                Binding = MouseButton.ScrollWheel
            });

            inputsManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = UtopiaActions.LandscapeSlicerUp,
                TriggerType = MouseTriggerMode.ScrollWheelBackWard,
                Binding = MouseButton.ScrollWheel
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
                ActionId = UtopiaActions.Toggle_Chat,
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
                ActionId = UtopiaActions.Exit_Chat,
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
                ActionId = UtopiaActions.Toggle_Interface,
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
        }
    }
}
