using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;

using Nuclex.UserInterface.Controls;


namespace Utopia.GUI.D3D.Inventory
{
    public class ToolBarUI : ContainerControl
    {
        const int _buttonSize = 46;

        private ButtonItemControl leftButton;
        private ButtonItemControl rightButton;

        private List<ButtonItemControl> buttons;

        private Player player;

        public ToolBarUI(Player player, Viewport viewport)
        {
            this.Bounds = new UniRectangle(0.0f, new UniScalar(1, -80), new UniScalar(1, 0), 80.0f);
            this.Name = "Toolbar";
            this.player = player;

            leftButton = new ButtonItemControl(player.LeftTool);
            leftButton.IsLink = true;
            leftButton.Name = "LeftTool";
            leftButton.Bounds = new UniRectangle(
               0, 0, _buttonSize, _buttonSize
            );
            this.Children.Add(leftButton);


            rightButton = new ButtonItemControl(player.RightTool);
            rightButton.Name = "RightTool";
            rightButton.IsLink = true;
            rightButton.Bounds = new UniRectangle(
               new UniScalar(1, -_buttonSize), 0, _buttonSize, _buttonSize
            );
            this.Children.Add(rightButton);

            buttons = new List<ButtonItemControl>(15);

            for (int x = 3; x < 15; x++)
            {
                ButtonItemControl btn = new ButtonItemControl(null);
                btn.Bounds = new UniRectangle(x * _buttonSize, 0, _buttonSize, _buttonSize);
                btn.IsLink = true;
                buttons.Add(btn);
                this.Children.Add(btn);
            }
        }

        public void Update()
        {

            for (int i = 0; i < player.inventory.toolbar.Count; i++)
            {
                buttons[i].Item = player.inventory.toolbar[i];

                if (buttons[i].Item == player.LeftTool)
                {
                    buttons[i].Highlight = true;
                }
                else
                {
                    buttons[i].Highlight = false;
                }
            }

            if (player.LeftTool != null)
            {
                leftButton.Item = player.LeftTool;

                if (leftButton.Item.Icon == null)
                    leftButton.Text = player.LeftTool.name;
                else
                    leftButton.Text = null;
            }

            if (player.RightTool != null)
            {
                rightButton.Item = player.RightTool;

                if (rightButton.Item.Icon == null)
                    rightButton.Text = player.RightTool.name;
                else
                    rightButton.Text = null;
            }
        }
    }
}
