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
        private WindowControl _colorPaletteWindow;

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

            _colorPaletteWindow = InitColorPalette();


            _controls.Add(_colorPaletteWindow);

            base.Initialize();
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
            _backButton.Bounds = new UniRectangle(_engine.ViewPort.Width - 200, _engine.ViewPort.Height - 60, 120, 24);
        }

    }
}
