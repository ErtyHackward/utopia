using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ninject;
using S33M3DXEngine;
using Utopia.Action;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Network;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using S33M3DXEngine.Main;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.GUI;

namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Provides gameplay inventory functionality, allows to perform transfers between containers and provides toolbar shortcuts functionality
    /// </summary>
    public class InventoryComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly InputsManager _inputManager;
        private readonly GuiManager _guiManager;
        
        private readonly IconFactory _iconFactory;
        private readonly ItemMessageTranslator _itemMessageTranslator;
        private readonly Hud _hud;
        private readonly ToolBarUi _toolBar;
        private CharacterInventory _playerInventoryWindow;
        private InventoryWindow _containerInventoryWindow;

        private InventoryCell _dragControl;
        private InventoryCell _toolbarSlot;
        private Point _dragOffset;
        private ItemInfoWindow _infoWindow;


        private SlotContainer<ContainedSlot> _sourceContainer;

        [Inject]
        public PlayerEntityManager PlayerManager { get; set; }

        /// <summary>
        /// Indicates if inventory is active now
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets or sets current player inventory window that will popup when user press inventory key
        /// </summary>
        public CharacterInventory PlayerInventoryWindow
        {
            get { return _playerInventoryWindow; }
            set {
                if (_playerInventoryWindow == value)
                    return;
                if (_playerInventoryWindow != null)
                {
                    UnregisterInventoryWindow(_playerInventoryWindow);
                }
                _playerInventoryWindow = value;
                if (_playerInventoryWindow != null)
                {
                    RegisterInventoryWindow(_playerInventoryWindow);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets optional container inventory window that will popup on container operations
        /// </summary>
        public InventoryWindow ContainerInventoryWindow
        {
            get { return _containerInventoryWindow; }
            set 
            {
                if (_containerInventoryWindow == value)
                    return;

                if (_containerInventoryWindow != null)
                {
                    UnregisterInventoryWindow(_containerInventoryWindow);
                }

                _containerInventoryWindow = value;

                if (_containerInventoryWindow != null)
                {
                    RegisterInventoryWindow(_containerInventoryWindow);
                }
            }
        }

        /// <summary>
        /// Occurs when user is requesting inventory or want to close it
        /// </summary>
        public event EventHandler<InventorySwitchEventArgs> SwitchInventory;

        private void OnSwitchInventory(bool closed)
        {
            var handler = SwitchInventory;
            if (handler != null) handler(this, new InventorySwitchEventArgs { Closing = closed });
        }


        public InventoryComponent(
            D3DEngine engine,
            InputsManager inputManager, 
            GuiManager guiManager, 
            IconFactory iconFactory,
            ItemMessageTranslator itemMessageTranslator, 
            Hud hud)
        {

            IsDefferedLoadContent = true;

            _engine = engine;
            _inputManager = inputManager;
            _guiManager = guiManager;
            _iconFactory = iconFactory;
            _itemMessageTranslator = itemMessageTranslator;
            _hud = hud;
            _toolBar = hud.ToolbarUi;
            _toolBar.SlotEnter += _toolBar_SlotEnter;
            _toolBar.SlotLeave += _toolBar_SlotLeave;

            _hud.SlotClicked += HudSlotClicked;
            _guiManager.Screen.Desktop.Clicked += DesktopClicked;

            _itemMessageTranslator.Enabled = false;

            _dragOffset = new Point(InventoryWindow.CellSize / 2, InventoryWindow.CellSize / 2);
        }

        void _toolBar_SlotLeave(object sender, InventoryWindowCellMouseEventArgs e)
        {
            _toolbarSlot = null;
        }

        void _toolBar_SlotEnter(object sender, InventoryWindowCellMouseEventArgs e)
        {
            _toolbarSlot = e.Cell;
        }
        
        public override void BeforeDispose()
        {
            _hud.SlotClicked -= HudSlotClicked;
            _guiManager.Screen.Desktop.Clicked -= DesktopClicked;
        }

        /// <summary>
        /// Allows the inventory window to perform inventory transfers (chest - player inventory, chest - chest etc)
        /// </summary>
        /// <param name="window"></param>
        public void RegisterInventoryWindow(InventoryWindow window)
        {
            // listen character window events
            if (window is CharacterInventory)
            {
                var charInventory = window as CharacterInventory;
                //charInventory.EquipmentSlotClicked += InventoryUiSlotClicked;
            }

            // listen standart inventory events
            //window.InventorySlotClicked += InventoryUiSlotClicked;
            window.CellMouseEnter += InventoryUiCellMouseEnter;
            window.CellMouseLeave += InventoryUiCellMouseLeave;
            window.CellMouseUp += charInventory_CellMouseUp;
            window.CellMouseDown += charInventory_CellMouseDown;
        }
        
        /// <summary>
        /// Removes all event handlers from the window, use this method when the windows is removed or hidden
        /// </summary>
        /// <param name="window"></param>
        public void UnregisterInventoryWindow(InventoryWindow window)
        {
            // remove all event handlers
            if (window is CharacterInventory)
            {
                var charInventory = window as CharacterInventory;
                //charInventory.EquipmentSlotClicked -= InventoryUiSlotClicked;
            }

            //window.InventorySlotClicked -= InventoryUiSlotClicked;
            window.CellMouseEnter -= InventoryUiCellMouseEnter;
            window.CellMouseLeave -= InventoryUiCellMouseLeave;
            window.CellMouseUp -= charInventory_CellMouseUp;
            window.CellMouseDown -= charInventory_CellMouseDown;
        }

        public override void LoadContent(DeviceContext context)
        {
            _infoWindow = new ItemInfoWindow(_iconFactory, _inputManager);

            _dragControl = new InventoryCell(null, _iconFactory, new Vector2I(), _inputManager)
            {
                Bounds = new UniRectangle(-100, -100, InventoryWindow.CellSize, InventoryWindow.CellSize),
                DrawCellBackground = false,
                IsClickTransparent = true, 
                DrawGroupId = 1
            };
        }

        private void DesktopClicked(object sender, EventArgs e)
        {
            if (IsActive && _dragControl.Slot != null)
            {
                // drop item to the world
                _itemMessageTranslator.DropToWorld();
                EndDrag();
            }
        }

        private void HudSlotClicked(object sender, SlotClickedEventArgs e)
        {
            var enabled = _itemMessageTranslator.Enabled;

            try
            {
                _itemMessageTranslator.Enabled = true;
                if (PlayerManager.Player.Toolbar[e.SlotIndex] != 0)
                {
                    var player = PlayerManager.Player;
                    var entityId = player.Toolbar[e.SlotIndex];
                    
                    // find the entity
                    var slot = player.Inventory.Find(entityId);

                    if (slot == null)
                        return;

                    slot = player.Inventory.PeekSlot(slot.GridPosition);

                    if (slot != null)
                    {
                        ContainedSlot taken;
                        player.Inventory.TakeItem(slot.GridPosition, slot.ItemsCount);
                        player.Equipment.Equip(EquipmentSlotType.Hand, slot, out taken);

                        if (taken != null)
                        {
                            player.Inventory.PutItem(taken.Item, slot.GridPosition, taken.ItemsCount);
                        }
                    }
                }
            }
            finally
            {
                _itemMessageTranslator.Enabled = enabled;
            }
        }

        private void ToolBarSlotClicked(object sender, InventoryWindowCellMouseEventArgs e)
        {
            if (_dragControl.Slot == null)
                return;
            if (_dragControl.Slot.Item is ITool)
            {
                PlayerManager.Player.Toolbar[e.Cell.InventoryPosition.Y] = _dragControl.Slot.Item.StaticId;
                _toolBar.SetSlot(e.Cell.InventoryPosition.Y, new ContainedSlot {
                    Item = _dragControl.Slot.Item,
                    GridPosition = e.Cell.InventoryPosition
                });

                _itemMessageTranslator.SetToolBar(e.Cell.InventoryPosition.Y, _dragControl.Slot.Item.StaticId);

                _sourceContainer.PutItem(_dragControl.Slot.Item, _dragControl.Slot.GridPosition, _dragControl.Slot.ItemsCount);

                _guiManager.Screen.Desktop.Children.Remove(_dragControl);
                _dragControl.Slot = null;
                _sourceContainer = null;
            }
        }

        private void InventoryUiCellMouseLeave(object sender, InventoryWindowCellMouseEventArgs e)
        {
            _infoWindow.ActiveItem = null;
        }

        private void InventoryUiCellMouseEnter(object sender, InventoryWindowCellMouseEventArgs e)
        {
            if (e.Cell.Slot == null)
                return;
            
            _infoWindow.ActiveItem = e.Cell.Slot.Item;
        }

        void charInventory_CellMouseDown(object sender, InventoryWindowEventArgs e)
        {
            var keyboard = _inputManager.KeyboardManager.CurKeyboardState;
            if (_dragControl.Slot == null)
            {
                // taking item
                var slot = e.Container.PeekSlot(e.SlotPosition);

                if (slot != null)
                {
                    var itemsCount = slot.ItemsCount;

                    if (e.MouseState.RightButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Pressed)
                        itemsCount = 1;

                    if (keyboard.IsKeyDown(Keys.ShiftKey) && itemsCount > 1)
                        itemsCount = slot.ItemsCount / 2;

                    if (!e.Container.TakeItem(slot.GridPosition, itemsCount))
                        throw new InvalidOperationException();

                    slot.ItemsCount = itemsCount;

                    BeginDrag(slot);

                    _sourceContainer = e.Container;
                }
            }
        }

        void charInventory_CellMouseUp(object sender, InventoryWindowEventArgs e)
        {
            var keyboard = _inputManager.KeyboardManager.CurKeyboardState;

            if (_dragControl.Slot != null)
            {
                if (e == null)
                {
                    if (_toolbarSlot != null)
                    {
                        ToolBarSlotClicked(sender, new InventoryWindowCellMouseEventArgs { Cell = _toolbarSlot });
                    }

                    CancelDrag();
                    return;
                }
                
                // put item

                // check slot we want to put to
                var slot = e.Container.PeekSlot(e.SlotPosition);

                if (slot != null && !slot.CanStackWith(_dragControl.Slot))
                {
                    // exchange
                    _dragControl.Slot.GridPosition = e.SlotPosition;
                    ContainedSlot slotTaken;
                    if (!e.Container.PutItemExchange(_dragControl.Slot.Item, _dragControl.Slot.GridPosition, _dragControl.Slot.ItemsCount, out slotTaken))
                        throw new InvalidOperationException();
                    UpdateDrag(slotTaken);
                    CancelDrag();
                }
                else
                {
                    // just put, gonna be okay, da da doo-doo-mmm
                    _dragControl.Slot.GridPosition = e.SlotPosition;

                    var itemsCount = _dragControl.Slot.ItemsCount;

                    if (e.MouseState.RightButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Pressed)
                    {
                        if (keyboard.IsKeyDown(Keys.ShiftKey) && itemsCount > 1)
                            itemsCount = itemsCount / 2;
                        else
                            itemsCount = 1;
                    }

                    if (!e.Container.PutItem(_dragControl.Slot.Item, _dragControl.Slot.GridPosition, itemsCount))
                        throw new InvalidOperationException();

                    _dragControl.Slot.ItemsCount -= itemsCount;
                    if (_dragControl.Slot.ItemsCount == 0)
                        EndDrag();
                    else _dragControl.BringToFront();
                }

            }
        }

        private void BeginDrag(ContainedSlot slot)
        {
            _dragControl.Slot = slot;
            _guiManager.Screen.Desktop.Children.Add(_dragControl);
            _dragControl.BringToFront();
        }

        private void UpdateDrag(ContainedSlot slot)
        {
            _dragControl.Slot = slot;
            _dragControl.BringToFront();
        }

        private void CancelDrag()
        {
            // return item on the place
            if (_dragControl.Slot != null)
            {
                _sourceContainer.PutItem(_dragControl.Slot.Item, _dragControl.Slot.GridPosition,
                                         _dragControl.Slot.ItemsCount);
            }
            EndDrag();
        }

        private void EndDrag()
        {
            _guiManager.Screen.Desktop.Children.Remove(_dragControl);
            _dragControl.Slot = null;
        }

        public override void Update(GameTime timeSpend)
        {
            var mouseState = _inputManager.MouseManager.CurMouseState;
            _dragControl.Bounds.Location = new UniVector(new UniScalar(mouseState.X - _dragOffset.X), new UniScalar(mouseState.Y - _dragOffset.Y));
            
            if (_inputManager.ActionsManager.isTriggered(UtopiaActions.OpenInventory) && _playerInventoryWindow != null)
            {
                if (IsActive) 
                    HideInventory();
                else
                    ShowInventory();
            }
        }

        /// <summary>
        /// Shows player inventory window and optinally container window
        /// </summary>
        /// <param name="otherParty"></param>
        public void ShowInventory(Container otherParty = null)
        {
            var windows = new List<InventoryWindow>();


            //_guiManager.Screen.Desktop.Children.Add(_infoWindow);
            var desktop = _guiManager.Screen.Desktop;

            windows.Add(_playerInventoryWindow);

            if (otherParty != null)
            {
                if (_containerInventoryWindow == null)
                    throw new InvalidOperationException("Unable to open container inventory because no inventory windows is associated");

                _containerInventoryWindow.Content = otherParty.Content;
                windows.Add(_containerInventoryWindow);
            }
            
            // show windows
            foreach (var inventoryWindow in windows)
            {
                desktop.Children.Add(inventoryWindow);
            }
            
            // locate windows on the screen
            var horisontalFreeSpace = Math.Max((int)_engine.ViewPort.Width - windows.Sum(w => (int)w.Bounds.Size.X.Offset), 0);
            var horisontalSpace = horisontalFreeSpace / (1 + windows.Count);
            
            int horisontalCurrentPos = horisontalSpace;
            for (int i = 0; i < windows.Count; i++)
            {
                var inventoryWindow = windows[i];

                var verticalSpace = Math.Max((int)_engine.ViewPort.Height - inventoryWindow.Bounds.Size.Y.Offset, 0) / 2;

                inventoryWindow.Bounds.Location.X = horisontalCurrentPos;
                inventoryWindow.Bounds.Location.Y = verticalSpace;

                horisontalCurrentPos += horisontalSpace + (int)inventoryWindow.Bounds.Size.X.Offset;
            }


            _itemMessageTranslator.Enabled = true;
            _inputManager.ActionsManager.IsMouseExclusiveMode = true;
            _guiManager.ForceExclusiveMode = true;
            _inputManager.MouseManager.MouseCapture = false;
            IsActive = true;

            OnSwitchInventory(false);
        }

        public void HideInventory()
        {
            if (_containerInventoryWindow != null)
            {
                if (_guiManager.Screen.Desktop.Children.Contains(_containerInventoryWindow))
                {
                    _guiManager.Screen.Desktop.Children.Remove(_containerInventoryWindow);
                }
            }

            //_guiManager.Screen.Desktop.Children.Remove(_infoWindow);
            _guiManager.Screen.Desktop.Children.Remove(_playerInventoryWindow);
            _itemMessageTranslator.Enabled = false;
            //Has this is A gui component, its own windows will automatically by protected for events going "through" it,
            //But in this case, I need to prevent ALL event to be sent while this component is activated
            _inputManager.ActionsManager.IsMouseExclusiveMode = false;
            _guiManager.ForceExclusiveMode = false;
            _inputManager.MouseManager.MouseCapture = true;
            IsActive = false;

            OnSwitchInventory(true);
        }

    }

    public class InventorySwitchEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates that inventory want to be closed :)
        /// </summary>
        public bool Closing { get; set; }
    }
}
