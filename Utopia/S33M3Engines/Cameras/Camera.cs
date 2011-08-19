using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3Engines.Struct;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler;
using S33M3Engines.Maths;
using S33M3Engines.Maths.Graphics;
using S33M3Engines.Shared.Delegates;
using S33M3Engines.WorldFocus;

namespace S33M3Engines.Cameras
{
    public abstract class Camera : ICamera, IWorldFocus
    {
        #region Private TimeDepending Variable ===> Will be LERPED, SLERPED or recomputed
        protected FTSValue<DVector3> _worldPosition = new FTSValue<DVector3>();
        protected FTSValue<Quaternion> _cameraOrientation = new FTSValue<Quaternion>();
        #endregion

        #region Private Variable
        protected Viewport? _viewport;
        protected Vector3 _lookAt;
        protected Vector3 _cameraUpVector = MVector3.Up;

        protected float _nearPlane = 0.5f;
        protected float _farPlane = 3000f;

        protected Matrix _projection3D;
        protected Matrix _closeProjection3D;
        protected Matrix _viewPorjection3D;

        protected Matrix _projection2D;

        protected BoundingFrustum _frustum;
        protected Matrix _view;

        private ICameraPlugin _cameraPlugin;

        private FTSValue<DVector3> _focusPoint = new FTSValue<DVector3>();

        protected D3DEngine _d3dEngine;
        #endregion

        #region Properties
        public Matrix View
        {
            get { return _view; }
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
                return ((Viewport)_viewport);
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

        public Matrix ViewProjection3D
        {
            get { return _viewPorjection3D; }
        }
        
        public Vector3 LookAt
        {
            get { return _lookAt; }
            set { _lookAt = value; CameraInitialize(); }
        }

        public float NearPlane
        {
            get { return _nearPlane; }
            set { _nearPlane = value; CameraInitialize(); }
        }

        public float FarPlane
        {
            get { return _farPlane; }
            set { _farPlane = value; CameraInitialize(); }
        }

        public DVector3 WorldPosition

        {
            get { return _worldPosition.ActualValue; }
            set { _worldPosition.Value = value; }
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

        public FTSValue<DVector3> FocusPoint
        {
            get
            {
                return _focusPoint;
            }
        }

        #endregion

        #region Public methods
        //Constructors
        public Camera(D3DEngine d3dEngine)
        {
            _d3dEngine = d3dEngine;
            d3dEngine.ViewPort_Updated += new D3DEngineDelegates.ViewPortUpdated(D3dEngine_ViewPort_Updated);
        }


        public virtual void Update(ref GameTime TimeSpend)
        {
        }

        public virtual void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual string GetInfo()
        {
            return null;
        }
        #endregion

        #region Private Methods

        public virtual void Initialize()
        {
            CameraInitialize();
        }

        protected virtual void CameraInitialize()
        {
            _cameraOrientation.Value = Quaternion.Identity;

            float aspectRatio = (float)Viewport.Width / Viewport.Height;

            Matrix.PerspectiveFovRH((float)Math.PI / 3, aspectRatio, NearPlane, FarPlane, out _projection3D);
            Matrix.PerspectiveFovRH((float)Math.PI / 3, aspectRatio, NearPlane, 80, out _closeProjection3D);
            Matrix.OrthoRH((float)Viewport.Width, (float)Viewport.Height, NearPlane, FarPlane, out _projection2D);

            //Set Mouse position
            Mouse.SetPosition((int)Viewport.Width / 2, (int)Viewport.Height / 2);
        }

        private void newCameraPluginDriver()
        {
            _worldPosition.Value = _cameraPlugin.CameraWorldPosition;
            _worldPosition.ValueInterp = _cameraPlugin.CameraWorldPosition;
            FocusPoint.Value = _cameraPlugin.CameraWorldPosition;
            FocusPoint.ValueInterp = _cameraPlugin.CameraWorldPosition;
        }

        void D3dEngine_ViewPort_Updated(Viewport viewport)
        {
            this.Viewport = viewport;
        }
        #endregion

    }
}
