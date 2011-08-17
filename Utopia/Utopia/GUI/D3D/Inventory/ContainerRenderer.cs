using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Visuals.Flat;

namespace Utopia.GUI.D3D.Inventory
{
    public class ContainerRenderer : IFlatControlRenderer<ContainerControl>
    {

        public void Render(ContainerControl control, IFlatGuiGraphics graphics)
        {

            if (control.background != null)
                graphics.DrawCustomTexture(control.background, control.GetAbsoluteBounds());

        }
    }
}
