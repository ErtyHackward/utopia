#region

using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.Shared.Sprites;
using Utopia.Shared.Structs;

#endregion

namespace Utopia.Editor
{
    public class PaletteButtonControl : ButtonControl
    {
        public PaletteButtonControl()
        {
            Color = Color.White;
        }

        public Color Color { get; set; }
        public SpriteTexture Texture { get; set; }
    }
}