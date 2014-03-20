using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using S33M3CoreComponents.Maths;
using Utopia.Worlds.GameClocks;

namespace Utopia.GUI.WindRose
{
    public class WindRoseComponent : GameComponent
    {
        #region Private Variables
        private readonly MainScreen _guiScreen;
        private readonly D3DEngine _engine;
        private readonly IPlayerManager _playerManager;
        private readonly IClock _worldclock;

        private CompassControl _compassPanel;

        #endregion

        #region Public Properties
        #endregion

        public WindRoseComponent(MainScreen guiScreen, D3DEngine engine, IPlayerManager playerManager, IClock worldclock)
        {
            _guiScreen = guiScreen;
            _engine = engine;
            _playerManager = playerManager;
            _worldclock = worldclock;
        }

        public override void Initialize()
        {            
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
             ImageLoadInformation imageLoadParam = new ImageLoadInformation()
                {
                    BindFlags = BindFlags.ShaderResource,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    MipLevels = 1
                };

             _compassPanel =
                     new CompassControl()
                     {
                         FrameName = "WindRose",
                         Bounds = new UniRectangle(new UniScalar(1.0f, -160), 10, 150, 75),
                         CompassTexture = ToDispose(new SpriteTexture(_engine.Device, ClientSettings.TexturePack + @"Gui\WindRose.png", Vector2I.Zero, imageLoadParam)),
                         DayCircle = ToDispose(new SpriteTexture(_engine.Device, ClientSettings.TexturePack + @"Gui\DayCircle.png", Vector2I.Zero, imageLoadParam)),
                         sampler = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipLinear)
                     };

            _compassPanel.LayoutFlags = S33M3CoreComponents.GUI.Nuclex.Controls.ControlLayoutFlags.Skip;
            _guiScreen.Desktop.Children.Add(_compassPanel);
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            _compassPanel.Rotation = getLookAtYaw(_playerManager.EntityRotations.LookAt);
            _compassPanel.RotationDayCycle = _worldclock.ClockTime.Time + MathHelper.Pi;
        }

        private static float getLookAtYaw(Vector3 vector)
        {
            double dx = vector.X;
            double dz = vector.Z;
            double yaw = 0;
            // Set yaw
            if (dx != 0)
            {
                // Set yaw start value based on dx
                if (dx < 0)
                {
                    yaw = 1.5 * Math.PI;
                }
                else
                {
                    yaw = 0.5 * Math.PI;
                }
                yaw -= Math.Atan(dz / dx);
            }
            else if (dz < 0)
            {
                yaw = Math.PI;
            }
            return (float)yaw;
        }

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
