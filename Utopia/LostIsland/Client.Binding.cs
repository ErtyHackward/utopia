using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Nuclex.UserInterface;
using Utopia.Editor;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.GUI.D3D.Map;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Weather;
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;
using S33M3Engines;
using S33M3Engines.InputHandler;
using S33M3Engines.Cameras;
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
using Utopia.Network;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.InputManager;
using Utopia.Entities.Managers;
using Utopia.Entities.Renderer;
using Utopia.Settings;
using Utopia.GUI;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Managers.Interfaces;
using S33M3Engines.Timers;
using Utopia.Entities.Renderer.Interfaces;
using Utopia;
using Utopia.Shared.Structs;

namespace LostIsland
{
    public partial class Client
    {
        public void Binding(IKernel iocContainer, UtopiaRender gameRenderer)
        {
            //Variables initialisation ==================================================================
            WorldParameters worldParam = new WorldParameters()
            {
                IsInfinite = true,
                Seed = iocContainer.Get<Server>().WorldSeed,
                SeaLevel = iocContainer.Get<Server>().SeaLevel,
                WorldChunkSize = new Location2<int>(ClientSettings.Current.Settings.GraphicalParameters.WorldSize,
                                                ClientSettings.Current.Settings.GraphicalParameters.WorldSize)
            };
            //===========================================================================================

            iocContainer.Bind<WorldParameters>().ToConstant(worldParam).InSingletonScope();
            iocContainer.Bind<IDynamicEntity>().ToConstant(iocContainer.Get<Server>().Player).InSingletonScope().Named("Player"); //Register the current Player.
            iocContainer.Bind<PlayerCharacter>().ToConstant(iocContainer.Get<Server>().Player).InSingletonScope(); //Register the current Player.

            iocContainer.Bind<PlayerEntityManager>().ToSelf().InSingletonScope();

            iocContainer.Bind<IEntitiesRenderer>().To<PlayerEntityRenderer>().InSingletonScope().Named("PlayerEntityRenderer");
            iocContainer.Bind<IEntitiesRenderer>().To<DynamicEntityRenderer>().InSingletonScope().Named("DefaultEntityRenderer");

            iocContainer.Bind<VisualWorldParameters>().ToSelf().InSingletonScope();
            iocContainer.Bind<VisualDynamicEntity>().ToSelf().InSingletonScope();

            iocContainer.Bind<IDynamicEntityManager>().To<DynamicEntityManager>().InSingletonScope();
            iocContainer.Bind<IStaticEntityManager>().To<StaticEntityManager>().InSingletonScope();
            iocContainer.Bind<IEntityPickingManager>().To<EntityPickAndCollisManager>().InSingletonScope();

            iocContainer.Bind<IStaticSpriteEntityRenderer>().To<StaticSpriteEntityRenderer>().InSingletonScope();



            //Chunk Landscape Creation Processors picking ====
            iocContainer.Bind<IWorldProcessorConfig>().To<s33m3WorldConfig>().InSingletonScope().Named("s33m3World");
            iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("s33m3WorldProcessor");
            iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");

            //Various ====
            iocContainer.Bind<IPickingRenderer>().To<PickingRenderer>().InSingletonScope();         // Use to display the picking cursor on block

            //Game Componenents =====
            iocContainer.Bind<IClock>().To<WorldClock>().InSingletonScope();
            iocContainer.Bind<ChatComponent>().ToSelf().InSingletonScope();
            iocContainer.Bind<MapComponent>().ToSelf().InSingletonScope();
            iocContainer.Bind<Hud>().ToSelf().InSingletonScope();
            iocContainer.Bind<EntityEditor>().ToSelf().InSingletonScope();
            iocContainer.Bind<IDrawableComponent>().To<SkyStars>().InSingletonScope().Named("Stars");
            iocContainer.Bind<ISkyDome>().To<RegularSkyDome>().InSingletonScope();
            iocContainer.Bind<IWeather>().To<Weather>().InSingletonScope();
            if (ClientSettings.Current.Settings.GraphicalParameters.CloudsQuality <= 0) iocContainer.Bind<IDrawableComponent>().To<Clouds>().InSingletonScope().Named("Clouds");
            else iocContainer.Bind<IDrawableComponent>().To<Clouds3D>().InSingletonScope().Named("Clouds");

            //System Management ======
            iocContainer.Bind<IChunkStorageManager>().To<SQLiteWorldStorageManager>().InSingletonScope();
            iocContainer.Bind<ICubeMeshFactory>().To<SolidCubeMeshFactory>().InSingletonScope().Named("SolidCubeMeshFactory");
            iocContainer.Bind<ICubeMeshFactory>().To<LiquidCubeMashFactory>().InSingletonScope().Named("LiquidCubeMeshFactory");
            iocContainer.Bind<ICamera>().To<FirstPersonCamera>().InSingletonScope(); //Type of camera used

        }
    }
}
