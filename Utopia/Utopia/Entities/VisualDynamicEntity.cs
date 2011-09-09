using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Interfaces;
using SharpDX;
using S33M3Engines.Maths;
using S33M3Engines.Struct;
using S33M3Engines.D3D;
using Utopia.Action;
using Utopia.Shared.Chunks;
using S33M3Engines.InputHandler.MouseHelper;
using Utopia.InputManager;
using S33M3Engines;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.Shared.Math;
using S33M3Engines.Cameras;
using Utopia.Shared.Structs;
using Utopia.Shared.Cubes;
using S33M3Physics.Verlet;
using S33M3Physics;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Entities.Voxel;

namespace Utopia.Entities
{
    /// <summary>
    /// Visual Class Wrapping a IDynamicEntity
    /// Could be Player, Monsters, ...
    /// </summary>
    public abstract class VisualDynamicEntity : VisualEntity, ICameraPlugin, IDisposable
    {
        #region Private variables

        //=======Should be moved inside VisualEntity when the schema will bind the Dynamicentity to voxelentity, if we go this way !
        private BoundingBox _boundingBox;
        private Vector3 _entityEyeOffset;         //Offset of the camera Placement inside the entity, from entity center point.
        private Vector3 _size;                    // ==> Should be extracted from the boundingbox around the voxel entity
        //================================================================================================================================

        private FTSValue<DVector3> _worldPosition = new FTSValue<DVector3>();           //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        private Vector3 _boundingMinPoint, _boundingMaxPoint;
        private ActionsManager _actions;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;
        private DVector3 _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private DVector3 _entityXAxis, _entityYAxis, _entityZAxis;
        private DVector3 _lookAt;
        private Matrix _headRotation;
        private Matrix _entityRotation;
        private double _rotationDelta;
        private double _moveDelta;
        private D3DEngine _engine;
        private double _accumPitchDegrees;
        private double _gravityInfluence;
        private float _groundBelowEntity;
        private VerletSimulator _physicSimu;
        private EntityDisplacementModes _displacementMode;
        private Location3<int> _pickedBlock, _previousPickedBlock, _newCubePlace;
        private bool _isBlockPicked;
        private Utopia.Shared.Structs.Landscape.TerraCube _pickedCube;
        private BoundingBox _playerSelectedBox, _playerPotentialNewBlock;
        #endregion

        #region Public Variables/Properties
        /// <summary> The Core component </summary>
        public readonly IDynamicEntity DynamicEntity;

        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual DVector3 CameraWorldPosition
        {
            get { return _worldPosition.ActualValue + _entityEyeOffset; }
        }

        public virtual Quaternion CameraOrientation
        {
            get { return _lookAtDirection.ActualValue; }
        }

        public EntityDisplacementModes Mode
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

        public VisualDynamicEntity(D3DEngine engine, 
                                   Vector3 size, 
                                   IDynamicEntity dynamicEntity, 
                                   ActionsManager actions, 
                                   InputsManager inputsManager, 
                                   SingleArrayChunkContainer cubesHolder, 
                                   VoxelMeshFactory voxelMeshFactory, 
                                   VoxelEntity voxelEntity)

            : base(voxelMeshFactory, voxelEntity)
        {
            _engine = engine;
            _actions = actions;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            DynamicEntity = dynamicEntity;
            _size = size;

            //_moveDirection.Value = Quaternion.Identity;

            //Check the position, if the possition is 0,0,0, find the better spawning Y value !
            if (DynamicEntity.Position == DVector3.Zero)
            {
                DynamicEntity.Position = new DVector3(0, AbstractChunk.ChunkSize.Y, 0);
            }

            _worldPosition.Value = DynamicEntity.Position;
            _worldPosition.ValueInterp = _worldPosition.Value;

            //Take back only the saved Yaw rotation (Or Heading) and only using it;
            _lookAtDirection.Value = DynamicEntity.Rotation;
            double playerSavedYaw = MQuaternion.getYaw(ref _lookAtDirection.Value);
            //Quaternion rotation;
            Quaternion.RotationAxis(ref MVector3.Up, (float)playerSavedYaw, out _lookAtDirection.Value);
            _lookAtDirection.ValueInterp = _lookAtDirection.Value;

            _moveDirection.Value = _lookAtDirection.Value;

            _physicSimu = new VerletSimulator(ref _boundingBox) { WithCollisionBounsing = false };
            _physicSimu.ConstraintFct += isCollidingWithTerrain;

            Mode = dynamicEntity.DisplacementMode;

        }
        #region Public Methods

        
        public void Initialize()
        {
            //Will be used to update the bounding box with world coordinate when the entity is moving
            _boundingMinPoint = new Vector3(-(_size.X / 2.0f), 0, -(_size.Z / 2.0f));
            _boundingMaxPoint = new Vector3(+(_size.X / 2.0f), _size.Y, +(_size.Z / 2.0f));

            RefreshBoundingBox(ref _worldPosition.Value, out _boundingBox);

            _entityEyeOffset = new Vector3(0, _size.Y / 100 * 80, 0);
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Update(ref GameTime timeSpent)
        {
            inputHandler();

            switch (_displacementMode)
            {
                case EntityDisplacementModes.Flying:
                    _gravityInfluence = 6;
                    break;
                case EntityDisplacementModes.Walking:
                    _gravityInfluence = 1;
                    break;
                case EntityDisplacementModes.Swiming:
                    _gravityInfluence = 1 / 2;
                    break;
                default:
                    break;
            }

            //Compute the delta following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            _moveDelta = DynamicEntity.MoveSpeed * _gravityInfluence * timeSpent.ElapsedGameTimeInS_HD;
            _rotationDelta = DynamicEntity.RotationSpeed * timeSpent.ElapsedGameTimeInS_HD;


            //Backup previous values
            _lookAtDirection.BackUpValue();
            _worldPosition.BackUpValue();

            //Compute Rotation with mouse
            EntityRotationsOnEvents(_displacementMode);

            //Keybord Movement
            EntityMovementsOnEvents(_displacementMode, ref timeSpent);

            //Physic simulation !
            PhysicOnEntity(_displacementMode, ref timeSpent);

            //Refresh location and Rotations compoent with the new values
            RefreshBoundingBox(ref _worldPosition.Value, out _boundingBox);

            //Send the Actual Position to the Dynamic Entity
            DynamicEntity.Position = _worldPosition.Value;
            DynamicEntity.Rotation = _lookAtDirection.Value;

            //Block Picking !?
            GetSelectedBlock();

            CheckHeadUnderWater();

            base.Update(ref timeSpent);
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            DVector3.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);
            Quaternion.Slerp(ref _lookAtDirection.ValuePrev, ref _lookAtDirection.Value, interpolationLd, out _lookAtDirection.ValueInterp);

            base.Interpolation(ref interpolationHd, ref interpolationLd);

        }

        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion

        #region Private Methods

        public void RefreshBoundingBox(ref DVector3 worldPosition, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox(_boundingMinPoint + worldPosition.AsVector3(),
                                          _boundingMaxPoint + worldPosition.AsVector3());
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
            if (_engine.UnlockedMouse == false)
            {
                Rotate(_inputsManager.MouseMoveDelta.X, _inputsManager.MouseMoveDelta.Y, 0.0f,  mode);
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

        #region EarlyPhysic Simulation
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

        private void PhysicSimulation(ref GameTime TimeSpend)
        {
            TerraCubeWithPosition groundCube;
            Location3<int> GroundDirection = new Location3<int>(0, -1, 0);
            DVector3 newWorldPosition;

            _cubesHolder.GetNextSolidBlockToPlayer(ref _boundingBox, ref GroundDirection, out groundCube);
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

        int _headCubeIndex;
        Utopia.Shared.Structs.Landscape.TerraCube _headCube;
        bool HeadInsideWater;
        private void CheckHeadUnderWater()
        {
            if (_cubesHolder.IndexSafe(MathHelper.Fastfloor(CameraWorldPosition.X), MathHelper.Fastfloor(CameraWorldPosition.Y), MathHelper.Fastfloor(CameraWorldPosition.Z), out _headCubeIndex))
            {
                //Get the cube at the camera position !
                _headCube = _cubesHolder.Cubes[_headCubeIndex];
                if (_headCube.Id == CubeId.Water || _headCube.Id == CubeId.WaterSource)
                {
                    //TODO Take into account the Offseting in case of Offseted Water !
                    HeadInsideWater = true;
                }
                else
                {
                    HeadInsideWater = false;
                }
            }
        }

        private void inputHandler()
        {
            if (_actions.isTriggered(Actions.Move_Mode))
            {
                if (_displacementMode == EntityDisplacementModes.Flying)
                {
                    Mode = EntityDisplacementModes.Walking;
                }
                else
                {
                    Mode = EntityDisplacementModes.Flying;
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
                    if (!MBoundingBox.Intersects(ref _boundingBox, ref _playerPotentialNewBlock) && _playerPotentialNewBlock.Maximum.Y <= AbstractChunk.ChunkSize.Y - 2)
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
    }
}
