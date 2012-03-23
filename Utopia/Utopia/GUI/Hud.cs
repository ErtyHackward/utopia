using System;
using SharpDX;
using Utopia.Entities;
using Utopia.GUI.Inventory;
using SharpDX.Direct3D11;
using Utopia.Settings;
using Utopia.Shared.Entities.Dynamic;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Sprites;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using Utopia.Action;
using S33M3_CoreComponents.Sprites;
using S33M3_CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.RenderStates;

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

        /// <summary>
        /// _toolbarUI is a fixed part of the hud
        /// </summary>
        private ToolBarUi _toolbarUi;

        private readonly PlayerCharacter _player;
        private readonly IconFactory _iconFactory;
        private readonly InputsManager _inputManager;

        public event EventHandler<SlotClickedEventArgs> SlotClicked;

        private void OnSlotClicked(SlotClickedEventArgs e)
        {
            var handler = SlotClicked;
            if (handler != null) handler(this, e);
        }

        public Hud(MainScreen screen, D3DEngine d3DEngine, PlayerCharacter player, IconFactory iconFactory, InputsManager inputManager)
        {
            this.IsDefferedLoadContent = true;

            _screen = screen;
            _iconFactory = iconFactory;
            _inputManager = inputManager;
            _player = player;
            _d3DEngine = d3DEngine;
            DrawOrders.UpdateIndex(0, 10000);
            _d3DEngine.ViewPort_Updated += D3DEngineViewPortUpdated;
            ToolbarUi = new ToolBarUi(new UniRectangle(0.0f, _d3DEngine.ViewPort.Height - 46, _d3DEngine.ViewPort.Width, 80.0f), _player, _iconFactory, _inputManager);

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

            if(Updatable)
                _screen.Desktop.Children.Add(ToolbarUi);
            //the guimanager will draw the GUI screen, not the Hud !
        }

        //Refresh Sprite Centering when the viewPort size change !
        private void D3DEngineViewPortUpdated(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {
            ToolbarUi.Bounds = new UniRectangle(0.0f, viewport.Height - 46, viewport.Width, 80.0f);
            ToolbarUi.Resized();
        }

        public override void Dispose()
        {
            _spriteRender.Dispose();
            _crosshair.Dispose();
            _font.Dispose();
            _d3DEngine.ViewPort_Updated -= D3DEngineViewPortUpdated;
            base.Dispose();
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
            //Clear the Depth Buffer Befor render the GUI !! => This draw must be DONE AFTER ALL other "3D" Draw.
            //context.ClearDepthStencilView(_d3DEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);

            _spriteRender.Begin(false);
            _spriteRender.Draw(_crosshair, ref _crosshair.ScreenPosition, ref _crosshair.ColorModifier);
            _spriteRender.End(context);

        }

        protected override void OnUpdatableChanged(object sender, EventArgs args)
        {
            base.OnUpdatableChanged(sender, args);

            if (!IsInitialized) return;

            if (Updatable)
            {
                if (!_screen.Desktop.Children.Contains(ToolbarUi))
                    _screen.Desktop.Children.Add(ToolbarUi);
            }
            else
            {
                _screen.Desktop.Children.Remove(ToolbarUi);
            }
        }
    }

    public class SlotClickedEventArgs : EventArgs
    {
        public int SlotIndex { get; set; }
    }
}