using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using SharpDX.Direct3D11;
using Utopia.Planets.Skybox;
using S33M3Engines.Struct;
using S33M3Engines.Threading;
using UtopiaContent.Effects.Terran;
using S33M3Engines.Struct.Vertex;
using System.Threading;
using S33M3Engines.StatesManager;
using SharpDX;
using Utopia.Planets.Terran.Chunk;
using Utopia.Planets.Terran.Cube;
using Utopia.Planets.Terran.World;
using S33M3Engines.Textures;
using Utopia.Entities.Living;
using Amib.Threading;
using S33M3Engines.Maths;
using Utopia.GameClock;
using Utopia.Planets.Terran.Flooding;
using Utopia.PlugIn;
using SharpDX.DXGI;
using S33M3Engines.Maths.Graphics;
using S33M3Engines.Sprites;
using Utopia.Shared.Landscaping;
using S33M3Engines.Shared.Math;
using Utopia.Planets.SkyDome;
using S33M3Engines;
using S33M3Engines.WorldFocus;
using S33M3Engines.Cameras;
using S33M3Engines.GameStates;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Chunks.ChunkWrapper;

namespace Utopia.Planets.Terran
{
    //Main Terra rendering Class
    public class Terra : GameComponent, IDebugInfo
    {
        #region private variables

        private HLSLTerran _terraEffect;
        private HLSLLiquid _liquidEffect;
        private int _chunkDrawByFrame;
        private PlanetSkyDome _skyDome;
        private ILivingEntity _player;
        private D3DEngine _d3dEngine;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        private GameStatesManager _gameStates;
        #endregion

        #region public properties
        public ILivingEntity Player { get { return _player; } }
        public TerraWorld World;
        public delegate void LiquidDrawing();
        public LiquidDrawing DrawLiquid;
        public ShaderResourceView Terra_View;
        public Clock GameClock;
        #endregion

        public Terra(D3DEngine d3dEngine, WorldFocusManager worldFocusManager, CameraManager camManager ,ref PlanetSkyDome skyDome, ref ILivingEntity player, ref int _worldSeed, ref Clock gameClock, LandscapeBuilder landscapeBuilder, GameStatesManager gameStates)
        {

            _d3dEngine = d3dEngine;
            _gameStates = gameStates;
            _worldFocusManager = worldFocusManager;
            _camManager = camManager;
            _skyDome = skyDome;
            _player = player;
            GameClock = gameClock;
            World = new TerraWorld(_d3dEngine, _worldFocusManager, _camManager, ref _worldSeed, landscapeBuilder);
            EntityImpact.Init(World);
            ChunkWrapper.Init(World);
            CubeMeshFactory.Init(World);
            DrawLiquid = DefaultDrawLiquid;
            //GameConsole.Write("Utopia started - Starting Position=" + TerraWorld.WorldStartUpX + ":" + TerraWorld.WorldStartUpZ + " Chunksize=" + TerraWorld.Chunksize);
        }

        public override void LoadContent()
        {
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, @"Textures/Terran/", @"ct*.png", FilterFlags.Point, out Terra_View);

             _terraEffect = new HLSLTerran(_d3dEngine, @"Effects/Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration);
             _liquidEffect = new HLSLLiquid(_d3dEngine, @"Effects/Terran/Liquid.hlsl", VertexCubeLiquid.VertexDeclaration);

            _terraEffect.TerraTexture.Value = Terra_View;
            _terraEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear); 

            _liquidEffect.TerraTexture.Value = Terra_View;
            _liquidEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinLinearMagPointMipLinear);

        }
        #region Update
        public override void Update(ref GameTime TimeSpend)
        {
            if (_camManager.ActiveCamera.WorldPosition.Y < 400)
            {
                ChunkUpdateManager();
                if (!_gameStates.DebugActif) CheckWrapping();     // Handle Playerzz impact on Terra (Mainly the location will trigger chunk creation/destruction)
                SortChunk();
            }
        }

        private void SortChunk()
        {
            World.SortChunk();
        }

        #region Update CHUNK rendering
        private void ChunkUpdateManager()
        {
            ProcessChunks_Empty();
            ProcessChunks_LandscapeCreated();
            ProcessChunks_LandscapeLightsSourceCreated();
            ProcessChunks_LandscapeLightsPropagated();
            ProcessChunks_MeshesChanged();
        }

        bool processInsync;

        private void ProcessChunks_Empty()
        {
            TerraChunk chunk;

            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
            {
                chunk = World.Chunks[chunkIndice];
                if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                if (chunk.State == ChunkState.Empty)
                {
                    if (WorkQueue.ThreadingActif) WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.CreateLandScape_Threaded), null, chunk as IThreadStatus, chunk.Priority);
                    else chunk.CreateLandScape();
                }
            }
        }

        private void ProcessChunks_LandscapeCreated()
        {
            TerraChunk chunk;

            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
            {
                chunk = World.Chunks[chunkIndice];
                if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                if (chunk.State == ChunkState.LandscapeCreated ||
                    chunk.State == ChunkState.UserChanged)
                {
                    if (WorkQueue.ThreadingActif) WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.CreateLightingSources_Threaded), null, chunk as IThreadStatus, chunk.Priority);
                    else chunk.CreateLightingSources();
                }
            }
        }

        // Syncronisation STEP !!! ==> No previous state pending job possible !! Wait for them all to be finished !
        private void ProcessChunks_LandscapeLightsSourceCreated()
        {
            processInsync = isUpdateInSync(ThreadSyncMode.UpdateReadyForLightPropagation);

            TerraChunk chunk;
            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
            {
                chunk = World.Chunks[chunkIndice];
                if (processInsync || chunk.Priority == WorkItemPriority.Highest)
                {
                    if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                    if (chunk.State == ChunkState.LandscapeLightsSourceCreated)
                    {
                        if (WorkQueue.ThreadingActif) WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.PropagateLights_Threaded), null, chunk as IThreadStatus, chunk.Priority);
                        else chunk.PropagateLights();
                    }
                }
            }
        }

        // Syncronisation STEP !!! ==> No previous state pending job possible !! Wait for them all to be finished !
        private void ProcessChunks_LandscapeLightsPropagated()
        {
            processInsync = isUpdateInSync(ThreadSyncMode.UpdateReadyForMeshCreation);

            TerraChunk chunk;
            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
            {
                chunk = World.Chunks[chunkIndice];
                if (processInsync || chunk.Priority == WorkItemPriority.Highest)
                {
                    if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!

                    if (chunk.State == ChunkState.LandscapeLightsPropagated)
                    {
                        if (WorkQueue.ThreadingActif) WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.CreateCubeMeshes_Threaded), null, chunk as IThreadStatus, chunk.Priority);
                        else chunk.CreateCubeMeshes();
                    }
                }
            }
        }

        int userOrder = 0;
        private void ProcessChunks_MeshesChanged()
        {
            userOrder = CheckUserModifiedChunks();

            TerraChunk chunk;
            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
            {
                chunk = World.Chunks[chunkIndice];
                if (chunk.ThreadStatus == ThreadStatus.Locked) continue; //Thread in working states ==> Cannot touch it !!!
                            
                if (chunk.UserChangeOrder != userOrder && chunk.Priority == WorkItemPriority.Highest)
                { //If this thread is user changed, but 
                    continue;
                }

                if (chunk.State == ChunkState.MeshesChanged)
                {
                    //Console.WriteLine(chunk.UserChangeOrder);

                    chunk.UserChangeOrder = 0;
                    chunk.Priority = WorkItemPriority.Normal;
                    //Si exécuté dans un thread => Doit fonctionner avec des device context deffered, avec un system de replay (Pas encore testé !!)
                    chunk.SendCubeMeshesToBuffers();

                    //if (WorkQueue.ThreadingActif) WorkQueue.DoWorkInThread(new WorkItemCallback(chunk.SendCubeMeshesToBuffers_Threaded), new Amib.Threading.Internal.WorkItemParam(), chunk as IThreadStatus, chunk.Priority, false);
                    //else chunk.SendCubeMeshesToBuffers(0);
                }
            }
        }

        private enum ThreadSyncMode
        {
            UpdateReadyForLightPropagation,
            UpdateReadyForMeshCreation,
            HighPriorityReadyToBeSendToGraphicalCard,
            ReadyForWrapping
        }


        private int CheckUserModifiedChunks()
        {
            TerraChunk chunk;
            int nbrUserThreads = 0;
            int lowestOrder = int.MaxValue;
            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
            {
                chunk = World.Chunks[chunkIndice];
                if (chunk.Priority == WorkItemPriority.Highest && chunk.State != ChunkState.DisplayInSyncWithMeshes) 
                    nbrUserThreads++;
                if (chunk.State == ChunkState.MeshesChanged && chunk.Priority == WorkItemPriority.Highest)
                {
                    if (chunk.UserChangeOrder < lowestOrder && chunk.UserChangeOrder > 0) lowestOrder = chunk.UserChangeOrder;
                    nbrUserThreads--;
                }
            }

            if (nbrUserThreads == 0) //All my threads are ready to render !
            {
                return lowestOrder;
            }
            else return 0;
        }

        private bool isUpdateInSync(ThreadSyncMode syncMode)
        {
            TerraChunk chunk;
            bool inSync = true;
            int nbrThread;
            switch (syncMode)
            {
                case ThreadSyncMode.UpdateReadyForLightPropagation:
                    for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
                    {
                        chunk = World.Chunks[chunkIndice];
                        if (chunk.State == ChunkState.Empty ||
                            chunk.State == ChunkState.LandscapeCreated || chunk.State == ChunkState.UserChanged)
                        {
                            inSync = false;
                            break;
                        }
                    }
                    break;
                case ThreadSyncMode.UpdateReadyForMeshCreation:
                    for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
                    {
                        chunk = World.Chunks[chunkIndice];
                        if (chunk.State == ChunkState.Empty ||
                            chunk.State == ChunkState.LandscapeCreated ||
                            chunk.State == ChunkState.LandscapeLightsSourceCreated || chunk.State == ChunkState.UserChanged)
                        {
                            inSync = false;
                            break;
                        }
                    }
                    break;
                case ThreadSyncMode.HighPriorityReadyToBeSendToGraphicalCard:
                    nbrThread = 0;
                    for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
                    {
                        chunk = World.Chunks[chunkIndice];
                        if (chunk.Priority == WorkItemPriority.Highest) nbrThread++;
                        if (chunk.State == ChunkState.MeshesChanged && chunk.Priority == WorkItemPriority.Highest)
                        {
                            nbrThread--;
                            break;
                        }
                        inSync = nbrThread == 0;
                    }
                    break;
                case ThreadSyncMode.ReadyForWrapping:
                    for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSurface; chunkIndice++)
                    {
                        chunk = World.Chunks[chunkIndice];
                        if (chunk.State != ChunkState.DisplayInSyncWithMeshes)
                        {
                            inSync = false;
                            break;
                        }
                    }
                    break;
            }

            return inSync;
        }

        #endregion

        #region Update MAP WRAPPING
        int _chunkCreationTrigger = (LandscapeBuilder.Worldsize.X / 2) - (1 * LandscapeBuilder.Chunksize);
        private void CheckWrapping()
        {
            if (!isUpdateInSync(ThreadSyncMode.ReadyForWrapping)) return;

            // Get World Border line ! => Highest and lowest X et Z chunk components
            //Compute Player position against WorldRange
            var resultmin = new DVector3(_player.WorldPosition.Value.X - World.WorldRange.Min.X,
                                        _player.WorldPosition.Value.Y - World.WorldRange.Min.Y,
                                        _player.WorldPosition.Value.Z - World.WorldRange.Min.Z);

            var resultmax = new DVector3(World.WorldRange.Max.X - _player.WorldPosition.Value.X,
                                        World.WorldRange.Max.Y - _player.WorldPosition.Value.Y,
                                        World.WorldRange.Max.Z - _player.WorldPosition.Value.Z);

            float wrapOrder = float.MaxValue;
            ChunkWrapType operation = ChunkWrapType.Z_Plus1;
            //FindLowest value !

            if (_chunkCreationTrigger > resultmin.X || _chunkCreationTrigger > resultmin.Z ||
                _chunkCreationTrigger > resultmax.X || _chunkCreationTrigger > resultmax.Z)
            {

                if (resultmin.X < wrapOrder)
                {
                    wrapOrder = (float)resultmin.X; operation = ChunkWrapType.X_Minus1;
                }

                if (resultmin.Z < wrapOrder)
                {
                    wrapOrder = (float)resultmin.Z; operation = ChunkWrapType.Z_Minus1;
                }

                if (resultmax.X < wrapOrder)
                {
                    wrapOrder = (float)resultmax.X; operation = ChunkWrapType.X_Plus1;
                }

                if (resultmax.Z < wrapOrder)
                {
                    wrapOrder = (float)resultmax.Z; operation = ChunkWrapType.Z_Plus1;
                }

                ChunkWrapper.AddWrapOperation(operation);

            }
        }
        #endregion

        #endregion

        #region Drawing

        public float SunColorBase;
        public override void DrawDepth0()
        {
            if (_camManager.ActiveCamera.WorldPosition.Y < 300)
            {
                StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.NotSet, GameDXStates.DXStates.DepthStencils.DepthEnabled);
#if DEBUG
                if (_gameStates.DebugDisplay == 2) StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Wired, GameDXStates.DXStates.NotSet, GameDXStates.DXStates.DepthStencils.DepthEnabled);
#endif
                _chunkDrawByFrame = 0;

                _terraEffect.Begin();
                _terraEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
                _terraEffect.CBPerFrame.Values.dayTime = GameClock.ClockTimeNormalized2;
                _terraEffect.CBPerFrame.Values.fogdist = ((LandscapeBuilder.Worldsize.X) / 2) - 48;
                _terraEffect.CBPerFrame.IsDirty = true;
                SunColorBase = GetSunColor();

                if (_player.HeadInsideWater) _terraEffect.CBPerFrame.Values.SunColor = new Vector3(SunColorBase / 3, SunColorBase / 3, SunColorBase);
                else _terraEffect.CBPerFrame.Values.SunColor = new Vector3(SunColorBase, SunColorBase, SunColorBase);

                DrawSolidFaces();
#if DEBUG
                DrawDebug();
#endif
            }
        }

        private void DrawSolidFaces()
        {
            TerraChunk chunk;
            Matrix worldFocus = Matrix.Identity;

            //Foreach faces type
            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSize * LandscapeBuilder.ChunkGridSize; chunkIndice++)
            {
                chunk = World.SortedChunks[chunkIndice];
                    
                if (chunk.Ready2Draw)
                {
                    //Only checking Frustum with the faceID = 0
                    chunk.FrustumCulled = !_camManager.ActiveCamera.Frustum.Intersects(chunk.ChunkWorldBoundingBox);

                    if (!chunk.FrustumCulled)
                    {
                        _worldFocusManager.CenterOnFocus(ref chunk.World, ref worldFocus);
                        _terraEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                        _terraEffect.CBPerDraw.Values.popUpYOffset = chunk.PopUpYOffset;
                        _terraEffect.CBPerDraw.IsDirty = true;
                        _terraEffect.Apply();

                        chunk.DrawSolidFaces();
                        _chunkDrawByFrame++;
                    }

                }
            }

            
        }

#if DEBUG
        private void DrawDebug()
        {
            TerraChunk chunk;
            Matrix worldFocus = Matrix.Identity;

            //Foreach faces type
            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSize * LandscapeBuilder.ChunkGridSize; chunkIndice++)
            {
                chunk = World.SortedChunks[chunkIndice];

                if (chunk.Ready2Draw)
                {
                    if (!chunk.FrustumCulled)
                    {
                        if (_gameStates.DebugDisplay == 1) chunk.ChunkBoundingBoxDisplay.Draw(_camManager.ActiveCamera, _worldFocusManager.WorldFocus);
                    }

                }
            }
        }
#endif
        private float GetSunColor()
        {
            float SunColorBase;
            if (GameClock.ClockTimeNormalized <= 0.2083944 || GameClock.ClockTimeNormalized > 0.9583824) // Between 23h00 and 05h00 => Dark night
            {
                SunColorBase = 0.05f;
            }
            else
            {
                if (GameClock.ClockTimeNormalized > 0.2083944 && GameClock.ClockTimeNormalized <= 0.4166951) // Between 05h00 and 10h00 => Go to Full Day
                {
                    SunColorBase = MathHelper.FullLerp(0.05f, 1, 0.2083944, 0.4166951, GameClock.ClockTimeNormalized);
                }
                else
                {
                    if (GameClock.ClockTimeNormalized > 0.4166951 && GameClock.ClockTimeNormalized <= 0.6666929) // Between 10h00 and 16h00 => Full Day
                    {
                        SunColorBase = 1f;
                    }
                    else
                    {
                        SunColorBase = MathHelper.FullLerp(1, 0.05f, 0.6666929, 0.9583824, GameClock.ClockTimeNormalized); //Go to Full night
                    }
                }
            }

            return SunColorBase;
        }


        public override void DrawDepth1()
        {
            if (_camManager.ActiveCamera.WorldPosition.Y < 300)
            {
                DrawLiquid();
            }
        }

        public override void DrawDepth2()
        {
        }

        //Default Liquid Drawing
        private void DefaultDrawLiquid()
        {
            Matrix worldFocus = Matrix.Identity;

            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.NotSet, GameDXStates.DXStates.DepthStencils.DepthEnabled);
#if DEBUG
            if (_gameStates.DebugDisplay == 2) StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Wired, GameDXStates.DXStates.NotSet, GameDXStates.DXStates.DepthStencils.DepthEnabled);
#endif
            TerraChunk chunk;

            _liquidEffect.Begin();
            _liquidEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            if (_player.HeadInsideWater) _liquidEffect.CBPerFrame.Values.SunColor = new Vector3(SunColorBase / 3, SunColorBase / 3, SunColorBase);
            else _liquidEffect.CBPerFrame.Values.SunColor = new Vector3(SunColorBase, SunColorBase, SunColorBase);
            _liquidEffect.CBPerFrame.IsDirty = true;

            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSize * LandscapeBuilder.ChunkGridSize; chunkIndice++)
            {
                chunk = World.SortedChunks[chunkIndice];
                if (chunk.Ready2Draw && !chunk.FrustumCulled) // !! Display all Changed one, even if the changed failed the Frustum culling test
                {
                    //Only If I have something to draw !
                    if (chunk.LiquidCubeVB != null)
                    {
                        _worldFocusManager.CenterOnFocus(ref chunk.World, ref worldFocus);
                        _liquidEffect.CBPerDraw.Values.popUpYOffset = chunk.PopUpYOffset;
                        _liquidEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocus);
                        _liquidEffect.CBPerDraw.IsDirty = true;
                        _liquidEffect.Apply();
                        chunk.DrawLiquidFaces();
                    }
                }
            }
        }

        #endregion

        #region CleanUp
        public override void UnloadContent()
        {
            Terra_View.Dispose();
            _liquidEffect.Dispose();
            _terraEffect.Dispose();
            World.Dispose();
            base.UnloadContent();
        }
        #endregion

        #region GetInfo Interface
        public string GetInfo()
        {
            int BprimitiveCount = 0;
            int VprimitiveCount = 0;
                        //Run over all chunks to see their status, and take action accordingly.
            for (int chunkIndice = 0; chunkIndice < LandscapeBuilder.ChunkGridSize * LandscapeBuilder.ChunkGridSize; chunkIndice++)
            {
                if (World.Chunks[chunkIndice].SolidCubeIB == null) continue;
                if (!World.Chunks[chunkIndice].FrustumCulled)
                {
                    VprimitiveCount += World.Chunks[chunkIndice].SolidCubeIB.IndicesCount;
                    if(World.Chunks[chunkIndice].LiquidCubeIB != null) VprimitiveCount += (World.Chunks[chunkIndice].LiquidCubeIB.IndicesCount);
                }
                BprimitiveCount += World.Chunks[chunkIndice].SolidCubeIB.IndicesCount;
                if (World.Chunks[chunkIndice].LiquidCubeIB != null) BprimitiveCount += (World.Chunks[chunkIndice].LiquidCubeIB.IndicesCount);
            }

            return string.Concat("<TerraCube Mod> BChunks : ", LandscapeBuilder.ChunkGridSize * LandscapeBuilder.ChunkGridSize, "; BPrim : ", BprimitiveCount, " DChunks : ", _chunkDrawByFrame, " DPrim : ", VprimitiveCount);
        }
        #endregion
    }
}
