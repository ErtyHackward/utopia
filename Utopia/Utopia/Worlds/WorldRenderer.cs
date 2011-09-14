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

        public WorldRenderer(IWorld world, UtopiaRender mainRenderer)
        {
            _world = world;

            //Register the various passed in component to be rendered
            mainRenderer.GameComponents.Add(_world.WorldClock);
            mainRenderer.GameComponents.Add(_world.WorldWeather);
            mainRenderer.GameComponents.Add(_world.WorldSkyDome);
            mainRenderer.GameComponents.Add(_world.WorldChunks);
        }

        #region Public methods
        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
        }

        public override void UnloadContent()
        {
        }

        public override void Update(ref GameTime TimeSpend)
        {
            //_world.WorldClock.Update(ref TimeSpend);
            //_world.WorldWeather.Update(ref TimeSpend);
            //_world.WorldSkyDome.Update(ref TimeSpend);
            //_world.WorldChunks.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            //_world.WorldClock.Interpolation(ref interpolation_hd, ref interpolation_ld);
            //_world.WorldWeather.Interpolation(ref interpolation_hd, ref interpolation_ld);
            //_world.WorldSkyDome.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override void Draw(int Index)
        {
            //_world.WorldChunks.Draw();
            //_world.WorldSkyDome.Draw();
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
