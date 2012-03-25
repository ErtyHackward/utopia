using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Cameras.Interfaces;

namespace S33M3CoreComponents.Cameras
{
    //Class handling the currently active camera.
    public class CameraManager<CamType> : GameComponent, ICameraManager where CamType : ICamera
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private CamType _activeCamera;
        #endregion

        #region Public properties/variables
        public CamType ActiveCamera
        {
            get { return _activeCamera; }
            set { ChangeActiveCamera(value); }
        }

        public ICamera ActiveBaseCamera
        {
            get { return _activeCamera; }
        }

        public event CameraChange ActiveCamera_Changed;
        #endregion

        public CameraManager(CamType camera)
        {
            ActiveCamera = camera;
        }

        public override void Dispose()
        {
            if (ActiveCamera_Changed != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in ActiveCamera_Changed.GetInvocationList())
                {
                    ActiveCamera_Changed -= (CameraChange)d;
                }
            }
            base.Dispose();
        }

        #region Public methods

        public override void Initialize()
        {
        }

        public override void LoadContent(DeviceContext context)
        {
        }

        public override void Update(GameTime timeSpend)
        {
            ActiveCamera.Update(timeSpend);
        }

        public override void Interpolation(double interpolation_hd, float interpolation_ld, long elapsedTime)
        {
            ActiveCamera.Interpolation(interpolation_hd, interpolation_ld, elapsedTime);
        }

        #endregion

        #region Private methods
        private void ChangeActiveCamera(CamType newCamera)
        {
            _activeCamera = newCamera;
            _activeCamera.CameraUpdateOrderChanged -= ActiveCamera_CameraUpdateOrderChanged;
            _activeCamera.CameraUpdateOrderChanged += ActiveCamera_CameraUpdateOrderChanged;
            if (ActiveCamera_Changed != null) ActiveCamera_Changed(newCamera);

        }

        private void ActiveCamera_CameraUpdateOrderChanged(ICamera camera, int newOrderId)
        {
            ChangeUpdateOrder(camera.CameraPlugin.CameraUpdateOrder + 1);
            logger.Info("A new Camera plugin as been set, the new Camera update has been changed accordingly this PLugin.updateOrder + 1 = {0}", this.UpdateOrder);
        }

        private void ChangeUpdateOrder(int newOrder)
        {
            UpdateOrder = newOrder;
        }
        #endregion
    }
}
