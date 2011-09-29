using System;
using System.Collections.Generic;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.Shared.Sprites;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Inventory;

namespace Utopia.GUI.D3D.Inventory
{
    /// <summary> InventoryWindow </summary>
    public class InventoryWindow : ContainerControl
    {
        private readonly PlayerCharacter _player;
        private DraggableItemControl[,] _uiGrid;

        public InventoryWindow(SpriteTexture back, PlayerCharacter player)
        {
            _player = player;
            InitializeComponent(back);            
        }

        private void InitializeComponent(SpriteTexture back)
        {
            var okButton = new ButtonControl();

            okButton.Bounds = new UniRectangle(
                new UniScalar(1.0f, -180.0f), new UniScalar(1.0f, -40.0f), 80, 24
                );
            okButton.Text = "Ok";

            okButton.Pressed += delegate { RemoveFromParent(); };

            Bounds = new UniRectangle(80.0f, 10.0f, 512.0f, 384.0f);
            //this.Title = "Inventory";
            Children.Add(okButton);

            BuildCharacterSheet(back);
            BuildGrid(back.Width + 5);
        }

        private void BuildCharacterSheet(SpriteTexture back)
        {
            var characterSheet = new ContainerControl();
            characterSheet.background = back;
            characterSheet.Bounds = new UniRectangle(0, 0, back.Width, back.Height);
            Children.Add(characterSheet);
            //XXX externalize charactersheet slot positions. clientsettings.xml or somewhere else
            BuildBodyslot(characterSheet, EquipmentSlotType.Head, 74, 2);
            BuildBodyslot(characterSheet, EquipmentSlotType.Neck, 82, 46, 16);
            BuildBodyslot(characterSheet, EquipmentSlotType.Torso, 74, 71);
            BuildBodyslot(characterSheet, EquipmentSlotType.RightHand, 145, 64);
            BuildBodyslot(characterSheet, EquipmentSlotType.LeftHand, 2, 64);
            BuildBodyslot(characterSheet, EquipmentSlotType.Legs, 110, 136);
            BuildBodyslot(characterSheet, EquipmentSlotType.Feet, 48, 178);
            BuildBodyslot(characterSheet, EquipmentSlotType.LeftRing, 5, 101, 16);
        }

        private void BuildBodyslot(Control parent, EquipmentSlotType inventorySlot, int x, int y, int size = 32)
        {
            var bodyCell = new InventoryCell(inventorySlot);
            bodyCell.Bounds = new UniRectangle(x, y, size, size);
            bodyCell.IsLink = true;
            parent.Children.Add(bodyCell);
        }

        public void BuildGrid(int xstart)
        {
            SlotContainer<ContainedSlot> slots = _player.Inventory;

             _uiGrid = new DraggableItemControl[slots.GridSize.X, slots.GridSize.Y];

            for (int x = 0; x < slots.GridSize.X; x++)
            {
                for (int y = 0; y < slots.GridSize.Y; y++)
                {
                    var control = new InventoryCell();
                    control.Bounds = new UniRectangle(xstart + x * 64, y * 64, 64, 64);
                    control.Name = x + "," + y;
                    Children.Add(control);

                    var drag = new DraggableItemControl();
                    drag.Bounds = DraggableItemControl.referenceBounds;
                    drag.Name = "drag " + x + "," + y;

                    control.Children.Add(drag);
                    _uiGrid[x, y] = drag;
                }
            }
        }

        public void Refresh()
        {
            SlotContainer<ContainedSlot> slots = _player.Inventory;

            foreach (var containedSlot in slots)
            {
                int x = containedSlot.GridPosition.X;
                int y = containedSlot.GridPosition.Y;
                _uiGrid[x, y].Item = containedSlot.Item;
            }
        }

    }
}