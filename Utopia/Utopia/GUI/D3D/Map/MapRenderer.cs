using System;
using System.Collections.Generic;
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
            graphics.DrawCustomTexture(control.MapTexture, control.GetAbsoluteBounds());
        }
    }
}
