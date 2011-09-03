using System;
using System.Collections.Generic;
using System.Linq;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.D3D.Effects.Basics;
using Utopia.Entities.Voxel;

namespace Utopia.Editor
{
    public class EntityEditorUi
    {
       

        private readonly EntityEditor _editorComponent;
        private HLSLVertexPositionColor _itemEffect;

        public byte SelectedColor { get; set; }

        public List<Control> Children = new List<Control>();

        public EntityEditorUi(EntityEditor editorComponent)
        {
            _editorComponent = editorComponent;

            //TODO remove all magic hardcoded numbers

            this.Children.Add(InitToolBar());
            this.Children.Add(InitPalette());
        }

        private WindowControl InitPalette()
        {
            const int rows = 16;
            const int cols = 4;
            const int btnSize = 20;

            const int y0 = 20;
            const int x0 = 0;

            WindowControl palette = new WindowControl();
            palette.Bounds = new UniRectangle(0, 0, (cols)*btnSize, (rows+1)*btnSize);

            int index = 0;
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (index == ColorLookup.Colours.Count()) break;

                    var color = ColorLookup.Colours[index];

                    PaletteButtonControl btn = new PaletteButtonControl();
                    btn.Bounds = new UniRectangle(x0 + x*btnSize, y0 + y*btnSize, btnSize, btnSize);
                    btn.Color = color;
                    int associatedindex = index; //for access inside closure 
                    btn.Pressed += (sender, e) => SelectedColor = (byte) associatedindex;
                    palette.Children.Add(btn);
                    index++;
                }
            }
            return palette;
        }

        private WindowControl InitToolBar()
        {
            const int buttonSize = 46;
            const int buttonsNbr = 5;
            const int margin = 20;

            WindowControl toolBar = new WindowControl();
            toolBar.Bounds = new UniRectangle(0.0f, 0, buttonsNbr * buttonSize, buttonSize + margin);
            toolBar.Title = "Edit tools";

            List<ButtonControl> buttons = new List<ButtonControl>(5);



            for (int x = 0; x < buttonsNbr; x++)
            {
                ButtonControl btn = new ButtonControl();
                btn.Bounds = new UniRectangle(x * buttonSize, margin, buttonSize, buttonSize);
                buttons.Add(btn);
                btn.Pressed += delegate(object sender, EventArgs e)
                                   {
                                       
                                   };

                toolBar.Children.Add(btn);
            }
            return toolBar;
        }
    }
}