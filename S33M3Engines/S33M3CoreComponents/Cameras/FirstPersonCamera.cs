using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine;
using SharpDX;
using S33M3CoreComponents.Maths.Graphics;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;

namespace S33M3CoreComponents.Cameras
{
    public class FirstPersonCamera : Camera
    {
        #region Private Variables
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

        #region Private Methods
        #endregion

        #region Public Methods
        //Called once before the drawing sequence ==> Computed interpolated values here !
        public override void Interpolation(double interpolation_hd, float interpolation_ld, long elapsedTime)
        {
            if (CameraPlugin != null)
            {
                //Get the interpolated value from the Camera pluggin !
                _worldPosition = CameraPlugin.CameraWorldPosition;
                _cameraOrientation = CameraPlugin.CameraOrientation;
                _cameraYAxisOrientation = CameraPlugin.CameraYAxisOrientation;
            }
            
            //Value are already interpolated by the pluggin, no need to interpolate again !

            //Compute the View Matrix
            _view = Matrix.Translation(_worldPosition.AsVector3() * -1) * Matrix.RotationQuaternion(Quaternion.Conjugate(_cameraOrientation));
            _viewProjection3D = _view * _projection3D;
            _frustum = new BoundingFrustum(_viewProjection3D);
        }

        protected override void CameraInitialize()
        {
            base.CameraInitialize();
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
