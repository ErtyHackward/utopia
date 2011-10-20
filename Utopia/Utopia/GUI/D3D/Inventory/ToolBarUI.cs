using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;

using Nuclex.UserInterface.Controls;
using Utopia.Entities;
using S33M3Engines.D3D;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;


namespace Utopia.GUI.D3D.Inventory
{
    public class ToolBarUi : ContainerControl
    {
        private readonly PlayerCharacter _player;
        const int ButtonSize = 46;

        private ButtonControl leftButton;//TODO iconButton
        private ButtonControl rightButton;

        private readonly List<ToolbarButtonControl> _buttons;

        private SlotContainer<ToolbarSlot> _toolbar;
        private CharacterEquipment _equipment;

        public ToolBarUi(UniRectangle Bounds, PlayerCharacter player, IconFactory iconFactory)
        {
            _player = player;
            _toolbar = player.Toolbar;
            _equipment = player.Equipment;
            
            //FIXME uniscalar relative positions doe not work, surely due to rectangle ordering  
            //this.Bounds = new UniRectangle(0.0f, new UniScalar(.5f, 0f), new UniScalar(1, 0), 80.0f);
            //TODO (simon) ToolBarUi remove all magic hardcoded numbers
            //this.Bounds = new UniRectangle(0.0f, 600-46, 1024, 80.0f);
            this.Bounds = Bounds;
            this.Name = "Toolbar";

            leftButton = new ButtonControl();
            leftButton.Name = "LeftTool";
            leftButton.Bounds = new UniRectangle(
               0, 0, ButtonSize, ButtonSize
            );
            leftButton.Pressed += delegate
                                      {
                                          _equipment.LeftSlot.Equipment.Use(player);
                                      };
            leftButton.Text = _equipment.LeftSlot.Item.DisplayName;
            this.Children.Add(leftButton);


            rightButton = new ButtonControl();
            rightButton.Name = "RightTool";
            rightButton.Bounds = new UniRectangle(
              ButtonSize, 0, ButtonSize, ButtonSize
            );
            rightButton.Text = _equipment.RightSlot.Item.DisplayName;
            rightButton.Pressed += delegate
            {
                _equipment.RightSlot.Equipment.Use(player);
            };
            this.Children.Add(rightButton);
            
            int nbrButton = 12;
            _buttons = new List<ToolbarButtonControl>(nbrButton);

            float fromX = ((Bounds.Right.Offset - Bounds.Left.Offset) - (ButtonSize * (nbrButton-3))) / 2;

            for (int x = 0; x < nbrButton; x++)
            {
                
                ToolbarButtonControl btn = new ToolbarButtonControl(_player,iconFactory,new ToolbarSlot(){GridPosition = new Vector2I(x,0)});
                btn.Bounds = new UniRectangle(fromX + (x * ButtonSize), 0, ButtonSize, ButtonSize);
                _buttons.Add(btn);
                btn.Pressed += delegate {
                    if (btn.LeftItem is ITool)
                        _equipment.LeftSlot.Item = (ITool)btn.LeftItem;
                };

                this.Children.Add(btn);
            }

            foreach (ToolbarSlot slot in _toolbar)
            {
                int y = slot.GridPosition.Y;
                _buttons[y].ToolbarSlot = slot;
            }

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

        //TODO (Simon) review ToolBarUI update, envent based may be better
        public void Update(ref GameTime gameTime)
        {
            //for (int i = 0; i < _inventory.Toolbar.Count; i++)
            //{
            //    buttons[i].Item = _inventory.Toolbar[i];

            //    if (_inventory.Toolbar[i] !=null)
            //    {
            //        buttons[i].Text = _inventory.Toolbar[i].UniqueName;
            //    }
               

            //    if (buttons[i].Item == _inventory.LeftTool)
            //    {
            //        buttons[i].Highlight = true;
            //    }
            //    else
            //    {
            //        buttons[i].Highlight = false;
            //    }
            //}

            //if (_inventory.LeftTool != null)
            //{
            //    leftButton.Item = _inventory.LeftTool;
            //    if (leftButton.Item.Icon == null)
            //        leftButton.Text = _inventory.LeftTool.UniqueName;
            //    else
            //        leftButton.Text = null;
            //}

            //if (_inventory.RightTool != null)
            //{
            //    rightButton.Item = _inventory.RightTool;

            //    if (rightButton.Item.Icon == null)
            //        rightButton.Text = _inventory.RightTool.UniqueName;
            //    else
            //        rightButton.Text = null;
            //}
        }

        protected override void OnMouseEntered()
        {
            base.OnMouseEntered();
        }
    }
}
