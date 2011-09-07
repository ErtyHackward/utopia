using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Nuclex.UserInterface;
using Utopia.Editor;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Weather;
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;
using S33M3Engines;
using S33M3Engines.InputHandler;
using S33M3Engines.Cameras;
using Utopia.Entities.Living;
using Utopia.Worlds.SkyDomes.SharedComp;
using S33M3Engines.D3D;
using Utopia.Worlds.SkyDomes;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks;
using Utopia.Worlds;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Cubes;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Shared.World.WorldConfigs;
using Utopia.Entities;
using Utopia.Worlds.Storage;
using Utopia.Action;

namespace Utopia
{
    public partial class UtopiaRender
    {
        private void ContainersBindings(IKernel iocContainer, WorldParameters worldParam)
        {
            iocContainer.Bind<WorldParameters>().ToConstant(worldParam).InSingletonScope();
            iocContainer.Bind<VisualWorldParameters>().ToSelf().InSingletonScope();


            iocContainer.Bind<GameStatesManager>().ToSelf().InSingletonScope();
            iocContainer.Bind<D3DEngine>().ToSelf().InSingletonScope();
            iocContainer.Bind<ICamera>().To<FirstPersonCamera>().InSingletonScope();
            iocContainer.Bind<CameraManager>().ToSelf().InSingletonScope();
            iocContainer.Bind<WorldRenderer>().ToSelf().InSingletonScope();
            iocContainer.Bind<SingleArrayChunkContainer>().ToSelf().InSingletonScope();

            iocContainer.Bind<EntityRenderer>().ToSelf().InSingletonScope();

            iocContainer.Bind<VoxelMeshFactory>().ToSelf().InSingletonScope();
            iocContainer.Bind<ItemRenderer>().ToSelf().InSingletonScope();

            iocContainer.Bind<IDrawableComponent>().To<SkyStars>().InSingletonScope().Named("Stars");
            iocContainer.Bind<IDrawableComponent>().To<Clouds>().InSingletonScope().Named("Clouds");

            iocContainer.Bind<ICubeMeshFactory>().To<SolidCubeMeshFactory>().InSingletonScope().Named("SolidCubeMeshFactory");
            iocContainer.Bind<ICubeMeshFactory>().To<LiquidCubeMashFactory>().InSingletonScope().Named("LiquidCubeMeshFactory");

            //Chunk Landscape
            iocContainer.Bind<ILandscapeManager>().To<LandscapeManager>().InSingletonScope();
            iocContainer.Bind<IWorldProcessorConfig>().To<FlatWorldConfig>().InSingletonScope().Named("FlatWorld");
            iocContainer.Bind<IWorldProcessorConfig>().To<s33m3WorldConfig>().InSingletonScope().Named("s33m3World");
            iocContainer.Bind<IWorldProcessor>().To<FlatWorldProcessor>().Named("FlatWorldProcessor");
            iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("s33m3WorldProcessor");
            iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");
            

            iocContainer.Bind<ILightingManager>().To<LightingManager>().InSingletonScope();

            //Chunk Mesh creator
            iocContainer.Bind<IChunkMeshManager>().To<ChunkMeshManager>().InSingletonScope();

            iocContainer.Bind<IChunksWrapper>().To<WorldChunksWrapper>().InSingletonScope();

            iocContainer.Bind<ISkyDome>().To<RegularSkyDome>().InSingletonScope();
            iocContainer.Bind<IWorldChunks>().To<WorldChunks>().InSingletonScope();
            iocContainer.Bind<IWorld>().To<World>().InSingletonScope();
            iocContainer.Bind<ILivingEntity>().To<Player>().InSingletonScope(); 
            iocContainer.Bind<IClock>().To<WorldClock>().InSingletonScope();
            iocContainer.Bind<IWeather>().To<Weather>().InSingletonScope();
            iocContainer.Bind<WorldFocusManager>().ToSelf().InSingletonScope();

            //Nuclex Screen (UI desktop) is a first class injectable now. any component who wants to draw something only needs the screen instance
            iocContainer.Bind<Screen>().ToSelf().InSingletonScope();

            iocContainer.Bind<UtopiaRender>().ToConstant(this); 
            //this is required for any component depending on game (having game in constructor params) 
            // or else the component gets a new game instance
            // normally/currently, only DebugComponent uses this for accedding to game.exit , _game.VSynSc  etc...
            iocContainer.Bind<DebugComponent>().ToSelf().InSingletonScope();

            iocContainer.Bind<GuiManager>().ToSelf().InSingletonScope();
            iocContainer.Bind<Hud>().ToSelf().InSingletonScope();

            iocContainer.Bind<EntityEditor>().ToSelf().InSingletonScope();

            iocContainer.Bind<IChunkStorageManager>().To<SQLiteStorageManager>().InSingletonScope();

            iocContainer.Bind<ActionsManager>().ToSelf().InSingletonScope();

        }
    }
}
