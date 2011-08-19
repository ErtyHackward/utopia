using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;

namespace S33M3Engines.Cameras
{
    //Class handling the currently active camera.
    public class CameraManager : GameComponent
    {
        #region Private variables
        private ICamera _activeCamera;
        #endregion

        #region Public properties/variables
        public ICamera ActiveCamera
        {
            get { return _activeCamera; }
            set { ChangeActiveCamera(value); }
        }

        public delegate void CameraChange(ICamera newCamera);
        public event CameraChange ActiveCamera_Changed;
        #endregion

        public CameraManager(ICamera camera)
        {
            ActiveCamera = camera;
        }

        #region Public methods

        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
            ActiveCamera.Initialize();
        }

        public override void UnloadContent()
        {
            ActiveCamera.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            ActiveCamera.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            ActiveCamera.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        #endregion

        #region Private methods
        private void ChangeActiveCamera(ICamera newCamera)
        {
            _activeCamera = newCamera;
            if (ActiveCamera_Changed != null) ActiveCamera_Changed(newCamera);
        }
        #endregion
    }
}
