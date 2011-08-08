using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using Utopia.Planets.Weather.Items;
using Utopia.GameClock;
using Utopia.Planets.Terran;

namespace Utopia.Planets.Weather
{
    public class WeatherManager : GameComponent, IDebugInfo
    {
        #region Private Properties
        IDrawableComponent _clouds;
        IUpdatableComponent _wind;
        Clock _gameClock;
        TerraWorld _terraworld;
        #endregion

        #region Public Properties
        #endregion

        public WeatherManager(Game game, Clock gameClock, TerraWorld terraworld)
            : base(game)
        {
            _gameClock = gameClock;
            _terraworld = terraworld;
        }

        #region public method

        public override void Initialize()
        {
            _wind = new Wind();
            _wind.Initialize();
            _clouds = new Clouds(this.Game, _terraworld, _wind as Wind);
            _clouds.Initialize();
        }

        public override void LoadContent()
        {
        }

        public override void UnloadContent()
        {
            _clouds.Dispose();
            _wind.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            _wind.Update(ref TimeSpend);
            _clouds.Update(ref TimeSpend);
        }

        public override void DrawDepth0()
        {
        }

        public override void DrawDepth1()
        {
        }

        public override void DrawDepth2()
        {
            _clouds.Draw();
        }
        #endregion

        #region private method
        #endregion

        #region GetInfo Interface
        public string GetInfo()
        {
            return "";
        }
        #endregion
    }
}
