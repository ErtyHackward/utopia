using System;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.GUI.Inventory;
using SharpDX.Direct3D11;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Sprites;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs;
using Utopia.Action;
using Utopia.Shared.Settings;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;

namespace Utopia.GUI
{

    /// <summary>
    /// Heads up display = crosshair + toolbar(s) / icons + life + mana + ... 
    /// </summary>
    public class Hud : DrawableGameComponent
    {
        private SpriteRenderer _spriteRender;
        private SpriteTexture _crosshair;
        private SpriteFont _font;
        private readonly MainScreen _screen;
        private readonly D3DEngine _d3DEngine;
        private int _selectedSlot;
        private readonly CameraManager<ICameraFocused> _camManager;

        /// <summary>
        /// _toolbarUI is a fixed part of the hud
        /// </summary>
        private ToolBarUi _toolbarUi;
        
        private readonly InputsManager _inputManager;

        public event EventHandler<SlotClickedEventArgs> SlotClicked;

        private void OnSlotClicked(SlotClickedEventArgs e)
        {
            var handler = SlotClicked;
            if (handler != null) handler(this, e);
        }

        public Hud(MainScreen screen, D3DEngine d3DEngine, ToolBarUi toolbar, InputsManager inputManager, CameraManager<ICameraFocused> camManager)
        {
            IsDefferedLoadContent = true;

            _screen = screen;
            _inputManager = inputManager;
            
            _d3DEngine = d3DEngine;
            DrawOrders.UpdateIndex(0, 10000);
            _d3DEngine.ViewPort_Updated += D3DEngineViewPortUpdated;
            ToolbarUi = toolbar;
            _camManager = camManager;

            _inputManager.KeyboardManager.IsRunning = true;
        }

        private void SelectSlot(int index)
        {
            // equip the slot
            _selectedSlot = index;
            OnSlotClicked(new SlotClickedEventArgs { SlotIndex = index });
        }

        /// <summary>
        /// _toolbarUI is a fixed part of the hud
        /// </summary>
        public ToolBarUi ToolbarUi
        {
            get { return _toolbarUi; }
            set { _toolbarUi = value; }
        }

        public override void LoadContent(DeviceContext context)
        {
            _crosshair = new SpriteTexture(_d3DEngine.Device, ClientSettings.TexturePack + @"Gui\Crosshair.png", _d3DEngine, _d3DEngine.ViewPort);
            _crosshair.ColorModifier = new Color4(0, 0, 1, 1);

            _spriteRender = new SpriteRenderer(_d3DEngine);

            _font = new SpriteFont();
            _font.Initialize("Lucida Console", 10f, System.Drawing.FontStyle.Regular, true, _d3DEngine.Device);

            if (Updatable)
            {
                _screen.Desktop.Children.Add(ToolbarUi);
                ToolbarUi.Locate(S33M3CoreComponents.GUI.Nuclex.Controls.ControlDock.HorisontalCenter | S33M3CoreComponents.GUI.Nuclex.Controls.ControlDock.VerticalBottom);
            }
            //the guimanager will draw the GUI screen, not the Hud !
        }

        //Refresh Sprite Centering when the viewPort size change !
        private void D3DEngineViewPortUpdated(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {
            var screenSize = new Vector2I((int)_d3DEngine.ViewPort.Width, (int)_d3DEngine.ViewPort.Height);
            ToolbarUi.Bounds.Location = new UniVector((screenSize.X - ToolbarUi.Bounds.Size.X.Offset) / 2, screenSize.Y - ToolbarUi.Bounds.Size.Y);
        }

        public override void BeforeDispose()
        {
            _toolbarUi.Dispose();
            _spriteRender.Dispose();
            _crosshair.Dispose();
            _font.Dispose();
            _d3DEngine.ViewPort_Updated -= D3DEngineViewPortUpdated;
        }

        private int _lastSlot = 9;//TODO dynamic / configurable amount of toolbar slots
                
        public override void Update(GameTime timeSpend)
        {
            //Process pressed keys by "event"
            foreach(var keyPressed in _inputManager.KeyboardManager.GetPressedChars())
            {
                switch (keyPressed)
                {
                    case '1': SelectSlot(0); break;
                    case '2': SelectSlot(1); break;
                    case '3': SelectSlot(2); break;
                    case '4': SelectSlot(3); break;
                    case '5': SelectSlot(4); break;
                    case '6': SelectSlot(5); break;
                    case '7': SelectSlot(6); break;
                    case '8': SelectSlot(7); break;
                    case '9': SelectSlot(8); break;
                    case '0': SelectSlot(9); break;
                    default:
                        break;
                }
            }

            if (_inputManager.ActionsManager.isTriggered(UtopiaActions.ToolBar_SelectPrevious))
            {
                var slot = _selectedSlot == 0 ?  _lastSlot : _selectedSlot-1;
                SelectSlot(slot);
            }

            else if (_inputManager.ActionsManager.isTriggered(UtopiaActions.ToolBar_SelectNext))
            {
                var slot = _selectedSlot == _lastSlot ? 0 : _selectedSlot + 1;
                SelectSlot(slot);
            }

            _toolbarUi.Update(timeSpend);
        }

        //Draw at 2d level ! (Last draw called)
        public override void Draw(DeviceContext context, int index)
        {
            if (_camManager.ActiveCamera.CameraType == CameraType.FirstPerson)
            {
                _spriteRender.Begin(false);
                _spriteRender.Draw(_crosshair, ref _crosshair.ScreenPosition, ref _crosshair.ColorModifier);
                _spriteRender.End(context);
            }
        }

        public override void EnableComponent(bool forced)
        {
            if (!AutoStateEnabled && !forced) return;

            if (!_screen.Desktop.Children.Contains(ToolbarUi))
                _screen.Desktop.Children.Add(ToolbarUi);

            var screenSize = new Vector2I((int)_d3DEngine.ViewPort.Width, (int)_d3DEngine.ViewPort.Height);

            ToolbarUi.Bounds.Location = new UniVector((screenSize.X - ToolbarUi.Bounds.Size.X.Offset) / 2, screenSize.Y - ToolbarUi.Bounds.Size.Y);
            base.EnableComponent();
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(ToolbarUi);
            base.DisableComponent();
        }
    }

    public class SlotClickedEventArgs : EventArgs
    {
        public int SlotIndex { get; set; }
    }
}