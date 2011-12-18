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
        private WindowControl _partsListWindow;

        private readonly List<Control> _controls = new List<Control>();

        /// <summary>
        /// Gets or sets current active model
        /// </summary>
        public VoxelModel ActiveModel { get; set; }

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

        #endregion

        public EditorComponent(D3DEngine engine, Screen screen)
        {
            _engine = engine;
            _screen = screen;
        }

        public override void Initialize()
        {
            _backButton = new ButtonControl { Text = "Back" };
            _backButton.Pressed += delegate { OnBackPressed(); };



            _toolsWindow = CreateToolsWindow();

            _partsListWindow = CreatePartsListWindow();

            _controls.Add(_partsListWindow);
            //_controls.Add(_colorPaletteWindow);
            _controls.Add(_toolsWindow);

            base.Initialize();
        }

        private WindowControl CreatePartsListWindow()
        {
            var listWindow = new WindowControl { Title = "Parts" };
            listWindow.Bounds = new UniRectangle(_engine.ViewPort.Width - 200 , 0, 200, 560);
            
            var partsLabel = new LabelControl { Text = "Parts" };
            partsLabel.Bounds = new UniRectangle(10, 25, 100, 20);

            var partsList = new ListControl();
            partsList.Bounds = new UniRectangle(10, 50, 180, 230);

            var framesLabel = new LabelControl { Text = "Frames" };
            framesLabel.Bounds = new UniRectangle(10, 285, 180, 20);

            var framesList = new ListControl();
            framesList.Bounds = new UniRectangle(10, 310, 180, 230);


            listWindow.Children.Add(partsLabel);
            listWindow.Children.Add(partsList);
            listWindow.Children.Add(framesLabel);
            listWindow.Children.Add(framesList);

            return listWindow;
        }

        private WindowControl CreateToolsWindow()
        {
            var toolsWindow = new WindowControl {Title = "Tools"};
            toolsWindow.Bounds = new UniRectangle(0, 0, 200, 100);

            var modesLabel = new LabelControl {Text = "Modes"};
            modesLabel.Bounds = new UniRectangle(10, 25, 100, 20);
            
            var viewModeButton = new ButtonControl { Text = "View" };
            viewModeButton.Bounds = new UniRectangle(2, 50, 45, 45);
            viewModeButton.Pressed += delegate { OnViewModePressed(); };

            var layoutModeButton = new ButtonControl { Text = "Layout" };
            layoutModeButton.Bounds = new UniRectangle(52, 50, 45, 45);
            layoutModeButton.Pressed += delegate { OnLayoyutModePressed(); };

            var frameModeButton = new ButtonControl { Text = "Frame" };
            frameModeButton.Bounds = new UniRectangle(102, 50, 45, 45);
            frameModeButton.Pressed += delegate { OnFrameModePressed(); };

            var animationModeButton = new ButtonControl { Text = "Anim" };
            animationModeButton.Bounds = new UniRectangle(152, 50, 45, 45);
            animationModeButton.Pressed += delegate { OnFrameModePressed(); };

            toolsWindow.Children.Add(viewModeButton);
            toolsWindow.Children.Add(layoutModeButton);
            toolsWindow.Children.Add(frameModeButton);
            toolsWindow.Children.Add(animationModeButton);

            LayoutControls(toolsWindow.Children, 6, 50, 45, 45);

            toolsWindow.Children.Add(modesLabel);

            return toolsWindow;
        }

        private void LayoutControls(IList<Control> controls, int leftMargin, int topMargin, int buttonWidth, int buttonHeight)
        {
            int currentX = leftMargin;

            for (int i = 0; i < controls.Count; i++)
            {
                controls[i].Bounds = new UniRectangle(currentX, topMargin, buttonWidth, buttonHeight);
                currentX += buttonWidth;
            }
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
        }

    }
}
