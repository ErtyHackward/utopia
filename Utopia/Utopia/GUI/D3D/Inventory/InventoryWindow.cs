using System;
using System.Collections.Generic;
using System.Linq;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.Shared.Sprites;
using Utopia.Entities;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Structs;

namespace Utopia.GUI.D3D.Inventory
{
    /// <summary> InventoryWindow </summary>
    public class InventoryWindow : ContainerControl
    {
        private readonly PlayerCharacter _player;
        private InventoryCell[,] _uiGrid;
        private IconFactory _iconFactory;
        private const int CellSize = 64;

        public InventoryWindow(SpriteTexture back, PlayerCharacter player, IconFactory iconFactory)
        {
            _player = player;
            _iconFactory = iconFactory;
            InitializeComponent(back);            
        }

        private void InitializeComponent(SpriteTexture back)
        {
           

            SlotContainer<ContainedSlot> slots = _player.Inventory;
            int w = back.Width + slots.GridSize.X*CellSize;
            int h = (slots.GridSize.Y+1) * CellSize ;

            Bounds = new UniRectangle(80.0f, 10.0f, w, h);
            //this.Title = "Inventory";
            
            var okButton = new ButtonControl();

            okButton.Bounds = new UniRectangle(
                new UniScalar(1.0f, -180.0f), new UniScalar(1.0f, -40.0f), 80, 24
                );
            okButton.Text = "Ok";

            okButton.Pressed += delegate { RemoveFromParent(); };
            
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

            // TODO charactersheet has to be redone, CharacterEquipment is not ContainedSlot based but uses specialized methods like SetHeadGear
 
           /* BuildBodyslot(characterSheet, EquipmentSlotType.Head, 74, 2);
            BuildBodyslot(characterSheet, EquipmentSlotType.Neck, 82, 46, 16);
            BuildBodyslot(characterSheet, EquipmentSlotType.Torso, 74, 71);
            BuildBodyslot(characterSheet, EquipmentSlotType.RightHand, 145, CellSize);
            BuildBodyslot(characterSheet, EquipmentSlotType.LeftHand, 2, CellSize);
            BuildBodyslot(characterSheet, EquipmentSlotType.Legs, 110, 136);
            BuildBodyslot(characterSheet, EquipmentSlotType.Feet, 48, 178);
            BuildBodyslot(characterSheet, EquipmentSlotType.LeftRing, 5, 101, 16);*/
        }

       /* private void BuildBodyslot(Control parent, EquipmentSlotType inventorySlot, int x, int y, int size = 32)
        {
            _player.Equipment.Torso
            var bodyCell = new InventoryCell(inventorySlot);
            bodyCell.Name = inventorySlot.ToString();
            bodyCell.Bounds = new UniRectangle(x, y, size, size);
            bodyCell.IsLink = true;
            parent.Children.Add(bodyCell);
        }*/

        public void BuildGrid(int xstart)
        {
            SlotContainer<ContainedSlot> slots = _player.Inventory;

            _uiGrid = new InventoryCell[slots.GridSize.X, slots.GridSize.Y];

            for (int x = 0; x < slots.GridSize.X; x++)
            {
                for (int y = 0; y < slots.GridSize.Y; y++)
                {
                    var control = new InventoryCell();
                    control.Bounds = new UniRectangle(xstart + x * CellSize, y * CellSize, CellSize, CellSize);
                    control.Name = x + "," + y;
                    Children.Add(control);

                    var drag = new DraggableItemControl(_iconFactory,_player.Inventory);
                    drag.Bounds = DraggableItemControl.ReferenceBounds;
                    drag.Name = "drag " + x + "," + y;
                    
                    control.Children.Add(drag);
                    _uiGrid[x, y] = control;
                }
            }
        }

        public void Refresh()
        {
            SlotContainer<ContainedSlot> slots = _player.Inventory;

            for (int x = 0; x < slots.GridSize.X; x++)
            {
                for (int y = 0; y < slots.GridSize.Y; y++)
                {
                    if (_uiGrid[x, y].Slot == null)
                    {
                        _uiGrid[x, y].Slot = new ContainedSlot();
                        _uiGrid[x, y].Slot.GridPosition = new Vector2I(x, y);
                    }
                    
                    DraggableItemControl drag = (DraggableItemControl) _uiGrid[x, y].Children.First();
                    drag.Item = null;
                }
            }

            //thins enum only has the non null items and grid positions
            foreach (var containedSlot in slots)
            {
                int x = containedSlot.GridPosition.X;
                int y = containedSlot.GridPosition.Y;
                _uiGrid[x, y].Slot = containedSlot;
                DraggableItemControl drag = (DraggableItemControl)_uiGrid[x, y].Children.First();
                drag.Item = containedSlot.Item;
            }
        }

    }
}