using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
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
using Utopia.Entities.Renderer;
using Ninject;

namespace Utopia.Entities.Managers
{
    public class PlayerEntityManager : DrawableGameComponent, ICameraPlugin, IVisualEntityContainer, IDebugInfo
    {
        #region Private variables
        //Engine System variables
        private D3DEngine _d3DEngine;
        private CameraManager _cameraManager;
        private WorldFocusManager _worldFocusManager;
        private ActionsManager _actions;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;

        //Block Picking variables
        private TerraCube _pickedCube;

        //Head UnderWater test
        private int _headCubeIndex;
        private TerraCube _headCube;

        //Player Visual characteristics (Not insde the PlayerCharacter object)
        private BoundingBox _playerBoundingBox;
        private Vector3 _boundingMinPoint, _boundingMaxPoint;                         //Use to recompute the bounding box in world coordinate
        private FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();         //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        private Vector3D _lookAt;
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
        private Vector3D _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private Vector3D _entityXAxis, _entityYAxis, _entityZAxis;

        //Drawing component
        private IEntitiesRenderer _playerRenderer;
        #endregion

        #region Public variables/properties
        /// <summary>
        /// The Player
        /// </summary>
        public readonly PlayerCharacter Player;
        /// <summary>
        /// The Player Voxel body
        /// </summary>
        public VisualEntity VisualEntity { get; set; }

        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual Vector3D CameraWorldPosition { get { return _worldPosition.Value + _entityEyeOffset; } }
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
                                   VisualEntity visualEntity,
                                   PlayerCharacter player,
                                   [Named("PlayerEntityRenderer")] IEntitiesRenderer playerRenderer)
        {
            _d3DEngine = engine;
            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _actions = actions;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _playerRenderer = playerRenderer;
            this.Player = player;
            this.VisualEntity = visualEntity;

            //Give the Renderer acces to the Voxel buffers, ...
            _playerRenderer.VisualEntity = this;
        }

        public override void Dispose()
        {
            this.VisualEntity.Dispose();
            // _playerRenderer.Dispose(); ==> REgistered with Ninject
        }

        void inputsManager_OnKeyPressed(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            Console.WriteLine(e.KeyChar);
        }

        #region Private Methods

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        private void RefreshBoundingBox(ref Vector3D worldPosition, out BoundingBox boundingBox)
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

            if (_actions.isTriggered(Actions.Use_LeftWhileCursorLocked))
            {
                //Enable Single block impact ==> For Testing purpose, shoul dbe removed ==============================================
                if (Player.EntityState.IsBlockPicked)
                {
                    //update the Entity "State" or not ?
                    Player.LeftToolUse();
                    //EntityImpact.ReplaceBlock(ref _pickedBlock, CubeId.Air);
                }
                //Enable Single block impact ==> For Testing purpose, shoul dbe removed ==============================================
            }

            if (_actions.isTriggered(Actions.Use_RightWhileCursorLocked))
            {
                
                //Avoid the player to add a block where he is located !
                if (Player.EntityState.IsBlockPicked)
                {
                    BoundingBox playerPotentialNewBlock;
                    ComputeBlockBoundingBox(ref Player.EntityState.NewBlockPosition, out playerPotentialNewBlock);

                    if (!MBoundingBox.Intersects(ref _playerBoundingBox, ref playerPotentialNewBlock))// && _playerPotentialNewBlock.Maximum.Y <= AbstractChunk.ChunkSize.Y - 2)
                    {
                        Player.RightToolUse();
                        //EntityImpact.ReplaceBlock(ref _newCubePlace, CubeId.Gravel);
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
            Player.EntityState.IsBlockPicked = false;

            Vector3D pickingPointInLine = _worldPosition.Value + _entityEyeOffset;
            //Sample 500 points in the view direction vector
            for (int ptNbr = 0; ptNbr < 500; ptNbr++)
            {
                pickingPointInLine += _lookAt * 0.02;

                if (_cubesHolder.isPickable(ref pickingPointInLine, out _pickedCube))
                {
                    Player.EntityState.PickedBlockPosition.X = MathHelper.Fastfloor(pickingPointInLine.X);
                    Player.EntityState.PickedBlockPosition.Y = MathHelper.Fastfloor(pickingPointInLine.Y);
                    Player.EntityState.PickedBlockPosition.Z = MathHelper.Fastfloor(pickingPointInLine.Z);

                    //Find the face picked up !
                    float FaceDistance;
                    Ray newRay = new Ray((_worldPosition.Value + _entityEyeOffset).AsVector3(), _lookAt.AsVector3());
                    
                    BoundingBox bBox;
                    ComputeBlockBoundingBox(ref Player.EntityState.PickedBlockPosition, out bBox);

                    newRay.Intersects(ref bBox, out FaceDistance);

                    Vector3D CollisionPoint = _worldPosition.Value + _entityEyeOffset + (_lookAt * FaceDistance);
                    MVector3.Round(ref CollisionPoint, 4);

                    Player.EntityState.NewBlockPosition = Player.EntityState.PickedBlockPosition;

                    if (CollisionPoint.X == Player.EntityState.PickedBlockPosition.X) Player.EntityState.NewBlockPosition.X--;
                    else
                        if (CollisionPoint.X == Player.EntityState.PickedBlockPosition.X + 1) Player.EntityState.NewBlockPosition.X++;
                        else
                            if (CollisionPoint.Y == Player.EntityState.PickedBlockPosition.Y) Player.EntityState.NewBlockPosition.Y--;
                            else
                                if (CollisionPoint.Y == Player.EntityState.PickedBlockPosition.Y + 1) Player.EntityState.NewBlockPosition.Y++;
                                else
                                    if (CollisionPoint.Z == Player.EntityState.PickedBlockPosition.Z) Player.EntityState.NewBlockPosition.Z--;
                                    else
                                        if (CollisionPoint.Z == Player.EntityState.PickedBlockPosition.Z + 1) Player.EntityState.NewBlockPosition.Z++;

                                       
                    Player.EntityState.IsBlockPicked = true;
                    break;
                }
            }

            Player.EntityState.PickedEntityId = 0;

            ////Create the bounding box around the cube !
            //if (_previousPickedBlock != _pickedBlock && _isBlockPicked)
            //{
            //    _playerSelectedBox = new BoundingBox(new Vector3(_pickedBlock.X - 0.002f, _pickedBlock.Y - 0.002f, _pickedBlock.Z - 0.002f), new Vector3(_pickedBlock.X + 1.002f, _pickedBlock.Y + 1.002f, _pickedBlock.Z + 1.002f));
            //    _blocCursor.Update(ref _playerSelectedBox);
            //}
        }

        private void ComputeBlockBoundingBox(ref Location3<int> BlockPlace, out BoundingBox BlockBoundingBox)
        {
            BlockBoundingBox = new BoundingBox(new Vector3(BlockPlace.X, BlockPlace.Y, BlockPlace.Z), new Vector3(BlockPlace.X + 1, BlockPlace.Y + 1, BlockPlace.Z + 1));
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
            Vector3D newWorldPosition;

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
        private void isCollidingWithTerrain(ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {

            BoundingBox _boundingBox2Evaluate;
            Vector3D newPositionWithColliding = previousPosition;
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
                    _gravityInfluence = 3;  // We will move 6 times faster if flying
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
                _worldPosition.Value += Vector3D.Down * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Up))
                _worldPosition.Value += Vector3D.Up * _moveDelta;
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
                _physicSimu.Impulses.Add(new Impulse(ref TimeSpend) { ForceApplied = new Vector3D(0, 300, 0) });
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
                    RotateLookAt(-headingDegrees, -pitchDegrees);
                    RotateMove(-headingDegrees);
                    break;
                case EntityDisplacementModes.Walking:
                    RotateLookAt(-headingDegrees, -pitchDegrees);
                    RotateMove(-headingDegrees);
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
                _moveDirection.Value = _moveDirection.Value * rotation;
            }

            _moveDirection.Value.Normalize();

            UpdateEntityData();
        }
        private void UpdateEntityData()
        {
            Matrix.RotationQuaternion(ref _moveDirection.Value, out _entityRotation);
            Matrix.Transpose(ref _entityRotation, out _entityRotation);

            _entityXAxis = new Vector3D(_entityRotation.M11, _entityRotation.M21, _entityRotation.M31);
            _entityYAxis = new Vector3D(_entityRotation.M12, _entityRotation.M22, _entityRotation.M32);
            _entityZAxis = new Vector3D(_entityRotation.M13, _entityRotation.M23, _entityRotation.M33);
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
                _lookAtDirection.Value = _lookAtDirection.Value * rotation;
            }

            // Rotate the camera about its local X axis.
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, (float)pitch, out rotation);
                _lookAtDirection.Value = rotation * _lookAtDirection.Value;
            }

            _lookAtDirection.Value.Normalize();
            UpdateHeadData();
        }

        private void UpdateHeadData()
        {
            Matrix.RotationQuaternion(ref _lookAtDirection.Value, out _headRotation);
            Matrix.Transpose(ref _headRotation, out _headRotation);

            _entityHeadXAxis = new Vector3D(_headRotation.M11, _headRotation.M21, _headRotation.M31);
            _entityHeadYAxis = new Vector3D(_headRotation.M12, _headRotation.M22, _headRotation.M32);
            _entityHeadZAxis = new Vector3D(_headRotation.M13, _headRotation.M23, _headRotation.M33);

            _lookAt = new Vector3D(-_entityHeadZAxis.X, -_entityHeadZAxis.Y, -_entityHeadZAxis.Z);
            _lookAt.Normalize();
        }
        #endregion
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
        }

        /// <summary>
        /// The allocated object here must be disposed
        /// </summary>
        public override void LoadContent()
        {
        }

        public override void Update(ref GameTime timeSpent)
        {
            inputHandler();             //Input handling

            GetSelectedBlock();         //Player Block Picking handling

            CheckHeadUnderWater();      //Under water head test

            RefreshEntityMovementAndRotation(ref timeSpent);   //Refresh player Movement + rotation

            //Refresh the player Bounding box
            RefreshBoundingBox(ref _worldPosition.Value, out _playerBoundingBox);

            _playerRenderer.Update(ref timeSpent);
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            Quaternion.Slerp(ref _lookAtDirection.ValuePrev, ref _lookAtDirection.Value, interpolationLd, out _lookAtDirection.ValueInterp);
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);

            _playerRenderer.Interpolation(ref interpolationHd, ref interpolationLd);

            //TODO To remove when Voxel Entity merge will done with Entity
            //Update the position and World Matrix of the Voxel body of the Entity.
            Vector3 entityCenteredPosition = _worldPosition.ValueInterp.AsVector3();
            //entityCenteredPosition.X -= Player.Size.X / 2;
            //entityCenteredPosition.Z -= Player.Size.Z / 2;
            VisualEntity.World = Matrix.Scaling(Player.Size) * Matrix.RotationQuaternion(_lookAtDirection.ValueInterp) * Matrix.Translation(entityCenteredPosition);
            //VisualEntity.World = Matrix.Scaling(Player.Size) * Matrix.Translation(entityCenteredPosition);
            //===================================================================================================================================
        }

        public override void Draw(int Index)
        {
            _playerRenderer.Draw(Index);
        }

        #endregion

        public string GetInfo()
        {
            return string.Format("Player {0} Pos: ({1}, {2}, {3})", Player.CharacterName, Math.Round(Player.Position.X, 1), Math.Round(Player.Position.Y, 1), Math.Round(Player.Position.Z, 1));
        }
    }
}
