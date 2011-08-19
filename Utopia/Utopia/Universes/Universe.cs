using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.D3D;
using Utopia.GameClock;
using Utopia.Entities.Living;
using Utopia.Planets;
using S33M3Engines.Struct;
using Utopia.USM;
using Utopia.Shared.Structs;
using Utopia.Shared.Landscaping;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;

namespace Utopia.Univers
{
    public class Universe : GameComponent, IDebugInfo
    {
        #region Private variables
        public Planet Planet;

        private Clock _gameClock;
        private ILivingEntity _player;

        //World Seed (Used to generate the Planet)
        private string _name;
        #endregion

        #region Public Properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        #endregion

        public Universe(D3DEngine d3dEngine, CameraManager camManager, WorldFocusManager worldFocusManager, GameStatesManager gameStates , LandscapeBuilder landscapeBuilder, Clock gameClock, ILivingEntity player, string Name)
        {
            _name = Name;
            _gameClock = gameClock;
            _player = player;

            //Main terrain creation
            Planet = new Planet(d3dEngine, camManager, worldFocusManager, gameStates,landscapeBuilder , _gameClock, _player, LandscapeBuilder.Seed, new Location3<int>(0, 0, 0));
            //UtopiaSaveManager.ChangePlanet(Planet.Planetnfo);
        }

        #region Public Methods
        public override void Initialize()
        {
            Planet.Initialize();
        }

        public override void LoadContent()
        {
            Planet.LoadContent();
        }

        public override void UnloadContent()
        {
            Planet.UnloadContent();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            Planet.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            Planet.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override void DrawDepth0()
        {
            Planet.DrawDepth0();
        }

        public override void DrawDepth1()
        {
            Planet.DrawDepth1();
        }

        public override void DrawDepth2()
        {
            Planet.DrawDepth2();
        }
        #endregion

        #region Private methods
        #endregion

        public string GetInfo()
        {
            return Planet.GetInfo();
        }
    }
}
