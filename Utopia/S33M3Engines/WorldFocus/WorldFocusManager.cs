using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace S33M3Engines.WorldFocus
{
    //Wrapper arround the world focus
    public class WorldFocusManager
    {
        #region Private Variable
        private IWorldFocus _worldFocus;
        #endregion

        #region Public Variables
        public IWorldFocus WorldFocus
        {
            get { return _worldFocus; }
            set { worldFocusChange(value); }
        }

        public delegate void WorldFocusChanged(IWorldFocus NewWorldFocus);
        public event WorldFocusChanged OnWorldFocus_Changed;

        #endregion

        public WorldFocusManager()
        {
        }

        #region Public methods
        //Apply matrix translation based on the world focus 
        // == Apply the inverse of the world focus location.
        public void CenterOnFocus(ref Matrix WorldMatrix, ref Matrix WorldFocusedMatrix)
        {
            WorldFocusedMatrix.M41 = WorldMatrix.M41 - (float)_worldFocus.FocusPoint.ActualValue.X;
            WorldFocusedMatrix.M42 = WorldMatrix.M42 - (float)_worldFocus.FocusPoint.ActualValue.Y;
            WorldFocusedMatrix.M43 = WorldMatrix.M43 - (float)_worldFocus.FocusPoint.ActualValue.Z;
        }
        #endregion

        #region Private methods
        private void worldFocusChange(IWorldFocus worldFocus)
        {
            _worldFocus = worldFocus;
            if (OnWorldFocus_Changed != null) OnWorldFocus_Changed(worldFocus);
        }
        #endregion
    }
}
