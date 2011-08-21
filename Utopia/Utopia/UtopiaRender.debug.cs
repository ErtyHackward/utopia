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
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;
using S33M3Engines;
using Size = System.Drawing.Size;
using S33M3Engines.Threading;
using Utopia.Entities.Living;

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
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;

        //Init phase used for testing purpose
        private void DebugInit(IKernel IoCContainer)
        {
            //=======================================================================================
            //New Class structure Acces =============================================================
            //=======================================================================================
            //Create the world components ===========================================================

            //Variables initialisation ==================================================================
            Utopia.Shared.World.WorldParameters worldParam = new Shared.World.WorldParameters()
            {
                ChunkSize = new Location3<int>(16, 128, 16),
                IsInfinite = true,
                Seed = 0,
                WorldSize = new Location2<int>(ClientSettings.Current.Settings.GraphicalParameters.WorldSize,
                                                ClientSettings.Current.Settings.GraphicalParameters.WorldSize)
            };
            Location2<int> worldStartUp = new Location2<int>(0 * worldParam.ChunkSize.X, 0 * worldParam.ChunkSize.Z);
            
            //Init a new Big array Holder.
            SingleArrayChunkContainer sglArrayChunkManager = new SingleArrayChunkContainer(worldParam);
            //===========================================================================================

            //Creating the IoC Bindings
            ContainersBindings(IoCContainer, worldParam);

            //-- Get the Main D3dEngine --
            _d3dEngine = IoCContainer.Get<D3DEngine>(new ConstructorArgument("startingSize", new Size(W, H)),
                                                     new ConstructorArgument("windowCaption", "Powered By S33m3 Engine ! Rulezzz"),
                                                     new ConstructorArgument("MaxNbrThreads", WorkQueue.ThreadPool.Concurrency));

            _d3dEngine.Initialize(); //Init the 3d Engine
            _d3dEngine.GameWindow.Closed += (o, args) => { _isFormClosed = true; }; //Subscribe to Close event
            if (!_d3dEngine.UnlockedMouse) System.Windows.Forms.Cursor.Hide();      //Hide the mouse by default !

            _inputHandler = IoCContainer.Get<InputHandlerManager>();
            DXStates.CreateStates(_d3dEngine);  //Create all States that could by used by the game.

            //-- Get Camera --
            ICamera camera = IoCContainer.Get<ICamera>(); // Create a firstPersonCamera viewer

            //-- Get World focus --
            _worldFocusManager = IoCContainer.Get<WorldFocusManager>();
            _worldFocusManager.WorldFocus = (IWorldFocus)camera; // Use the camera as a the world focus

            //-- Get StateManager --
            _gameStateManagers = IoCContainer.Get<GameStatesManager>();
            _gameStateManagers.DebugActif = false;
            _gameStateManagers.DebugDisplay = 0;

            //-- Get Camera manager --
            _camManager = IoCContainer.Get<CameraManager>();

            //-- Create Entity Player --
            //TODO : Create an entity manager that will be responsible to render the various entities instead of leaving each entity to render itself.
            _player = IoCContainer.Get<ILivingEntity>(new ConstructorArgument("Name", "s33m3"),
                                                      new ConstructorArgument("startUpWorldPosition", new DVector3((worldParam.WorldSize.X / 2.0) + worldStartUp.X, 90, (worldParam.WorldSize.Z / 2.0f) + worldStartUp.Z)),
                                                      new ConstructorArgument("size", new Vector3(0.5f, 1.9f, 0.5f)),
                                                      new ConstructorArgument("walkingSpeed", 5f),
                                                      new ConstructorArgument("flyingSpeed", 30f),
                                                      new ConstructorArgument("headRotationSpeed", 10f)); 

            ((Player)_player).Mode = LivingEntityMode.FreeFirstPerson;
            GameComponents.Add(_player);

            //Attached the Player to the camera =+> The player will be used as Camera Holder !
            camera.CameraPlugin = _player;
            GameComponents.Add(_camManager); //The camera is using the _player to get it's world positions and parameters, so the _player updates must be done BEFORE the camera !


            //-- Clock --
            _worldClock = IoCContainer.Get<IClock>(new ConstructorArgument("input", _inputHandler),
                                                   new ConstructorArgument("clockSpeed", 480f),
                                                   new ConstructorArgument("startTime", (float)Math.PI * 1f));
            //-- Weather --
            _weather = IoCContainer.Get<IWeather>();
            //-- SkyDome --
            _skyDome = IoCContainer.Get<ISkyDome>();
            //-- Chunks --
            _chunks = IoCContainer.Get<IWorldChunks>(new ConstructorArgument("worldStartUpPosition", worldStartUp));

            //Create the World Components wrapper -----------------------
            _currentWorld = IoCContainer.Get<IWorld>();

            //Send the world to render
            _worldRenderer = IoCContainer.Get<WorldRenderer>();
            GameComponents.Add(_worldRenderer);             //Bind worldRendered to main loop.

            
            //=======================================================================================
            //=======================================================================================
        }
    }
}
