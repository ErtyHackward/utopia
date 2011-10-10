using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using S33M3Engines.Cameras;
using S33M3Engines.Struct;
using Utopia.Shared.Structs;

namespace Utopia.Resources.ModelComp
{
    public class Line3D : IDrawable
    {
        #region Private variable
        private List<VertexPositionColor> _pointsList = new List<VertexPositionColor>();

        public VertexPositionColor[] PointsList
        {
            get { return _pointsList.ToArray(); }
        }
        #endregion

        #region Public properties
        #endregion

        public Line3D(Color color, Vector3 EndTo)
            : this(Vector3.Zero, EndTo, color)
        {
        }

        public Line3D(Vector3 StartFrom, Vector3 EndTo, Color color)
        {
            AddLine(StartFrom, EndTo, color);
        }

        #region Private methods
        #endregion

        #region Public methods
        public void AddLine(Vector3 EndTo, Color color)
        {
            AddLine(Vector3.Zero, EndTo, color);
        }

        public void AddLine(Vector3 StartFrom, Vector3 EndTo, Color color)
        {
            _pointsList.Add(new VertexPositionColor(StartFrom, color));
            _pointsList.Add(new VertexPositionColor(EndTo, color));
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
