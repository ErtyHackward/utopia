#region
using System;
using SharpDX;
using S33M3CoreComponents.Maths;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Structs;
using S33M3CoreComponents.WorldFocus.Interfaces;

#endregion

namespace S33M3CoreComponents.Cameras
{

    public enum CameraType
    {
        FirstPerson,
        ThirdPerson
    }

    public delegate void CameraUpdateOrder(ICamera camera, int newOrderId);

    public abstract class Camera : BaseComponent, ICamera
    {
        #region Private TimeDepending Variable ===> Will be LERPED, SLERPED or recomputed
        protected FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();
        protected FTSValue<Quaternion> _cameraOrientation = new FTSValue<Quaternion>();
        protected FTSValue<Quaternion> _cameraYAxisOrientation = new FTSValue<Quaternion>();
        protected FTSValue<Vector3> _lookAt = new FTSValue<Vector3>();
        #endregion

        #region Private Variable
        protected ViewportF? _viewport;
        protected Vector3 _cameraUpVector = VectorsCst.Up3;

        protected float _nearPlane = 0.5f;
        protected float _farPlane = 3000f;

        protected Matrix _view;
        protected Matrix _projection3D;
        protected Matrix _viewProjection3D;
        protected Matrix _worldViewMatrix;

        protected SimpleBoundingFrustum _frustum;

        private ICameraPlugin _cameraPlugin;

        protected D3DEngine _d3dEngine;

        #endregion

        #region Properties

        public event CameraUpdateOrder CameraUpdateOrderChanged;

        public CameraType CameraType { get; set; }

        public bool NewlyActivatedCamera { get; set; }

        public SimpleBoundingFrustum Frustum
        {
            get { return _frustum; }
        }

        public ViewportF Viewport
        {
            get
            {
                if (_viewport == null)
                {
                    _viewport = _d3dEngine.ViewPort;
                }
                return ((ViewportF)_viewport);
            }
            set
            {
                _viewport = value;
                CameraInitialize();
            }
        }

        public Matrix ViewProjection3D
        {
            get { return _viewProjection3D; }
        }

        public Matrix View
        {
            get { return _view; }
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

        public FTSValue<Vector3D> WorldPosition
        {
            get { return _worldPosition; }
        }

        public FTSValue<Quaternion> Orientation
        {
            get { return _cameraOrientation; }
        }

        public FTSValue<Vector3> LookAt
        {
            get { return _lookAt; }
        }

        public FTSValue<Quaternion> YAxisOrientation
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

        public Matrix Projection
        {
            get { return _projection3D; }
        }

        #endregion

        #region Public methods

        //Constructors
        public Camera(D3DEngine d3dEngine,
                      float nearPlane,
                      float farPlane)
        {
            _d3dEngine = d3dEngine;
            d3dEngine.ScreenSize_Updated += D3dEngine_ScreenSize_Updated;
            _nearPlane = nearPlane;
            _farPlane = farPlane;
            CameraInitialize();
        }

        public virtual void FTSUpdate(GameTime timeSpend)
        {
        }

        public virtual void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
        }

        public override void BeforeDispose()
        {
            _d3dEngine.ScreenSize_Updated -= D3dEngine_ScreenSize_Updated;

            if (CameraUpdateOrderChanged != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in CameraUpdateOrderChanged.GetInvocationList())
                {
                    CameraUpdateOrderChanged -= (CameraUpdateOrder)d;
                }
            }
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
            _cameraOrientation.Initialize(Quaternion.Identity);

            float aspectRatio = Viewport.Width / Viewport.Height;

            Matrix.PerspectiveFovLH((float)Math.PI / 3, aspectRatio, NearPlane, FarPlane, out _projection3D);

            var matrix = Matrix.Identity;

            if (_frustum == null) _frustum = new SimpleBoundingFrustum(ref matrix);
        }

        protected virtual void newCameraPluginDriver()
        {
            _worldPosition.Initialize(_cameraPlugin.CameraWorldPosition);

            if (CameraUpdateOrderChanged != null) CameraUpdateOrderChanged(this, _cameraPlugin.CameraUpdateOrder);
        }

        private void D3dEngine_ScreenSize_Updated(ViewportF viewport, Texture2DDescription newBackBufferDescr)
        {
            Viewport = viewport;
        }

        #endregion
    }
}