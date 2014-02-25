using System;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.GameDXStates;

namespace Utopia.GUI
{
    public class ModelControlRenderer : IFlatControlRenderer<ModelControl>
    {
        public void Render(ModelControl control, IFlatGuiGraphics graphics)
        {
            var instance = control.ModelInstance;

            if (instance != null && control.VoxelEffect != null)
            {

                var bounds = control.GetAbsoluteBounds();
                var screenBounds = graphics.Engine.ViewPort.Bounds;
                var voxelEffect = control.VoxelEffect;
                var context = graphics.Engine.ImmediateContext;
                
                //var texture = new RenderedTexture2D(graphics.Engine, (int)bounds.Width, (int)bounds.Height, Format.R8G8B8A8_UNorm)
                //{
                //    BackGroundColor = new Color4(0, 0, 0, 0)
                //};

                float aspectRatio = bounds.Width / bounds.Height;
                Matrix projection;
                var fov = (float)Math.PI / 3.6f;
                Matrix.PerspectiveFovLH(fov, aspectRatio, 0.5f, 100f, out projection);
                Matrix view = Matrix.LookAtLH(new Vector3(0, 0, -1.9f), Vector3.Zero, Vector3.UnitY);

                //Set custom ViewPort
                graphics.Engine.SetCustomViewPort(new ViewportF(bounds.X, bounds.Y, bounds.Width, bounds.Height));

                //Rendering the Tool
                RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

                voxelEffect.Begin(context);

                voxelEffect.CBPerFrame.Values.LightDirection = Vector3.Zero;
                voxelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(view * projection);
                voxelEffect.CBPerFrame.IsDirty = true;
                
                var state = instance.VoxelModel.GetMainState();

                instance.SetState(state);

                var sphere = BoundingSphere.FromBox(state.BoundingBox);

                var rMax = 2f * Math.Sin(fov / 2);

                var size = state.BoundingBox.GetSize();

                var offset = -size / 2 - state.BoundingBox.Minimum;

                var scale = (float)rMax / sphere.Radius; // Math.Min(scaleFactor / size.X, Math.Min(scaleFactor / size.Y, scaleFactor / size.Z));

                instance.World = Matrix.Translation(offset) * Matrix.Scaling(scale) * Matrix.RotationY(MathHelper.Pi + MathHelper.PiOver4) * control.AlterTransform * Matrix.RotationQuaternion(control.Rotation);

                control.VisualVoxelModel.Draw(context, control.VoxelEffect, instance);

                graphics.Engine.SetScreenViewPort();
            }

        }
    }
}