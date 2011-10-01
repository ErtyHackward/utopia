using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;

using Nuclex.UserInterface.Controls;
using Utopia.Shared.Chunks.Entities.Inventory;
using S33M3Engines.D3D;
using Utopia.Shared.Structs;


namespace Utopia.GUI.D3D.Inventory
{
    public class ToolBarUi : ContainerControl
    {
        const int ButtonSize = 46;

        //private ButtonItemControl leftButton;
        //private ButtonItemControl rightButton;

        private readonly List<ToolbarButtonControl> _buttons;

        //private PlayerInventory _inventory;

        public ToolBarUi(UniRectangle Bounds)
        {
            //FIXME uniscalar relative positions doe not work, surely due to rectangle ordering  
            //this.Bounds = new UniRectangle(0.0f, new UniScalar(.5f, 0f), new UniScalar(1, 0), 80.0f);
            //TODO (simon) ToolBarUi remove all magic hardcoded numbers
            //this.Bounds = new UniRectangle(0.0f, 600-46, 1024, 80.0f);
            this.Bounds = Bounds;
            this.Name = "Toolbar";
            //_inventory = inventory;

            //leftButton = new ButtonItemControl(inventory.LeftTool);
            //leftButton.IsLink = true;
            //leftButton.Name = "LeftTool";
            //leftButton.Bounds = new UniRectangle(
            //   0, 0, _buttonSize, _buttonSize
            //);
            //this.Children.Add(leftButton);


            //rightButton = new ButtonItemControl(inventory.RightTool);
            //rightButton.Name = "RightTool";
            //rightButton.IsLink = true;
            //rightButton.Bounds = new UniRectangle(
            //  1024-46, 0, _buttonSize, _buttonSize
            //);
            //this.Children.Add(rightButton);
            int nbrButton = 15;
            _buttons = new List<ToolbarButtonControl>(nbrButton);

            float fromX = ((Bounds.Right.Offset - Bounds.Left.Offset) - (ButtonSize * nbrButton)) / 2;

            for (int x = 0; x < nbrButton; x++)
            {
                ToolbarButtonControl btn = new ToolbarButtonControl(new ToolbarSlot(){GridPosition = new Vector2I(x,0)});
                btn.Bounds = new UniRectangle(fromX + (x * ButtonSize), 0, ButtonSize, ButtonSize);
                _buttons.Add(btn);
                btn.Pressed += delegate(object sender, EventArgs e)
                                   {
                                       //inventory.LeftTool = btn.Item as Tool;
                                   };

                this.Children.Add(btn);
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
