using System;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.Shared.Sprites;
using Utopia.Shared.Structs;

namespace Utopia.Editor
{
    public class PaletteButtonControl : ButtonControl
    {
        public Color? Color { get; set; }
        public SpriteTexture Texture { get; set; }
    }
}