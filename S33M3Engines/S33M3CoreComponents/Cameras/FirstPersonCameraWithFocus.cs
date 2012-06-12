using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Windows.Forms;
using S33M3DXEngine;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.WorldFocus;
using S33M3Resources.Structs;
using S33M3CoreComponents.WorldFocus.Interfaces;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.Main;

namespace S33M3CoreComponents.Cameras
{
    public class FirstPersonCameraWithFocus : Camera, ICameraFocused, IWorldFocus
    {
        #region Private Variables
        WorldFocusManager _worldFocusManager;
        protected Matrix _viewProjection3D_focused;
        protected Matrix _view_focused;
        private readonly FTSValue<Vector3D> _focusPoint = new FTSValue<Vector3D>();
        private readonly FTSValue<Matrix> _focusPointMatrix = new FTSValue<Matrix>();
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
        //Constructors

        public FirstPersonCameraWithFocus(D3DEngine d3dEngine, 
                                          WorldFocusManager worldFocusManager,
                                          float nearPlane,
                                          float farPlane)
            : base(d3dEngine, nearPlane, farPlane)
        {
            _worldFocusManager = worldFocusManager;
            this.CameraType = Cameras.CameraType.FirstPerson;
        }

        #region Private Methods
        #endregion

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

            //Value are already interpolated by the pluggin, no need to interpolate again !
            FocusPoint.ValueInterp = _worldPosition;
            FocusPointMatrix.ValueInterp = Matrix.Translation(-1 * _worldPosition.AsVector3());
            
            Matrix MTranslation = Matrix.Translation((_worldPosition - _worldFocusManager.WorldFocus.FocusPoint.ValueInterp).AsVector3() * -1); //Inverse the Translation
            Quaternion inverseRotation = Quaternion.Conjugate(_cameraOrientation); //Inverse the rotation
            Matrix MRotation = Matrix.RotationQuaternion(inverseRotation);                                             
            Matrix.Multiply(ref MTranslation, ref MRotation, out _view_focused);

            _viewProjection3D_focused = _view_focused * _projection3D;
            _viewProjection3D = Matrix.Translation(_worldPosition.AsVector3() * -1) * MRotation * _projection3D;


            _frustum = new SimpleBoundingFrustum(ref _viewProjection3D);

            //SharpDXboundingFrustum BF = new SharpDXboundingFrustum(_viewProjection3D);
            
            //Refresh the lookat camera vector
            _lookAt = MQuaternion.GetLookAtFromQuaternion(inverseRotation);
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
