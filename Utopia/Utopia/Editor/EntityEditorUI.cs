using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.D3D.Effects.Basics;

namespace Utopia.Editor
{
    public class EntityEditorUi : WindowControl
    {
        private const int ButtonSize = 46;

        private readonly List<ButtonControl> _buttons;
        private readonly EntityEditor _editorComponent;
        private HLSLVertexPositionColor _itemEffect;

        public EntityEditorUi(EntityEditor editorComponent)
        {
            _editorComponent = editorComponent;

            //TODO remove all magic hardcoded numbers
            this.Bounds = new UniRectangle(0.0f,0, 1024, 80.0f);

            this.Name = "EntityEditor";
            this.Title = "Entity Editor";
            _buttons = new List<ButtonControl>(15);

            for (int x = 3; x < 19; x++)
            {
                ButtonControl btn = new ButtonControl();
                btn.Bounds = new UniRectangle(x*ButtonSize, 20, ButtonSize, ButtonSize);
                _buttons.Add(btn);
                btn.Pressed += delegate(object sender, EventArgs e)
                                   {
                                       //inventory.LeftTool = btn.Item as Tool;
                                   };

                this.Children.Add(btn);
            }
        }
    }
}