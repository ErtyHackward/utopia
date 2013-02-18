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
        public override void FTSUpdate(GameTime timeSpend)
        {
            if (CameraPlugin != null)
            {
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
                    //Console.WriteLine("From Update : " + _worldPosition.Value);
                    _cameraOrientation.Value = CameraPlugin.CameraOrientation;
                    _cameraYAxisOrientation.Value = CameraPlugin.CameraYAxisOrientation;
                }

                Matrix cameraRotation;
                Matrix.RotationQuaternion(ref _cameraOrientation.Value, out cameraRotation);
                _lookAt.Value = new Vector3(cameraRotation.M13, cameraRotation.M23, cameraRotation.M33);
            }
        }

        //Called once before the drawing sequence ==> Computed interpolated values here !
        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            //Do interpolation on the value received at update time
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);
            Quaternion.Slerp(ref _cameraOrientation.ValuePrev, ref _cameraOrientation.Value, interpolationLd, out _cameraOrientation.ValueInterp);
            Quaternion.Slerp(ref _cameraYAxisOrientation.ValuePrev, ref _cameraYAxisOrientation.Value, interpolationLd, out _cameraYAxisOrientation.ValueInterp);

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

            //Camera computation ============================================================
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
