using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines;
using S33M3Engines.D3D;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Editor;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities.Models;

namespace LostIsland.Client.Components
{
    public class EditorComponent : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        private readonly Screen _screen;

        private ButtonControl _backButton;
        private WindowControl _toolsWindow;
        private WindowControl _modelNavigationWindow;

        private readonly List<Control> _controls = new List<Control>();

        /// <summary>
        /// Gets or sets current model translation, rotation and scaling
        /// </summary>
        public Matrix Transform { get; set; }

        #region Events
        public event EventHandler BackPressed;

        private void OnBackPressed()
        {
            var handler = BackPressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ViewModePressed;

        private void OnViewModePressed()
        {
            var handler = ViewModePressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler LayoyutModePressed;

        private void OnLayoyutModePressed()
        {
            var handler = LayoyutModePressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler FrameModePressed;

        private void OnFrameModePressed()
        {
            var handler = FrameModePressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler<SelectionIndexChangedEventArgs> StatesIndexChanged;

        private void OnStateIndexChanged(SelectionIndexChangedEventArgs e)
        {
            var handler = StatesIndexChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<SelectionIndexChangedEventArgs> PartsIndexChanged;

        private void OnPartsIndexChanged(SelectionIndexChangedEventArgs e)
        {
            var handler = PartsIndexChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<SelectionIndexChangedEventArgs> FramesIndexChanged;

        private void OnFramesIndexChanged(SelectionIndexChangedEventArgs e)
        {
            var handler = FramesIndexChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        public EditorComponent(D3DEngine engine, Screen screen)
        {
            _engine = engine;
            _screen = screen;

            _engine.ViewPort_Updated += ViewportUpdated;
        }

        private void ViewportUpdated(Viewport port)
        {
            UpdateLayout();
        }

        public override void Initialize()
        {
            _backButton = new ButtonControl { Text = "Back" };
            _backButton.Pressed += delegate { OnBackPressed(); };



            _toolsWindow = CreateToolsWindow();

            _modelNavigationWindow = CreateNavigationWindow();

            _controls.Add(_modelNavigationWindow);
            //_controls.Add(_colorPaletteWindow);
            _controls.Add(_toolsWindow);

            base.Initialize();
        }

        public void UpdateNavigation(VisualVoxelModel model, int selectedState, int selectedPart, int selectedFrame)
        {
            var stateList = _modelNavigationWindow.Children.Get<ListControl>("statesList");
            var partsList = _modelNavigationWindow.Children.Get<ListControl>("partsList");
            var framesList = _modelNavigationWindow.Children.Get<ListControl>("framesList");

            stateList.Items.Clear();
            partsList.Items.Clear();
            framesList.Items.Clear();

            if (model == null)
            {
                return;
            }
            
            for (int i = 0; i < model.VoxelModel.States.Count; i++)
            {
                stateList.Items.Add(i.ToString());
            }

            foreach (var voxelModelPart in model.VoxelModel.Parts)
            {
                partsList.Items.Add(voxelModelPart.Name);
            }

            if (selectedPart != -1)
            {
                for (int i = 0; i < model.VoxelModel.Parts[selectedPart].Frames.Count; i++)
                {
                    framesList.Items.Add(i.ToString());
                }
            }

        }

        private WindowControl CreateNavigationWindow()
        {
            var height = _engine.ViewPort.Height;

            var listWindow = new WindowControl { Title = "Navigation" };
            listWindow.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, 0, 200, height - 40);

            
            var statesLabel = new LabelControl { Text = "States" };
            statesLabel.Bounds = new UniRectangle(0, 0, 70, 20);
            var statesAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0,0, 50, 20) };
            var statesDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 50, 20) };
            var statesList = new ListControl { Name = "statesList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            statesList.Bounds = new UniRectangle(0, 0, 180, 20);
            statesList.SelectionMode = ListSelectionMode.Single;
            statesList.SelectionChanged += delegate { 
                OnStateIndexChanged(new SelectionIndexChangedEventArgs { Index = statesList.SelectedItems.Count > 0 ? statesList.SelectedItems[0] : -1 }); 
            };
            
            var partsLabel = new LabelControl { Text = "Parts" };
            partsLabel.Bounds = new UniRectangle(0, 0, 70, 20);
            var partsAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 50, 20) };
            var partsDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 50, 20) };
            var partsList = new ListControl { Name = "partsList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            partsList.Bounds = new UniRectangle(0, 0, 180, 20);
            partsList.SelectionMode = ListSelectionMode.Single;
            partsList.SelectionChanged += delegate
            {
                OnPartsIndexChanged(new SelectionIndexChangedEventArgs { Index = statesList.SelectedItems.Count > 0 ? statesList.SelectedItems[0] : -1 });
            };

            var framesLabel = new LabelControl { Text = "Frames" };
            framesLabel.Bounds = new UniRectangle(0, 0, 70, 20);
            var framesAddButton = new ButtonControl { Text = "Add", Bounds = new UniRectangle(0, 0, 50, 20) };
            var framesDeleteButton = new ButtonControl { Text = "Del", Bounds = new UniRectangle(0, 0, 50, 20) };
            var framesList = new ListControl { Name ="framesList", LayoutFlags = ControlLayoutFlags.WholeRow | ControlLayoutFlags.FreeHeight };
            framesList.Bounds = new UniRectangle(0, 0, 180, 20);
            framesList.SelectionMode = ListSelectionMode.Single;
            framesList.SelectionChanged += delegate
            {
                OnFramesIndexChanged(new SelectionIndexChangedEventArgs { Index = statesList.SelectedItems.Count > 0 ? statesList.SelectedItems[0] : -1 });
            };


            listWindow.Children.Add(statesLabel);
            listWindow.Children.Add(statesAddButton);
            listWindow.Children.Add(statesDeleteButton);
            listWindow.Children.Add(statesList);

            listWindow.Children.Add(partsLabel);
            listWindow.Children.Add(partsAddButton);
            listWindow.Children.Add(partsDeleteButton);
            listWindow.Children.Add(partsList);

            listWindow.Children.Add(framesLabel);
            listWindow.Children.Add(framesAddButton);
            listWindow.Children.Add(framesDeleteButton);
            listWindow.Children.Add(framesList);

            listWindow.UpdateLayout();

            return listWindow;
        }
        
        private WindowControl CreateToolsWindow()
        {
            var toolsWindow = new WindowControl {Title = "Tools"};
            toolsWindow.Bounds = new UniRectangle(0, 0, 200, 110);

            var modesLabel = new LabelControl {Text = "Modes"};
            modesLabel.Bounds = new UniRectangle(10, 25, 100, 20);

            var modesButtonsGroup = new Control { Bounds = new UniRectangle(0, 0, 180, 45), LeftTopMargin = new Vector2(), RightBottomMargin = new Vector2(), ControlsSpacing = new Vector2() };
            
            var viewModeButton = new ButtonControl { Text = "View" };
            viewModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            viewModeButton.Pressed += delegate { OnViewModePressed(); };

            var layoutModeButton = new ButtonControl { Text = "Layout" };
            layoutModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            layoutModeButton.Pressed += delegate { OnLayoyutModePressed(); };

            var frameModeButton = new ButtonControl { Text = "Frame" };
            frameModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            frameModeButton.Pressed += delegate { OnFrameModePressed(); };

            var animationModeButton = new ButtonControl { Text = "Anim" };
            animationModeButton.Bounds = new UniRectangle(0, 0, 45, 45);
            animationModeButton.Pressed += delegate { OnFrameModePressed(); };

            modesButtonsGroup.Children.Add(viewModeButton);
            modesButtonsGroup.Children.Add(layoutModeButton);
            modesButtonsGroup.Children.Add(frameModeButton);
            modesButtonsGroup.Children.Add(animationModeButton);
            
            modesButtonsGroup.UpdateLayout();
            
            toolsWindow.Children.Add(modesLabel);
            toolsWindow.Children.Add(modesButtonsGroup);

            toolsWindow.UpdateLayout();

            return toolsWindow;
        }

        private WindowControl InitColorPalette()
        {
            //XXX parametrize UI sizes
            const int rows = 16;
            const int cols = 4;
            const int btnSize = 20;

            const int y0 = 20;
            const int x0 = 0;

            var palette = new WindowControl { Title = "Colors" };
            palette.Bounds = new UniRectangle(0, 0, (cols) * btnSize, (rows + 1) * btnSize);

            int index = 0;
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (index == ColorLookup.Colours.Count()) break;

                    var color = ColorLookup.Colours[index];

                    var btn = new PaletteButtonControl();
                    btn.Bounds = new UniRectangle(x0 + x * btnSize, y0 + y * btnSize, btnSize, btnSize);
                    btn.Color = color;
                    int associatedindex = index; //for access inside closure 
                    btn.Pressed += (sender, e) =>
                                       {

                                       };

                    palette.Children.Add(btn);
                    index++;
                }
            }
            return palette;
        }

        protected override void OnEnabledChanged()
        {
            if (!IsInitialized) return;

            if (Enabled)
            {
                foreach (var control in _controls)
                {
                    _screen.Desktop.Children.Add(control); 
                }
                
                _screen.Desktop.Children.Add(_backButton);
                UpdateLayout();
            }
            else
            {
                foreach (var control in _controls)
                {
                    _screen.Desktop.Children.Remove(control);    
                }
                
                _screen.Desktop.Children.Remove(_backButton);
            }
            
            base.OnEnabledChanged();
        }

        public void UpdateLayout()
        {
            _backButton.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 30, 120, 24);
            _modelNavigationWindow.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, 0, 200, _engine.ViewPort.Height - 40);
            _modelNavigationWindow.UpdateLayout();
        }

    }

    public class SelectionIndexChangedEventArgs : EventArgs
    {
        public int Index { get; set; }
    }
}
