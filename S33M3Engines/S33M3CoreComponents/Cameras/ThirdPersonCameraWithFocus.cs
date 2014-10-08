using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.WorldFocus.Interfaces;
using S33M3Resources.Structs;
using SharpDX;
using S33M3CoreComponents.WorldFocus;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using System.Windows.Forms;

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

        private float _offsetDistance = 5.0f;
        private float _zoomingPower = 0;
        private float _zoomingStep = 5f;

        private bool _isbackLooking = true;

        private InputsManager _inputManager;
        #endregion

        #region Public Properties

        public float MaxDistance { get; set; }

        public float Distance { 
            get { return _offsetDistance; } 
            set { _offsetDistance = value; } 
        }

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

        public Vector3 CameraPosition { get; set; }

        public delegate bool CheckCameraPosition(ref Vector3D newPosition2Evaluate);
        public CheckCameraPosition CheckCamera;
        #endregion

        public ThirdPersonCameraWithFocus(D3DEngine d3dEngine, 
                         WorldFocusManager worldFocusManager,                
                         float nearPlane,
                         float farPlane,
                         InputsManager inputManager)
            : base(d3dEngine, nearPlane, farPlane)
        {
            _inputManager = inputManager;
            _worldFocusManager = worldFocusManager;
            this.CameraType = Cameras.CameraType.ThirdPerson;

            MaxDistance = 50f;
        }

        #region Public Methods
        public override void FTSUpdate(S33M3DXEngine.Main.GameTime timeSpend)
        {
            if (_inputManager.KeyboardManager.CurKeyboardState.IsKeyDown(Keys.LControlKey))
            {
                _isbackLooking = false;
            }
            else _isbackLooking = true;

            if (_inputManager.ActionsManager.isTriggered(Actions.ScrollWheelBackward))
            {
                _zoomingPower = -1f * 8 * _offsetDistance / MaxDistance;
                _zoomingStep = 30f * _offsetDistance / MaxDistance;
            }

            if (_inputManager.ActionsManager.isTriggered(Actions.ScrollWheelForward))
            {
                _zoomingPower = 1f * 8 * _offsetDistance / MaxDistance;
                _zoomingStep = 30f * _offsetDistance / MaxDistance;
            }

            if (CameraPlugin == null) return;

            if (NewlyActivatedCamera)
            {
                _worldPosition.Initialize(CameraPlugin.CameraWorldPosition);
                _cameraOrientation.Initialize(CameraPlugin.CameraOrientation);
                _cameraYAxisOrientation.Initialize(CameraPlugin.CameraYAxisOrientation);
                NewlyActivatedCamera = false;
            }
            else
            {
                _worldPosition.BackUpValue();
                _cameraOrientation.BackUpValue();
                _cameraYAxisOrientation.BackUpValue();
                //Get the Camera Position and Rotation from the attached Entity to the camera !
                _worldPosition.Value = CameraPlugin.CameraWorldPosition;
                _cameraOrientation.Value = CameraPlugin.CameraOrientation;
                _cameraYAxisOrientation.Value = CameraPlugin.CameraYAxisOrientation;
            }
            //Get the LookAt Vector
            Matrix cameraRotation;
            Matrix.RotationQuaternion(ref _cameraOrientation.Value, out cameraRotation);
            _lookAt.Value = new Vector3(cameraRotation.M13, cameraRotation.M23, cameraRotation.M33);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (_zoomingPower != 0.0f)
            {
                if (_zoomingPower > 0)
                {
                    _offsetDistance -= _zoomingStep * (float)elapsedTime;
                    _zoomingPower -= _zoomingStep * (float)elapsedTime;
                    if (_zoomingPower < 0.0f) _zoomingPower = 0.0f;
                    if (_offsetDistance < 0.0f) _offsetDistance = 0.0f;
                }
                else
                {
                    _offsetDistance += _zoomingStep * (float)elapsedTime;
                    _zoomingPower += _zoomingStep * (float)elapsedTime;
                    if (_zoomingPower > 0.0f) _zoomingPower = 0.0f;
                    if (_offsetDistance > MaxDistance) _offsetDistance = MaxDistance;
                }
            }

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

            int way = -1;
            if (_isbackLooking == false)
            {
                way = 1;
            }

            //float _validatedOffsetDistance = _offsetDistance;
            //Vector3 cameraFocusedPosition = _zAxis * way * _validatedOffsetDistance;
            //Vector3D evaluatedCameraWorldPosition;
            //bool isCameraPositionCorrect = false;
            //while (isCameraPositionCorrect == false && _validatedOffsetDistance > 0)
            //{
            //    isCameraPositionCorrect = true;
            //    foreach (CheckCameraPosition fct in CheckCamera.GetInvocationList())
            //    {
            //        evaluatedCameraWorldPosition = _worldPosition.ValueInterp + cameraFocusedPosition;
            //        isCameraPositionCorrect = fct(ref evaluatedCameraWorldPosition);
            //        if (isCameraPositionCorrect == false) break;
            //    }
            //    _validatedOffsetDistance -= 0.01f;
            //    if (_validatedOffsetDistance < 0) _validatedOffsetDistance = 0;
            //    cameraFocusedPosition = _zAxis * way * _validatedOffsetDistance;
            //}

            float _validatedOffsetDistance = 0; // _offsetDistance;
            Vector3 cameraFocusedPosition = _zAxis * way * _validatedOffsetDistance;
            Vector3D evaluatedCameraWorldPosition;
            bool isCameraPositionCorrect = true;
            if (CheckCamera != null)
            {
                while (isCameraPositionCorrect == true && _validatedOffsetDistance < _offsetDistance)
                {
                    isCameraPositionCorrect = true;
                    foreach (CheckCameraPosition fct in CheckCamera.GetInvocationList())
                    {
                        evaluatedCameraWorldPosition = _worldPosition.ValueInterp + cameraFocusedPosition;
                        isCameraPositionCorrect = fct(ref evaluatedCameraWorldPosition);
                        if (isCameraPositionCorrect == false)
                            _validatedOffsetDistance -= 0.03f;
                        break;
                    }

                    _validatedOffsetDistance += 0.01f;
                    if (_validatedOffsetDistance > _offsetDistance) _validatedOffsetDistance = _offsetDistance;
                    cameraFocusedPosition = _zAxis * way * _validatedOffsetDistance;
                }
            }
            else
            {
                cameraFocusedPosition = _zAxis * way * _offsetDistance;
            }
            
            if (_isbackLooking == false)
            {
                _view_focused = Matrix.LookAtLH(cameraFocusedPosition, cameraFocusedPosition - _zAxis, _yAxis);
            }
            else
            {
                _view_focused = Matrix.LookAtLH(cameraFocusedPosition, cameraFocusedPosition + _zAxis, _yAxis);
            }

            _viewProjection3D_focused = _view_focused * _projection3D;

            //NOT Focused camera computation ============================================================
            Vector3 cameraPosition = _worldPosition.ValueInterp.AsVector3() + cameraFocusedPosition;

            CameraPosition = cameraPosition;

            if (_isbackLooking == false)
            {
                _view = Matrix.LookAtLH(cameraPosition, cameraPosition - _zAxis, _yAxis);
            }
            else
            {
                _view = Matrix.LookAtLH(cameraPosition, cameraPosition + _zAxis, _yAxis);
            }

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
