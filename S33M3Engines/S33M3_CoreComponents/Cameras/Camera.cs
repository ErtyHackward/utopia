#region
using System;
using SharpDX;
using S33M3_CoreComponents.Maths.Graphics;
using S33M3_CoreComponents.Maths;
using S33M3_DXEngine;
using S33M3_DXEngine.Main;
using SharpDX.Direct3D11;
using S33M3_CoreComponents.Cameras.Interfaces;
using S33M3_Resources.Structs;
using S33M3_CoreComponents.WorldFocus.Interfaces;
#endregion

namespace S33M3_CoreComponents.Cameras
{

    public enum CameraType
    {
        FirstPerson
    }

    public delegate void CameraUpdateOrder(ICamera camera, int newOrderId);

    public abstract class Camera : Component, ICamera
    {
        #region Private TimeDepending Variable ===> Will be LERPED, SLERPED or recomputed
        protected Vector3D _worldPosition = new Vector3D();
        protected Quaternion _cameraOrientation = new Quaternion();
        protected Quaternion _cameraYAxisOrientation = new Quaternion();
        #endregion

        #region Private Variable
        protected Viewport? _viewport;
        protected Vector3 _lookAt;
        protected Vector3 _cameraUpVector = VectorsCst.Up3;

        protected float _nearPlane = 0.5f;
        protected float _farPlane = 3000f;

        protected Matrix _view;
        protected Matrix _projection3D;
        protected Matrix _viewProjection3D;
        protected Matrix _worldViewMatrix;

        protected Matrix _projection2D;

        protected BoundingFrustum _frustum;

        private ICameraPlugin _cameraPlugin;

        protected D3DEngine _d3dEngine;

        #endregion

        #region Properties

        public event CameraUpdateOrder CameraUpdateOrderChanged;

        public CameraType CameraType { get; set; }

        public BoundingFrustum Frustum
        {
            get { return _frustum; }
        }

        public Viewport Viewport
        {
            get
            {
                if (_viewport == null)
                {
                    _viewport = _d3dEngine.ViewPort;
                }
                return ((Viewport) _viewport);
            }
            set
            {
                _viewport = value;
                CameraInitialize();
            }
        }

        public Matrix Projection3D
        {
            get { return _projection3D; }
        }

        public Matrix View
        {
            get { return _view; }
        }

        public Matrix Projection2D
        {
            get { return _projection2D; }
        }

        public Matrix ViewProjection3D
        {
            get { return _viewProjection3D; }
        }

        public float NearPlane
        {
            get { return _nearPlane; }
            set
            {
                _nearPlane = value;
                CameraInitialize();
            }
        }

        public float FarPlane
        {
            get { return _farPlane; }
            set
            {
                _farPlane = value;
                CameraInitialize();
            }
        }

        public Vector3D WorldPosition
        {
            get { return _worldPosition; }
        }

        public Quaternion Orientation
        {
            get { return _cameraOrientation; }
        }

        public Vector3 LookAt
        {
            get { return _lookAt; }
        }

        public Quaternion YAxisOrientation
        {
            get { return _cameraYAxisOrientation; }
        }

        public ICameraPlugin CameraPlugin
        {
            get { return _cameraPlugin; }
            set
            {
                _cameraPlugin = value;
                newCameraPluginDriver();
            }
        }
        #endregion

        #region Public methods

        //Constructors
        public Camera(D3DEngine d3dEngine, 
                      float nearPlane,
                      float farPlane)
        {
            _d3dEngine = d3dEngine;
            d3dEngine.ViewPort_Updated += D3dEngine_ViewPort_Updated;
            _nearPlane = nearPlane;
            _farPlane = farPlane;
            CameraInitialize();
        }

        public virtual void Update(GameTime timeSpend)
        {
        }

        public virtual void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
        }

        public override void Dispose()
        {
            _d3dEngine.ViewPort_Updated -= D3dEngine_ViewPort_Updated;

            if (CameraUpdateOrderChanged != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in CameraUpdateOrderChanged.GetInvocationList())
                {
                    CameraUpdateOrderChanged -= (CameraUpdateOrder)d;
                }
            }

            base.Dispose();
        }

        #region IDebugInfo Members
        public virtual bool ShowDebugInfo { get; set; }
        public virtual string GetDebugInfo()
        {
            return null;
        }
        #endregion

        #endregion

        #region Private Methods
        protected virtual void CameraInitialize()
        {
            _cameraOrientation = Quaternion.Identity;

            float aspectRatio = Viewport.Width/Viewport.Height;

            Matrix.PerspectiveFovLH((float) Math.PI/3, aspectRatio, NearPlane, FarPlane, out _projection3D);
            Matrix.OrthoLH(Viewport.Width, Viewport.Height, NearPlane, FarPlane, out _projection2D);

            if (_frustum == null) _frustum = new BoundingFrustum(Matrix.Identity);
        }

        protected virtual void newCameraPluginDriver()
        {
            _worldPosition = _cameraPlugin.CameraWorldPosition;

            if (CameraUpdateOrderChanged != null) CameraUpdateOrderChanged(this, _cameraPlugin.CameraUpdateOrder);
        }

        private void D3dEngine_ViewPort_Updated(Viewport viewport)
        {
            Viewport = viewport;
        }

        #endregion
    }
}