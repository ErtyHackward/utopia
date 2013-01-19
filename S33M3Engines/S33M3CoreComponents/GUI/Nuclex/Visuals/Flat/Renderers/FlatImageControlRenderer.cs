using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;

namespace S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Renderers
{
    public class FlatImageControlRenderer : IFlatControlRenderer<Controls.Desktop.ImageControl>
    {
        public void Render(Controls.Desktop.ImageControl control, IFlatGuiGraphics graphics)
        {
            RectangleF rect = control.GetAbsoluteBounds();

            //rect.X = control.Bounds.Location.X.Offset;
            //rect.Y = control.Bounds.Location.Y.Offset;
            //rect.Width  = control.Bounds.Size.X.Offset;
            //rect.Height = control.Bounds.Size.Y.Offset;

            rect.X = (int)rect.X;
            rect.Y = (int)rect.Y;


            if (control.Image != null)
            {
                if (control.Stretch)
                {
                    graphics.DrawCustomTexture(control.Image, ref rect, 0, control.DrawGroupId);
                }
                else
                {
                    graphics.DrawCustomTextureTiled(control.Image, ref rect, 0, control.DrawGroupId);
                }
            }
        }
    }
}
