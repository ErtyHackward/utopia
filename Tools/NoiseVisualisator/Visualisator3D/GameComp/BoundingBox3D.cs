using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine;
using S33M3Resources.Effects.Basics;
using S33M3DXEngine.Buffers;
using S33M3Resources.VertexFormats;
using SharpDX;
using S33M3DXEngine.Main.Interfaces;
using S33M3Resources.Structs;
using S33M3CoreComponents.Cameras.Interfaces;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace NoiseVisualisator.Visualisator3D.GameComp
{
    public class BoundingBox3D : IDisposable
    {
        #region Private variable
        private D3DEngine _d3dEngine;
        private HLSLVertexPositionColor _wrappedEffect;
        //Buffer _vertexBuffer;
        private VertexBuffer<VertexPosition3Color> _vertexBuffer;
        private Line3D[] _lines = new Line3D[12];
        private Matrix BB3dworld;
        #endregion

        #region Public properties
        #endregion

        public BoundingBox3D(D3DEngine d3dEngine, Vector3 BBDimension, HLSLVertexPositionColor effect, ByteColor color)
        {
            _d3dEngine = d3dEngine;
            _wrappedEffect = effect;
            CreateBBShape(BBDimension / 2, ref color);
        }

        private void CreateBBShape(Vector3 BBDimension, ref ByteColor color)
        {
            VertexPosition3Color[] ptList = new VertexPosition3Color[24];
            ptList[0] = new VertexPosition3Color() { Position = new Vector3(-BBDimension.X, -BBDimension.Y, -BBDimension.Z), Color = color };
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
            _vertexBuffer.SetData(_d3dEngine.ImmediateContext, ptList);
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
        //My points are already in world space !
        public void Draw(DeviceContext context,Matrix View,Matrix Projection)
        {
            Matrix WorldFocused = Matrix.Translation(64,64,64);
            Matrix ViewP = Matrix.Transpose ( View * Projection);
            _wrappedEffect.Begin(context);
            _wrappedEffect.CBPerDraw.Values.World = Matrix.Transpose(WorldFocused);
            _wrappedEffect.CBPerDraw.IsDirty = true;
            _wrappedEffect.CBPerFrame.Values.View = Matrix.Transpose(View);
            _wrappedEffect.CBPerFrame.Values.Projection = Matrix.Transpose(Projection);
            _wrappedEffect.CBPerFrame.IsDirty = true;
            _wrappedEffect.Apply(context);

            _vertexBuffer.SetToDevice(context, 0);

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

    public class Line3D : IDrawable
    {
        #region Private variable
        private List<VertexPosition3Color> _pointsList = new List<VertexPosition3Color>();

        public VertexPosition3Color[] PointsList
        {
            get { return _pointsList.ToArray(); }
        }
        #endregion

        #region Public properties
        #endregion

        public Line3D(Color4 color, Vector3 EndTo)
            : this(Vector3.Zero, EndTo, color)
        {
        }

        public Line3D(Vector3 StartFrom, Vector3 EndTo, Color4 color)
        {
            AddLine(StartFrom, EndTo, color);
        }

        #region Private methods
        #endregion

        #region Public methods
        public void AddLine(Vector3 EndTo, ByteColor color)
        {
            AddLine(Vector3.Zero, EndTo, color);
        }

        public void AddLine(Vector3 StartFrom, Vector3 EndTo, ByteColor color)
        {
            _pointsList.Add(new VertexPosition3Color(StartFrom, color));
            _pointsList.Add(new VertexPosition3Color(EndTo, color));
        }

        public void Draw(ref ICamera camera, ref Matrix world)
        {
        }

        public void Draw()
        {
            //GEngine.GD.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, _pointList.ToArray(), 0, _pointList.Count() / 2);
        }
        #endregion

        public void Draw(DeviceContext context, int index)
        {
        }

        public void Update(S33M3DXEngine.Main.GameTime timeSpend)
        {
        }

        public void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
        }

        public void Dispose()
        {
        }
    }
}
