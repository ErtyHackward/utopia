﻿using System;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.Shared.Sprites;
using S33M3Engines.Sprites;
using SharpDX;
using Nuclex.UserInterface;
using Utopia.Action;
using Utopia.Entities;
using Utopia.GUI.D3D.Inventory;
using SharpDX.Direct3D11;
using Utopia.InputManager;
using Utopia.Settings;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;

namespace Utopia.GUI.D3D
{

    /// <summary>
    /// Heads up display = crosshair + toolbar(s) / icons + life + mana + ... 
    /// </summary>
      
    public class Hud : DrawableGameComponent
    {
        private SpriteRenderer _spriteRender;
        private SpriteTexture _crosshair;
        private SpriteFont _font;
        private readonly Screen _screen;
        private readonly D3DEngine _d3DEngine;
        private int _selectedSlot;

        /// <summary>
        /// _toolbarUI is a fixed part of the hud
        /// </summary>
        private ToolBarUi _toolbarUi;

        private readonly PlayerCharacter _player;
        private IconFactory iconFactory;
        private readonly InputsManager _inputManager;
        private readonly ActionsManager _actions;

        public event EventHandler<SlotClickedEventArgs> SlotClicked;

        private void OnSlotClicked(SlotClickedEventArgs e)
        {
            var handler = SlotClicked;
            if (handler != null) handler(this, e);
        }

        public Hud(Screen screen, D3DEngine d3DEngine, PlayerCharacter player, IconFactory iconFactory, InputManager.InputsManager inputManager, ActionsManager actions)
        {
            _screen = screen;
            _actions = actions;
            this.iconFactory = iconFactory;
            _inputManager = inputManager;
            _player = player;
            _d3DEngine = d3DEngine;
            DrawOrders.UpdateIndex(0, 9000);
            _d3DEngine.ViewPort_Updated += D3dEngine_ViewPort_Updated;
            ToolbarUi = new ToolBarUi(new UniRectangle(0.0f, _d3DEngine.ViewPort.Height - 46, _d3DEngine.ViewPort.Width, 80.0f), _player, iconFactory);

            _inputManager.KeyBoardListening = true;
            _inputManager.OnKeyPressed += new InputsManager.KeyPress(_inputManager_OnKeyPressed);
        }

        void _inputManager_OnKeyPressed(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            switch (e.KeyChar)
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
            }
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

        public override void LoadContent()
        {
            _crosshair = new SpriteTexture(_d3DEngine.Device, ClientSettings.TexturePack + @"Gui\Crosshair.png", ref _d3DEngine.ViewPort_Updated, _d3DEngine.ViewPort);

            _spriteRender = new SpriteRenderer();
            _spriteRender.Initialize(_d3DEngine);

            _font = new SpriteFont();
            _font.Initialize("Segoe UI Mono", 13f, System.Drawing.FontStyle.Regular, true, _d3DEngine.Device);

            
            _screen.Desktop.Children.Add(ToolbarUi);
            //the guimanager will draw the GUI screen, not the Hud !
        }

        //Refresh Sprite Centering when the viewPort size change !
        private void D3dEngine_ViewPort_Updated(Viewport viewport)
        {
            ToolbarUi.Bounds = new UniRectangle(0.0f, viewport.Height - 46, viewport.Width, 80.0f);
            ToolbarUi.Resized();
        }

        public override void UnloadContent()
        {
            _spriteRender.Dispose();
            _crosshair.Dispose();
            _font.Dispose();
            _d3DEngine.ViewPort_Updated -= D3dEngine_ViewPort_Updated;
        }

        private int _lastSlot = 9;//TODO dynamic / configurable amount of toolbar slots
                
        public override void Update(ref GameTime timeSpent)
        {
           //TODO skip empty toolbar slots
            
            if (_actions.isTriggered(Actions.ToolBar_SelectPrevious))
            {
                int slot = _selectedSlot == 0 ?  _lastSlot : _selectedSlot-1;
                SelectSlot(slot);
            }

            else if (_actions.isTriggered(Actions.ToolBar_SelectNext))
            {
                int slot = _selectedSlot == _lastSlot ? 0 : _selectedSlot + 1;
                SelectSlot(slot);
            }

            _toolbarUi.Update(ref timeSpent);
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        //Draw at 2d level ! (Last draw called)
        public override void Draw(int Index)
        {
            _spriteRender.Begin(SpriteRenderer.FilterMode.Linear);
            _spriteRender.Render(_crosshair, ref _crosshair.ScreenPosition, new Color4(1, 0, 0, 1));
            _spriteRender.End();

        }

        protected override void OnEnabledChanged(object sender, System.EventArgs args)
        {
            base.OnEnabledChanged(sender, args);
            if (Enabled)
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