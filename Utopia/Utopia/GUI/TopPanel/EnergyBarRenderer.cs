using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Effects.Basics;
using S33M3Resources.Primitives;
using S33M3Resources.VertexFormats;
using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.GameDXStates;

namespace Utopia.GUI.TopPanel
{
    public class EnergyBarRenderer : IFlatControlRenderer<EnergyBar>
    {
        private HLSLVertexPositionColor _barGrid;
        private VertexBuffer<VertexPosition3Color> _vertexBuffer;
        private IndexBuffer<ushort> _indexBuffer;

        public EnergyBarRenderer()
        {
        }

        private void InitRendered(D3DEngine engine)
        {
            Vector3[] position;
            ushort[] indices;
            Generator.Box(new Vector3(1.0f, 1.0f, 1.0f), Generator.PrimitiveType.LineList, out position, out indices);
            VertexPosition3Color[] ptList = new VertexPosition3Color[position.Length];

            for (int i = 0; i < ptList.Length; i++)
            {
                ptList[i].Position = position[i];
                ptList[i].Color = SharpDX.Color.Red;
            }

            _vertexBuffer = new VertexBuffer<VertexPosition3Color>(engine.Device, ptList.Length, PrimitiveTopology.LineList, "BoundingBox3D_vertexBuffer");
            _vertexBuffer.SetData(engine.ImmediateContext, ptList);

            _indexBuffer = new IndexBuffer<ushort>(engine.Device, indices.Length, "BoundingBox3D_indexBuffer");
            _indexBuffer.SetData(engine.ImmediateContext, indices);

            _barGrid = new HLSLVertexPositionColor(engine.Device);
        }

        public void Render(EnergyBar control, IFlatGuiGraphics graphics)
        {
            if (_barGrid == null) InitRendered(graphics.Engine);

            var bounds = control.GetAbsoluteBounds();
            var context = graphics.Engine.ImmediateContext;


            float aspectRatio = bounds.Width / bounds.Height;
            Matrix projection;
            var fov = (float)Math.PI / 3.6f;
            Matrix.OrthoLH(bounds.Width, bounds.Height, 0.5f, 100f, out projection);
            //Matrix.PerspectiveFovLH(fov, aspectRatio, 0.5f, 100f, out projection);

            Vector3 eye = new Vector3(3.0f, 1.0f, 1.9f);
            Vector3 lookat = new Vector3(0f, 0f, 0.0f);
            Matrix view = Matrix.LookAtLH(eye, lookat, Vector3.UnitY);

            //Set custom ViewPort
            graphics.Engine.SetCustomViewPort(new ViewportF(bounds.X, bounds.Y, bounds.Width, bounds.Height));

            //Rendering the Tool
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.CullNone, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            _barGrid.Begin(context);
            _vertexBuffer.SetToDevice(context, 0);
            _indexBuffer.SetToDevice(context, 0);

            _barGrid.CBPerFrame.Values.ViewProjection = Matrix.Transpose(view * projection);
            _barGrid.CBPerFrame.IsDirty = true;

            _barGrid.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Scaling(20f, 20f, 60f) * Matrix.Identity );
            _barGrid.CBPerDraw.IsDirty = true;

            _barGrid.Apply(context);

            context.DrawIndexed(_indexBuffer.IndicesCount, 0 , 0);

            graphics.Engine.SetScreenViewPort();
        }
    }
}
