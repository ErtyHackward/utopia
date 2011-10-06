using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.World;
using Utopia;
using Ninject;
using Utopia.Network;
using Utopia.Settings;
using Utopia.Shared.Structs;
using Utopia.Worlds.Cubes;
using Utopia.Shared.Structs.Landscape;

namespace LostIsland.Client
{
    public partial class GameClient
    {
        public UtopiaRender CreateNewGameEngine(IKernel iocContainer)
        {
            //Prapare the world parameter variable from server sources ==================================
            WorldParameters worldParam = new WorldParameters()
            {
                IsInfinite = true,
                Seed = iocContainer.Get<Server>().WorldSeed,
                SeaLevel = iocContainer.Get<Server>().SeaLevel,
                WorldChunkSize = new Location2<int>(ClientSettings.Current.Settings.GraphicalParameters.WorldSize,   //Define the visible Client chunk size
                                                ClientSettings.Current.Settings.GraphicalParameters.WorldSize)
            };
            //===========================================================================================

            this.Binding(iocContainer, worldParam); // Bind various Components against concrete class.

            //=======================================================================================================================
            //Create the various Concrete classe Binded, forwarding appropriate value. ==============================================
            //=======================================================================================================================

            //Init Block Profiles
            VisualCubeProfile.InitCubeProfiles(iocContainer.Get<ICubeMeshFactory>("SolidCubeMeshFactory"),     //The default binded Solid Cube Mesh Factory
                                               iocContainer.Get<ICubeMeshFactory>("LiquidCubeMeshFactory"),    //The default binded Water Cube Mesh Factory
                                               @"Config\CubesProfile.xml");                                    //The path to the Cubes Profiles descriptions
            CubeProfile.InitCubeProfiles(@"Config\CubesProfile.xml");                                          // Init the cube profiles use by shared application (Similar than VisualCubeProfile, but without visual char.)
    


            return new UtopiaRender(iocContainer);
        }
    }
}
