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
        /// <summary>
        /// Function used to center a Translation ONLY world matrix around the focus world point.
        /// </summary>
        /// <param name="WorldTranslationMatrix"></param>
        /// <param name="WorldFocusedMatrix"></param>
        public void CenterTranslationMatrixOnFocus(ref Matrix WorldTranslationMatrix, ref Matrix WorldFocusedMatrix)
        {
            WorldFocusedMatrix.M41 = WorldTranslationMatrix.M41 - (float)_worldFocus.FocusPoint.ValueInterp.X;
            WorldFocusedMatrix.M42 = WorldTranslationMatrix.M42 - (float)_worldFocus.FocusPoint.ValueInterp.Y;
            WorldFocusedMatrix.M43 = WorldTranslationMatrix.M43 - (float)_worldFocus.FocusPoint.ValueInterp.Z;
        }

        /// <summary>
        /// Function used to center a world matrix around the focus world point.
        /// </summary>
        /// <param name="WorldTranslationMatrix"></param>
        /// <param name="WorldFocusedMatrix"></param>
        public Matrix CenterOnFocus(ref Matrix WorldMatrix)
        {
            return WorldMatrix * _worldFocus.FocusPointMatrix.ValueInterp;
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
