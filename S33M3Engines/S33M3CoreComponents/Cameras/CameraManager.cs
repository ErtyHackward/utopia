using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.WorldFocus.Interfaces;

namespace S33M3CoreComponents.Cameras
{
    //Class handling the currently active camera.
    public class CameraManager<CamType> : GameComponent, ICameraManager where CamType : ICamera
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private CamType _activeCamera;
        private WorldFocusManager _worldFocusManager;

        private InputsManager _inputManager;
        private int _cameraChangeIndex = -1;
        private List<CamType> _registeredCameras = new List<CamType>();
        #endregion

        #region Public properties/variables
        public CamType ActiveCamera
        {
            get { return _activeCamera; }
        }

        public ICamera ActiveBaseCamera
        {
            get { return _activeCamera; }
        }

        public event CameraChange ActiveCamera_Changed;
        #endregion

        public CameraManager(InputsManager inputManager, WorldFocusManager worldFocusManager)
        {
            _inputManager = inputManager;
            _worldFocusManager = worldFocusManager;
        }

        public override void BeforeDispose()
        {
            if (ActiveCamera_Changed != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in ActiveCamera_Changed.GetInvocationList())
                {
                    ActiveCamera_Changed -= (CameraChange)d;
                }
            }
        }

        #region Public methods

        public override void Initialize()
        {
        }

        public override void LoadContent(DeviceContext context)
        {
        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime timeSpend)
        {
            ActiveCamera.Update(timeSpend);

            if (_inputManager.ActionsManager.isTriggered(Actions.ChangeCameraType))
            {
                MoveToNextActiveCamera();
            }
        }

        public override void Interpolation(double interpolation_hd, float interpolation_ld, long elapsedTime)
        {
            ActiveCamera.Interpolation(interpolation_hd, interpolation_ld, elapsedTime);
        }

        public void RegisterNewCamera(CamType camera)
        {
            _registeredCameras.Add(camera);
            if (_activeCamera == null) MoveToNextActiveCamera();
        }

        public void SetCamerasPlugin(ICameraPlugin camplugin)
        {
            foreach (var camera in _registeredCameras)
            {
                camera.CameraPlugin = camplugin;
            }
        }
        #endregion

        #region Private methods
        private void MoveToNextActiveCamera()
        {
            if (_registeredCameras.Count == 0) return;
            _cameraChangeIndex++;
            if (_cameraChangeIndex >= _registeredCameras.Count) _cameraChangeIndex = 0;
            ChangeActiveCamera(_registeredCameras[_cameraChangeIndex]);
        }

        private void ChangeActiveCamera(CamType newCamera)
        {
            _activeCamera = newCamera;
            _activeCamera.CameraUpdateOrderChanged -= ActiveCamera_CameraUpdateOrderChanged;
            _activeCamera.CameraUpdateOrderChanged += ActiveCamera_CameraUpdateOrderChanged;

            //Change the focus
            _worldFocusManager.WorldFocus = (IWorldFocus)newCamera;

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
