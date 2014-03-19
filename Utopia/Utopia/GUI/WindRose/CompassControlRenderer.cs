using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.GUI.WindRose
{
    public class CompassControlRenderer : IFlatControlRenderer<CompassControl>
    {
        //Will render itSelf the Compass sprite

        public void Render(CompassControl control, IFlatGuiGraphics graphics)
        {
            if (control.HidedPanel) return;
            RectangleF absoluteBound = control.GetAbsoluteBounds();
            ByteColor color = control.Color;
            // This is simple! A panel consists of a single element we need to draw.
            graphics.DrawElement(control.FrameName, ref absoluteBound, ref color);

            //http://www.gamedev.net/topic/649450-how-to-rotate-the-texture-coordinate-in-the-hlsl/
        }
    }
}
