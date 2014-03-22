using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Interfaces;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using Utopia.GUI.NuclexUIPort.Controls.Desktop;
using SharpDX.Direct3D11;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat;
using Control = S33M3CoreComponents.GUI.Nuclex.Controls.Control;
using MouseButtons = S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons;
using Screen = S33M3CoreComponents.GUI.Nuclex.MainScreen;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using S33M3DXEngine.Debug.Interfaces;
using System.Reflection;

namespace S33M3CoreComponents.GUI
{
    /// <summary>
    /// General GUI manager component, handles mouse input and draws the gui
    /// other component/classes have the responsability to add controls to _screen,
    /// No one should reference this class apart from the game initialization (ninject bind and get)
    ///
    /// </summary>
    public class GuiManager : DrawableGameComponent, IDebugInfo
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private int _forceExclusiveModeCpt;
        private bool _forceExclusiveMode;

        public bool ForceExclusiveMode
        {
            get { return _forceExclusiveMode; }
            set
            {
                _forceExclusiveMode = value;
                if (value) _forceExclusiveModeCpt++;
                else _forceExclusiveModeCpt--;
                _forceExclusiveMode = _forceExclusiveModeCpt > 0;
            }
        }

        private ConcurrentQueue<Action> _guiActions = new ConcurrentQueue<Action>();

        /// <summary>Draws the GUI</summary>
        private IGuiVisualizer _guiVisualizer;
        //The assembly that will contains the "user" made components, it will be looked by reflection to find the components
        private List<Assembly> _plugInComponentAssemblies;

        /// <summary>The GUI screen representing the desktop</summary>
        private readonly Screen _screen;

        public Screen Screen
        {
            get { return _screen; }
        }

        private readonly D3DEngine _d3DEngine;
        private InputsManager _inputManager;

        private string _debugString;
        private string _skinPath;

        /// <summary>
        /// Indicates that some dialog just closed
        /// </summary>
        public static bool DialogClosed;

        public GuiManager(Screen screen,
                          D3DEngine d3DEngine,
                          InputsManager inputManager,
                          string skinPath = @"GUI\Skins\Default\Default.skin.xml",
                          List<Assembly> plugInComponentAssemblies = null
                          )
        {

            _plugInComponentAssemblies = plugInComponentAssemblies;
            _screen = screen;
            _d3DEngine = d3DEngine;
            _inputManager = inputManager;
            _skinPath = skinPath;

            _d3DEngine.GameWindow.KeyPress += GameWindowKeyPress;
            _d3DEngine.GameWindow.KeyDown += GameWindowKeyDown;
            _d3DEngine.GameWindow.KeyUp += GameWindowKeyUp;

            _d3DEngine.ScreenSize_Updated += _d3DEngine_ScreenSize_Updated;

            _screen.Width = _d3DEngine.ViewPort.Width;
            _screen.Height = _d3DEngine.ViewPort.Height;
            _screen.Desktop.ChildsAlignment = VerticalAlignment.Center;

            DrawOrders.UpdateIndex(0, 10001);
            UpdateOrder = _inputManager.UpdateOrder + 1;

            this.IsSystemComponent = true;

            //Register Action Manager for mouse click handling.
            _inputManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = Actions.LeftMousePressed,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.LeftButton
            });

            _inputManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = Actions.LeftMouseRelease,
                TriggerType = MouseTriggerMode.ButtonReleased,
                Binding = MouseButton.LeftButton
            });

            _inputManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = Actions.RightMousePressed,
                TriggerType = MouseTriggerMode.ButtonPressed,
                Binding = MouseButton.RightButton
            });

            _inputManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = Actions.RightMouseRelease,
                TriggerType = MouseTriggerMode.ButtonReleased,
                Binding = MouseButton.RightButton
            });

            _inputManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = Actions.ScrollWheelForward,
                TriggerType = MouseTriggerMode.ScrollWheelForward,
                Binding = MouseButton.ScrollWheel
            });

            _inputManager.ActionsManager.AddActions(new MouseTriggeredAction()
            {
                ActionId = Actions.ScrollWheelBackward,
                TriggerType = MouseTriggerMode.ScrollWheelBackWard,
                Binding = MouseButton.ScrollWheel
            });
        }

        public void RunInGuiThread(Action action)
        {
            _guiActions.Enqueue(action);
        }

        void _d3DEngine_ScreenSize_Updated(SharpDX.ViewportF viewport, Texture2DDescription newBackBuffer)
        {
            _screen.Width = viewport.Width;
            _screen.Height = viewport.Height;
        }

        public override void BeforeDispose()
        {
            _d3DEngine.GameWindow.KeyPress -= GameWindowKeyPress;
            _d3DEngine.GameWindow.KeyDown -= GameWindowKeyDown;
            _d3DEngine.GameWindow.KeyUp -= GameWindowKeyUp;
        }

        public override void LoadContent(DeviceContext context)
        {
            _guiVisualizer = ToDispose(FlatGuiVisualizer.FromFile(_d3DEngine, _skinPath, _plugInComponentAssemblies));
        }

        public override void UnloadContent()
        {
        }

        private void GameWindowKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Home:
                case Keys.End:
                case Keys.Back:
                case Keys.Return:
                case Keys.Escape:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Delete:
                    _screen.InjectKeyRelease(e.KeyData);
                    break;
                case Keys.Tab:
                    _screen.InjectKeyRelease(Keys.Down);
                    break;
            }
        }

        private void GameWindowKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Home:
                case Keys.End:
                case Keys.Back:
                case Keys.Return:
                case Keys.Escape:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Delete:
                    _screen.InjectKeyPress(e.KeyData);
                    break;
                case Keys.Tab:
                    _screen.InjectKeyPress(Keys.Down);
                    break;
            }

            _screen.InjectKeyPressLookUp(e.KeyData);
        }

        private void GameWindowKeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar) || char.IsPunctuation(e.KeyChar) || char.IsWhiteSpace(e.KeyChar) || char.IsSeparator(e.KeyChar) || char.IsSymbol(e.KeyChar))
                _screen.InjectCharacter(e.KeyChar);
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            Action action;
            while (_guiActions.TryDequeue(out action))
            {
                action();
            }

            //Check for Mouse Overing states on the gui
            if (_screen.IsMouseOverGui == true && this.CatchExclusiveActions == false)
            {
                this.CatchExclusiveActions = true;
                _inputManager.ActionsManager.IsMouseExclusiveMode = true;
            }
            else
            {
                if (_screen.IsMouseOverGui == false && this.CatchExclusiveActions == true)
                {
                    this.CatchExclusiveActions = false;
                    _inputManager.ActionsManager.IsMouseExclusiveMode = false;
                }
            }

            DialogClosed = false;
            InjectMouseInput();

            _screen.Update();
        }

        public bool ScreenIsLocked
        {
            get { return _screen.Desktop.Children.Contains(DialogHelper.DialogBg); }
        }

        public void MessageBox(string message, string title = "", string[] buttonsText = null, System.Action<string> action = null)
        {
            var screenWidth = _d3DEngine.ViewPort.Width;
            var screenHeight = _d3DEngine.ViewPort.Height;

            var windowWidth = 300;
            var windowHeight = 100;

            if (buttonsText == null)
                buttonsText = new[] { "Ok" };

            var mouseCapture = _inputManager.MouseManager.MouseCapture;

            _inputManager.MouseManager.MouseCapture = false;
            var mbWindow = new WindowControl { Title = title, Bounds = new UniRectangle((screenWidth - windowWidth) / 2, (screenHeight - windowHeight) / 2, windowWidth, windowHeight) };

            mbWindow.Children.Add(new LabelControl { Text = message, Autosizing = true, Bounds = new UniRectangle(15, 25, windowWidth - 40, 40) });

            var buttonsPlace = new Control { Bounds = new UniRectangle(0, 0, 0, 20), LayoutFlags = ControlLayoutFlags.WholeRowCenter, LeftTopMargin = new SharpDX.Vector2() };

            foreach (var text in buttonsText)
            {
                var button = new ButtonControl { Text = text, Bounds = new UniRectangle((windowWidth - 50) / 2, windowHeight - 30, 20 + 5 * text.Length, 20) };

                buttonsPlace.Bounds.Size.X += button.Bounds.Size.X + 5;
                
                var text1 = text;
                button.Pressed += delegate
                {
                    _screen.Desktop.Children.Remove(DialogHelper.DialogBg);
                    mbWindow.Close();
                    DialogClosed = true;
                    if (action != null) 
                        action(text1);

                    _inputManager.MouseManager.MouseCapture = mouseCapture;
                };

                buttonsPlace.Children.Add(button);
            }

            buttonsPlace.UpdateLayout();

            mbWindow.Children.Add(buttonsPlace);
            mbWindow.UpdateLayout();

            _screen.Desktop.Children.Add(mbWindow);

            // block all underlying controls
            if (!_screen.Desktop.Children.Contains(DialogHelper.DialogBg))
                _screen.Desktop.Children.Add(DialogHelper.DialogBg);
            DialogHelper.DialogBg.BringToFront();

            mbWindow.BringToFront();
        }

        public void SetDialogMode(bool dialogMode)
        {
            if (_screen.Desktop.Children.Contains(DialogHelper.DialogBg)) _screen.Desktop.Children.Remove(DialogHelper.DialogBg);
            ForceExclusiveMode = dialogMode;
            _inputManager.ActionsManager.IsMouseExclusiveMode = dialogMode;
            _inputManager.MouseManager.MouseCapture = !dialogMode;
        }

        //Draw at 2d level ! (Last draw called)
        public override void Draw(DeviceContext context, int index)
        {
            //Clear the Depth buffer, resetting it to 0
            context.ClearDepthStencilView(_d3DEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);
            _guiVisualizer.Draw(_screen);
            var v = (FlatGuiVisualizer)_guiVisualizer;

            if (ShowDebugInfo)
            {
                _debugString = string.Format("Gui Draw calls : {0} Sprites : {1}", v.Graphics.DrawCalls, v.Graphics.DrawItems);
            }
        }

        private void InjectMouseInput()
        {
            if (_inputManager.ActionsManager.isTriggered(Actions.LeftMousePressed, CatchExclusiveActions || _forceExclusiveMode))
            {
                _screen.InjectMousePress(MouseButtons.Left);
            }

            if (_inputManager.ActionsManager.isTriggered(Actions.LeftMouseRelease, CatchExclusiveActions || _forceExclusiveMode))
            {
                _screen.InjectMouseRelease(MouseButtons.Left);
            }

            if (_inputManager.ActionsManager.isTriggered(Actions.RightMousePressed, CatchExclusiveActions || _forceExclusiveMode))
            {
                _screen.InjectMousePress(MouseButtons.Right);
            }

            if (_inputManager.ActionsManager.isTriggered(Actions.RightMouseRelease, CatchExclusiveActions || _forceExclusiveMode))
            {
                _screen.InjectMouseRelease(MouseButtons.Right);
            }

            if (_inputManager.ActionsManager.isTriggered(Actions.ScrollWheelForward, CatchExclusiveActions || _forceExclusiveMode))
            {
                _screen.InjectMouseWheel(1);
            }

            if (_inputManager.ActionsManager.isTriggered(Actions.ScrollWheelBackward, CatchExclusiveActions || _forceExclusiveMode))
            {
                _screen.InjectMouseWheel(-1);
            }

            _screen.InjectMouseMove(_inputManager.MouseManager.CurMouseState.X, _inputManager.MouseManager.CurMouseState.Y);
        }

        #region Debug Info
        public bool ShowDebugInfo { get; set; }
        public string GetDebugInfo()
        {
            if (ShowDebugInfo == false) return null;
            return _debugString;
        }
        #endregion
    }
}
