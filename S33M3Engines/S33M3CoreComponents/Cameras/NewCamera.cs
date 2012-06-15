using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.WorldFocus.Interfaces;
using SharpDX;
using S33M3Resources.Structs;
using S33M3CoreComponents.WorldFocus;
using S33M3DXEngine;

namespace S33M3CoreComponents.Cameras
{
    public class NewCamera : Camera, ICameraFocused, IWorldFocus
    {
        #region Private Variables
        private WorldFocusManager _worldFocusManager;
        private Matrix _viewProjection3D_focused;
        private Matrix _view_focused;
        private FTSValue<Vector3D> _focusPoint = new FTSValue<Vector3D>();
        private FTSValue<Matrix> _focusPointMatrix = new FTSValue<Matrix>();

        private Vector3 _xAxis, _yAxis, _zAxis;
        #endregion

        #region Public Properties
        public FTSValue<Vector3D> FocusPoint
        {
            get { return _focusPoint; }
        }

        public FTSValue<Matrix> FocusPointMatrix
        {
            get { return _focusPointMatrix; }
        }

        public Matrix ViewProjection3D_focused
        {
            get { return _viewProjection3D_focused; }
        }

        public Matrix View_focused
        {
            get { return _view_focused; }
        }
        #endregion

        public NewCamera(D3DEngine d3dEngine, 
                         WorldFocusManager worldFocusManager,                
                         float nearPlane,
                         float farPlane)
            : base(d3dEngine, nearPlane, farPlane)
        {
            _worldFocusManager = worldFocusManager;
            this.CameraType = Cameras.CameraType.FirstPerson;
        }

        #region Public Methods
        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            if (CameraPlugin != null)
            {
                //Get the Camera Position and Rotation from the attached Entity to the camera !
                _worldPosition = CameraPlugin.CameraWorldPosition;
                _cameraOrientation = CameraPlugin.CameraOrientation;
                _cameraYAxisOrientation = CameraPlugin.CameraYAxisOrientation;
            }

            //The camera is used as a focus Point = Is the 0;0;0 reference point in world space !!
            FocusPoint.ValueInterp = _worldPosition;
            FocusPointMatrix.ValueInterp = Matrix.Translation(_worldPosition.AsVector3());

            ComputeCameraMatrices();

            _frustum = new SimpleBoundingFrustum(ref _viewProjection3D);
        }
        #endregion

        #region Private Methods
        private void ComputeCameraMatrices()
        {
            //Normalize the quaternion
            Quaternion.Normalize(ref _cameraOrientation, out _cameraOrientation);
            //Extract the Rotation Matrix
            Matrix.RotationQuaternion(ref _cameraOrientation, out _view);

            //Extract the 3 axis from the RotationMatrix
            _xAxis = new Vector3(_view_focused.M11, _view_focused.M21, _view.M31);
            _yAxis = new Vector3(_view_focused.M12, _view_focused.M22, _view.M32);
            _zAxis = new Vector3(_view_focused.M13, _view_focused.M23, _view.M33);

            //Extract the LookAtVector
            _lookAt = _zAxis;

            //Focused camera computation ============================================================
            _view_focused = _view;
            //Get camera focused position == Always Zero in case of focused camera !!
            Vector3 cameraFocusedPosition = Vector3.Zero; //(_worldPosition - _worldFocusManager.WorldFocus.FocusPoint.ValueInterp).AsVector3();

            //Recompute the view Matrix
            _view_focused.M31 = Vector3.Dot(_xAxis, cameraFocusedPosition) * -1;
            _view_focused.M32 = Vector3.Dot(_yAxis, cameraFocusedPosition) * -1;
            _view_focused.M33 = Vector3.Dot(_zAxis, cameraFocusedPosition) * -1;

            _viewProjection3D_focused = _view_focused * _projection3D;

            //NOT Focused camera computation ============================================================
            Vector3 cameraPosition = _worldPosition.AsVector3();

            //Recompute the view Matrix
            _view.M31 = Vector3.Dot(_xAxis, cameraPosition) * -1;
            _view.M32 = Vector3.Dot(_yAxis, cameraPosition) * -1;
            _view.M33 = Vector3.Dot(_zAxis, cameraPosition) * -1;

            _viewProjection3D = _view_focused * _projection3D;
        }

        protected override void CameraInitialize()
        {
            base.CameraInitialize();
        }

        protected override void newCameraPluginDriver()
        {
            FocusPoint.Value = CameraPlugin.CameraWorldPosition;
            FocusPoint.ValueInterp = CameraPlugin.CameraWorldPosition;
            base.newCameraPluginDriver();
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
