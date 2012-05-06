using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Structs;
using SharpDX;
using S33M3DXEngine;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Maths;
using SharpDX.Direct3D11;

namespace Samples.Entities
{
    public class Entity : DrawableGameComponent, ICameraPlugin
    {
        //Engine System variables
        private D3DEngine _d3DEngine;
        private CameraManager<ICamera> _cameraManager;
        private InputsManager _inputsManager;

        private FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();         //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _cameraYAxisOrientation = new FTSValue<Quaternion>();   //LookAtYAxis angle
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        private Vector3D _lookAt;
        private Vector3 _entityEyeOffset;                                     //Offset of the camera Placement inside the entity, from entity center point.

        //Mouvement handling variables
        private double _accumPitchDegrees;
        private double _gravityInfluence;
        private double _groundBelowEntity;
        private double _rotationDelta;
        private double _moveDelta;
        private Matrix _headRotation;
        private Matrix _entityRotation;
        private Vector3D _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private Vector3D _entityXAxis, _entityYAxis, _entityZAxis;
        private bool _stopMovedAction = false;

        public Vector3D LookAt
        {
            get { return _lookAt; }
            set { _lookAt = value; }
        }


        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual Vector3D CameraWorldPosition { get { return _worldPosition.ValueInterp + _entityEyeOffset; } }
        public virtual Quaternion CameraOrientation { get { return _lookAtDirection.ValueInterp; } }
        public virtual Quaternion CameraYAxisOrientation { get { return _cameraYAxisOrientation.ValueInterp; } }
        public virtual int CameraUpdateOrder { get { return this.UpdateOrder; } }

        public Entity(D3DEngine engine,
                                   CameraManager<ICamera> cameraManager,
                                   InputsManager inputsManager)
        {
            _d3DEngine = engine;
            _cameraManager = cameraManager;
            _inputsManager = inputsManager;
            UpdateOrder = 0;

            _inputsManager.MouseManager.MouseCapture = true;
        }

        private void RefreshEntityMovementAndRotation(ref GameTime timeSpent)
        {
            //Compute the delta following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            _moveDelta = 35 * timeSpent.ElapsedGameTimeInS_HD;
            _rotationDelta = 10 * timeSpent.ElapsedGameTimeInS_HD;

            //Backup previous values
            _lookAtDirection.BackUpValue();
            _worldPosition.BackUpValue();
            _cameraYAxisOrientation.BackUpValue();

            //Rotation with mouse
            EntityRotationsOnEvents();

            //Movement
            FreeFirstPersonMove();
        }

        private void FreeFirstPersonMove()
        {
            Vector3D moveVector = Vector3D.Zero;

            if (_inputsManager.KeyboardManager.CurKeyboardState.IsKeyDown(System.Windows.Forms.Keys.F1))
                _inputsManager.MouseManager.MouseCapture = true;

            if (_inputsManager.KeyboardManager.CurKeyboardState.IsKeyDown(System.Windows.Forms.Keys.F2))
                _inputsManager.MouseManager.MouseCapture = false;

            if (_inputsManager.KeyboardManager.CurKeyboardState.IsKeyDown(System.Windows.Forms.Keys.Z))
                moveVector += _lookAt;

            if (_inputsManager.KeyboardManager.CurKeyboardState.IsKeyDown(System.Windows.Forms.Keys.Z))
                moveVector += _lookAt;

            if (_inputsManager.KeyboardManager.CurKeyboardState.IsKeyDown(System.Windows.Forms.Keys.S))
                moveVector -= _lookAt;

            if (_inputsManager.KeyboardManager.CurKeyboardState.IsKeyDown(System.Windows.Forms.Keys.Q))
                moveVector -= _entityHeadXAxis;

            if (_inputsManager.KeyboardManager.CurKeyboardState.IsKeyDown(System.Windows.Forms.Keys.D))
                moveVector += _entityHeadXAxis;

            if (_inputsManager.KeyboardManager.CurKeyboardState.IsKeyDown(System.Windows.Forms.Keys.W))
                moveVector += Vector3D.Down;

            if (_inputsManager.KeyboardManager.CurKeyboardState.IsKeyDown(System.Windows.Forms.Keys.X))
                moveVector += Vector3D.Up;

            moveVector.Normalize();
            _worldPosition.Value += moveVector * _moveDelta;
        }

        private void EntityRotationsOnEvents()
        {
            if (_inputsManager.MouseManager.MouseCapture)
            {
                Rotate(_inputsManager.MouseManager.MouseMoveDelta.X, _inputsManager.MouseManager.MouseMoveDelta.Y, 0.0f);
            }
        }

        private void Rotate(double headingDegrees, double pitchDegrees, double rollDegrees)
        {
            if (headingDegrees == 0 && pitchDegrees == 0 && rollDegrees == 0) return;

            //Affect mouse sensibility stored in Delta to the mouvement that has been realized
            headingDegrees *= _rotationDelta;
            pitchDegrees *= _rotationDelta;
            rollDegrees *= _rotationDelta;

            RotateLookAt(headingDegrees, pitchDegrees);
            RotateMove(headingDegrees);
        }

        private void RotateMove(double headingDegrees)
        {
            double heading = MathHelper.ToRadians(headingDegrees);
            Quaternion rotation;

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, (float)heading, out rotation);
                _moveDirection.Value = _moveDirection.Value * rotation;
            }

            _moveDirection.Value.Normalize();

            UpdateEntityData();
        }
        private void UpdateEntityData()
        {
            Matrix.RotationQuaternion(ref _moveDirection.Value, out _entityRotation);

            _entityXAxis = new Vector3D(_entityRotation.M11, _entityRotation.M21, _entityRotation.M31);
            _entityYAxis = new Vector3D(_entityRotation.M12, _entityRotation.M22, _entityRotation.M32);
            _entityZAxis = new Vector3D(_entityRotation.M13, _entityRotation.M23, _entityRotation.M33) * -1;
        }

        private void RotateLookAt(double headingDegrees, double pitchDegrees)
        {
            //To avoid the Camera to make full loop on the Pitch axis
            _accumPitchDegrees += pitchDegrees;

            if (_accumPitchDegrees > 90.0f)
            {
                pitchDegrees = 90.0f - (_accumPitchDegrees - pitchDegrees);
                _accumPitchDegrees = 90.0f;
            }

            if (_accumPitchDegrees < -90.0f)
            {
                pitchDegrees = -90.0f - (_accumPitchDegrees - pitchDegrees);
                _accumPitchDegrees = -90.0f;
            }

            //Inverse the mouse move impact on the rotation
            double heading = MathHelper.ToRadians(headingDegrees) * -1;
            double pitch = MathHelper.ToRadians(pitchDegrees) * -1;
            Quaternion rotation;

            // Rotate the camera about its local X axis.
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, (float)pitch, out rotation);
                _lookAtDirection.Value = rotation * _lookAtDirection.Value;
            }

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, (float)heading, out rotation); //Transform the rotation angle from mouse into a quaternion
                _lookAtDirection.Value = _lookAtDirection.Value * rotation;             //Add this value to the existing Entity quaternion rotation
                _cameraYAxisOrientation.Value = _cameraYAxisOrientation.Value * rotation;
            }

            _lookAtDirection.Value.Normalize();
            UpdateHeadData();
        }

        private void UpdateHeadData()
        {
            //Get the lookAt vector
            _headRotation = Matrix.RotationQuaternion(Quaternion.Conjugate(_lookAtDirection.Value));

            _entityHeadXAxis = new Vector3D(_headRotation.M11, _headRotation.M21, _headRotation.M31);
            _entityHeadYAxis = new Vector3D(_headRotation.M12, _headRotation.M22, _headRotation.M32);
            _entityHeadZAxis = new Vector3D(_headRotation.M13, _headRotation.M23, _headRotation.M33);

            _lookAt = new Vector3D(_entityHeadZAxis.X, _entityHeadZAxis.Y, _entityHeadZAxis.Z);
            _lookAt.Normalize();
        }

        public override void Initialize()
        {
            //Set Position
            //Set the entity world position following the position received from server
            _worldPosition.Value = new Vector3D(-30, 100,-20);
            _worldPosition.ValuePrev = new Vector3D(-30, 100, -20);

            //Set LookAt
            //Take back only the saved server Yaw rotation (Or Heading) and only using it;
            _lookAtDirection.Value = Quaternion.Identity;

            _cameraYAxisOrientation.Value = _lookAtDirection.Value;
            _cameraYAxisOrientation.ValuePrev = _lookAtDirection.Value;

            //Set Move direction = to LookAtDirection
            _moveDirection.Value = _lookAtDirection.Value;
        }

        /// <summary>
        /// The allocated object here must be disposed
        /// </summary>
        public override void LoadContent(DeviceContext context)
        {
        }

        public override void BeforeDispose()
        {
            _inputsManager.MouseManager.MouseCapture = false;
        }

        public override void Update(GameTime timeSpend)
        {
            RefreshEntityMovementAndRotation(ref timeSpend);   //Refresh player Movement + rotation
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long timePassed)
        {
            //TODO FIXME NAsty bug here, not a number float arithmetic exception sometimes - surely a server side fix to do !
            Quaternion.Slerp(ref _lookAtDirection.ValuePrev, ref _lookAtDirection.Value, interpolationLd, out _lookAtDirection.ValueInterp);
            Quaternion.Slerp(ref _cameraYAxisOrientation.ValuePrev, ref _cameraYAxisOrientation.Value, interpolationLd, out _cameraYAxisOrientation.ValueInterp);
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);
        }

        public override void Draw(DeviceContext context, int index)
        {
        }

    }
}

