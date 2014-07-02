using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ninject;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.Sound;
using S33M3DXEngine;
using S33M3Resources.Structs.Vertex;
using Utopia.Action;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Entities.Renderer;
using Utopia.GUI.Crafting;
using Utopia.Network;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using S33M3DXEngine.Main;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.GUI;
using Utopia.Shared.Settings;
using Utopia.Shared.World;

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

        private readonly CubeRenderer _cubeRenderer;
        private readonly IconFactory _iconFactory;
        private ToolBarUi _toolBar;
        private CharacterInventory _playerInventoryWindow;
        private ContainerWindow _containerInventoryWindow;

        private InventoryCell _dragControl;
        private InventoryCell _hoverSlot;
        private Point _dragOffset;
        private ItemInfoWindow _infoWindow;
        private bool _inventoryActive;

        private SlotContainer<ContainedSlot> _sourceContainer;
        private ItemMessageTranslator _itemMessageTranslator;

        /// <summary>
        /// Indicates if inventory is active now
        /// </summary>
        public bool IsActive {
            get { return _inventoryActive; }
        }

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
        public ContainerWindow ContainerInventoryWindow
        {
            get { return _containerInventoryWindow; }
            set 
            {
                if (_containerInventoryWindow == value)
                    return;

                if (_containerInventoryWindow != null)
                {
                    UnregisterInventoryWindow(_containerInventoryWindow);
                    _containerInventoryWindow.CraftButton.Pressed -= CraftButton_Pressed;
                }

                _containerInventoryWindow = value;

                if (_containerInventoryWindow != null)
                {
                    RegisterInventoryWindow(_containerInventoryWindow);
                    _containerInventoryWindow.CraftButton.Pressed += CraftButton_Pressed;
                }
            }
        }

        void CraftButton_Pressed(object sender, EventArgs e)
        {
            if (_containerInventoryWindow.CanCraft)
            {
                var recipe = (Recipe)_containerInventoryWindow.RecipesList.SelectedItem;
                var recipeIndex = _containerInventoryWindow.Player.EntityFactory.Config.Recipes.IndexOf(recipe);

                var enabled = ItemMessageTranslator.Enabled;

                ItemMessageTranslator.Enabled = false;
                _containerInventoryWindow.Player.EntityState.PickedEntityLink = _containerInventoryWindow.Container.GetLink();
                _containerInventoryWindow.Player.CraftUse(recipeIndex);
                ItemMessageTranslator.Enabled = enabled;
                _containerInventoryWindow.Update();
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


        public bool IsToolbarSwitching { get; set; }

        [Inject]
        public ItemMessageTranslator ItemMessageTranslator
        {
            get { return _itemMessageTranslator; }
            set { 
                _itemMessageTranslator = value;
                _itemMessageTranslator.Enabled = false;
            }
        }

        [Inject]
        public PlayerEntityManager PlayerManager { get; set; }

        [Inject]
        public ISoundEngine SoundEngine { get; set; }

        [Inject]
        public Hud Hud { get; set; }


        public InventoryComponent(
            D3DEngine engine,
            InputsManager inputManager, 
            GuiManager guiManager, 
            IconFactory iconFactory,
            VisualWorldParameters worldParameters)
        {

            IsDefferedLoadContent = true;

            _engine = engine;
            _inputManager = inputManager;
            _guiManager = guiManager;
            _iconFactory = iconFactory;
            
            _guiManager.Screen.Desktop.Clicked += DesktopClicked;

            _cubeRenderer = new CubeRenderer(engine, worldParameters);

            _dragOffset = new Point(InventoryWindow.CellSize / 2, InventoryWindow.CellSize / 2);
        }

        private void OnSlotTaken(ContainedSlot slot)
        {

        }

        private void OnSlotPut(ContainedSlot slot)
        {
            if (slot.Item.PutSound != null)
            {
                SoundEngine.StartPlay2D(slot.Item.PutSound);
            }
        }

        void _toolBar_SlotLeave(object sender, InventoryWindowCellMouseEventArgs e)
        {
            _hoverSlot = null;
        }

        void _toolBar_SlotEnter(object sender, InventoryWindowCellMouseEventArgs e)
        {
            _hoverSlot = e.Cell;
        }
        
        public override void BeforeDispose()
        {
            Hud.SlotClicked -= HudSlotClicked;
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

        public override void Initialize()
        {
            _toolBar = Hud.ToolbarUi;
            _toolBar.SlotEnter += _toolBar_SlotEnter;
            _toolBar.SlotLeave += _toolBar_SlotLeave;

            Hud.SlotClicked += HudSlotClicked;
        }

        public override void LoadContent(DeviceContext context)
        {
            _infoWindow = new ItemInfoWindow(_iconFactory, _inputManager);

            _cubeRenderer.LoadContent(context);

            ContainerInventoryWindow.CubeRenderer = _cubeRenderer;
            ContainerInventoryWindow.VoxelEffect = ToDispose(new HLSLVoxelModel(_engine.Device, Path.Combine(ClientSettings.EffectPack, @"Entities\VoxelModel.hlsl"), VertexVoxel.VertexDeclaration));
            
            _dragControl = new InventoryCell(null, _iconFactory, new Vector2I(), _inputManager)
            {
                Bounds = new UniRectangle(-100, -100, InventoryWindow.CellSize, InventoryWindow.CellSize),
                DrawCellBackground = false,
                IsClickTransparent = true, 
                DrawGroupId = 1
            };
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            _cubeRenderer.Dispose();

            base.Dispose(disposeManagedResources);
        }

        private void DesktopClicked(object sender, EventArgs e)
        {
            if (IsActive && _dragControl.Slot != null)
            {
                // drop item to the world
                ItemMessageTranslator.DropToWorld();
                EndDrag();
            }
        }

        private void HudSlotClicked(object sender, SlotClickedEventArgs e)
        {
            var enabled = ItemMessageTranslator.Enabled;

            try
            {
                IsToolbarSwitching = true;
                ItemMessageTranslator.Enabled = true;
                

                if (PlayerManager.PlayerCharacter.Toolbar[e.SlotIndex] != 0)
                {
                    var player = PlayerManager.PlayerCharacter;
                    var blueprintId = player.Toolbar[e.SlotIndex];
                    
                    // find the entity
                    var slot = player.Inventory.FirstOrDefault(s => s.Item.BluePrintId == blueprintId);

                    if (slot == null)
                        return;
                    
                    
                    ContainedSlot taken;
                    player.Inventory.TakeItem(slot.GridPosition, slot.ItemsCount);
                    player.Equipment.Equip(EquipmentSlotType.Hand, slot, out taken);

                    if (taken != null)
                    {
                        bool returned = false;
                        var prevPos = player.ActiveToolInventoryPosition;

                        // try to put item on previous pos
                        if (prevPos.X >= 0 && prevPos.Y >= 0)
                        {
                            if (player.Inventory.PeekSlot(prevPos) == null)
                            {
                                returned = player.Inventory.PutItem(taken.Item, prevPos, taken.ItemsCount);
                            }
                        }

                        // do exchange if we have no place to return
                        if (!returned)
                            player.Inventory.PutItem(taken.Item, slot.GridPosition, taken.ItemsCount);
                    }

                    player.ActiveToolInventoryPosition = slot.GridPosition;
                }
            }
            finally
            {
                IsToolbarSwitching = false;
                ItemMessageTranslator.Enabled = enabled;
            }
        }


        private void ToolBarSlotClicked(object sender, InventoryWindowCellMouseEventArgs e)
        {
            if (_dragControl.Slot == null)
                return;

            PlayerManager.PlayerCharacter.Toolbar[e.Cell.InventoryPosition.Y] = _dragControl.Slot.Item.BluePrintId;
            _toolBar.SetSlot(e.Cell.InventoryPosition.Y, new ContainedSlot
                {
                    Item = _dragControl.Slot.Item,
                    GridPosition = e.Cell.InventoryPosition
                });

            ItemMessageTranslator.SetToolBar(e.Cell.InventoryPosition.Y, _dragControl.Slot.Item.StaticId);

            _sourceContainer.PutItem(_dragControl.Slot.Item, _dragControl.Slot.GridPosition,
                                     _dragControl.Slot.ItemsCount);

            _guiManager.Screen.Desktop.Children.Remove(_dragControl);
            _dragControl.Slot = null;
            _sourceContainer = null;
        }

        private void InventoryUiCellMouseLeave(object sender, InventoryWindowCellMouseEventArgs e)
        {
            _hoverSlot = null;
            _infoWindow.ActiveItem = null;
        }

        private void InventoryUiCellMouseEnter(object sender, InventoryWindowCellMouseEventArgs e)
        {
            _hoverSlot = e.Cell;

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

                    OnSlotTaken(slot);

                }
            }
        }

        void charInventory_CellMouseUp(object sender, InventoryWindowEventArgs e)
        {
            var keyboard = _inputManager.KeyboardManager.CurKeyboardState;

            if (_dragControl.Slot != null)
            {

                if (_hoverSlot == null)
                {
                    CancelDrag();
                    return;
                }

                if (_hoverSlot.Parent == _toolBar)
                {
                    ToolBarSlotClicked(sender, new InventoryWindowCellMouseEventArgs { Cell = _hoverSlot });
                    CancelDrag();
                    return;
                }

                e = ( (InventoryWindow)_hoverSlot.Parent ).CreateEventArgs(_hoverSlot);
                
                // put item

                // check slot we want to put to
                var slot = e.Container.PeekSlot(e.SlotPosition);

                if (slot != null && !slot.CanStackWith(_dragControl.Slot))
                {
                    // exchange

                    // first check if we can perform exchange here, we can't if we took not the whole pack
                    var srcSlot = _sourceContainer.PeekSlot(_dragControl.Slot.GridPosition);
                    if (srcSlot != null)
                    {
                        CancelDrag();
                    }
                    else
                    {
                        var prevPosition = _dragControl.Slot.GridPosition;
                        _dragControl.Slot.GridPosition = e.SlotPosition;
                        ContainedSlot slotTaken;
                        if (!e.Container.PutItemExchange(_dragControl.Slot.Item, _dragControl.Slot.GridPosition, _dragControl.Slot.ItemsCount, out slotTaken))
                            throw new InvalidOperationException();

                        OnSlotPut(_dragControl.Slot);

                        slotTaken.GridPosition = prevPosition;
                        UpdateDrag(slotTaken);
                        CancelDrag();
                    }
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

                    OnSlotPut(_dragControl.Slot);

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
                if (!_sourceContainer.PutItem(_dragControl.Slot.Item, _dragControl.Slot.GridPosition, _dragControl.Slot.ItemsCount))
                    throw new InvalidOperationException();
            }
            EndDrag();
        }

        private void EndDrag()
        {
            _guiManager.Screen.Desktop.Children.Remove(_dragControl);
            _dragControl.Slot = null;
        }

        public override void FTSUpdate(GameTime timeSpend)
        {
            var mouseState = _inputManager.MouseManager.CurMouseState;
            _dragControl.Bounds.Location = new UniVector(new UniScalar(mouseState.X - _dragOffset.X), new UniScalar(mouseState.Y - _dragOffset.Y));
        }

        /// <summary>
        /// Shows player inventory window and optinally container window
        /// </summary>
        /// <param name="otherParty"></param>
        public void ShowInventory(Container otherParty = null)
        {
            var windows = new List<InventoryWindow>();
            
            var desktop = _guiManager.Screen.Desktop;

            windows.Add(_playerInventoryWindow);

            if (otherParty != null)
            {
                if (_containerInventoryWindow == null)
                    throw new InvalidOperationException("Unable to open container inventory because no inventory windows is associated");

                _containerInventoryWindow.Container = otherParty;
                windows.Add(_containerInventoryWindow);
            }
            
            // show windows
            foreach (var inventoryWindow in windows)
            {
                inventoryWindow.LayoutFlags = ControlLayoutFlags.Center;
                inventoryWindow.IsVisible = true;
                desktop.Children.Add(inventoryWindow);
            }
            
            desktop.UpdateLayout();

            if (otherParty != null && _containerInventoryWindow != null)
            {
                _containerInventoryWindow.Update();
            }

            ItemMessageTranslator.Enabled = true;
            _inventoryActive = true;

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

            _guiManager.Screen.Desktop.Children.Remove(_playerInventoryWindow);
            ItemMessageTranslator.Enabled = false;
            _inventoryActive = false;

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
