#region

using System;
using S33M3Engines.D3D;
using S33M3Engines.InputHandler;
using S33M3Engines.Maths;
using S33M3Engines.Maths.Graphics;
using S33M3Engines.Struct;
using S33M3Engines.WorldFocus;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3Engines.Shared.Math;

#endregion

namespace S33M3Engines.Cameras
{

    public enum CameraType
    {
        FirstPerson
    }

    public abstract class Camera : GameComponent, ICamera, IWorldFocus
    {
        #region Private TimeDepending Variable ===> Will be LERPED, SLERPED or recomputed
        protected FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();
        protected FTSValue<Quaternion> _cameraOrientation = new FTSValue<Quaternion>();
        #endregion

        #region Private Variable

        protected Viewport? _viewport;
        protected Vector3 _lookAt;
        protected Vector3 _cameraUpVector = MVector3.Up;

        protected float _nearPlane = 0.5f;
        protected float _farPlane = 3000f;

        protected Matrix _projection3D;
        protected Matrix _viewProjection3D_focused;
        protected Matrix _viewProjection3D;
        protected Matrix _worldViewMatrix;

        protected Matrix _projection2D;

        protected BoundingFrustum _frustum;
        protected Matrix _view_focused;

        private ICameraPlugin _cameraPlugin;

        private readonly FTSValue<Vector3D> _focusPoint = new FTSValue<Vector3D>();
        private readonly FTSValue<Matrix> _focusPointMatrix = new FTSValue<Matrix>();

        protected D3DEngine _d3dEngine;

        #endregion

        #region Properties

        public CameraType CameraType { get; set; }

        public Matrix View_focused
        {
            get { return _view_focused; }
        }

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

        public Matrix Projection2D
        {
            get { return _projection2D; }
        }

        public Matrix ViewProjection3D_focused
        {
            get { return _viewProjection3D_focused; }
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
            get { return _worldPosition.ValueInterp; }
            set { _worldPosition.Value = value; }
        }

        public Quaternion Orientation
        {
            get { return _cameraOrientation.ValueInterp; }
            set { _cameraOrientation.Value = value; }
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

        public FTSValue<Vector3D> FocusPoint
        {
            get { return _focusPoint; }
        }

        public FTSValue<Matrix> FocusPointMatrix
        {
            get { return _focusPointMatrix; }
        }

        #endregion

        #region Public methods

        //Constructors
        public Camera(D3DEngine d3dEngine)
        {
            _d3dEngine = d3dEngine;
            d3dEngine.ViewPort_Updated += D3dEngine_ViewPort_Updated;
        }

        public override void Dispose()
        {
            _d3dEngine.ViewPort_Updated -= D3dEngine_ViewPort_Updated;
            base.Dispose();
        }

        public virtual string GetInfo()
        {
            return null;
        }

        #endregion

        #region Private Methods

        public override void Initialize()
        {
            CameraInitialize();
        }

        protected virtual void CameraInitialize()
        {
            _cameraOrientation.Value = Quaternion.Identity;

            float aspectRatio = Viewport.Width/Viewport.Height;

            Matrix.PerspectiveFovLH((float) Math.PI/3, aspectRatio, NearPlane, FarPlane, out _projection3D);
            Matrix.OrthoLH(Viewport.Width, Viewport.Height, NearPlane, FarPlane, out _projection2D);

            //Set Mouse position
            //Mouse.SetPosition((int) Viewport.Width/2, (int) Viewport.Height/2);
        }

        private void newCameraPluginDriver()
        {
            _worldPosition.Value = _cameraPlugin.CameraWorldPosition;
            _worldPosition.ValueInterp = _cameraPlugin.CameraWorldPosition;
            FocusPoint.Value = _cameraPlugin.CameraWorldPosition;
            FocusPoint.ValueInterp = _cameraPlugin.CameraWorldPosition;
        }

        private void D3dEngine_ViewPort_Updated(Viewport viewport)
        {
            Viewport = viewport;
        }

        #endregion
    }
}