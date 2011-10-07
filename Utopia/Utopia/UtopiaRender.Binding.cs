using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Utopia.Entities;
using Utopia.InputManager;
using S33M3Engines.Timers;
using S33M3Engines;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.Action;
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;
using S33M3Engines.Cameras;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Network;
using Utopia.Worlds;
using Nuclex.UserInterface;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Managers;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.World;
using S33M3Engines.D3D.DebugTools;

namespace Utopia
{
    public partial class UtopiaRender
    {
        private void SystemBinding(IKernel iocContainer)
        {
            iocContainer.Bind<UtopiaRender>().ToConstant(this);                 //Self reference
            iocContainer.Bind<GameStatesManager>().ToSelf().InSingletonScope(); //Application shared states

            //DirectX layer & Helper ===================================
            iocContainer.Bind<D3DEngine>().ToSelf().InSingletonScope();         //DirectX Engine
            iocContainer.Bind<WorldFocusManager>().ToSelf().InSingletonScope(); //Focus
            //==========================================================

            //User Input Handling ======================================
            iocContainer.Bind<InputsManager>().ToSelf().InSingletonScope();     //Input management
            iocContainer.Bind<ActionsManager>().ToSelf().InSingletonScope();    //Action management
            //==========================================================

            iocContainer.Bind<CameraManager>().ToSelf().InSingletonScope();     //Camera manager
            iocContainer.Bind<TimerManager>().ToSelf().InSingletonScope();      //Ingame based Timer class
            iocContainer.Bind<GuiManager>().ToSelf().InSingletonScope();        //Gui base class

            iocContainer.Bind<VoxelMeshFactory>().ToSelf().InSingletonScope();  //Voxel Factory
            iocContainer.Bind<IconFactory>().ToSelf().InSingletonScope();       //Icon Factory

            //Landscape Creation/Acces/Management ====================================
            iocContainer.Bind<SingleArrayChunkContainer>().ToSelf().InSingletonScope();         //The client  "Big" Array
            iocContainer.Bind<ILandscapeManager>().To<LandscapeManager>().InSingletonScope();   //Interface betwee the big array and landscape processors
            iocContainer.Bind<ILightingManager>().To<LightingManager>().InSingletonScope();     //Landscape lightings
            iocContainer.Bind<IChunkMeshManager>().To<ChunkMeshManager>().InSingletonScope();   //Chunk Mesh + Entities creation
            iocContainer.Bind<IWorldChunks>().To<WorldChunks>().InSingletonScope();             //Chunk Management (Update/Draw)
            iocContainer.Bind<IChunksWrapper>().To<WorldChunksWrapper>().InSingletonScope();    //Chunk "Wrapping" inside the big Array
            iocContainer.Bind<WorldGenerator>().ToSelf().InSingletonScope();                    //World Generator Class
            //=============================================================

            //Entities ====================================================
            iocContainer.Bind<IChunkEntityImpactManager>().To<ChunkEntityImpactManager>().InSingletonScope(); //Impact on player action (From server events)
            iocContainer.Bind<IEntityPickingManager>().To<EntityPickAndCollisManager>().InSingletonScope();   //Entites picking and collision handling vs player
            iocContainer.Bind<IDynamicEntityManager>().To<DynamicEntityManager>().InSingletonScope();         //Dynamic Entity manager
            iocContainer.Bind<PlayerEntityManager>().ToSelf().InSingletonScope();                             //The player manager
            //Register the Player Against IDynamicEntity and PlayerCharacter
            iocContainer.Bind<PlayerCharacter>().ToConstant(iocContainer.Get<Server>().Player).InSingletonScope(); //Register the current Player.
            iocContainer.Bind<IDynamicEntity>().ToConstant(iocContainer.Get<Server>().Player).InSingletonScope().Named("Player"); //Register the current Player.
            
            //=============================================================

            //World Object Container ======================================
            iocContainer.Bind<IWorld>().To<World>().InSingletonScope();
            //=============================================================

            //Nuclex GUI ==================================================
            iocContainer.Bind<Screen>().ToSelf().InSingletonScope();
            //=============================================================

            //Debug displayer component ===================================
            iocContainer.Bind<DebugComponent>().ToSelf().InSingletonScope();
            iocContainer.Bind<FPS>().ToSelf().InSingletonScope();
            //=============================================================

            //Network Related =============================================
            iocContainer.Bind<EntityMessageTranslator>().ToSelf().InSingletonScope();
            iocContainer.Bind<ItemMessageTranslator>().ToSelf().InSingletonScope();
            //=============================================================


        }
    }
}
