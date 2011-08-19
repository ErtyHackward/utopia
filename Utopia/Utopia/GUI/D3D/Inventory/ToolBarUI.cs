using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;

using Nuclex.UserInterface.Controls;
using Utopia.Shared.Chunks.Entities.Inventory;
using S33M3Engines.D3D;


namespace Utopia.GUI.D3D.Inventory
{
    public class ToolBarUI : ContainerControl
    {
        const int _buttonSize = 46;

        private ButtonItemControl leftButton;
        private ButtonItemControl rightButton;

        private List<ButtonItemControl> buttons;

        private PlayerInventory _inventory;

        public ToolBarUI(PlayerInventory inventory)
        {
            //FIXME uniscalar relative positions doe not work, surely due to rectangle ordering  
            //this.Bounds = new UniRectangle(0.0f, new UniScalar(.5f, 0f), new UniScalar(1, 0), 80.0f);
            //TODO remove all magic hardcoded numbers
            this.Bounds = new UniRectangle(0.0f, 600-46, 1024, 80.0f);
            
            this.Name = "Toolbar";
            _inventory = inventory;

            leftButton = new ButtonItemControl(inventory.LeftTool);
            leftButton.IsLink = true;
            leftButton.Name = "LeftTool";
            leftButton.Bounds = new UniRectangle(
               0, 0, _buttonSize, _buttonSize
            );
            this.Children.Add(leftButton);


            rightButton = new ButtonItemControl(inventory.RightTool);
            rightButton.Name = "RightTool";
            rightButton.IsLink = true;
            rightButton.Bounds = new UniRectangle(
              1024-46, 0, _buttonSize, _buttonSize
            );
            this.Children.Add(rightButton);

            buttons = new List<ButtonItemControl>(15);

            for (int x = 3; x < 19; x++)
            {
                ButtonItemControl btn = new ButtonItemControl(null);
                btn.Bounds = new UniRectangle(x * _buttonSize, 0, _buttonSize, _buttonSize);
                btn.IsLink = true;
                buttons.Add(btn);
                this.Children.Add(btn);
            }
        }

        //TODO review ToolBarUI update, envent based may be better
        public void Update(ref GameTime gameTime)
        {

            for (int i = 0; i < _inventory.toolbar.Count; i++)
            {
                buttons[i].Item = _inventory.toolbar[i];

                if (buttons[i].Item == _inventory.LeftTool)
                {
                    buttons[i].Highlight = true;
                }
                else
                {
                    buttons[i].Highlight = false;
                }
            }

            if (_inventory.LeftTool != null)
            {
                leftButton.Item = _inventory.LeftTool;

                if (leftButton.Item.Icon == null)
                    leftButton.Text = _inventory.LeftTool.UniqueName;
                else
                    leftButton.Text = null;
            }

            if (_inventory.RightTool != null)
            {
                rightButton.Item = _inventory.RightTool;

                if (rightButton.Item.Icon == null)
                    rightButton.Text = _inventory.RightTool.UniqueName;
                else
                    rightButton.Text = null;
            }
        }

        protected override void OnMouseEntered()
        {
            base.OnMouseEntered();
        }
    }
}
