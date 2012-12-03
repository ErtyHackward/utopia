using S33M3CoreComponents.Sprites2D;

namespace S33M3CoreComponents.GUI.Nuclex.Controls.Desktop
{
    /// <summary>
    /// Displays an image on the size of the control
    /// </summary>
    public class ImageControl : Control
    {
        public SpriteTexture Image { get; set; }

        public bool Stretch { get; set; }
    }
}
