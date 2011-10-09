using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Nuclex.UserInterface;
using Utopia.Editor;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.GUI.D3D.Map;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Weather;
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;
using S33M3Engines;
using S33M3Engines.InputHandler;
using S33M3Engines.Cameras;
using Utopia.Worlds.SkyDomes.SharedComp;
using S33M3Engines.D3D;
using Utopia.Worlds.SkyDomes;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks;
using Utopia.Worlds;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Cubes;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Shared.World.WorldConfigs;
using Utopia.Entities;
using Utopia.Worlds.Storage;
using Utopia.Action;
using Utopia.Network;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.InputManager;
using Utopia.Entities.Managers;
using Utopia.Entities.Renderer;
using Utopia.Settings;
using Utopia.GUI;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Managers.Interfaces;
using S33M3Engines.Timers;
using Utopia.Entities.Renderer.Interfaces;
using Utopia;
using Utopia.Shared.Structs;
using S33M3Engines.D3D.DebugTools;
using Utopia.Shared.Config;
using System.Windows.Forms;

namespace LostIsland.Client
{
    public partial class GameClient
    {
        private void Binding(IKernel iocContainer, WorldParameters worldParam)
        {
            iocContainer.Bind<WorldParameters>().ToConstant(worldParam).InSingletonScope();
            iocContainer.Bind<VisualWorldParameters>().ToSelf().InSingletonScope();

            //Chunk Landscape Creation Processors picking ====
            iocContainer.Bind<IWorldProcessorConfig>().To<s33m3WorldConfig>().InSingletonScope();
            iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("s33m3WorldProcessor");
            iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");

            //Various ====
            iocContainer.Bind<IPickingRenderer>().To<PickingRenderer>().InSingletonScope();         // Use to display the picking cursor on block
            iocContainer.Bind<FPS>().ToSelf().InSingletonScope();

            //Entities ====
            iocContainer.Bind<IEntitiesRenderer>().To<PlayerEntityRenderer>().InSingletonScope().Named("PlayerEntityRenderer");    //Rendering Player
            iocContainer.Bind<IEntitiesRenderer>().To<DynamicEntityRenderer>().InSingletonScope().Named("DefaultEntityRenderer");  //Rendering Dynamic Entities

            //Game Componenents =====
            iocContainer.Bind<IClock>().To<WorldClock>().InSingletonScope();
            iocContainer.Bind<ChatComponent>().ToSelf().InSingletonScope();
            iocContainer.Bind<MapComponent>().ToSelf().InSingletonScope();
            iocContainer.Bind<Hud>().ToSelf().InSingletonScope();
            iocContainer.Bind<EntityEditor>().ToSelf().InSingletonScope();
            iocContainer.Bind<IDrawableComponent>().To<SkyStars>().InSingletonScope().Named("Stars");
            iocContainer.Bind<ISkyDome>().To<RegularSkyDome>().InSingletonScope();
            iocContainer.Bind<IWeather>().To<Weather>().InSingletonScope();
            if (ClientSettings.Current.Settings.GraphicalParameters.CloudsQuality <= 0) iocContainer.Bind<IDrawableComponent>().To<Clouds>().InSingletonScope().Named("Clouds");
            else iocContainer.Bind<IDrawableComponent>().To<Clouds3D>().InSingletonScope().Named("Clouds");

            //System Management ======
            iocContainer.Bind<IChunkStorageManager>().To<SQLiteWorldStorageManager>().InSingletonScope().WithConstructorArgument("forceNew", false)
                                                                                                        .WithConstructorArgument("UserName", _server.ServerConnection.Login);

            iocContainer.Bind<ICubeMeshFactory>().To<SolidCubeMeshFactory>().InSingletonScope().Named("SolidCubeMeshFactory");
            iocContainer.Bind<ICubeMeshFactory>().To<LiquidCubeMashFactory>().InSingletonScope().Named("LiquidCubeMeshFactory");
            iocContainer.Bind<ICamera>().To<FirstPersonCamera>().InSingletonScope(); //Type of camera used

        }

        private void BindActions(ActionsManager actionManager)
        {
            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Forward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Forward
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Move_Forward,
                TriggerType = MouseTriggerMode.ButtonDown,
                Binding = MouseButton.LeftAndRightButton
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Backward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Backward
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_StrafeLeft,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeLeft
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_StrafeRight,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeRight
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Down,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Down
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Up,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Up
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Jump,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Jump
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Mode,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Mode
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Run,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Run
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_FullScreen,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.FullScreen
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_LockMouseCursor,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.LockMouseCursor
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_Left,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_Right,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_LeftWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = true
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_RightWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton,
                WithCursorLocked = true
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_LeftWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = false
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_RightWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton,
                WithCursorLocked = false
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Block_SelectNext,
                TriggerType = MouseTriggerMode.ScrollWheelForward,
                Binding = MouseButton.ScrollWheel
            });

            actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Block_SelectPrevious,
                TriggerType = MouseTriggerMode.ScrollWheelBackWard,
                Binding = MouseButton.ScrollWheel
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.World_FreezeTime,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.FreezeTime
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_VSync,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.VSync
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_ShowDebugUI,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.F12 }
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_Exit,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.Escape }
            });

            actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.DebugUI_Insert,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.Insert }
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.Toggle_Chat,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Chat
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.EntityUse,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Use
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.EntityThrow,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Throw
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.OpenInventory,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Inventory
            });

            actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.OpenMap,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Map
            });
        }
    }
}
