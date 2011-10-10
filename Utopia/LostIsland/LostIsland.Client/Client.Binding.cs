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
using S33M3Engines.Cameras;
using Utopia.Worlds.SkyDomes.SharedComp;
using S33M3Engines.D3D;
using Utopia.Worlds.SkyDomes;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks;
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
using S33M3Engines.D3D.DebugTools;
using Utopia.Effects.Shared;

namespace LostIsland.Client
{
    public partial class GameClient
    {
        public void EarlyBinding(IKernel iocContainer)
        {
            iocContainer.Bind<IChunkEntityImpactManager>().To<ChunkEntityImpactManager>().InSingletonScope(); //Impact on player action (From server events)
        }

        public void LateBinding(IKernel iocContainer, WorldParameters worldParam)
        {
            //DirectX layer & Helper ===================================
            iocContainer.Bind<D3DEngine>().ToSelf().InSingletonScope();         //DirectX Engine
            iocContainer.Bind<WorldFocusManager>().ToSelf().InSingletonScope(); //Focus
            //==========================================================

            //Parameters ===============================================
            iocContainer.Bind<WorldParameters>().ToConstant(worldParam).InSingletonScope();
            iocContainer.Bind<VisualWorldParameters>().ToSelf().InSingletonScope();

            //System Objects Management ================================
            iocContainer.Bind<GameStatesManager>().ToSelf().InSingletonScope(); //Application shared states
            iocContainer.Bind<ICamera>().To<FirstPersonCamera>().InSingletonScope(); //Type of camera used
            iocContainer.Bind<CameraManager>().ToSelf().InSingletonScope();     //Camera manager
            iocContainer.Bind<TimerManager>().ToSelf().InSingletonScope();      //Ingame based Timer class
            iocContainer.Bind<SharedFrameCB>().ToSelf().InSingletonScope();      //Ingame based Timer class

            //Network Related =============================================
            iocContainer.Bind<EntityMessageTranslator>().ToSelf().InSingletonScope();
            iocContainer.Bind<ItemMessageTranslator>().ToSelf().InSingletonScope();
            //=============================================================

            //User Input Handling ======================================
            iocContainer.Bind<InputsManager>().ToSelf().InSingletonScope();     //Input management
            iocContainer.Bind<ActionsManager>().ToSelf().InSingletonScope();    //Action management
            //==========================================================

            //GUI =========================================================
            iocContainer.Bind<GuiManager>().ToSelf().InSingletonScope();        //Gui base class
            iocContainer.Bind<Screen>().ToSelf().InSingletonScope();
            //=============================================================

            //Inventory ===================================================
            iocContainer.Bind<IconFactory>().ToSelf().InSingletonScope();       //Icon Factory
            //=============================================================

            //Debug displayer component ===================================
            iocContainer.Bind<FPS>().ToSelf().InSingletonScope();
            //=============================================================

            //Game Componenents =========================================
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

            //Landscape Creation/Acces/Management ====================================
            iocContainer.Bind<IChunkStorageManager>().To<SQLiteWorldStorageManager>().InSingletonScope();
            iocContainer.Bind<ICubeMeshFactory>().To<SolidCubeMeshFactory>().InSingletonScope().Named("SolidCubeMeshFactory");
            iocContainer.Bind<ICubeMeshFactory>().To<LiquidCubeMashFactory>().InSingletonScope().Named("LiquidCubeMeshFactory");
            iocContainer.Bind<SingleArrayChunkContainer>().ToSelf().InSingletonScope();         //The client  "Big" Array
            iocContainer.Bind<ILandscapeManager>().To<LandscapeManager>().InSingletonScope();   //Interface betwee the big array and landscape processors
            iocContainer.Bind<ILightingManager>().To<LightingManager>().InSingletonScope();     //Landscape lightings
            iocContainer.Bind<IChunkMeshManager>().To<ChunkMeshManager>().InSingletonScope();   //Chunk Mesh + Entities creation
            iocContainer.Bind<IWorldChunks>().To<WorldChunks>().InSingletonScope();             //Chunk Management (Update/Draw)
            iocContainer.Bind<IChunksWrapper>().To<WorldChunksWrapper>().InSingletonScope();    //Chunk "Wrapping" inside the big Array
            iocContainer.Bind<WorldGenerator>().ToSelf().InSingletonScope();                    //World Generator Class
            iocContainer.Bind<IWorldProcessorConfig>().To<ErtyHackwardWorldConfig>().InSingletonScope();
            iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("ErtyHackwardPlanWorldProcessor");
            //iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");
            //=============================================================

            //Entities related stuff ====================================================
            iocContainer.Bind<IPickingRenderer>().To<PickingRenderer>().InSingletonScope();         // Use to display the picking cursor on block
            iocContainer.Bind<IEntityPickingManager>().To<EntityPickAndCollisManager>().InSingletonScope();   //Entites picking and collision handling vs player
            iocContainer.Bind<IDynamicEntityManager>().To<DynamicEntityManager>().InSingletonScope();         //Dynamic Entity manager
            iocContainer.Bind<PlayerEntityManager>().ToSelf().InSingletonScope();                             //The player manager
            //Register the Player Against IDynamicEntity and PlayerCharacter
            iocContainer.Bind<PlayerCharacter>().ToConstant(iocContainer.Get<Server>().Player).InSingletonScope(); //Register the current Player.
            iocContainer.Bind<IDynamicEntity>().ToConstant(iocContainer.Get<Server>().Player).InSingletonScope().Named("Player"); //Register the current Player.
            iocContainer.Bind<IEntitiesRenderer>().To<PlayerEntityRenderer>().InSingletonScope().Named("PlayerEntityRenderer");    //Rendering Player
            iocContainer.Bind<IEntitiesRenderer>().To<DynamicEntityRenderer>().InSingletonScope().Named("DefaultEntityRenderer");  //Rendering Dynamic Entities
            iocContainer.Bind<VoxelMeshFactory>().ToSelf().InSingletonScope();  //Voxel Factory
            //=============================================================

        }
    }
}
