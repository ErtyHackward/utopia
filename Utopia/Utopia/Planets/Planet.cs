using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using Utopia.Planets.Terran;
using Utopia.Planets.Weather;
using Utopia.GameClock;
using Utopia.Entities.Living;
using Utopia.Planets.Skybox;
using S33M3Engines.Struct;
using Utopia.Shared.Structs;
using Utopia.Planets.SkyDome;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;
using Utopia.Shared.Landscaping;

namespace Utopia.Planets
{
    public class Planet : GameComponent, IDebugInfo
    {
        public struct PlanetInfo
        {
            public int Seed;
            public Location3<int> UniverseLocation;
        }

        #region Private variables
        public Terra Terra;
        private WeatherManager _weatherMng;
        private PlanetSkyDome _planetSkyDome;

        private Clock _gameClock;
        private ILivingEntity _player;

        //World Seed (Used to generate the Planet)
        private PlanetInfo _planetnfo;
        #endregion

        #region Public Properties
        public PlanetInfo Planetnfo
        {
            get { return _planetnfo; }
            set { _planetnfo = value; }
        }
        #endregion

        public Planet(D3DEngine d3dEngine, CameraManager camManager, WorldFocusManager worldFocusManager, GameStatesManager gameStates , LandscapeBuilder landscapeBuilder ,Clock gameClock, ILivingEntity player,int planetSeed, Location3<int> universeLocation)
        {
            _gameClock = gameClock;
            _player = player;
            _planetnfo = new PlanetInfo() { Seed = planetSeed, UniverseLocation = universeLocation }; ;
            //Skydome creation
            _planetSkyDome = new PlanetSkyDome(d3dEngine, camManager, worldFocusManager, gameClock);
            //Main terrain creation
            Terra = new Planets.Terran.Terra(d3dEngine, worldFocusManager, camManager, ref _planetSkyDome, ref _player, ref _planetnfo.Seed, ref _gameClock, landscapeBuilder, gameStates);
            //player.TerraWorld = Terra.World;
            //Weather management
            _weatherMng = new Planets.Weather.WeatherManager(d3dEngine, camManager,  _gameClock, Terra.World);
        }

        #region Public Methods
        public override void Initialize()
        {
            _planetSkyDome.Initialize();
            Terra.Initialize();
            _weatherMng.Initialize();
        }

        public override void LoadContent()
        {
            _planetSkyDome.LoadContent();
            Terra.LoadContent();
            _weatherMng.LoadContent();
        }

        public override void UnloadContent()
        {
            _planetSkyDome.UnloadContent();
            Terra.UnloadContent();
            _weatherMng.UnloadContent();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            _planetSkyDome.Update(ref TimeSpend);
            Terra.Update(ref TimeSpend);
            _weatherMng.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _planetSkyDome.Interpolation(ref interpolation_hd, ref interpolation_ld);
            Terra.Interpolation(ref interpolation_hd, ref interpolation_ld);
            _weatherMng.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override void DrawDepth0()
        {
            _planetSkyDome.DrawDepth0();
            Terra.DrawDepth0();
            _weatherMng.DrawDepth0();
        }

        public override void DrawDepth1()
        {
            _planetSkyDome.DrawDepth1();
            Terra.DrawDepth1();
            _weatherMng.DrawDepth1();
        }

        public override void DrawDepth2()
        {
            _planetSkyDome.DrawDepth2();
            Terra.DrawDepth2();
            _weatherMng.DrawDepth2();
        }
        #endregion

        #region Private methods
        #endregion

        public string GetInfo()
        {
            return Terra.GetInfo();
        }
    }
}
