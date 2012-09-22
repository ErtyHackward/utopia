using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3CoreComponents.GUI.Nuclex;

namespace Utopia.GUI.Inventory
{
    public class ContainerRenderer : IFlatControlRenderer<ContainerControl>
    {
        public void Render(ContainerControl control, IFlatGuiGraphics graphics)
        {
            if (control.background != null)
            {
                RectangleF absoluteBounds = control.GetAbsoluteBounds();
                graphics.DrawCustomTexture(control.background, ref absoluteBounds, 0, control.DrawGroupId);
            }
        }
    }
}
