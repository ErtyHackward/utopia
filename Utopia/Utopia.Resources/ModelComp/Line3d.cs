using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Shared.Structs;
using S33M3_Resources.Struct.Vertex;
using S33M3_CoreComponents.Cameras.Interfaces;
using S33M3_Resources.VertexFormats;
using S33M3_Resources.Structs;

namespace Utopia.Resources.ModelComp
{
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
    }
}
