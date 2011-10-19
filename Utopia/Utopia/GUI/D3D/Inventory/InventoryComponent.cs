﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Nuclex.UserInterface;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.KeyboardHelper;
using S33M3Engines.Shared.Sprites;
using SharpDX;
using Utopia.Action;
using Utopia.Entities;
using Utopia.Entities.Managers;
using Utopia.Network;
using Utopia.Settings;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;
using Screen = Nuclex.UserInterface.Screen;

namespace Utopia.GUI.D3D.Inventory
{
    /// <summary>
    /// Provides gameplay inventory functionality
    /// </summary>
    public class InventoryComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly ActionsManager _actionManager;
        private readonly Screen _screen;
        private readonly PlayerEntityManager _playerManager;
        private readonly IconFactory _iconFactory;
        private readonly ItemMessageTranslator _itemMessageTranslator;

        private InventoryWindow _inventoryUi;
        private SpriteTexture _backgroundTex;
        private InventoryCell _dragControl;
        private Point _dragOffset;

        public InventoryComponent(
            D3DEngine engine, 
            ActionsManager actionManager, 
            Screen screen, 
            PlayerEntityManager playerManager, 
            IconFactory iconFactory,
            ItemMessageTranslator itemMessageTranslator)
        {
            _engine = engine;
            _actionManager = actionManager;
            _screen = screen;
            _playerManager = playerManager;
            _iconFactory = iconFactory;
            _itemMessageTranslator = itemMessageTranslator;

            _itemMessageTranslator.Enabled = false;
        }

        public override void LoadContent()
        {
            _backgroundTex = new SpriteTexture(_engine.Device, ClientSettings.TexturePack + @"charactersheet.png", new Vector2(0, 0));
            _inventoryUi = new PlayerInventory(_backgroundTex, _playerManager.Player.Inventory, _iconFactory, new Point(280, 120));
            _inventoryUi.SlotClicked += InventoryUiSlotClicked;
            _dragControl = new InventoryCell(null, _iconFactory, new Vector2I()) 
            { 
                Bounds = new UniRectangle(0, 0, InventoryWindow.CellSize, InventoryWindow.CellSize),
                IsClickTransparent = true
            };
        }

        void InventoryUiSlotClicked(object sender, InventoryWindowEventArgs e)
        {
            var keyboard = Keyboard.GetState();
            if (_dragControl.Slot == null)
            {
                // taking item
                var slot = e.Container.PeekSlot(e.SlotPosition);


                if (slot != null)
                {
                    var itemsCount = slot.ItemsCount;

                    if (e.MouseState.RightButton == S33M3Engines.InputHandler.MouseHelper.ButtonState.Pressed)
                        itemsCount = 1;

                    if (keyboard.IsKeyDown(Keys.ShiftKey) && itemsCount > 1)
                        itemsCount = slot.ItemsCount / 2;

                    if (!e.Container.TakeItem(slot.GridPosition, itemsCount))
                        throw new InvalidOperationException();

                    slot.ItemsCount = itemsCount;

                    BeginDrag(slot, e.Offset);
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

                    if (e.MouseState.RightButton == S33M3Engines.InputHandler.MouseHelper.ButtonState.Pressed)
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

        private void BeginDrag(ContainedSlot slot, Point offset)
        {
            _dragControl.Slot = slot;
            _dragOffset = new Point(InventoryWindow.CellSize / 2, InventoryWindow.CellSize / 2);  // offset;
            _screen.Desktop.Children.Add(_dragControl);
            _dragControl.BringToFront();
        }

        private void UpdateDrag(ContainedSlot slot)
        {
            _dragControl.Slot = slot;
            _dragControl.BringToFront();
        }

        private void EndDrag()
        {
            _screen.Desktop.Children.Remove(_dragControl);
            _dragControl.Slot = null;
        }

        public override void Update(ref GameTime timeSpent)
        {

            if (_dragControl.Slot != null)
            {
                var mouseState = Mouse.GetState();
                _dragControl.Bounds.Location = new UniVector(new UniScalar(mouseState.X - _dragOffset.X), new UniScalar(mouseState.Y - _dragOffset.Y));
            }

            if (_actionManager.isTriggered(Actions.OpenInventory))
            {
                if (_screen.Desktop.Children.Contains(_inventoryUi))
                {
                    _screen.Desktop.Children.Remove(_inventoryUi);
                    _itemMessageTranslator.Enabled = false;
                    _playerManager.HandleToolsUse = true;
                    _engine.MouseCapture = true;                    
                }
                else
                {
                    _screen.Desktop.Children.Add(_inventoryUi);
                    _itemMessageTranslator.Enabled = true;
                    _playerManager.HandleToolsUse = false;
                    _engine.MouseCapture = false;
                }
            }
        }

        public override void Dispose()
        {
            _inventoryUi.SlotClicked -= InventoryUiSlotClicked;
            _backgroundTex.Dispose();
        }
    }
}
