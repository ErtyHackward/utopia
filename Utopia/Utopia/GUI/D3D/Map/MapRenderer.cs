using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Visuals.Flat;
using Utopia.GUI.D3D.Inventory;

namespace Utopia.GUI.D3D.Map
{
    public class MapRenderer : IFlatControlRenderer<MapControl>
    {
        public void Render(MapControl control, IFlatGuiGraphics graphics)
        {
            if(control.MapTexture != null)
                graphics.DrawCustomTexture(control.MapTexture, control.GetAbsoluteBounds());
            if (control.PlayerMarker != null)
            {
                var bounds = control.GetAbsoluteBounds();

                var scaleX = control.MapTexture.Width / bounds.Width;
                var scaleY = control.MapTexture.Height / bounds.Height;

                var point = control.MarkerPosition;

                point.Offset(control.MapTexture.Width / 2, control.MapTexture.Height / 2);

                point = new Point((int)(bounds.X + point.X / scaleX), (int)(bounds.Y + point.Y / scaleY));

                graphics.DrawCustomTexture(control.PlayerMarker,
                                           new Nuclex.UserInterface.RectangleF(
                                               point.X - control.PlayerMarker.Width / 2,
                                               point.Y - control.PlayerMarker.Height / 2,
                                               control.PlayerMarker.Width, control.PlayerMarker.Height));
            }
        }
    }
}
