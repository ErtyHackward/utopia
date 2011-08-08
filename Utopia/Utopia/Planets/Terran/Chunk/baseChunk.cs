using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Planets.Terran.Cube;
using S33M3Engines.Struct;
using SharpDX;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Threading;
using Utopia.Planets.Terran.World;
using S33M3Engines.D3D;
using SharpDX.Direct3D11;
using UtopiaContent.ModelComp;
using Utopia.Planets.Terran.Lighting;
using Amib.Threading;
using Utopia.USM;
using Utopia.Shared.Structs;
using Utopia.Shared.Landscaping;

namespace Utopia.Planets.Terran.Chunk
{
    public enum ChunkState : byte
    {
        Empty,
        LandscapeCreated,
        LandscapeLightsSourceCreated,
        LandscapeLightsPropagated,
        MeshesChanged,
        DisplayInSyncWithMeshes,
        UserChanged
    }

    public abstract class baseChunk : IThreadStatus
    {
        #region Private Variables
        protected Range<int> _cubeRange;
        protected BoundingBox _chunkWorldBoundingBox;
        protected ChunkState _state;
        protected ThreadStatus _threadStatus;
        protected LandScape _landscape;
        protected TerraWorld _terraWorld;
        protected Game _game;
        protected bool _frustumCulled = false;
        private List<Location3<int>> _lightSourcesArray = new List<Location3<int>>();

        private bool _borderChunk = false;
        private bool _ready2Draw = false;
        private bool _landscapeInitialized = false;

        private WorkItemPriority _priority = WorkItemPriority.Normal;
        private int _userChangeOrder = 0;
        private float _popUpYOffset = -60;

        public Location2<int> LightPropagateBorderOffset = new Location2<int>(0, 0);

        //private int LandscapeSaveTicket = -1;
        public Matrix World;

        #endregion

        #region public Properties
        public float PopUpYOffset
        {
            get { PopUpOffsetRefresh();  return _popUpYOffset; }
        }
        public ThreadStatus ThreadStatus { get { return _threadStatus; } set { _threadStatus = value; } }
        public ChunkState State { get { return _state; } set { _state = value; }}
        public bool Ready2Draw { get { return _ready2Draw; } set { _ready2Draw = value; } }
        public bool LandscapeInitialized { get { return _landscapeInitialized; } set { _landscapeInitialized = value; } }

        public Int64 ChunkID { get; set; }
        public bool BorderChunk { get { return _borderChunk; } }

        public int UserChangeOrder
        {
            get { return _userChangeOrder; }
            set { _userChangeOrder = value; }
        }

        public WorkItemPriority Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public BoundingBox ChunkWorldBoundingBox { get { return _chunkWorldBoundingBox; } }
        public Range<int> CubeRange
        {
            get { return _cubeRange; }
            set { _cubeRange = value; RangeChanged(); }
        }

        public bool FrustumCulled
        {
            get { return _frustumCulled; }
            set { _frustumCulled = value; }
        }

#if DEBUG
        private BoundingBox3D _chunkBoundingBoxDisplay;
        public BoundingBox3D ChunkBoundingBoxDisplay { get { return _chunkBoundingBoxDisplay; } }
#endif

        #endregion

        public baseChunk(Game game, Range<int> CubeRange, LandScape Landscape, TerraWorld World)
        {
            //Init variables

#if DEBUG
            _chunkBoundingBoxDisplay = new BoundingBox3D(game, new Vector3((float)(CubeRange.Max.X - CubeRange.Min.X), (float)(CubeRange.Max.Y - CubeRange.Min.Y), (float)(CubeRange.Max.Z - CubeRange.Min.Z)), S33M3Engines.D3D.Effects.Basics.DebugEffect.DebugEffectVPC, Color.Tomato);
#endif
            _chunkWorldBoundingBox = new BoundingBox();

            _game = game;
            _terraWorld = World;
            this.CubeRange = CubeRange;
            _landscape = Landscape;

            Initialize();
        }

        private void Initialize()
        {
            //Compute the world Offset that needs to be applied on the Cubes coord stored inside the chunk to make them go inside world space.
            _state = ChunkState.Empty;
        }

        #region private methods

        private void PopUpOffsetRefresh()
        {
            if (_popUpYOffset < 0)
            {
                _popUpYOffset += 1;
            }
        }

        private void RangeChanged() // Start it also if the World offset Change !!!
        {
            ChunkID = (((Int64)_cubeRange.Min.X) << 32) + _cubeRange.Min.Z; //_cubeRange.GetHashCode();
            //if (LandscapeSaveTicket != -1)
            //{
            //    UtopiaSaveManager.GetData.FreeTicket(LandscapeSaveTicket);
            //    LandscapeSaveTicket = -1;
            //}

            RefreshWorldMatrix();
            _popUpYOffset = -60;
        }

        private void RefreshWorldMatrix()
        {
            Matrix.Translation(_cubeRange.Min.X, _cubeRange.Min.Y, _cubeRange.Min.Z, out World); //Create a matrix for world translation

            //Refresh the bounding Box to make it in world coord.
            _chunkWorldBoundingBox.Minimum = new Vector3(_cubeRange.Min.X, _cubeRange.Min.Y, _cubeRange.Min.Z);
            _chunkWorldBoundingBox.Maximum = new Vector3(_cubeRange.Max.X, _cubeRange.Max.Y, _cubeRange.Max.Z);

#if DEBUG
            ChunkBoundingBoxDisplay.Update(ref _chunkWorldBoundingBox);
#endif

        }

        public void RefreshChunkWorldMatrix()
        {
            RefreshWorldMatrix();
        }
        #endregion

        #region public methods

        #region Landscape creation !

        //Génération des cubes dans le chunk !
        public object CreateLandScape_Threaded(object stateInfo)
        {
            //Request the change done to the landscape
            //if (LandscapeSaveTicket == -1)
            //    LandscapeSaveTicket = UtopiaSaveManager.GetData.RequestChunkData(ChunkID);

            CreateLandScape();
            _threadStatus = ThreadStatus.Idle; //Thread Work finished

            return null;
        }

        public void CreateLandScape()
        {
            LandscapeBuilder.CreateChunkLandscape(_landscape.Cubes, ref _cubeRange, LandscapeInitialized); 

            _borderChunk = ChunkFinder.isBorderChunk(_cubeRange.Min.X, _cubeRange.Min.Z, ref _terraWorld.WorldRange);
            LandscapeInitialized = true;

            State = ChunkState.LandscapeCreated;

            //if (LandscapeSaveTicket != -1)
            //{
            //    CubeData[] savedCubes = UtopiaSaveManager.GetData.Data[LandscapeSaveTicket];
            //    if (savedCubes != null)
            //    {
            //        State = ChunkState.LandscapeCreated;
            //        UtopiaSaveManager.GetData.FreeTicket(LandscapeSaveTicket);
            //        LandscapeSaveTicket = -1;

            //        if (savedCubes.Length > 0)
            //        {
            //            //Console.WriteLine("Processing : " + savedCubes.Length + " cubes");
            //            _landscaper.InsertSavedBlocks(_landscape, savedCubes);
            //        }
            //        //else
            //        //{
            //        //    Console.WriteLine("No cubes to process !");
            //        //}
            //    }
            //}
            //else
            //{
            //    State = ChunkState.LandscapeCreated;
            //}
        }

        #endregion

        #region ChunkLandscape ==> Vertex & Index Buffer !

        public object CreateCubeMeshes_Threaded(object stateInfo)
        {
            CreateCubeMeshes();
            if (State != ChunkState.UserChanged)
            {
                State = ChunkState.MeshesChanged;
            }
            _threadStatus = ThreadStatus.Idle; //Thread Work finished

            return null;
        }

        public virtual void CreateCubeMeshes()
        {
        }
        public virtual void DrawSolidFaces()
        {
        }
        public virtual void DrawLiquidFaces()
        {
        }
        public virtual void DrawSeeThrough2Faces()
        {
        }
        #endregion

        #region Landscape Lighting
        //Call after landscape creation, Or specifically before a buffer Refresh If needed !

        public object CreateLightingSources_Threaded(object stateInfo)
        {
            CreateLightingSources();
            _threadStatus = ThreadStatus.Idle; //Thread Work finished
            return null;
        }

        public void CreateLightingSources()
        {
            TerraLighting.SetLightSources(_landscape, ref _cubeRange);
            State = ChunkState.LandscapeLightsSourceCreated;
        }

        public object PropagateLights_Threaded(object stateInfo)
        {
            PropagateLights();
            _threadStatus = ThreadStatus.Idle; //Thread Work finished
            return null;
        }

        public void PropagateLights()
        {
            bool BorderAsLightSource = false;
            if (LightPropagateBorderOffset.X != 0 || LightPropagateBorderOffset.Z != 0) BorderAsLightSource = true;
            Range<int> cubeRangeWithOffset = _cubeRange;
            if (LightPropagateBorderOffset.X > 0) cubeRangeWithOffset.Max.X += LightPropagateBorderOffset.X;
            else cubeRangeWithOffset.Min.X += LightPropagateBorderOffset.X;

            if (LightPropagateBorderOffset.Z > 0) cubeRangeWithOffset.Max.Z += LightPropagateBorderOffset.Z;
            else cubeRangeWithOffset.Min.Z += LightPropagateBorderOffset.Z;

            TerraLighting.PropagateLightSource(_terraWorld, ref cubeRangeWithOffset, BorderAsLightSource);
            State = ChunkState.LandscapeLightsPropagated;

            LightPropagateBorderOffset = new Location2<int>(0,0);
        }

        #endregion

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
#if DEBUG
            ChunkBoundingBoxDisplay.Dispose();
#endif
        }

        #endregion

        #region IThreadState Members
        #endregion
    }
}
