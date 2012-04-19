using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main.Interfaces;
using S33M3DXEngine;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using Utopia.Worlds.Weather;
using Utopia.Shared.World;
using S33M3CoreComponents.WorldFocus;
using Utopia.Worlds.GameClocks;
using Utopia.Components;
using S33M3CoreComponents.Maths.Noises;
using S33M3DXEngine.Buffers;
using S33M3Resources.VertexFormats;
using UtopiaContent.Effects.Weather;
using S33M3Resources.Structs;
using Utopia.Effects.Shared;
using SharpDX;
using S33M3DXEngine.Main;
using SharpDX.Direct3D11;

namespace Utopia.Worlds.SkyDomes.SharedComp
{
    public partial class Clouds : DrawableGameComponent
    {
        #region Private variables
        //All type of clouds input Parameters
        private D3DEngine _d3dEngine;
        private VisualWorldParameters _worldParam;
        private IWeather _weather;
        private CameraManager<ICameraFocused> _camManager;
        private WorldFocusManager _worldFocusManager;
        private IClock _worldclock;
        private StaggingBackBuffer _solidBackBuffer;
        private CloudType _cloudsType;
        #endregion

        #region Public variables/properties
        public CloudType CloudsType
        {
            get { return _cloudsType; }
            set { _cloudsType = value; }
        }
        #endregion

        public Clouds(D3DEngine d3dEngine, CameraManager<ICameraFocused> camManager, IWeather weather, VisualWorldParameters worldParam, WorldFocusManager worldFocusManager, IClock worldclock, StaggingBackBuffer solidBackBuffer, CloudType cloudType)
        {
            //this.IsDefferedLoadContent = true;
            _d3dEngine = d3dEngine;
            _worldParam = worldParam;
            _weather = weather;
            _camManager = camManager;
            _worldclock = worldclock;
            _worldFocusManager = worldFocusManager;
            _solidBackBuffer = solidBackBuffer;
            _cloudsType = cloudType;
        }

        #region Private methods
        #endregion

        #region Public methods
        public override void Initialize()
        {
            Initialize2D();
            Initialize3D();
        }

        public override void LoadContent(DeviceContext context)
        {
            LoadContent2D(context);
        }

        public override void Update(GameTime timeSpent)
        {
            switch (_cloudsType)
            {
                case CloudType.None:
                    break;
                case CloudType.Cloud2D:
                    Update2D(timeSpent);
                    break;
                case CloudType.Cloud3D:
                    Update3D(timeSpent);
                    break;
                default:
                    break;
            }
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long elapsedTime)
        {
            switch (_cloudsType)
            {
                case CloudType.None:
                    break;
                case CloudType.Cloud2D:
                    Interpolation2D(interpolationHd, interpolationLd, elapsedTime);
                    break;
                case CloudType.Cloud3D:
                    Interpolation3D(interpolationHd, interpolationLd, elapsedTime);
                    break;
                default:
                    break;
            }
        }

        public override void Draw(DeviceContext context, int index)
        {
            switch (_cloudsType)
            {
                case CloudType.None:
                    break;
                case CloudType.Cloud2D:
                    Draw2D(context, index);
                    break;
                case CloudType.Cloud3D:
                    Draw3D(context, index);
                    break;
                default:
                    break;
            }
        }

        public override void BeforeDispose()
        {
            BeforeDispose2D();
            BeforeDispose3D();
        }
        #endregion

        public enum CloudType
        {
            None,
            Cloud2D,
            Cloud3D
        }
    }
}
