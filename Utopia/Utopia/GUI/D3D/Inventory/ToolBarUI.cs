using System;
using System.Collections.Generic;
using Nuclex.UserInterface;
using Utopia.Entities;
using S33M3Engines.D3D;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;


namespace Utopia.GUI.D3D.Inventory
{
    public class ToolBarUi : ContainerControl
    {
        private readonly PlayerCharacter _player;
        const int ButtonSize = 46;

        private readonly List<InventoryCell> _buttons;

        /// <summary>
        /// Occurs when some slot get clicked
        /// </summary>
        public event EventHandler<InventoryWindowCellMouseEventArgs> SlotClicked;

        protected void OnSlotClicked(InventoryWindowCellMouseEventArgs e)
        {
            var handler = SlotClicked;
            if (handler != null) handler(this, e);
        }

        public ToolBarUi(UniRectangle bounds, PlayerCharacter player, IconFactory iconFactory)
        {
            _player = player;
 
            Bounds = bounds;
            Name = "Toolbar";

            
            int nbrButton = 10;
            _buttons = new List<InventoryCell>(nbrButton);

            float fromX = ((bounds.Right.Offset - bounds.Left.Offset) - (ButtonSize * (nbrButton-3))) / 2;

            for (int x = 0; x < nbrButton; x++)
            {

                var btn = new InventoryCell(null, iconFactory, new Vector2I(0, x))
                              {
                                  Bounds = new UniRectangle(fromX + (x * ButtonSize), 0, ButtonSize, ButtonSize)
                                  
                              };
                btn.MouseDown += BtnMouseDown;
                
                if (_player.Toolbar[x] != 0)
                    btn.Slot = new ContainedSlot { Item = _player.FindToolById(_player.Toolbar[x]) };

                _buttons.Add(btn);

                Children.Add(btn);
            }


        }

        void BtnMouseDown(object sender, MouseDownEventArgs e)
        {
            OnSlotClicked(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        public void SetSlot(int i, ContainedSlot slot)
        {
            _buttons[i].Slot = slot;
        }

        public void Resized()
        {
            float fromX = ((Bounds.Right.Offset - Bounds.Left.Offset) - (ButtonSize * _buttons.Count)) / 2;
            int btNbr = 0;
            foreach (var bt in _buttons)
            {
                bt.Bounds = new UniRectangle(fromX + (btNbr * ButtonSize), 0, ButtonSize, ButtonSize);
                btNbr++;
            }
        }

        public void Update(ref GameTime gameTime)
        {
            if (_player.Equipment.LeftTool == null)
                return;

            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_player.Toolbar[i] != 0)
                {
                    _buttons[i].IsCellSelected = _player.Equipment.LeftTool.StaticId == _player.Toolbar[i];
                }
                else
                    _buttons[i].IsCellSelected = false;
            }
        }

    }
}
