using System;
using System.Linq;
using System.Windows.Forms;
using Ninject;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Entities.Managers;
using Utopia.GUI.Inventory;
using SharpDX.Direct3D11;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs;
using Utopia.Action;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Web.Responses;
using Utopia.Shared.Settings;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.GUI.TopPanel;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Weather;

namespace Utopia.GUI
{
    /// <summary>
    /// Heads up display = crosshair + toolbar(s) / icons + life + mana + ... 
    /// </summary>
    public class Hud : DrawableGameComponent
    {
        #region Private variables
        private readonly MainScreen _screen;
        private readonly D3DEngine _d3DEngine;
        private readonly CameraManager<ICameraFocused> _camManager;
        private readonly InputsManager _inputManager;
        private readonly PlayerEntityManager _playerEntityManager;

        private SpriteRenderer _spriteRender;
        private SpriteTexture _crosshair;
        private SpriteFont _font;

        private ToolBarUi _toolbarUi;
        private int _selectedSlot;
        private EnergyBarsContainer _energyBar;
        private WeatherContainer _weatherContainer;
        private TooltipControl _tooltip;
        #endregion

        #region Public properties
        public bool IsHidden { get; set; }

        public bool DisableNumbersHandling { get; set; }

        public ToolBarUi ToolbarUi
        {
            get { return _toolbarUi; }
            set { _toolbarUi = value; }
        }
        #endregion

        #region Events
        public event EventHandler<SlotClickedEventArgs> SlotClicked;
        #endregion

        public Hud(MainScreen screen,
           D3DEngine d3DEngine,
           ToolBarUi toolbar,
           InputsManager inputManager,
           CameraManager<ICameraFocused> camManager,
           PlayerEntityManager playerEntityManager,
        IWeather weather,
        IWorldChunks worldChunks)
        {
            IsDefferedLoadContent = true;

            _screen = screen;
            _inputManager = inputManager;
            _playerEntityManager = playerEntityManager;

            _d3DEngine = d3DEngine;
            DrawOrders.UpdateIndex(0, 10000);
            _d3DEngine.ScreenSize_Updated += D3DEngineViewPortUpdated;
            ToolbarUi = toolbar;
            toolbar.LayoutFlags = ControlLayoutFlags.Skip;
            _camManager = camManager;

            _inputManager.KeyboardManager.IsRunning = true;
            IsHidden = false;

            _tooltip = new TooltipControl();
            _energyBar = new EnergyBarsContainer(d3DEngine, playerEntityManager);
            _energyBar.LayoutFlags = ControlLayoutFlags.Skip;
            _energyBar.Bounds.Location = new UniVector(0, 0); //Always bound to top left location of the screen !

            _weatherContainer = new WeatherContainer(d3DEngine, weather, worldChunks, playerEntityManager);
            _weatherContainer.LayoutFlags = ControlLayoutFlags.Skip;
            _weatherContainer.Bounds.Location = new UniVector(0, 0); //Always bound to top left location of the screen !

            _screen.ToolTipShow += _screen_ToolTipShow;
            _screen.ToolTipHide += _screen_ToolTipHide;
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
                _screen.Desktop.Children.Add(_energyBar);
                _screen.Desktop.Children.Add(_weatherContainer);
                ToolbarUi.Locate(S33M3CoreComponents.GUI.Nuclex.Controls.ControlDock.HorisontalCenter | S33M3CoreComponents.GUI.Nuclex.Controls.ControlDock.VerticalBottom);
            }
            //the guimanager will draw the GUI screen, not the Hud !
        }

        public override void BeforeDispose()
        {
            if (_toolbarUi != null) _toolbarUi.Dispose();
            if (_spriteRender != null) _spriteRender.Dispose();
            if (_crosshair != null) _crosshair.Dispose();
            if (_font != null) _font.Dispose();
            if (_tooltip != null) _tooltip.Dispose();
            if (_weatherContainer != null) _weatherContainer.Dispose();
            if (_energyBar != null) _energyBar.Dispose();
            if (_d3DEngine != null) _d3DEngine.ScreenSize_Updated -= D3DEngineViewPortUpdated;
        }

        #region Public methods
        public override void EnableComponent(bool forced)
        {
            if (!AutoStateEnabled && !forced)
                return;

            if (!_screen.Desktop.Children.Contains(ToolbarUi))
                _screen.Desktop.Children.Add(ToolbarUi);

            if (!_screen.Desktop.Children.Contains(_energyBar))
            {
                _screen.Desktop.Children.Add(_energyBar);
            }

            if (!_screen.Desktop.Children.Contains(_weatherContainer))
            {
                _screen.Desktop.Children.Add(_weatherContainer);
            }

            var screenSize = new Vector2I((int)_d3DEngine.ViewPort.Width, (int)_d3DEngine.ViewPort.Height);

            ToolbarUi.Bounds.Location = new UniVector((screenSize.X - ToolbarUi.Bounds.Size.X.Offset) / 2, screenSize.Y - ToolbarUi.Bounds.Size.Y);

            base.EnableComponent();
        }

        public override void DisableComponent()
        {
            _screen.Desktop.Children.Remove(ToolbarUi);
            _screen.Desktop.Children.Remove(_energyBar);
            _screen.Desktop.Children.Remove(_weatherContainer);
            base.DisableComponent();
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            if (_inputManager.ActionsManager.isTriggered(UtopiaActions.ToggleInterface))
            {
                IsHidden = !IsHidden;
                if (IsHidden)
                {
                    _screen.HideAll();
                }
                else
                {
                    _screen.ShowAll();
                }
            }

            if (!DisableNumbersHandling)
            {
                foreach (var keyPressed in _inputManager.KeyboardManager.GetPressedChars())
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
                    }
                }
            }

            if (_inputManager.ActionsManager.isTriggered(UtopiaActions.ToolBarSelectPrevious))
            {
                if (_playerEntityManager.PlayerCharacter.Toolbar.Count(i => i != 0) < 2)
                    return;

                while (true)
                {
                    _selectedSlot--;

                    if (_selectedSlot == -1)
                        _selectedSlot = _playerEntityManager.PlayerCharacter.Toolbar.Count - 1;

                    if (_playerEntityManager.PlayerCharacter.Toolbar[_selectedSlot] != 0)
                        break;
                }

                SelectSlot(_selectedSlot);
            }

            else if (_inputManager.ActionsManager.isTriggered(UtopiaActions.ToolBarSelectNext))
            {
                if (_playerEntityManager.PlayerCharacter.Toolbar.Count(i => i != 0) < 2)
                    return;

                while (true)
                {
                    _selectedSlot++;

                    if (_selectedSlot == _playerEntityManager.PlayerCharacter.Toolbar.Count)
                        _selectedSlot = 0;

                    if (_playerEntityManager.PlayerCharacter.Toolbar[_selectedSlot] != 0)
                        break;
                }

                SelectSlot(_selectedSlot);
            }

            _toolbarUi.Update(timeSpend);
            _energyBar.Update(timeSpend);
            _weatherContainer.Update(timeSpend);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            _energyBar.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }

        //Draw at 2d level ! (Last draw called)
        public override void Draw(DeviceContext context, int index)
        {
            if (IsHidden) return;
            if (_camManager.ActiveCamera.CameraType == CameraType.FirstPerson)
            {
                _spriteRender.Begin(false, context);
                _spriteRender.Draw(_crosshair, ref _crosshair.ScreenPosition, ref _crosshair.ColorModifier);
                _spriteRender.End(context);
            }
        }

        #endregion

        #region Private methods
        //ToolBar UI management ==================================================================
        private void OnSlotClicked(SlotClickedEventArgs e)
        {
            if (SlotClicked != null) SlotClicked(this, e);
        }

        private void _screen_ToolTipHide(object sender, EventArgs e)
        {
            _tooltip.Close();
        }

        private void _screen_ToolTipShow(object sender, ToolTipEventArgs e)
        {
            var cell = e.Control as InventoryCell;

            if (cell != null)
            {
                _tooltip.SetText(cell.Slot.Item.Name, cell.Slot.Item.Description ?? "This item has no description, try to guess what is it.");
            }

            var bounds = e.Control.GetAbsoluteBounds();

            if (bounds.Bottom + _tooltip.Bounds.Size.Y.Offset > _screen.Height)
            {
                _tooltip.Bounds.Location = new UniVector(bounds.Left, bounds.Top - _tooltip.Bounds.Size.Y);
            }
            else
            {
                _tooltip.Bounds.Location = new UniVector(bounds.Left, bounds.Bottom);
            }

            _tooltip.Show(_screen);
            _tooltip.BringToFront();
        }

        private void SelectSlot(int index)
        {
            // equip the slot
            _selectedSlot = index;
            OnSlotClicked(new SlotClickedEventArgs { SlotIndex = index });
        }

        //=========================================================================================
        //Refresh location when the viewport widnows size is changing !
        private void D3DEngineViewPortUpdated(ViewportF viewport, Texture2DDescription newBackBufferDescr)
        {
            var screenSize = new Vector2I((int)_d3DEngine.ViewPort.Width, (int)_d3DEngine.ViewPort.Height);
            ToolbarUi.Bounds.Location = new UniVector((screenSize.X - ToolbarUi.Bounds.Size.X.Offset) / 2, screenSize.Y - ToolbarUi.Bounds.Size.Y);
        }
        #endregion
    }

    public class SlotClickedEventArgs : EventArgs
    {
        public int SlotIndex { get; set; }
    }
}