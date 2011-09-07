using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using Utopia.Worlds.Chunks;
using Utopia.Network;

namespace Utopia.Worlds
{
    /// <summary>
    /// Is responsible to render the world binded to it !
    /// </summary>
    public class WorldRenderer : DrawableGameComponent, IDebugInfo
    {
        #region Private variables
        private IWorld _world;
        #endregion

        #region Public Properties/Variables
        public IWorld World
        {
            get { return _world; }
            set { _world = value; }
        }
        #endregion

        public WorldRenderer(IWorld world)
        {
            _world = world;
        }

        #region Public methods
        public override void Initialize()
        {
            _world.WorldClock.Initialize();
            _world.WorldWeather.Initialize();
            _world.WorldSkyDome.Initialize();
            _world.WorldChunks.Initialize();
        }

        public override void LoadContent()
        {
        }

        public override void UnloadContent()
        {
            //They have not been created here, so no need to dispose them !
            //_world.WorldClock.Dispose();
            //_world.WorldWeather.Dispose();
            //_world.WorldSkyDome.Dispose();
            //_world.WorldChunks.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            _world.WorldClock.Update(ref TimeSpend);
            _world.WorldWeather.Update(ref TimeSpend);
            _world.WorldSkyDome.Update(ref TimeSpend);
            _world.WorldChunks.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _world.WorldClock.Interpolation(ref interpolation_hd, ref interpolation_ld);
            _world.WorldWeather.Interpolation(ref interpolation_hd, ref interpolation_ld);
            _world.WorldSkyDome.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override void Draw()
        {
            _world.WorldChunks.Draw();
            _world.WorldSkyDome.Draw();
        }

        #endregion

        #region Private methods
        #endregion


        public string GetInfo()
        {
            throw new NotImplementedException();
        }
    }
}
