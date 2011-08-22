using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

using Nuclex.UserInterface;
using SharpDX.Direct3D11;
using Utopia.Shared.Chunks.Entities.Inventory;
using S33M3Engines.Shared.Sprites;


namespace Utopia.GUI.D3D.Inventory
{
    /// <summary> InventoryWindow </summary>
    public class InventoryWindow : ContainerControl
    {

       private ButtonControl _okButton;
        private Utopia.Shared.Chunks.Entities.Inventory.PlayerInventory _inventory;

        public InventoryWindow(PlayerInventory inventory, SpriteTexture back)
        {
            _inventory = inventory;
            InitializeComponent(back);
        }

        /// <summary>Called when the user clicks on the okay button</summary>
        /// <param name="sender">Button the user has clicked on</param>
        /// <param name="arguments">Not used</param>
        private void okClicked(object sender, EventArgs arguments)
        {
            this.RemoveFromParent();
        }

        private void InitializeComponent(SpriteTexture back)
        {
           
            this._okButton = new Nuclex.UserInterface.Controls.Desktop.ButtonControl();

            this._okButton.Bounds = new UniRectangle(
                new UniScalar(1.0f, -180.0f), new UniScalar(1.0f, -40.0f), 80, 24
            );
            this._okButton.Text = "Ok";
            //this.okButton.ShortcutButton = Buttons.A;
            this._okButton.Pressed += new EventHandler(okClicked);

            this.Bounds = new UniRectangle(80.0f, 10.0f, 512.0f, 384.0f);
            //this.Title = "Inventory";
            Children.Add(this._okButton);

            buildCharacterSheet(back);
            buildGrid(back.TextureDescr.Width + 5);

        }

        private void buildCharacterSheet(SpriteTexture back)
        {
            ContainerControl characterSheet = new ContainerControl();
            characterSheet.background = back;
            characterSheet.Bounds = new UniRectangle(0, 0, back.TextureDescr.Width, back.TextureDescr.Height);
            Children.Add(characterSheet);

            buildBodyslot(characterSheet, EquipmentSlot.Head, 74, 2);
            buildBodyslot(characterSheet, EquipmentSlot.Neck, 82, 46, 16);
            buildBodyslot(characterSheet, EquipmentSlot.Torso, 74, 71);
            buildBodyslot(characterSheet, EquipmentSlot.RightHand, 145, 64);
            buildBodyslot(characterSheet, EquipmentSlot.LeftHand, 2, 64);
            buildBodyslot(characterSheet, EquipmentSlot.Legs, 110, 136);
            buildBodyslot(characterSheet, EquipmentSlot.Feet, 48, 178);
            buildBodyslot(characterSheet, EquipmentSlot.LeftRing, 5, 101, 16);

        }

        private void buildBodyslot(Control parent, EquipmentSlot inventorySlot, int x, int y, int size = 32)
        {
            InventoryCell bodyCell = new InventoryCell(inventorySlot);
            bodyCell.Bounds = new UniRectangle(x, y, size, size);
            bodyCell.IsLink = true;
            parent.Children.Add(bodyCell);
        }

        public void buildGrid(int xstart)
        {

            //List<ContainedSlot> items = _inventory.Bag.Items;

            int gridSize = 5; // grid is gridSize*gridSize

            int cell = 0;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    InventoryCell control = new InventoryCell();
                    control.Bounds = new UniRectangle(xstart + x * Item.IconSize, y * Item.IconSize, Item.IconSize, Item.IconSize);
                    control.Name = x + "," + y;
                    Children.Add(control);

                    //control.Item = items[cell];
                    DraggableItemControl drag = new DraggableItemControl();
                    drag.Bounds = DraggableItemControl.referenceBounds;
                    drag.Name = "drag " + x + "," + y;

                    //if (cell < items.Count()) drag.Item = items[cell];

                    control.Children.Add(drag);

                    cell++;
                }
            }
        }
    }
}
