﻿using System;
using System.Collections.Generic;
using Utopia.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Inputs;


namespace Utopia.GUI.Inventory
{
    /// <summary>
    /// Represents a toolbar control
    /// </summary>
    public class ToolBarUi : ContainerControl
    {
        private readonly PlayerCharacter _player;
        const int ButtonSize = 46;

        protected readonly List<InventoryCell> _toolbarSlots;

        /// <summary>
        /// Occurs when some slot get clicked
        /// </summary>
        public event EventHandler<InventoryWindowCellMouseEventArgs> SlotClicked;

        protected void OnSlotClicked(InventoryWindowCellMouseEventArgs e)
        {
            var handler = SlotClicked;
            if (handler != null) handler(this, e);
        }

        public ToolBarUi(PlayerCharacter player, IconFactory iconFactory, InputsManager inputManager)
        {
            _player = player;
 
            Name = "Toolbar";

            int nbrButton = 10;
            _toolbarSlots = new List<InventoryCell>(nbrButton);

            //float fromX = ((bounds.Right.Offset - bounds.Left.Offset) - (ButtonSize * (nbrButton))) / 2;

            for (int x = 0; x < nbrButton; x++)
            {

                var btn = new InventoryCell(null, iconFactory, new Vector2I(0, x), inputManager)
                              {
                                  //Bounds = new UniRectangle(fromX + (x * ButtonSize), 0, ButtonSize, ButtonSize)
                              };
                btn.MouseDown += BtnMouseDown;

                
                if (_player.Toolbar[x] != 0)
                    btn.Slot = new ContainedSlot { Item = _player.FindToolById(_player.Toolbar[x]) };

                _toolbarSlots.Add(btn);

                Children.Add(btn);
            }


        }

        void BtnMouseDown(object sender, MouseDownEventArgs e)
        {
            OnSlotClicked(new InventoryWindowCellMouseEventArgs { Cell = (InventoryCell)sender });
        }

        public void SetSlot(int i, ContainedSlot slot)
        {
            _toolbarSlots[i].Slot = slot;
        }

        public void Resized()
        {
            float fromX = ((Bounds.Right.Offset - Bounds.Left.Offset) - (ButtonSize * _toolbarSlots.Count)) / 2;
            int btNbr = 0;
            foreach (var bt in _toolbarSlots)
            {
                bt.Bounds = new UniRectangle(fromX + (btNbr * ButtonSize), 0, ButtonSize, ButtonSize);

                btNbr++;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_player.Equipment.LeftTool == null)
                return;

            for (int i = 0; i < _toolbarSlots.Count; i++)
            {
                if (_player.Toolbar[i] != 0)
                {
                    _toolbarSlots[i].IsCellSelected = _player.Equipment.LeftTool.StaticId == _player.Toolbar[i];
                }
                else
                    _toolbarSlots[i].IsCellSelected = false;
            }
        }

    }
}
