#region

using System.Collections.Generic;
using System.Linq;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.Shared.Sprites;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Entities.Voxel;

#endregion

namespace Utopia.Editor
{
    public class EntityEditorUi
    {
        private readonly EntityEditor _editorComponent;

        public byte SelectedIndex { get; set; }
        public bool IsTexture { get; set; }

        public bool IsColor
        {
            get { return !IsTexture; }
            set { IsTexture = !value; }
        }

        public List<Control> Children = new List<Control>();

        public EntityEditorUi(EntityEditor editorComponent)
        {
            _editorComponent = editorComponent;

            //TODO remove all magic hardcoded numbers

            Children.Add(InitToolBar());
            Children.Add(InitColorPalette());
            Children.Add(InitTexturePalette());
        }

        private WindowControl InitColorPalette()
        {
            const int rows = 16;
            const int cols = 4;
            const int btnSize = 20;

            const int y0 = 20;
            const int x0 = 0;

            WindowControl palette = new WindowControl();
            palette.Bounds = new UniRectangle(0, 0, (cols)*btnSize, (rows + 1)*btnSize);

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
                    btn.Pressed += (sender, e) =>
                                       {
                                           SelectedIndex = (byte) associatedindex;
                                           IsColor = true;
                                       };

                    palette.Children.Add(btn);
                    index++;
                }
            }
            return palette;
        }


        private WindowControl InitTexturePalette()
        {
            ShaderResourceView arrayResourceView = _editorComponent._texture;

            int count = arrayResourceView.Description.Texture2DArray.ArraySize;

            const int rows = 8;
            const int cols = 4;
            const int btnSize = 34;

            const int y0 = 23;
            const int x0 = 3;

            WindowControl palette = new WindowControl();
            palette.Bounds = new UniRectangle(100, 0, (cols)*btnSize, (rows + 1)*btnSize);

            int index = 0;
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (index == count) break;

                    PaletteButtonControl btn = new PaletteButtonControl();
                    btn.Bounds = new UniRectangle(x0 + x*btnSize, y0 + y*btnSize, btnSize, btnSize);
                    btn.Texture = new SpriteTexture(btnSize, btnSize, arrayResourceView, Vector2.Zero);
                    btn.Texture.Index = index;
                    int associatedindex = index; //new variable for access inside btn.pressed closure 
                    btn.Pressed += (sender, e) =>
                                       {
                                           SelectedIndex = (byte) associatedindex;
                                           IsTexture = true;
                                       };
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
            toolBar.Bounds = new UniRectangle(0.0f, 0, buttonsNbr*buttonSize, buttonSize + margin);
            toolBar.Title = "Edit tools";

            List<ButtonControl> buttons = new List<ButtonControl>(5);


            for (int x = 0; x < buttonsNbr; x++)
            {
                ButtonControl btn = new ButtonControl();
                btn.Bounds = new UniRectangle(x*buttonSize, margin, buttonSize, buttonSize);
                buttons.Add(btn);
                btn.Pressed += delegate { };

                toolBar.Children.Add(btn);
            }
            return toolBar;
        }
    }
}