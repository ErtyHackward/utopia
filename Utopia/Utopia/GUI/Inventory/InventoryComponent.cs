﻿using System;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Network;
using Utopia.Settings;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Sprites;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;
using Utopia.Action;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.GUI;

namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Provides gameplay inventory functionality
    /// </summary>
    public class InventoryComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly InputsManager _inputManager;
        private readonly GuiManager _guiManager;
        private readonly PlayerEntityManager _playerManager;
        private readonly IconFactory _iconFactory;
        private readonly ItemMessageTranslator _itemMessageTranslator;
        private readonly Hud _hud;
        private readonly ToolBarUi _toolBar;

        private PlayerInventory _inventoryUi;
        private SpriteTexture _backgroundTex;
        private InventoryCell _dragControl;
        private Point _dragOffset;
        private ItemInfoWindow _infoWindow;

        private SlotContainer<ContainedSlot> _sourceContainer;

        /// <summary>
        /// Indicates if inventory is active now
        /// </summary>
        public bool IsActive { get; private set; }

        public InventoryComponent(
            D3DEngine engine, 
            InputsManager inputManager, 
            GuiManager guiManager, 
            PlayerEntityManager playerManager, 
            IconFactory iconFactory,
            ItemMessageTranslator itemMessageTranslator, 
            Hud hud)
        {

            this.IsDefferedLoadContent = true;

            _engine = engine;
            _inputManager = inputManager;
            _guiManager = guiManager;
            _playerManager = playerManager;
            _iconFactory = iconFactory;
            _itemMessageTranslator = itemMessageTranslator;
            _hud = hud;
            _toolBar = hud.ToolbarUi;

            _hud.SlotClicked += HudSlotClicked;
            _guiManager.Screen.Desktop.Clicked += DesktopClicked;

            _itemMessageTranslator.Enabled = false;

            _dragOffset = new Point(InventoryWindow.CellSize / 2, InventoryWindow.CellSize / 2);
        }

        void DesktopClicked(object sender, EventArgs e)
        {
            if (IsActive && _dragControl.Slot != null)
            {
                // drop item to the world
                _itemMessageTranslator.DropToWorld();
                EndDrag();
            }
        }

        void HudSlotClicked(object sender, SlotClickedEventArgs e)
        {
            var enabled = _itemMessageTranslator.Enabled;

            try
            {
                _itemMessageTranslator.Enabled = true;
                if (_playerManager.Player.Toolbar[e.SlotIndex] != 0)
                {
                    var player = _playerManager.Player;
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
                        player.Equipment.Equip(EquipmentSlotType.LeftHand, slot, out taken);

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

        public override void LoadContent(DeviceContext context)
        {
            _backgroundTex = new SpriteTexture(_engine.Device, ClientSettings.TexturePack + @"charactersheet.png", Vector2I.Zero);

            _infoWindow = new ItemInfoWindow(_iconFactory, _inputManager);

            _inventoryUi = new PlayerInventory(_backgroundTex, _playerManager.Player, _iconFactory, new Point(180, 120), _inputManager);
            _inventoryUi.InventorySlotClicked += InventoryUiSlotClicked;
            _inventoryUi.EquipmentSlotClicked += InventoryUiSlotClicked;
            _inventoryUi.CellMouseEnter += InventoryUiCellMouseEnter;
            _inventoryUi.CellMouseLeave += InventoryUiCellMouseLeave;
            _toolBar.SlotClicked += ToolBarSlotClicked;

            _dragControl = new InventoryCell(null, _iconFactory, new Vector2I(), _inputManager)
            {
                Bounds = new UniRectangle(-100, -100, InventoryWindow.CellSize, InventoryWindow.CellSize),
                DrawCellBackground = false,
                IsClickTransparent = true
            };
        }

        void ToolBarSlotClicked(object sender, InventoryWindowCellMouseEventArgs e)
        {
            if (_dragControl.Slot == null)
                return;
            if (_dragControl.Slot.Item is ITool)
            {
                _playerManager.Player.Toolbar[e.Cell.InventoryPosition.Y] = _dragControl.Slot.Item.StaticId;
                _toolBar.SetSlot(e.Cell.InventoryPosition.Y, new ContainedSlot { Item = _dragControl.Slot.Item, GridPosition = e.Cell.InventoryPosition });

                _itemMessageTranslator.SetToolBar(e.Cell.InventoryPosition.Y, _dragControl.Slot.Item.StaticId);

                _sourceContainer.PutItem(_dragControl.Slot.Item, _dragControl.Slot.GridPosition, _dragControl.Slot.ItemsCount);

                _guiManager.Screen.Desktop.Children.Remove(_dragControl);
                _dragControl.Slot = null;

                _sourceContainer = null;
            }

        }

        void InventoryUiCellMouseLeave(object sender, InventoryWindowCellMouseEventArgs e)
        {
            _infoWindow.ActiveItem = null;
        }

        void InventoryUiCellMouseEnter(object sender, InventoryWindowCellMouseEventArgs e)
        {
            if (e.Cell.Slot == null)
                return;
            
            _infoWindow.ActiveItem = e.Cell.Slot.Item;
        }


        void InventoryUiSlotClicked(object sender, InventoryWindowEventArgs e)
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
            else
            {
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

        private void EndDrag()
        {
            _guiManager.Screen.Desktop.Children.Remove(_dragControl);
            _dragControl.Slot = null;
        }

        public override void Update(GameTime timeSpend)
        {

            //if (_dragControl.Slot != null)
            {
                var mouseState = _inputManager.MouseManager.CurMouseState;
                _dragControl.Bounds.Location = new UniVector(new UniScalar(mouseState.X - _dragOffset.X), new UniScalar(mouseState.Y - _dragOffset.Y));
            }

            if (_inputManager.ActionsManager.isTriggered(UtopiaActions.OpenInventory))
            {
                if (_guiManager.Screen.Desktop.Children.Contains(_inventoryUi))
                {
                    _guiManager.Screen.Desktop.Children.Remove(_infoWindow);
                    _guiManager.Screen.Desktop.Children.Remove(_inventoryUi);
                    _itemMessageTranslator.Enabled = false;
                    //Has this is A gui component, its own windows will automatically by protected for events going "through" it,
                    //But in this case, I need to prevent ALL event to be sent while this component is activated
                    _inputManager.ActionsManager.IsMouseExclusiveMode = false;
                    _guiManager.ForceExclusiveMode--;
                    _inputManager.MouseManager.MouseCapture = true;
                    IsActive = false;
                }
                else
                {
                    _guiManager.Screen.Desktop.Children.Add(_infoWindow);
                    _guiManager.Screen.Desktop.Children.Add(_inventoryUi);
                    _itemMessageTranslator.Enabled = true;
                    _inputManager.ActionsManager.IsMouseExclusiveMode = true;
                    _guiManager.ForceExclusiveMode++;
                    _inputManager.MouseManager.MouseCapture = false;
                    IsActive = true;
                }
            }
        }

        public override void Dispose()
        {
            _hud.SlotClicked -= HudSlotClicked;
            _inventoryUi.InventorySlotClicked -= InventoryUiSlotClicked;
            _guiManager.Screen.Desktop.Clicked -= DesktopClicked;
            _backgroundTex.Dispose();
        }
    }
}
