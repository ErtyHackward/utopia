using System;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer;
using Utopia.Entities.Voxel;
using Utopia.Network;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Entities.Renderer.Interfaces;
using Ninject;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Cubes;
using Utopia.Shared.Settings;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Physics.Verlet;
using S33M3CoreComponents.Maths;
using SharpDX.Direct3D11;
using Utopia.Action;
using S33M3CoreComponents.Physics;
using S33M3DXEngine.Debug.Interfaces;
using Utopia.Entities.EntityMovement;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Responsible for:
    /// 1) current player physics interaction with the world. 
    /// 2) player movement input handling
    /// 3) player entity drawing
    /// 4) picking of the block
    /// </summary>
    public partial class PlayerEntityManager : DrawableGameComponent, ICameraPlugin, IVisualVoxelEntityContainer, IDebugInfo
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        //Engine System variables
        private D3DEngine _d3DEngine;
        private CameraManager<ICameraFocused> _cameraManager;
        private WorldFocusManager _worldFocusManager;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;

        //Block Picking variables
        public TerraCubeWithPosition PickedCube;
        public TerraCubeWithPosition NewCube;

        //Head UnderWater test
        private int _headCubeIndex;
        private TerraCube _headCube;

        //Player Visual characteristics (Not insde the PlayerCharacter object)
        private VisualEntity _pickedUpEntity;
        private Vector3D _pickedUpEntityPosition;
        
        private FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();         //World Position
        //private Vector3D _lookAt;
        private Vector3 _entityEyeOffset;                                     //Offset of the camera Placement inside the entity, from entity center point.

        //Mouvement handling variables
        private VerletSimulator _physicSimu;
        private float _gravityInfluence;
        private double _groundBelowEntity;
        private float _moveDelta;
        private IPickingRenderer _pickingRenderer;
        private IEntityPickingManager _entityPickingManager;
        private bool _stopMovedAction = false;

        //Drawing component
        private IEntitiesRenderer _playerRenderer;

        //Event related variables
        private double _fallMaxHeight;

        private TerraCubeWithPosition _groundCube;
        private CubeProfile _groundCubeProgile;

        //Will be used to compute entity rotation movements
        private EntityRotations _entityRotations;
        #endregion

        #region Public variables/properties

        public Vector3 LookAt
        {
            get { return _entityRotations.LookAt.ValueInterp; }
        }

        /// <summary>
        /// The Player
        /// </summary>
        public readonly PlayerCharacter Player;

        /// <summary>
        /// The Player Voxel body, its a class that will wrap the player character object with a Voxel Body
        /// </summary>
        public VisualVoxelEntity VisualVoxelEntity { get; set; }

        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual Vector3D CameraWorldPosition { get { return _worldPosition.Value + _entityEyeOffset; } }
        public virtual Quaternion CameraOrientation { get { return _entityRotations.EyeOrientation.Value; } }
        public virtual Quaternion CameraYAxisOrientation { get { return _entityRotations.BodyOrientation.Value; } }
        public virtual int CameraUpdateOrder { get { return this.UpdateOrder; } }

        public bool IsHeadInsideWater { get; set; }

        public bool CatchExclusiveAction { get; set; }

        public EntityDisplacementModes DisplacementMode
        {
            get { return Player.DisplacementMode; }
            set
            {
                Player.DisplacementMode = value;
                _entityRotations.SetDisplacementMode(Player.DisplacementMode, _worldPosition.Value + _entityEyeOffset);
#if DEBUG
                logger.Info("{0} is now {1}", Player.CharacterName, value.ToString());
#endif
                if (value == EntityDisplacementModes.Walking || value == EntityDisplacementModes.Swiming)
                {
                    _physicSimu.StartSimulation(ref _worldPosition.Value, ref _worldPosition.Value);
                }
                else
                {
                    _physicSimu.StopSimulation();
                }
            }
        }

        public bool HasMouseFocus { get; set; }

        public float PlayerOnOffsettedBlock { get; set; }
        public double GroundBelowEntity
        {
            get { return _groundBelowEntity; }
            set { _groundBelowEntity = value; }
        }

        public IWorldChunks WorldChunks { get; set; }
        public bool MousepickDisabled { get; set; }

        private bool _landscapeInitiazed;
        public bool LandscapeInitiazed
        {
            get { return _landscapeInitiazed; }
            set { _landscapeInitiazed = value; }
        }
        
        public float OffsetBlockHitted { get; set; }
        
        [Inject, Named("PlayerEntityRenderer")]
        public IEntitiesRenderer PlayerRenderer
        {
            get { return _playerRenderer; }
            set 
            { 
                _playerRenderer = value;
                //Give the Renderer acces to the Voxel buffers, ...
                _playerRenderer.VoxelEntityContainer = this;
            }
        }

        public SingleArrayChunkContainer CubesHolder
        {
            get { return _cubesHolder; }
        }

        [Inject]
        public IEntityPickingManager EntityPickingManager
        {
            get { return _entityPickingManager; }
            set
            {
                _entityPickingManager = value;
                _entityPickingManager.Player = this;
            }
        }

        #endregion

        #region Events

        public delegate void LandingGround(double fallHeight, TerraCubeWithPosition landedCube);
        public event LandingGround OnLanding;

        /// <summary>
        /// Occurs when player press one or more moves keys
        /// </summary>
        public event EventHandler WalkStart;

        private void OnWalkStart()
        {
            var handler = WalkStart;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when player releases all move keys
        /// </summary>
        public event EventHandler WalkEnd;

        private void OnWalkEnd()
        {
            var handler = WalkEnd;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        public PlayerEntityManager(D3DEngine engine,
                                   CameraManager<ICameraFocused> cameraManager,
                                   WorldFocusManager worldFocusManager,
                                   InputsManager inputsManager,
                                   SingleArrayChunkContainer cubesHolder,
                                   PlayerCharacter player,
                                   IPickingRenderer pickingRenderer,
                                   VoxelModelManager voxelModelManager
            )
        {
            _d3DEngine = engine;
            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _pickingRenderer = pickingRenderer;
            
            Player = player;

            this.ShowDebugInfo = true;

            //Create a visualVoxelEntity (== Assign a voxel body to the PlayerCharacter)
            VisualVoxelEntity = new VisualVoxelEntity(player, voxelModelManager);
            
            HasMouseFocus = Updatable;
            UpdateOrder = 0;
        }

        public override void BeforeDispose()
        {
            //Clean Up event Delegates
            if (OnLanding != null)
            {
                //Remove all Events associated to this Event (That haven't been unsubscribed !)
                foreach (Delegate d in OnLanding.GetInvocationList())
                {
                    OnLanding -= (LandingGround)d;
                }
            }

        }

        #region Private Methods
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            //Compute the Eye position into the entity
            _entityEyeOffset = new Vector3(0, Player.DefaultSize.Y / 100 * 80, 0);

            //Set Position
            //Set the entity world position following the position received from server
            _worldPosition.Value = Player.Position;
            _worldPosition.ValuePrev = Player.Position;

            //Compute the initial Player world bounding box
            VisualVoxelEntity.RefreshWorldBoundingBox(ref _worldPosition.Value);

            //Init Velret physic simulator
            _physicSimu = new VerletSimulator(ref VisualVoxelEntity.LocalBBox) { WithCollisionBouncing = false };
            _physicSimu.ConstraintFct += WorldChunks.isCollidingWithTerrain;
            _physicSimu.ConstraintFct += EntityPickingManager.isCollidingWithEntity;

            _entityRotations = new EntityRotations(_inputsManager, _physicSimu);
            _entityRotations.EntityRotationSpeed = Player.RotationSpeed;
            _entityRotations.SetOrientation(Player.HeadRotation, _worldPosition.Value + _entityEyeOffset);

            //Set displacement mode
            DisplacementMode = Player.DisplacementMode;

            _playerRenderer.Initialize();
        }

        /// <summary>
        /// The allocated object here must be disposed
        /// </summary>
        public override void LoadContent(DeviceContext context)
        {
            _playerRenderer.LoadContent(context);
        }

        public override void UnloadContent()
        {
            this.DisableComponent();
            _playerRenderer.UnloadContent();
            this.IsInitialized = false;
        }

        //string[] test = new string[100];
        //int i;
        //long from, to;
        public override void Update( GameTime timeSpend)
        {
            if (_landscapeInitiazed == false) return;

            //Input handling
            inputHandler();

            //Picking
            GetSelectedEntity();

            //Refresh player Movement + rotation
            UpdateEntityMovementAndRotation(ref timeSpend);   
            CheckAfterNewPosition();

            //Refresh the player Bounding box
            VisualVoxelEntity.RefreshWorldBoundingBox(ref _worldPosition.Value);
            
            _playerRenderer.Update(timeSpend);
        }


        public override void Interpolation(double interpolationHd, float interpolationLd, long timePassed)
        {
            //Interpolate rotations values
            _entityRotations.Interpolation(interpolationHd, interpolationLd, timePassed);
            //Interpolate the world position of the player
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);

            //TODO To remove when Voxel Entity merge will done with Entity
            VisualVoxelEntity.World = Matrix.RotationQuaternion(_entityRotations.EyeOrientation.ValueInterp) * Matrix.Translation(_worldPosition.ValueInterp.AsVector3());
            //===================================================================================================================================
            
            CheckHeadUnderWater();      //Under water head test

            _playerRenderer.Interpolation(interpolationHd, interpolationLd, timePassed);
        }

        public override void Draw(DeviceContext context, int index)
        {
            _playerRenderer.Draw(context, index);
        }

        #endregion

        //Debug Info interface
        public bool ShowDebugInfo { get; set; }
        
        public string GetDebugInfo()
        {
            return string.Format("Player {0} Pos: [{1:000}; {2:000}; {3:000}] PickedBlock: {4}; NewBlockPlace: {5}", Player.CharacterName,
                                                                                  Math.Round(Player.Position.X, 1),
                                                                                  Math.Round(Player.Position.Y, 1),
                                                                                  Math.Round(Player.Position.Z, 1),
                                                                                  Player.EntityState.IsBlockPicked ? Player.EntityState.PickedBlockPosition.ToString() : "None",
                                                                                  Player.EntityState.IsBlockPicked ? Player.EntityState.NewBlockPosition.ToString() : "None"
                                                                                  );            
        }
    }
}
