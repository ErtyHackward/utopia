﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Cameras;
using S33M3Engines.Maths;
using S33M3Engines.Buffers;
using S33M3Engines.D3D;
using S33M3Engines.D3D.Effects.Basics;
using SharpDX.Direct3D;
using Utopia.Shared.Structs;
using S33M3Engines;
using S33M3Engines.WorldFocus;

namespace Utopia.Resources.ModelComp
{
    public class BoundingBox3D : IDisposable
    {
        #region Private variable
        private D3DEngine _d3dEngine;
        private WorldFocusManager _worldFocusManager;
        private HLSLVertexPositionColor _wrappedEffect;
        //Buffer _vertexBuffer;
        private VertexBuffer<VertexPositionColor> _vertexBuffer;
        private Line3D[] _lines = new Line3D[12];
        private Matrix BB3dworld;
        #endregion

        #region Public properties
        #endregion

        public BoundingBox3D(D3DEngine d3dEngine, WorldFocusManager worldFocusManager, Vector3 BBDimension, HLSLVertexPositionColor effect, Color color)
        {
            _d3dEngine = d3dEngine;
            _worldFocusManager = worldFocusManager;
            _wrappedEffect = effect;
            CreateBBShape(ref BBDimension, ref color);
        }

        private void CreateBBShape(ref Vector3 BBDimension, ref Color color)
        {
            VertexPositionColor[] ptList = new VertexPositionColor[24];
            ptList[0] = new VertexPositionColor() { Position = new Vector3(0, 0, 0), Color = color };
            ptList[1] = new VertexPositionColor() { Position = new Vector3(0, 0, BBDimension.Z), Color = color };
            ptList[2] = new VertexPositionColor() { Position = new Vector3(0, 0, BBDimension.Z), Color = color };
            ptList[3] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, 0, BBDimension.Z), Color = color };
            ptList[4] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, 0, BBDimension.Z), Color = color };
            ptList[5] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, 0, 0), Color = color };
            ptList[6] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, 0, 0), Color = color };
            ptList[7] = new VertexPositionColor() { Position = new Vector3(0, 0, 0), Color = color };

            ptList[8] = new VertexPositionColor() { Position = new Vector3(0, BBDimension.Y, 0), Color = color };
            ptList[9] = new VertexPositionColor() { Position = new Vector3(0, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[10] = new VertexPositionColor() { Position = new Vector3(0, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[11] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[12] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[13] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, BBDimension.Y, 0), Color = color };
            ptList[14] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, BBDimension.Y, 0), Color = color };
            ptList[15] = new VertexPositionColor() { Position = new Vector3(0, BBDimension.Y, 0), Color = color };

            ptList[16] = new VertexPositionColor() { Position = new Vector3(0, 0, 0), Color = color };
            ptList[17] = new VertexPositionColor() { Position = new Vector3(0, BBDimension.Y, 0), Color = color };
            ptList[18] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, 0, 0), Color = color };
            ptList[19] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, BBDimension.Y, 0), Color = color };
            ptList[20] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, 0, BBDimension.Z), Color = color };
            ptList[21] = new VertexPositionColor() { Position = new Vector3(BBDimension.X, BBDimension.Y, BBDimension.Z), Color = color };
            ptList[22] = new VertexPositionColor() { Position = new Vector3(0, 0, BBDimension.Z), Color = color };
            ptList[23] = new VertexPositionColor() { Position = new Vector3(0, BBDimension.Y, BBDimension.Z), Color = color };

            _vertexBuffer = new VertexBuffer<VertexPositionColor>(_d3dEngine, 24, VertexPositionColor.VertexDeclaration, PrimitiveTopology.LineList, "BoundingBox3D_vertexBuffer");
            _vertexBuffer.SetData(ptList);
        }

        public void Update(ref BoundingBox bb)
        {
            BB3dworld = Matrix.Translation(bb.Minimum);
        }

        public void Update(ref BoundingBox bb, Vector3 scalingSize)
        {
            BB3dworld = Matrix.Scaling(scalingSize) * Matrix.Translation(bb.Minimum);
        }

        #region Private methods
        #endregion

        #region Public methods
        //My points are already on world space !
        public void Draw(ICamera camera)
        {
            Matrix WorldFocused = Matrix.Identity;
            WorldFocused = _worldFocusManager.CenterOnFocus(ref BB3dworld);

            _wrappedEffect.Begin();
            _wrappedEffect.CBPerDraw.Values.World = Matrix.Transpose(WorldFocused);
            _wrappedEffect.CBPerDraw.IsDirty = true;
            _wrappedEffect.CBPerFrame.Values.View = Matrix.Transpose(camera.View_focused);
            _wrappedEffect.CBPerFrame.Values.Projection = Matrix.Transpose(camera.Projection3D);
            _wrappedEffect.CBPerFrame.IsDirty = true;
            _wrappedEffect.Apply();

            _vertexBuffer.SetToDevice(0);

            _d3dEngine.Context.Draw(24, 0);
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
