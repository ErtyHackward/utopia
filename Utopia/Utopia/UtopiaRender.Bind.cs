using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
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
using Utopia.Shared.World.FlatWorld;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Cubes;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Worlds.Chunks.ChunkLighting;

namespace Utopia
{
    public partial class UtopiaRender
    {
        private void ContainersBindings(IKernel iocContainer, WorldParameters worldParam)
        {
            iocContainer.Bind<WorldParameters>().ToConstant(worldParam).InSingletonScope();

            iocContainer.Bind<GameStatesManager>().ToSelf().InSingletonScope();
            iocContainer.Bind<D3DEngine>().ToSelf().InSingletonScope();
            iocContainer.Bind<InputHandlerManager>().ToSelf().InSingletonScope();
            iocContainer.Bind<ICamera>().To<FirstPersonCamera>().InSingletonScope();
            iocContainer.Bind<CameraManager>().ToSelf().InSingletonScope();
            iocContainer.Bind<WorldRenderer>().ToSelf().InSingletonScope();
            iocContainer.Bind<SingleArrayChunkContainer>().ToSelf().InSingletonScope();


            iocContainer.Bind<IDrawableComponent>().To<SkyStars>().Named("Stars");
            iocContainer.Bind<IDrawableComponent>().To<Clouds>().Named("Clouds");

            iocContainer.Bind<ICubeMeshFactory>().To<SolidCubeMeshFactory>().InSingletonScope().Named("SolidCubeMeshFactory");

            //Chunk Landscape
            iocContainer.Bind<ILandscapeManager>().To<LandscapeManager>().InSingletonScope();
            iocContainer.Bind<IWorldProcessorConfig>().To<FlatWorldProcessorConfig>().InSingletonScope().Named("FlatWorld");
            iocContainer.Bind<IWorldProcessorConfig>().To<DummyWorldConfigurationConfig>().InSingletonScope().Named("DummyWorld");
            iocContainer.Bind<IWorldProcessor>().To<FlatWorldProcessor>().Named("FlatWorldProcessor");

            iocContainer.Bind<ILightingManager>().To<LightingManager>().InSingletonScope();

            //Chunk Mesh creator
            iocContainer.Bind<IChunkMeshManager>().To<ChunkMeshManager>().InSingletonScope();

            iocContainer.Bind<IChunksWrapper>().To<WorldChunksWrapper>().InSingletonScope();

            iocContainer.Bind<ISkyDome>().To<RegularSkyDome>();
            iocContainer.Bind<IWorldChunks>().To<WorldChunks>().InSingletonScope();
            iocContainer.Bind<IWorld>().To<World>();
            iocContainer.Bind<ILivingEntity>().To<Player>().InSingletonScope(); 
            iocContainer.Bind<IClock>().To<WorldClock>().InSingletonScope();
            iocContainer.Bind<IWeather>().To<Weather>().InSingletonScope();
            iocContainer.Bind<WorldFocusManager>().ToSelf().InSingletonScope();
        }
    }
}
