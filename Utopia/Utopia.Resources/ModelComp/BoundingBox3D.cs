using System;
using SharpDX;
using SharpDX.Direct3D;
using S33M3DXEngine;
using S33M3CoreComponents.WorldFocus;
using S33M3Resources.Effects.Basics;
using S33M3DXEngine.Buffers;
using S33M3Resources.VertexFormats;
using S33M3Resources.Structs;
using S33M3CoreComponents.Cameras.Interfaces;
using SharpDX.Direct3D11;
using S33M3Resources.Primitives;

namespace Utopia.Resources.ModelComp
{
    public class BoundingBox3D : IDisposable
    {
        #region Private variable
        private D3DEngine _d3dEngine;
        private WorldFocusManager _worldFocusManager;
        private HLSLVertexPositionColor _wrappedEffect;
        //Buffer _vertexBuffer;
        private VertexBuffer<VertexPosition3Color> _vertexBuffer;
        private IndexBuffer<ushort> _indexBuffer;
        public Matrix BB3dworld;
        #endregion

        public BoundingBox3D(D3DEngine d3dEngine, WorldFocusManager worldFocusManager, Vector3 BBDimension, HLSLVertexPositionColor effect, ByteColor color)
        {
            _d3dEngine = d3dEngine;
            _worldFocusManager = worldFocusManager;
            _wrappedEffect = effect;
            CreateBBShape(BBDimension, ref color);
        }

        private void CreateBBShape(Vector3 BBDimension, ref ByteColor color)
        {
            Vector3[] position;
            ushort[] indices;
            Generator.Box(BBDimension, Generator.PrimitiveType.LineList, out position, out indices);

            VertexPosition3Color[] ptList = new VertexPosition3Color[position.Length];

            for (int i = 0; i < ptList.Length; i++)
            {
                ptList[i].Position = position[i];
                ptList[i].Color = color;
            }
            
            _vertexBuffer = new VertexBuffer<VertexPosition3Color>(_d3dEngine.Device, ptList.Length, PrimitiveTopology.LineList, "BoundingBox3D_vertexBuffer");
            _vertexBuffer.SetData(_d3dEngine.ImmediateContext,ptList);

            _indexBuffer = new IndexBuffer<ushort>(_d3dEngine.Device, indices.Length, "BoundingBox3D_indexBuffer");
            _indexBuffer.SetData(_d3dEngine.ImmediateContext, indices);
        }

        public void Update(Vector3 centerPosition, Vector3 scalingSize, float YOffset = 0)
        {
            if (YOffset == 0)
            {
                BB3dworld = Matrix.Scaling(scalingSize) * Matrix.Translation(centerPosition);
            }
            else
            {
                BB3dworld = Matrix.Scaling(new Vector3(scalingSize.X, scalingSize.Y - YOffset, scalingSize.Z)) * Matrix.Translation(centerPosition);
            }
        }

        public void Update(ref BoundingBox bb)
        {
            BB3dworld = Matrix.Scaling(new Vector3(bb.Maximum.X - bb.Minimum.X, bb.Maximum.Y - bb.Minimum.Y, bb.Maximum.Z - bb.Minimum.Z)) * Matrix.Translation(bb.GetCenter());
        }

        #region Private methods
        #endregion

        #region Public methods
        //My points are already in world space !
        public void Draw(DeviceContext context, ICameraFocused camera)
        {
            Matrix WorldFocused = Matrix.Identity;
            WorldFocused = _worldFocusManager.CenterOnFocus(ref BB3dworld);

            _wrappedEffect.Begin(context);
            _wrappedEffect.CBPerDraw.Values.World = Matrix.Transpose(WorldFocused);
            _wrappedEffect.CBPerDraw.IsDirty = true;
            _wrappedEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(camera.ViewProjection3D_focused);
            _wrappedEffect.CBPerFrame.IsDirty = true;
            _wrappedEffect.Apply(context);

            _vertexBuffer.SetToDevice(context, 0);
            _indexBuffer.SetToDevice(context, 0);

            _d3dEngine.ImmediateContext.DrawIndexed(_indexBuffer.IndicesCount, 0, 0);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_vertexBuffer != null) _vertexBuffer.Dispose();
            if (_indexBuffer != null) _indexBuffer.Dispose();
        }

        #endregion
    }
}
