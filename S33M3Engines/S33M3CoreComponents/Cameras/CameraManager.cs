using System;
using System.Collections.Generic;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.WorldFocus.Interfaces;

namespace S33M3CoreComponents.Cameras
{
    /// <summary>
    /// Class handling the currently active camera.
    /// </summary>
    /// <typeparam name="TCamType"></typeparam>
    public class CameraManager<TCamType> : GameComponent, ICameraManager where TCamType : class, ICamera
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private TCamType _activeCamera;
        private WorldFocusManager _worldFocusManager;

        private InputsManager _inputManager;
        private int _cameraChangeIndex = -1;
        private List<TCamType> _registeredCameras = new List<TCamType>();
        #endregion

        #region Public properties/variables
        public TCamType ActiveCamera
        {
            get { return _activeCamera; }
        }

        public ICamera ActiveBaseCamera
        {
            get { return _activeCamera; }
        }

        /// <summary>
        /// Occurs when current active camera was changed
        /// </summary>
        public event EventHandler<CameraChangedEventArgs> ActiveCameraChanged;

        private void OnActiveCameraChanged(ICamera camera)
        {
            var handler = ActiveCameraChanged;
            if (handler != null) handler(this, new CameraChangedEventArgs { Camera = camera });
        }

        #endregion

        public CameraManager(TCamType camera = null)
            : this(null, null, camera)
        {
        }

        public CameraManager(InputsManager inputManager, WorldFocusManager worldFocusManager, TCamType camera = null)
        {
            _inputManager = inputManager;
            _worldFocusManager = worldFocusManager;
            if (camera != null) RegisterNewCamera(camera);
        }

        #region Public methods

        public override void FTSUpdate(GameTime timeSpend)
        {
            if (_inputManager != null && _inputManager.ActionsManager.isTriggered(Actions.ChangeCameraType))
            {
                MoveToNextActiveCamera();
            }

            ActiveCamera.FTSUpdate(timeSpend);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            ActiveCamera.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }

        public void RegisterNewCamera(TCamType camera)
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

        private void ChangeActiveCamera(TCamType newCamera)
        {
            _activeCamera = newCamera;
            _activeCamera.CameraUpdateOrderChanged -= ActiveCamera_CameraUpdateOrderChanged;
            _activeCamera.CameraUpdateOrderChanged += ActiveCamera_CameraUpdateOrderChanged;

            //Change the focus
            if (_worldFocusManager != null)
            {
                _worldFocusManager.WorldFocus = (IWorldFocus)newCamera;
            }
            newCamera.NewlyActivatedCamera = true;
            OnActiveCameraChanged(newCamera);
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

    public class CameraChangedEventArgs : EventArgs
    {
        public ICamera Camera { get; set; }
    }
}
