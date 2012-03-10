using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.GUI.Inventory;
using S33M3_CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3_CoreComponents.GUI.Nuclex;
using RectangleF = S33M3_CoreComponents.GUI.Nuclex.RectangleF;
using System.Drawing;

namespace Utopia.GUI.Map
{
    public class MapRenderer : IFlatControlRenderer<MapControl>
    {
        public void Render(MapControl control, IFlatGuiGraphics graphics)
        {
            if (control.MapTexture != null)
            {
                RectangleF bound = control.GetAbsoluteBounds();
                graphics.DrawCustomTexture(control.MapTexture, ref bound);
            }
            if (control.PlayerMarker != null)
            {
                var bounds = control.GetAbsoluteBounds();

                var scaleX = control.MapTexture.Width / bounds.Width;
                var scaleY = control.MapTexture.Height / bounds.Height;

                var point = control.MarkerPosition;

                point.Offset(control.MapTexture.Width / 2, control.MapTexture.Height / 2);

                point = new Point((int)(bounds.X + point.X / scaleX), (int)(bounds.Y + point.Y / scaleY));

                RectangleF bound = new RectangleF(
                                               point.X - control.PlayerMarker.Width / 2,
                                               point.Y - control.PlayerMarker.Height / 2,
                                               control.PlayerMarker.Width, control.PlayerMarker.Height);
                graphics.DrawCustomTexture(control.PlayerMarker, ref bound);
            }
        }
    }
}
