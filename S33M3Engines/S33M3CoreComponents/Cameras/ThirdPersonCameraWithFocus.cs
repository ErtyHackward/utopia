using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.WorldFocus.Interfaces;
using S33M3Resources.Structs;
using SharpDX;
using S33M3CoreComponents.WorldFocus;
using S33M3DXEngine;

namespace S33M3CoreComponents.Cameras
{
    public class ThirdPersonCameraWithFocus : Camera, ICameraFocused, IWorldFocus
    {
        #region Private Variables
        private WorldFocusManager _worldFocusManager;
        private Matrix _viewProjection3D_focused;
        private Matrix _view_focused;
        private FTSValue<Vector3D> _focusPoint = new FTSValue<Vector3D>();
        private FTSValue<Matrix> _focusPointMatrix = new FTSValue<Matrix>();

        private Vector3 _xAxis, _yAxis, _zAxis;

        private float _springConstant = 16.0f;
        private float _dampingConstant = 8.0f;
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

        public float SpringConstant
        {
            get { return _springConstant; }
            set { _springConstant = value; _dampingConstant = (float)(2.0f * Math.Sqrt(value)); }
        }
        #endregion

        public ThirdPersonCameraWithFocus(D3DEngine d3dEngine, 
                         WorldFocusManager worldFocusManager,                
                         float nearPlane,
                         float farPlane)
            : base(d3dEngine, nearPlane, farPlane)
        {
            _worldFocusManager = worldFocusManager;
            this.CameraType = Cameras.CameraType.FirstPerson;
        }

        #region Public Methods
        public override void Update(S33M3DXEngine.Main.GameTime timeSpend)
        {
            if (CameraPlugin != null)
            {
                _worldPosition.BackUpValue();
                _cameraOrientation.BackUpValue();
                _cameraYAxisOrientation.BackUpValue();

                //Get the Camera Position and Rotation from the attached Entity to the camera !
                _worldPosition.Value = CameraPlugin.CameraWorldPosition;
                _cameraOrientation.Value = CameraPlugin.CameraOrientation;
                _cameraYAxisOrientation.Value = CameraPlugin.CameraYAxisOrientation;
                Matrix cameraRotation;
                Matrix.RotationQuaternion(ref _cameraOrientation.ValueInterp, out cameraRotation);
                _lookAt.Value = new Vector3(cameraRotation.M13, cameraRotation.M23, cameraRotation.M33);
            }
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            //Do interpolation on the value received at update time
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);
            Quaternion.Slerp(ref _cameraOrientation.ValuePrev, ref _cameraOrientation.Value, interpolationLd, out _cameraOrientation.ValueInterp);
            Quaternion.Slerp(ref _cameraYAxisOrientation.ValuePrev, ref _cameraYAxisOrientation.Value, interpolationLd, out _cameraYAxisOrientation.ValueInterp);

            //The camera is used as a focus Point = Its the 0;0;0 reference point in world space !!
            FocusPoint.ValueInterp = _worldPosition.ValueInterp;
            FocusPointMatrix.ValueInterp = Matrix.Translation(_worldPosition.ValueInterp.AsVector3() * -1);

            ComputeCameraMatrices();

            _frustum = new SimpleBoundingFrustum(ref _viewProjection3D);
        }
        #endregion

        #region Private Methods
        private void ComputeCameraMatrices()
        {
            //These view matrix computation are derived directly from Matrix.lookatlh() where I'm only doing needed math operations.

            //Extract the Rotation Matrix
            Matrix.RotationQuaternion(ref _cameraOrientation.ValueInterp, out _view);

            //Extract the 3 axis from the RotationMatrix
            _xAxis = new Vector3(_view.M11, _view.M21, _view.M31);
            _yAxis = new Vector3(_view.M12, _view.M22, _view.M32);
            _zAxis = new Vector3(_view.M13, _view.M23, _view.M33);

            //Extract the LookAtVector
            _lookAt.ValueInterp = _zAxis;

            //Focused camera computation ============================================================
            _view_focused = _view;
            _viewProjection3D_focused = _view_focused * _projection3D;

            //NOT Focused camera computation ============================================================
            Vector3 cameraPosition = _worldPosition.ValueInterp.AsVector3();

            //Recompute the view Matrix
            Vector3.Dot(ref _xAxis, ref cameraPosition, out _view.M41);
            Vector3.Dot(ref _yAxis, ref cameraPosition, out _view.M42);
            Vector3.Dot(ref _zAxis, ref cameraPosition, out _view.M43);
            _view.M41 *= -1;
            _view.M42 *= -1;
            _view.M43 *= -1;
            _viewProjection3D = _view * _projection3D;
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
            return string.Concat("<FirstPersonCamera> X : ", WorldPosition.ValueInterp.X.ToString("0.000"), " Y : ", WorldPosition.ValueInterp.Y.ToString("0.000"), " Z : ", WorldPosition.ValueInterp.Z.ToString("0.000"), " Pitch : ", _lookAt.ValueInterp.X.ToString("0.000"), " Yaw : ", _lookAt.ValueInterp.Y.ToString("0.000"));
        }
        #endregion
    }
}
