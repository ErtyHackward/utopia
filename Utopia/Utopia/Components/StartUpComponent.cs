using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using System.IO;

namespace Utopia.Components
{
    public class StartUpComponent : DrawableGameComponent
    {
        #region Private variables
        private FileInfo[] _imagesSlideShowPath;
        private int _slideShowDelayInMS;
        private bool _slidShowFinished;
        #endregion

        #region Public variables/Properties
        #endregion

        #region Event
        public event EventHandler SlideShowFinished;
        #endregion
        public StartUpComponent()
        {
        }

        public void SetSlideShows(FileInfo[] imagesSlideShowPath, int slideShowDelayInMS)
        {
            _imagesSlideShowPath = imagesSlideShowPath;
            _slideShowDelayInMS = slideShowDelayInMS;
        }
        #region Public methods
        public override void Initialize()
        {
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
        }

        public override void Update(GameTime timeSpent)
        {
            if (_slidShowFinished == false)
            {
                if (SlideShowFinished != null) SlideShowFinished(this, null);
            }
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
        }
        #endregion

        #region Private methods
        #endregion
    }
}
