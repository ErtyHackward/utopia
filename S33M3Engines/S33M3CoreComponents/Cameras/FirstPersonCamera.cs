using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine;
using SharpDX;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Cameras
{
    public class FirstPersonCamera : Camera
    {
        #region Private Variables
        private Vector3 _xAxis, _yAxis, _zAxis;
        #endregion

        #region Public Properties
        #endregion
        //Constructors

        public FirstPersonCamera(D3DEngine d3dEngine,
                                 float nearPlane,
                                 float farPlane)
            : base(d3dEngine, nearPlane, farPlane)
        {
            this.CameraType = Cameras.CameraType.FirstPerson;
        }

        #region Public Methods
        //Called once before the drawing sequence ==> Computed interpolated values here !
        public override void Interpolation(double interpolation_hd, float interpolation_ld, long elapsedTime)
        {
            if (CameraPlugin != null)
            {
                //Get the Camera Position and Rotation from the attached Entity to the camera !
                _worldPosition = CameraPlugin.CameraWorldPosition;
                _cameraOrientation = CameraPlugin.CameraOrientation;
                _cameraYAxisOrientation = CameraPlugin.CameraYAxisOrientation;
            }

            ComputeCameraMatrices();

            _frustum = new SimpleBoundingFrustum(ref _viewProjection3D);
        }

        #endregion

        #region Private Methods
        private void ComputeCameraMatrices()
        {
            //These view matrix computation are derived directly from Matrix.lookatlh() where I'm only doing needed math operations.

            //Extract the Rotation Matrix
            Matrix.RotationQuaternion(ref _cameraOrientation, out _view);

            //Extract the 3 axis from the RotationMatrix
            _xAxis = new Vector3(_view.M11, _view.M21, _view.M31);
            _yAxis = new Vector3(_view.M12, _view.M22, _view.M32);
            _zAxis = new Vector3(_view.M13, _view.M23, _view.M33);

            //Extract the LookAtVector
            _lookAt = _zAxis;

            //Camera computation ============================================================
            Vector3 cameraPosition = _worldPosition.AsVector3();

            //Recompute the view Matrix
            Vector3.Dot(ref _xAxis, ref cameraPosition, out _view.M41);
            Vector3.Dot(ref _yAxis, ref cameraPosition, out _view.M42);
            Vector3.Dot(ref _zAxis, ref cameraPosition, out _view.M43);
            _view.M41 *= -1;
            _view.M42 *= -1;
            _view.M43 *= -1;
            _viewProjection3D = _view * _projection3D;
        }

        #endregion

        #region IDebugInfo Members
        public override bool ShowDebugInfo { get; set; }
        public override string GetDebugInfo()
        {
            return string.Concat("<FirstPersonCamera> X : ", WorldPosition.X.ToString("0.000"), " Y : ", WorldPosition.Y.ToString("0.000"), " Z : ", WorldPosition.Z.ToString("0.000"), " Pitch : ", _lookAt.X.ToString("0.000"), " Yaw : ", _lookAt.Y.ToString("0.000"));
        }
        #endregion
    }
}
