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

namespace Utopia.Entities
{
    public class PlayerEntityManager : DrawableGameComponent, ICameraPlugin
    {
        #region Private variables
        //Engine System variables
        private D3DEngine _engine;
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
        #endregion

        #region Public variables/properties
        /// <summary>
        /// The Player
        /// </summary>
        public PlayerCharacter Player;

        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual DVector3 CameraWorldPosition { get { return _worldPosition.Value + _entityEyeOffset; } }
        public virtual Quaternion CameraOrientation { get { return _lookAtDirection.Value; } }

        public bool IsHeadInsideWater { get; set; }

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


        public PlayerEntityManager(D3DEngine engine,
                                   CameraManager cameraManager,
                                   WorldFocusManager worldFocusManager,
                                   ActionsManager actions,
                                   InputsManager inputsManager,
                                   SingleArrayChunkContainer cubesHolder,
                                   VoxelMeshFactory voxelMeshFactory)
        {
            _engine = engine;
            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _actions = actions;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _voxelMeshFactory = voxelMeshFactory;
        }

        #region Private Methods

        #region Player InputHandling
        /// <summary>
        /// Handle Player input handling
        /// </summary>
        private void inputHandler()
        {

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

        #endregion

        #region Public Methods
        public override void Initialize()
        {
            //_physicSimu = new VerletSimulator(ref _playerBoundingBox) { WithCollisionBounsing = false };
            //_physicSimu.ConstraintFct += isCollidingWithTerrain;

            //Mode = Player.DisplacementMode;

            ////Check the position, if the possition is 0,0,0, find the better spawning Y value !
            //if (Player.Position == DVector3.Zero)
            //{
            //    Player.Position = new DVector3(0, AbstractChunk.ChunkSize.Y, 0);
            //}

            ////Set Position
            ////Set the entity world position following the position received from server
            //WorldPosition.Value = DynamicEntity.Position;
            //WorldPosition.ValuePrev = DynamicEntity.Position;

            ////Set LookAt
            ////Take back only the saved server Yaw rotation (Or Heading) and only using it;
            //_lookAtDirection.Value = DynamicEntity.Rotation;
            //double playerSavedYaw = MQuaternion.getYaw(ref _lookAtDirection.Value);
            //Quaternion.RotationAxis(ref MVector3.Up, (float)playerSavedYaw, out _lookAtDirection.Value);
            //_lookAtDirection.ValuePrev = _lookAtDirection.Value;

            ////Set Move direction = to LookAtDirection
            //_moveDirection.Value = _lookAtDirection.Value;
        }

        public override void LoadContent()
        {
        }

        public override void Dispose()
        {
        }

        public override void Update(ref GameTime timeSpent)
        {
            inputHandler();             //Input handling

            GetSelectedBlock();         //Player Block Picking handling

            CheckHeadUnderWater();      //Under water head test
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
        }

        public override void Draw()
        {
        }

        #endregion
    }
}
