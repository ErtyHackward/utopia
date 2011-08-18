using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.KeyboardHelper;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using S33M3Engines.Cameras;
using UtopiaContent.ModelComp;
using SharpDX;
using Utopia.Planets.Terran;
using S33M3Engines.Struct;
using Utopia.Planets.Terran.World;
using Utopia.Planets.Terran.Cube;
using S33M3Engines.Maths;
using S33M3Engines.D3D.Effects.Basics;
using Utopia.PlugIn;
using Utopia.GameDXStates;
using Utopia.USM;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;
using Utopia.Shared;
using Utopia.Settings;
using Utopia.Shared.Config;
using Utopia.Worlds;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Weather;
using Utopia.Worlds.SkyDomes.SharedComp;
using Ninject;
using Ninject.Parameters;

namespace Utopia
{
    public partial class UtopiaRender : Game
    {

        private WorldRenderer _worldRenderer;
        private IWorld _currentWorld;
        private IClock _worldClock;
        private ISkyDome _skyDome;
        private IWorldChunks _chunks;
        private IWeather _weather;

        //Init phase used for testing purpose
        private void DebugInit()
        {
            ICamera camera = new FirstPersonCamera(this);  // Create a firstPersonCAmera viewer
            WorldFocus = (IWorldFocus)camera; //Set the World Focus on my Camera

            //Create an entity, link to camera to it.
            _player = new Entities.Living.Player(this, "s33m3", camera, InputHandler,
                                                 new DVector3((LandscapeBuilder.Worldsize.X / 2.0) + LandscapeBuilder.WorldStartUpX, 90, (LandscapeBuilder.Worldsize.Z / 2.0f) + LandscapeBuilder.WorldStartUpZ),
                                                 new Vector3(0.5f, 1.9f, 0.5f),
                                                 5f, 30f, 10f)
            {
                Mode = Entities.Living.LivingEntityMode.FreeFirstPerson //Defaulted to "Flying" mode
            };
            GameComponents.Add(_player);

            //Attached the Player to the camera =+> The player will be used as Camera Holder !
            camera.CameraPlugin = _player; //The camera is using the _player to get it's world positions and parameters, so the _player updates must be done BEFORE the camera !
            GameComponents.Add(camera);

            //=======================================================================================
            //New Class structure Acces =============================================================
            //=======================================================================================
            //Create the world components ===========================================================
            Utopia.Shared.World.WorldParameters worldParam = new  Shared.World.WorldParameters()
                    { 
                        ChunkSize = new Location3<int>(16, 128, 16),
                        IsInfinite = true,
                        Seed = 0,
                        WorldSize = new Location2<int>(ClientSettings.Current.Settings.GraphicalParameters.WorldSize, 
                                                       ClientSettings.Current.Settings.GraphicalParameters.WorldSize)
                    };
            Location2<int> worldStartUp = new Location2<int>(0 * worldParam.ChunkSize.X, 0 * worldParam.ChunkSize.Z);

            //Init the Big array.
            SingleArrayDataProvider.ChunkCubes = new SingleArrayChunkCube(ref worldParam);

            using (IKernel kernel = new StandardKernel())
            {
                kernel.Bind<IClock>()
                      .To<WorldClock>()
                      .InSingletonScope()
                      .WithConstructorArgument("game", this)
                      .WithConstructorArgument("clockSpeed", 480f)
                      .WithConstructorArgument("startTime", (float)Math.PI * 1f);

                //-- Clock --
                //_worldClock = new WorldClock(this, 480, (float)Math.PI * 1f);
                _worldClock = kernel.Get<IClock>();
            }

            //-- Weather --
            _weather = new Weather(_worldClock);

            //-- SkyDome --
            _skyDome = new RegularSkyDome(this, 
                                         _worldClock, 
                                         _weather, 
                                         new SkyStars(this, _worldClock),
                                         new Clouds(this, _weather, ref worldParam)
                                         );

            //-- Chunks --
            _chunks = new WorldChunks(this, worldParam, worldStartUp, _worldClock);

            //Create the World Components wrapper
            _currentWorld = new World(){
                                        WorldChunks = _chunks,
                                        WorldClock = _worldClock,
                                        WorldSkyDome = _skyDome,
                                        WorldWeather = _weather
                                       };

            //Send the world to render
            _worldRenderer = new WorldRenderer(this, _currentWorld);

            //Bind worldRendered to main loop.
            GameComponents.Add(_worldRenderer);
            //=======================================================================================
            //=======================================================================================

            //Set Default Camera !
            base.ActivCamera = camera;
        }
    }
}
