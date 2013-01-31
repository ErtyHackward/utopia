using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LtreeVisualizer.Components
{
    public class Entity : GameComponent, ICameraPlugin
    {
        private Vector3D _entityWorldPosition;

        public Vector3D EntityWorldPosition
        {
            get { return _entityWorldPosition; }
            set { _entityWorldPosition = value; }
        }
        private Quaternion _entityOrientation;
        private Quaternion _entityYAxisOrientation;

        #region ICameraPlugin Interface implementation
        public Vector3D CameraWorldPosition
        {
            get { return _entityWorldPosition; }
        }

        public Quaternion CameraOrientation
        {
            get { return _entityOrientation; }
        }

        public Quaternion CameraYAxisOrientation
        {
            get { return _entityYAxisOrientation; }
        }

        public int CameraUpdateOrder
        {
            get { return UpdateOrder; }
        }

        #endregion

        public Entity(Vector3D worldPosition, Quaternion entityOrientation)
        {
            UpdateOrder = 10;

            _entityWorldPosition = worldPosition;
            _entityOrientation = entityOrientation;
            _entityYAxisOrientation = Quaternion.Identity;
        }
    }
}
