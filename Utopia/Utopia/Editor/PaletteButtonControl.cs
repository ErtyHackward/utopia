#region

using Utopia.Shared.Structs;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sprites;
using S33M3Resources.Structs;
using SharpDX;

#endregion

namespace Utopia.Editor
{
    public class PaletteButtonControl : ButtonControl
    {
        public PaletteButtonControl()
        {
            Color = SharpDX.Color.White;
        }

        public SpriteTexture Texture { get; set; }
    }
}