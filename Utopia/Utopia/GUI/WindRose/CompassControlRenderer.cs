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

            //Draw the DayCircle First
            SharpDX.Rectangle sourceRect = new SharpDX.Rectangle(0, 0, 150, 75);
            graphics.DrawCustomTexture(control.DayCircle, ref sourceRect, ref absoluteBound, control.RotationDayCycle, control.sampler);

            //Draw the main WindRose for direction
            graphics.DrawCustomTexture(control.CompassTexture, ref sourceRect, ref absoluteBound, control.Rotation, control.sampler);
        }
    }
}
