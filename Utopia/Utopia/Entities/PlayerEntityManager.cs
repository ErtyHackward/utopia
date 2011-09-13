using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines;
using S33M3Engines.WorldFocus;
using S33M3Engines.Cameras;
using Utopia.Action;
using Utopia.InputManager;
using Utopia.Shared.Chunks;
using Utopia.Entities.Voxel;
using Utopia.Shared.Structs;
using SharpDX;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Cubes;
using S33M3Engines.Maths;
using S33M3Engines.Shared.Math;
using S33M3Engines.Struct;
using S33M3Physics.Verlet;
using S33M3Physics;
using UtopiaContent.Effects.Terran;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Chunks.Entities.Concrete;
using S33M3Engines.StatesManager;

namespace Utopia.Entities
{
    public class PlayerEntityManager : DrawableGameComponent, ICameraPlugin
    {
        #region Private variables
        //Engine System variables
        private D3DEngine _d3DEngine;
        private CameraManager _cameraManager;
        private WorldFocusManager _worldFocusManager;
        private ActionsManager _actions;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;
        private VoxelMeshFactory _voxelMeshFactory;

        //Block Picking variables
        private bool _isBlockPicked;
        private Location3<int> _pickedBlock, _previousPickedBlock, _newCubePlace;
        private BoundingBox _playerSelectedBox, _playerPotentialNewBlock;
        private TerraCube _pickedCube;

        //Head UnderWater test
        private int _headCubeIndex;
        private TerraCube _headCube;

        //Player Visual characteristics (Not insde the PlayerCharacter object)
        private BoundingBox _playerBoundingBox;
        private Vector3 _boundingMinPoint, _boundingMaxPoint;                         //Use to recompute the bounding box in world coordinate
        private FTSValue<DVector3> _worldPosition = new FTSValue<DVector3>();         //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        private DVector3 _lookAt;
        private Vector3 _entityEyeOffset;                                     //Offset of the camera Placement inside the entity, from entity center point.

        //Mouvement handling variables
        private VerletSimulator _physicSimu;
        private EntityDisplacementModes _displacementMode;
        private double _accumPitchDegrees;
        private double _gravityInfluence;
        private float _groundBelowEntity;
        private double _rotationDelta;
        private double _moveDelta;
        private Matrix _headRotation;
        private Matrix _entityRotation;
        private DVector3 _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private DVector3 _entityXAxis, _entityYAxis, _entityZAxis;

        //Draw Variables
        private HLSLTerran _playerEffect;
        private VertexBuffer<VertexCubeSolid> _vertexBuffer;
        private List<VertexCubeSolid> _vertice;
        private bool _altered;

        #endregion

        #region Public variables/properties
        /// <summary>
        /// The Player
        /// </summary>
        public readonly PlayerCharacter Player;
        /// <summary>
        /// The Player Voxel body
        /// </summary>
        public readonly VoxelEntity VoxelEntity;

        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual DVector3 CameraWorldPosition { get { return _worldPosition.Value + _entityEyeOffset; } }
        public virtual Quaternion CameraOrientation { get { return _lookAtDirection.Value; } }

        public bool IsHeadInsideWater { get; set; }

        public EntityDisplacementModes DisplacementMode
        {
            get { return _displacementMode; }
            set
            {
                _displacementMode = value;
                if (_displacementMode == EntityDisplacementModes.Walking)
                {
                    _physicSimu.StartSimulation(ref _worldPosition.Value, ref _worldPosition.Value);
                }
                else
                {
                    _physicSimu.StopSimulation();
                }
            }
        }
        #endregion


        public PlayerEntityManager(D3DEngine engine,
                                   CameraManager cameraManager,
                                   WorldFocusManager worldFocusManager,
                                   ActionsManager actions,
                                   InputsManager inputsManager,
                                   SingleArrayChunkContainer cubesHolder,
                                   VoxelMeshFactory voxelMeshFactory,
                                   VoxelEntity voxelEntity,
                                   PlayerCharacter player)
        {
            _d3DEngine = engine;
            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _actions = actions;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _voxelMeshFactory = voxelMeshFactory;
            this.Player = player;
            this.VoxelEntity = voxelEntity;
        }

        #region Private Methods

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        private void RefreshBoundingBox(ref DVector3 worldPosition, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox(_boundingMinPoint + worldPosition.AsVector3(),
                                          _boundingMaxPoint + worldPosition.AsVector3());
        }

        #region Player InputHandling
        /// <summary>
        /// Handle Player input handling - Movement and rotation input are not handled here
        /// </summary>
        private void inputHandler()
        {

            if (_actions.isTriggered(Actions.Move_Mode))
            {
                if (_displacementMode == EntityDisplacementModes.Flying)
                {
                    DisplacementMode = EntityDisplacementModes.Walking;
                }
                else
                {
                    DisplacementMode = EntityDisplacementModes.Flying;
                }
            }

            if (_actions.isTriggered(Actions.Block_Add))
            {
                //Enable Single block impact ==> For Testing purpose, shoul dbe removed ==============================================
                if (_isBlockPicked)
                {
                    EntityImpact.ReplaceBlock(ref _pickedBlock, CubeId.Air);
                }
                //Enable Single block impact ==> For Testing purpose, shoul dbe removed ==============================================
            }

            if (_actions.isTriggered(Actions.Block_Remove))
            {

                //Avoid the player to add a block where he is located !
                if (_isBlockPicked)
                {
                    if (!MBoundingBox.Intersects(ref _playerBoundingBox, ref _playerPotentialNewBlock) && _playerPotentialNewBlock.Maximum.Y <= AbstractChunk.ChunkSize.Y - 2)
                    {
                        EntityImpact.ReplaceBlock(ref _newCubePlace, CubeId.Gravel);
                    }
                }
                //Enable Single block impact ==> For Testing purpose, shoul dbe removed ==============================================
            }

            //Did I use the scrollWheel
            //if (_actions.isTriggered(Actions.Block_SelectNext))
            //{
            //    _buildingCubeIndex++;
            //    if (_buildingCubeIndex >= VisualCubeProfile.CubesProfile.Length) _buildingCubeIndex = 1;

            //    _buildingCube = VisualCubeProfile.CubesProfile[_buildingCubeIndex];
            //}

            //if (_actions.isTriggered(Actions.Block_SelectPrevious))
            //{
            //    _buildingCubeIndex--;
            //    if (_buildingCubeIndex <= 0) _buildingCubeIndex = VisualCubeProfile.CubesProfile.Length - 1;
            //    _buildingCube = VisualCubeProfile.CubesProfile[_buildingCubeIndex];
            //}
        }
        #endregion

        #region Player Block Picking
        private void GetSelectedBlock()
        {
            _isBlockPicked = false;

            _previousPickedBlock = _pickedBlock;

            DVector3 pickingPointInLine = _worldPosition.Value + _entityEyeOffset;
            //Sample 500 points in the view direction vector
            for (int ptNbr = 0; ptNbr < 500; ptNbr++)
            {
                pickingPointInLine += _lookAt * 0.02;

                if (_cubesHolder.isPickable(ref pickingPointInLine, out _pickedCube))
                {
                    _pickedBlock.X = MathHelper.Fastfloor(pickingPointInLine.X);
                    _pickedBlock.Y = MathHelper.Fastfloor(pickingPointInLine.Y);
                    _pickedBlock.Z = MathHelper.Fastfloor(pickingPointInLine.Z);

                    //Find the face picked up !
                    float FaceDistance;
                    Ray newRay = new Ray((_worldPosition.Value + _entityEyeOffset).AsVector3(), _lookAt.AsVector3());
                    BoundingBox bBox = new SharpDX.BoundingBox(new Vector3(_pickedBlock.X, _pickedBlock.Y, _pickedBlock.Z), new Vector3(_pickedBlock.X + 1, _pickedBlock.Y + 1, _pickedBlock.Z + 1));
                    newRay.Intersects(ref bBox, out FaceDistance);

                    DVector3 CollisionPoint = _worldPosition.Value + _entityEyeOffset + (_lookAt * FaceDistance);
                    MVector3.Round(ref CollisionPoint, 4);

                    _newCubePlace = new Location3<int>(_pickedBlock.X, _pickedBlock.Y, _pickedBlock.Z);
                    if (CollisionPoint.X == _pickedBlock.X) _newCubePlace.X--;
                    else
                        if (CollisionPoint.X == _pickedBlock.X + 1) _newCubePlace.X++;
                        else
                            if (CollisionPoint.Y == _pickedBlock.Y) _newCubePlace.Y--;
                            else
                                if (CollisionPoint.Y == _pickedBlock.Y + 1) _newCubePlace.Y++;
                                else
                                    if (CollisionPoint.Z == _pickedBlock.Z) _newCubePlace.Z--;
                                    else
                                        if (CollisionPoint.Z == _pickedBlock.Z + 1) _newCubePlace.Z++;


                    _playerPotentialNewBlock = new BoundingBox(new Vector3(_newCubePlace.X, _newCubePlace.Y, _newCubePlace.Z), new Vector3(_newCubePlace.X + 1, _newCubePlace.Y + 1, _newCubePlace.Z + 1));
                    _isBlockPicked = true;

                    break;
                }
            }

            ////Create the bounding box around the cube !
            //if (_previousPickedBlock != _pickedBlock && _isBlockPicked)
            //{
            //    _playerSelectedBox = new BoundingBox(new Vector3(_pickedBlock.X - 0.002f, _pickedBlock.Y - 0.002f, _pickedBlock.Z - 0.002f), new Vector3(_pickedBlock.X + 1.002f, _pickedBlock.Y + 1.002f, _pickedBlock.Z + 1.002f));
            //    _blocCursor.Update(ref _playerSelectedBox);
            //}
        }
        #endregion

        #region UnderWaterTest
        private void CheckHeadUnderWater()
        {
            if (_cubesHolder.IndexSafe(MathHelper.Fastfloor(CameraWorldPosition.X), MathHelper.Fastfloor(CameraWorldPosition.Y), MathHelper.Fastfloor(CameraWorldPosition.Z), out _headCubeIndex))
            {
                //Get the cube at the camera position !
                _headCube = _cubesHolder.Cubes[_headCubeIndex];
                if (_headCube.Id == CubeId.Water || _headCube.Id == CubeId.WaterSource)
                {
                    //TODO Take into account the Offseting in case of Offseted Water !
                    IsHeadInsideWater = true;
                }
                else
                {
                    IsHeadInsideWater = false;
                }
            }
        }
        #endregion

        #region Physic simulation for Collision detection
        private void PhysicOnEntity(EntityDisplacementModes mode, ref GameTime TimeSpend)
        {
            switch (mode)
            {
                case EntityDisplacementModes.Flying:
                    break;
                case EntityDisplacementModes.Walking:
                    PhysicSimulation(ref TimeSpend);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Check if the player is on the ground, or not.
        /// </summary>
        /// <param name="TimeSpend"></param>
        private void PhysicSimulation(ref GameTime TimeSpend)
        {
            TerraCubeWithPosition groundCube;
            Location3<int> GroundDirection = new Location3<int>(0, -1, 0);
            DVector3 newWorldPosition;

            _cubesHolder.GetNextSolidBlockToPlayer(ref _playerBoundingBox, ref GroundDirection, out groundCube);
            if (groundCube.Cube.Id != CubeId.Error)
            {
                _groundBelowEntity = groundCube.Position.Y + 1;
            }

            _physicSimu.Simulate(ref TimeSpend, out newWorldPosition);
            _worldPosition.Value = newWorldPosition;

            if (_worldPosition.Value.Y > _groundBelowEntity)
            {
                _physicSimu.OnGround = false;
            }
        }

        /// <summary>
        /// Validate player move against surrending landscape, if move not possible, it will be "rollbacked"
        /// </summary>
        /// <param name="newPosition2Evaluate"></param>
        /// <param name="previousPosition"></param>
        private void isCollidingWithTerrain(ref DVector3 newPosition2Evaluate, ref DVector3 previousPosition)
        {

            BoundingBox _boundingBox2Evaluate;
            DVector3 newPositionWithColliding = previousPosition;
            //Create a Bounding box with my new suggested position, taking only the X that has been changed !
            //X Testing
            newPositionWithColliding.X = newPosition2Evaluate.X;
            RefreshBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate))
                newPositionWithColliding.X = previousPosition.X;

            //Y Testing
            newPositionWithColliding.Y = newPosition2Evaluate.Y;

            //My Position raise  ==> If I were on the ground, I'm no more
            if (previousPosition.Y < newPositionWithColliding.Y && _physicSimu.OnGround) _physicSimu.OnGround = false;

            RefreshBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate))
            {
                //If Jummping
                if (previousPosition.Y < newPositionWithColliding.Y)
                {
                    newPositionWithColliding.Y = previousPosition.Y;
                }
                else //Falling
                {
                    newPositionWithColliding.Y = _groundBelowEntity;
                    previousPosition.Y = _groundBelowEntity; // ==> This way I stop the Y move !
                    _physicSimu.OnGround = true; // On ground ==> Activite the force that will counter the gravity !!
                }
            }

            //Z Testing
            newPositionWithColliding.Z = newPosition2Evaluate.Z;
            RefreshBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate))
                newPositionWithColliding.Z = previousPosition.Z;

            newPosition2Evaluate = newPositionWithColliding;
        }
        #endregion

        #region Entity Movement + rotation

        private void RefreshEntityMovementAndRotation(ref GameTime timeSpent)
        {
            switch (_displacementMode)
            {
                case EntityDisplacementModes.Flying:
                    _gravityInfluence = 6;  // We will move 6 times faster if flying
                    break;
                case EntityDisplacementModes.Walking:
                    _gravityInfluence = 1;
                    break;
                case EntityDisplacementModes.Swiming:
                    _gravityInfluence = 1 / 2; // We will move 2 times slower when swimming
                    break;
                default:
                    break;
            }

            //Compute the delta following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            _moveDelta = Player.MoveSpeed * _gravityInfluence * timeSpent.ElapsedGameTimeInS_HD;
            _rotationDelta = Player.RotationSpeed * timeSpent.ElapsedGameTimeInS_HD;

            //Backup previous values
            _lookAtDirection.BackUpValue();
            _worldPosition.BackUpValue();

            //Rotation with mouse
            EntityRotationsOnEvents(_displacementMode);

            //Movement
            EntityMovementsOnEvents(_displacementMode, ref timeSpent);

            //Physic simulation !
            PhysicOnEntity(_displacementMode, ref timeSpent);

            //Send the Actual Position to the Entity object only of it has change !!!
            //The Change check is done at DynamicEntity level
            Player.Position = _worldPosition.Value;
            Player.Rotation = _lookAtDirection.Value;
        }

        #region Movement Management
        private void EntityMovementsOnEvents(EntityDisplacementModes mode, ref GameTime TimeSpend)
        {
            switch (mode)
            {
                case EntityDisplacementModes.Flying:
                    FreeFirstPersonMove();
                    break;
                case EntityDisplacementModes.Walking:
                    WalkingFirstPerson(ref TimeSpend);
                    break;
                default:
                    break;
            }
        }

        private void FreeFirstPersonMove()
        {
            if (_actions.isTriggered(Actions.Move_Forward))
                _worldPosition.Value += _lookAt * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Backward))
                _worldPosition.Value -= _lookAt * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeLeft))
                _worldPosition.Value -= _entityHeadXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeRight))
                _worldPosition.Value += _entityHeadXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Down))
                _worldPosition.Value += DVector3.Down * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Up))
                _worldPosition.Value += DVector3.Up * _moveDelta;
        }

        private void WalkingFirstPerson(ref GameTime TimeSpend)
        {
            _physicSimu.Freeze(true, false, true);

            //Move 3 time slower if not touching ground
            if (!_physicSimu.OnGround) _moveDelta /= 3f;

            if (_actions.isTriggered(Actions.Move_Forward))
                if (_actions.isTriggered(Actions.Move_Run)) _physicSimu.PrevPosition += _entityZAxis * _moveDelta * 2f;
                else _physicSimu.PrevPosition += _entityZAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Backward))
                _physicSimu.PrevPosition -= _entityZAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeLeft))
                _physicSimu.PrevPosition += _entityXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeRight))
                _physicSimu.PrevPosition -= _entityXAxis * _moveDelta;

            if (_physicSimu.OnGround && _actions.isTriggered(Actions.Move_Jump))
                _physicSimu.Impulses.Add(new Impulse(ref TimeSpend) { ForceApplied = new DVector3(0, 300, 0) });
        }
        #endregion

        #region Head + Body Rotation management
        private void EntityRotationsOnEvents(EntityDisplacementModes mode)
        {
            if (_d3DEngine.UnlockedMouse == false)
            {
                Rotate(_inputsManager.MouseMoveDelta.X, _inputsManager.MouseMoveDelta.Y, 0.0f, mode);
            }
        }

        private void Rotate(double headingDegrees, double pitchDegrees, double rollDegrees, EntityDisplacementModes mode)
        {
            if (headingDegrees == 0 && pitchDegrees == 0 && rollDegrees == 0) return;

            headingDegrees *= _rotationDelta;
            pitchDegrees *= _rotationDelta;
            rollDegrees *= _rotationDelta;

            switch (mode)
            {
                case EntityDisplacementModes.Flying:
                    RotateLookAt(headingDegrees, pitchDegrees);
                    RotateMove(headingDegrees);
                    break;
                case EntityDisplacementModes.Walking:
                    RotateLookAt(headingDegrees, pitchDegrees);
                    RotateMove(headingDegrees);
                    break;
                default:
                    break;
            }
        }

        private void RotateMove(double headingDegrees)
        {
            double heading = MathHelper.ToRadians(headingDegrees);
            Quaternion rotation;

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, (float)heading, out rotation);
                _moveDirection.Value = rotation * _moveDirection.Value;
            }

            _moveDirection.Value.Normalize();

            UpdateEntityData();
        }
        private void UpdateEntityData()
        {
            Matrix.RotationQuaternion(ref _moveDirection.Value, out _entityRotation);

            _entityXAxis = new DVector3(_entityRotation.M11, _entityRotation.M21, _entityRotation.M31);
            _entityYAxis = new DVector3(_entityRotation.M12, _entityRotation.M22, _entityRotation.M32);
            _entityZAxis = new DVector3(_entityRotation.M13, _entityRotation.M23, _entityRotation.M33);
        }

        private void RotateLookAt(double headingDegrees, double pitchDegrees)
        {
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

            double heading = MathHelper.ToRadians(headingDegrees);
            double pitch = MathHelper.ToRadians(pitchDegrees);
            Quaternion rotation;

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, (float)heading, out rotation);
                _lookAtDirection.Value = rotation * _lookAtDirection.Value;
            }

            // Rotate the camera about its local X axis.
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, (float)pitch, out rotation);
                _lookAtDirection.Value = _lookAtDirection.Value * rotation;
            }

            _lookAtDirection.Value.Normalize();
            UpdateHeadData();
        }

        private void UpdateHeadData()
        {
            Matrix.RotationQuaternion(ref _lookAtDirection.Value, out _headRotation);

            _entityHeadXAxis = new DVector3(_headRotation.M11, _headRotation.M21, _headRotation.M31);
            _entityHeadYAxis = new DVector3(_headRotation.M12, _headRotation.M22, _headRotation.M32);
            _entityHeadZAxis = new DVector3(_headRotation.M13, _headRotation.M23, _headRotation.M33);

            _lookAt = new DVector3(-_entityHeadZAxis.X, -_entityHeadZAxis.Y, -_entityHeadZAxis.Z);
            _lookAt.Normalize();
        }
        #endregion

        #endregion

        #region Player Drawing
        private void RefreshBodyMesh()
        {
            if (!_altered) return;

            _vertice = _voxelMeshFactory.GenCubesFaces(VoxelEntity.Blocks);

            if (_vertice.Count != 0)
            {
                _vertexBuffer.SetData(_vertice.ToArray());
            }

            _altered = false;
        }
        #endregion
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            //Init Velret physic simulator
            _physicSimu = new VerletSimulator(ref _playerBoundingBox) { WithCollisionBounsing = false };
            _physicSimu.ConstraintFct += isCollidingWithTerrain;

            //Set displacement mode
            DisplacementMode = Player.DisplacementMode;

            //Compute the Eye position into the entity
            _entityEyeOffset = new Vector3(0, Player.Size.Y / 100 * 80, 0);

            //Will be used to update the bounding box with world coordinate when the entity is moving
            _boundingMinPoint = new Vector3(-(Player.Size.X / 2.0f), 0, -(Player.Size.Z / 2.0f));
            _boundingMaxPoint = new Vector3(+(Player.Size.X / 2.0f), Player.Size.Y, +(Player.Size.Z / 2.0f));

            //Compute the initial Player world bounding box
            RefreshBoundingBox(ref _worldPosition.Value, out _playerBoundingBox);

            //Set Position
            //Set the entity world position following the position received from server
            _worldPosition.Value = Player.Position;
            _worldPosition.ValuePrev = Player.Position;

            //Set LookAt
            //Take back only the saved server Yaw rotation (Or Heading) and only using it;
            _lookAtDirection.Value = Player.Rotation;
            double playerSavedYaw = MQuaternion.getYaw(ref _lookAtDirection.Value);
            Quaternion.RotationAxis(ref MVector3.Up, (float)playerSavedYaw, out _lookAtDirection.Value);
            _lookAtDirection.ValuePrev = _lookAtDirection.Value;

            //Set Move direction = to LookAtDirection
            _moveDirection.Value = _lookAtDirection.Value;

            //initilize Draw buffers
            _vertice = _voxelMeshFactory.GenCubesFaces(VoxelEntity.Blocks);
            _altered = true;
        }

        /// <summary>
        /// The allocated object here must be disposed
        /// </summary>
        public override void LoadContent()
        {
            _vertexBuffer = _voxelMeshFactory.InitBuffer(_vertice);
            _playerEffect = new HLSLTerran(_d3DEngine, @"Effects/Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration);

            //Create the Voxel body
            RefreshBodyMesh();
        }

        public override void Dispose()
        {
            if (_playerEffect != null) _playerEffect.Dispose();
            if (_vertexBuffer != null) _vertexBuffer.Dispose();
        }

        public override void Update(ref GameTime timeSpent)
        {
            inputHandler();             //Input handling

            GetSelectedBlock();         //Player Block Picking handling

            CheckHeadUnderWater();      //Under water head test

            RefreshEntityMovementAndRotation(ref timeSpent);   //Refresh player Movement + rotation

            //Refresh the player Bounding box
            RefreshBoundingBox(ref _worldPosition.Value, out _playerBoundingBox);
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            Quaternion.Slerp(ref _lookAtDirection.ValuePrev, ref _lookAtDirection.Value, interpolationLd, out _lookAtDirection.ValueInterp);
            DVector3.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);
        }

        public override void Draw()
        {
            //Applying Correct Render States
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.CullNone, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _playerEffect.Begin();

            _playerEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D);
            _playerEffect.CBPerFrame.IsDirty = true;

            if (_worldPosition.ActualValue.X == _cameraManager.ActiveCamera.WorldPosition.X && _worldPosition.ActualValue.Z == _cameraManager.ActiveCamera.WorldPosition.Z) return;

            Vector3 entityCenteredPosition = _worldPosition.ActualValue.AsVector3();
            entityCenteredPosition.X -= Player.Size.X / 2;
            entityCenteredPosition.Z -= Player.Size.Z / 2;

            Matrix world = Matrix.Scaling(Player.Size) * Matrix.Translation(entityCenteredPosition);

            world = _worldFocusManager.CenterOnFocus(ref world);

            _playerEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _playerEffect.CBPerDraw.IsDirty = true;
            _playerEffect.Apply();

            _vertexBuffer.SetToDevice(0);
            _d3DEngine.Context.Draw(_vertexBuffer.VertexCount, 0);
        }

        #endregion
    }
}
