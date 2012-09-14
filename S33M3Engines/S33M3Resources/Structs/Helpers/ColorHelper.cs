using SharpDX;

namespace S33M3Resources.Structs.Helpers
{
    public static class ColorHelper
    {
        public static Color ToSharpColor(System.Drawing.Color color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

        public static System.Drawing.Color ToSystemColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color4 ToColor4(System.Drawing.Color color)
        {
            return new Color4(ToSharpColor(color).ToRgba());
        }

        public static System.Drawing.Color ToSystemColor(Color4 color4)
        {
            return ToSystemColor(Color.FromRgba(color4.ToRgba()));
        }
    }
}
