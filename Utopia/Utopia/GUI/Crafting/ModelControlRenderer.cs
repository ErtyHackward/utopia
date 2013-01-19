using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using SharpDX;

namespace Utopia.GUI.Crafting
{
    public class ModelControlRenderer : IFlatControlRenderer<ModelControl>
    {
        public void Render(ModelControl control, IFlatGuiGraphics graphics)
        {
            var instance = control.ModelInstance;

            if (instance != null && control.VoxelEffect != null)
            {

                var bounds = control.GetAbsoluteBounds();
                
                var view = Matrix.LookAtLH(new Vector3(), Vector3.One, Vector3.UnitY);
                var projection = Matrix.PerspectiveOffCenterLH(bounds.Left, bounds.Right, bounds.Bottom,
                                                                       bounds.Top,
                                                                       0.01f, 1f);

                var voxelEffect = control.VoxelEffect;
                voxelEffect.Begin(graphics.Engine.ImmediateContext);
                voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(view * projection);
                voxelEffect.CBPerFrame.IsDirty = true;

                instance.World = Matrix.Scaling(1f / 16);

                control.VisualVoxelModel.Draw(graphics.Engine.ImmediateContext, voxelEffect, instance);
            }

        }
    }
}
