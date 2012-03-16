using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using Utopia.Shared.Structs;
using S33M3DXEngine;
using S33M3CoreComponents.WorldFocus;
using S33M3Resources.Effects.Basics;
using S33M3DXEngine.Buffers;
using S33M3Resources.Structs.Vertex;
using S33M3Resources.VertexFormats;
using S33M3Resources.Structs;
using S33M3CoreComponents.Cameras.Interfaces;

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
        private Line3D[] _lines = new Line3D[12];
        private Matrix BB3dworld;
        #endregion

        #region Public properties
        #endregion

        public BoundingBox3D(D3DEngine d3dEngine, WorldFocusManager worldFocusManager, Vector3 BBDimension, HLSLVertexPositionColor effect, ByteColor color)
        {
            _d3dEngine = d3dEngine;
            _worldFocusManager = worldFocusManager;
            _wrappedEffect = effect;
            CreateBBShape(BBDimension / 2, ref color);
        }

        private void CreateBBShape(Vector3 BBDimension, ref ByteColor color)
        {
            VertexPosition3Color[] ptList = new VertexPosition3Color[24];
            ptList[0] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, -BBDimension.Y, -BBDimension.Z ), Color = color };
            ptList[1] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, -BBDimension.Y, BBDimension.Z), Color = color };
            ptList[2] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, -BBDimension.Y, BBDimension.Z), Color = color };
            ptList[3] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, -BBDimension.Y, BBDimension.Z), Color = color };
            ptList[4] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, -BBDimension.Y, BBDimension.Z), Color = color };
            ptList[5] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, -BBDimension.Y, -BBDimension.Z), Color = color };
            ptList[6] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, -BBDimension.Y, -BBDimension.Z), Color = color };
            ptList[7] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, -BBDimension.Y, -BBDimension.Z), Color = color };

            ptList[8] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, BBDimension.Y, -BBDimension.Z), Color = color };
            ptList[9] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[10] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[11] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[12] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[13] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, BBDimension.Y, -BBDimension.Z), Color = color };
            ptList[14] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, BBDimension.Y, -BBDimension.Z), Color = color };
            ptList[15] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, BBDimension.Y, -BBDimension.Z), Color = color };

            ptList[16] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, -BBDimension.Y, -BBDimension.Z), Color = color };
            ptList[17] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, BBDimension.Y, -BBDimension.Z), Color = color };
            ptList[18] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, -BBDimension.Y, -BBDimension.Z), Color = color };
            ptList[19] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, BBDimension.Y, -BBDimension.Z), Color = color };
            ptList[20] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, -BBDimension.Y, BBDimension.Z), Color = color };
            ptList[21] = new VertexPosition3Color() { Position = new Vector3(BBDimension.X, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[22] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, -BBDimension.Y, BBDimension.Z), Color = color };
            ptList[23] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, BBDimension.Y, BBDimension.Z), Color = color };

            _vertexBuffer = new VertexBuffer<VertexPosition3Color>(_d3dEngine.Device, 24, VertexPosition3Color.VertexDeclaration, PrimitiveTopology.LineList, "BoundingBox3D_vertexBuffer");
            _vertexBuffer.SetData(_d3dEngine.ImmediateContext,ptList);
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

        #region Private methods
        #endregion

        #region Public methods
        //My points are already on world space !
        public void Draw(ICameraFocused camera)
        {
            Matrix WorldFocused = Matrix.Identity;
            WorldFocused = _worldFocusManager.CenterOnFocus(ref BB3dworld);

            _wrappedEffect.Begin(_d3dEngine.ImmediateContext);
            _wrappedEffect.CBPerDraw.Values.World = Matrix.Transpose(WorldFocused);
            _wrappedEffect.CBPerDraw.IsDirty = true;
            _wrappedEffect.CBPerFrame.Values.View = Matrix.Transpose(camera.View_focused);
            _wrappedEffect.CBPerFrame.Values.Projection = Matrix.Transpose(camera.Projection3D);
            _wrappedEffect.CBPerFrame.IsDirty = true;
            _wrappedEffect.Apply(_d3dEngine.ImmediateContext);

            _vertexBuffer.SetToDevice(_d3dEngine.ImmediateContext,0);

            _d3dEngine.ImmediateContext.Draw(24, 0);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_vertexBuffer != null) _vertexBuffer.Dispose();
        }

        #endregion
    }
}
