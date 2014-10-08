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

            //Draw the Mask Arrow
            SharpDX.Rectangle sourceRectMaskArrow = new SharpDX.Rectangle(0, 0, 150, 150);
            RectangleF absoluteBoundMask = new RectangleF(absoluteBound.X, absoluteBound.Y, absoluteBound.Width, 150);

            graphics.DrawCustomTexture(control.MaskArrow, ref sourceRectMaskArrow, ref absoluteBoundMask, 0.0f, control.sampler);

            SharpDX.Rectangle sourceRectSoulStoneIcon = new SharpDX.Rectangle(0, 0, 25, 25);
            RectangleF absoluteBoundSoulStone = new RectangleF(absoluteBound.X + 62.5f, absoluteBound.Y + 72.5f, 25, 25);

            graphics.DrawCustomTexture(control.SoulStoneIcon, ref sourceRectSoulStoneIcon, ref absoluteBoundSoulStone, 0.0f, control.sampler, 0, new ByteColor((int)(control.SoulStoneFacing * 255f), 0,0,255));
        }
    }
}
