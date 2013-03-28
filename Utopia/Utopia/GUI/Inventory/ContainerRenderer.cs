using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3CoreComponents.GUI.Nuclex;

namespace Utopia.GUI.Inventory
{
    public class ContainerRenderer : IFlatControlRenderer<ContainerControl>
    {
        public void Render(ContainerControl control, IFlatGuiGraphics graphics)
        {
            if (control.DisplayBackground && control.Background != null)
            {
                var absoluteBounds = control.GetAbsoluteBounds();
                graphics.DrawCustomTexture(control.Background, ref absoluteBounds, 0, control.DrawGroupId);
            }
        }
    }
}
