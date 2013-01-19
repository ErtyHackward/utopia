using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;

namespace Utopia.GUI.Inventory
{
    public class ModelControlRenderer : IFlatControlRenderer<ModelControl>
    {
        public void Render(ModelControl control, IFlatGuiGraphics graphics)
        {
            var instance = control.ModelInstance;

            if (instance != null)
            {
                control.VisualVoxelModel.Draw(graphics.Engine.ImmediateContext, control.VoxelEffect, instance);
            }

        }
    }
}
