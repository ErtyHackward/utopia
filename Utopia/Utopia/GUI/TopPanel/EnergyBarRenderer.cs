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
        private VertexBuffer<VertexPosition3Color> _vertexWireBuffer;
        private VertexBuffer<VertexPosition3Color> _vertexSolidBuffer;

        private IndexBuffer<ushort> _indexWireBuffer;
        private IndexBuffer<ushort> _indexSolidBuffer;

        public EnergyBarRenderer()
        {
        }

        private void InitRendered(D3DEngine engine)
        {
            Vector3[] position;
            ushort[] indices;
            VertexPosition3Color[] ptList;
            //WIRE VERTEX DATA CREATION =============================================================================
            Generator.Box(new Vector3(1.0f, 1.0f, 1.0f), Generator.PrimitiveType.LineList, out position, out indices);
            ptList = new VertexPosition3Color[position.Length];

            for (int i = 0; i < ptList.Length; i++)
            {
                ptList[i].Position = position[i];
                ptList[i].Color = SharpDX.Color.Black;
            }

            _vertexWireBuffer = new VertexBuffer<VertexPosition3Color>(engine.Device, ptList.Length, PrimitiveTopology.LineList, "BoundingBox3D_vertexBuffer");
            _vertexWireBuffer.SetData(engine.ImmediateContext, ptList);

            _indexWireBuffer = new IndexBuffer<ushort>(engine.Device, indices.Length, "BoundingBox3D_indexBuffer");
            _indexWireBuffer.SetData(engine.ImmediateContext, indices);

            //SOLID VERTEX DATA CREATION =============================================================================
            Generator.Box(new Vector3(1.0f, 1.0f, 1.0f), Generator.PrimitiveType.TriangleList, out position, out indices);
            ptList = new VertexPosition3Color[position.Length];

            for (int i = 0; i < ptList.Length; i++)
            {
                ptList[i].Position = position[i];
                ptList[i].Color = SharpDX.Color.Red;
            }

            _vertexSolidBuffer = new VertexBuffer<VertexPosition3Color>(engine.Device, ptList.Length, PrimitiveTopology.TriangleList, "BoundingBox3D_vertexBuffer");
            _vertexSolidBuffer.SetData(engine.ImmediateContext, ptList);

            _indexSolidBuffer = new IndexBuffer<ushort>(engine.Device, indices.Length, "BoundingBox3D_indexBuffer");
            _indexSolidBuffer.SetData(engine.ImmediateContext, indices);


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

            Vector3 eye = new Vector3(-2.5f, 1.5f, 45.0f);
            Matrix view = Matrix.LookAtLH(eye, Vector3.Zero, Vector3.UnitY);

            //Set custom ViewPort
            graphics.Engine.SetCustomViewPort(new ViewportF(bounds.X, bounds.Y, bounds.Width, bounds.Height));

            //Rendering the Tool
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            _barGrid.Begin(context);
            _barGrid.CBPerFrame.Values.ViewProjection = Matrix.Transpose(view * projection);
            _barGrid.CBPerFrame.IsDirty = true;

            //Solid Drawing
            _vertexSolidBuffer.SetToDevice(context, 0);
            _indexSolidBuffer.SetToDevice(context, 0);
            float width = (bounds.Width - (bounds.Width / 5)) / 2;
            _barGrid.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Scaling(width, 20f, 90f) * Matrix.Translation((bounds.Width - width) /2.0f, 0f, 0f));
            _barGrid.CBPerDraw.IsDirty = true;

            _barGrid.Apply(context);

            context.DrawIndexed(_indexSolidBuffer.IndicesCount, 0, 0);

            //Wire Drawing
            _vertexWireBuffer.SetToDevice(context, 0);
            _indexWireBuffer.SetToDevice(context, 0);
            width = bounds.Width - (bounds.Width / 5);
            _barGrid.CBPerDraw.Values.World = Matrix.Transpose(Matrix.Scaling(width, 20f, 90f));
            _barGrid.CBPerDraw.IsDirty = true;

            _barGrid.Apply(context);

            context.DrawIndexed(_indexWireBuffer.IndicesCount, 0 , 0);

            graphics.Engine.SetScreenViewPort();
        }
    }
}
