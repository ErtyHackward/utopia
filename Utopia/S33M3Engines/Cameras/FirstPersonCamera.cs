using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using SharpDX;
using S33M3Engines.InputHandler;
using S33M3Engines.Maths;
using System.Windows.Forms;
using S33M3Engines.Maths.Graphics;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.Struct;
using S33M3Engines.WorldFocus;
using S33M3Engines.Shared.Math;

namespace S33M3Engines.Cameras
{
    public class FirstPersonCamera : Camera
    {
        #region Private Variables
        WorldFocusManager _worldFocusManager;
        #endregion

        #region Public Properties
        #endregion
        //Constructors

        public FirstPersonCamera(D3DEngine d3dEngine, WorldFocusManager worldFocusManager)
            : base(d3dEngine)
        {
            _worldFocusManager = worldFocusManager;
            this.CameraType = Cameras.CameraType.FirstPerson;
        }

        #region Private Methods
        #endregion

        #region Public Methods
        //Called at fixed interval of time
        public override void Update(ref GameTime TimeSpend)
        {
            //Memorise the last timedepend variables
            _worldPosition.BackUpValue();
            _cameraOrientation.BackUpValue();

            if (CameraPlugin != null)
            {
                //Get the Camera Position and Rotation from the attached Entity to the camera !
                _worldPosition.Value = CameraPlugin.CameraWorldPosition;
                _cameraOrientation.Value = CameraPlugin.CameraOrientation;

                //Set the new Computed focus point
                base.FocusPoint.Value = _worldPosition.Value; // == Position of my camera !
                //Compute the derived translation matrix
                base.FocusPointMatrix.Value = Matrix.Translation(_worldPosition.Value.AsVector3());

                //Compute new View matrix based on the Position and Orientation from a Quaternion (No Euler angles, to have the possibility to slerps those value)
                //To compute the view camera matrix, we need to take the inverse of the World position of the camera (it explains the * - 1)
                Matrix MTranslation = Matrix.Translation(-1 * (_worldPosition.Value - _worldFocusManager.WorldFocus.FocusPoint.Value).AsVector3());
                Matrix MRotation = Matrix.RotationQuaternion(_cameraOrientation.Value);
                Matrix.Multiply(ref MTranslation, ref MRotation, out _view_focused);

                _viewProjection3D_focused = _view_focused * this.Projection3D;

                _viewProjection3D = Matrix.Translation(-_worldPosition.ValueInterp.AsVector3()) * MRotation * _projection3D;

                //Compute the Frustum from World position. (Not focused)
                //World View : Matrix.Translation(-_worldPosition.Value.AsVector3()) * MRotation
                _frustum = new BoundingFrustum(_viewProjection3D);
            }
            else
            {
                //S33M3Engines.D3D.DebugTools.GameConsole.Write("The Camera is not Attached to en entity!!!");
            }
        }

        //Called once before the drawing sequence ==> Computed interpolated values here !
        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolation_hd, out _worldPosition.ValueInterp);

            base.FocusPoint.ValueInterp = _worldPosition.ValueInterp;
            base.FocusPointMatrix.ValueInterp = Matrix.Translation(-1 * _worldPosition.ValueInterp.AsVector3());

            Quaternion.Slerp(ref _cameraOrientation.ValuePrev, ref _cameraOrientation.Value, (float)interpolation_ld, out _cameraOrientation.ValueInterp);

            //Recompute the interpolated View Matrix
            Matrix MTranslation = Matrix.Translation(-(_worldPosition.ValueInterp - _worldFocusManager.WorldFocus.FocusPoint.ValueInterp).AsVector3());
            Matrix MRotation = Matrix.RotationQuaternion(_cameraOrientation.ValueInterp);
            Matrix.Multiply(ref MTranslation, ref MRotation, out _view_focused);

            _viewProjection3D_focused = _view_focused * this.Projection3D;

            //_viewProjection3D = Matrix.Translation(-_worldPosition.ValueInterp.AsVector3()) * MRotation * _projection3D;
            //_frustum = new BoundingFrustum(_viewProjection3D);
        }

        protected override void CameraInitialize()
        {
            base.CameraInitialize();
        }

        #endregion

        #region IDebugInfo Members

        public override string GetInfo()
        {
            return string.Concat("<FirstPersonCamera> X : ", WorldPosition.X.ToString("0.000"), " Y : ", WorldPosition.Y.ToString("0.000"), " Z : ", WorldPosition.Z.ToString("0.000"), " Pitch : ", _lookAt.X.ToString("0.000"), " Yaw : ", _lookAt.Y.ToString("0.000"));
        }

        #endregion
    }
}
