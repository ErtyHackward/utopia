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
using S33M3CoreComponents.GUI.Nuclex.Controls;

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
        private LabelControl _dateTime;

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
                         MaskArrow = ToDispose(new SpriteTexture(_engine.Device, ClientSettings.TexturePack + @"Gui\MaskArrow.png", Vector2I.Zero, imageLoadParam)),
                         SoulStoneIcon = ToDispose(new SpriteTexture(_engine.Device, ClientSettings.TexturePack + @"Gui\SoulStoneIcon.png", Vector2I.Zero, imageLoadParam)),
                         sampler = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipLinear),                         
                     };

             _dateTime = new LabelControl() { Color = Color.Tomato, Text = "", Bounds = new UniRectangle(-50, -5, new UniScalar(1.0f, 0.0f), 20.0f) };
             _compassPanel.Children.Add(_dateTime);

            _compassPanel.LayoutFlags = S33M3CoreComponents.GUI.Nuclex.Controls.ControlLayoutFlags.Skip;
            _guiScreen.Desktop.Children.Add(_compassPanel);
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            double yaw;
            if (getLookAtYaw(_playerManager.EntityRotations.LookAt, out yaw))
            {
                _compassPanel.Rotation = (float)yaw;
            }

            var currentDateTime = _worldclock.Now;
            string currentDay = currentDateTime.Day.ToString();
            if (currentDateTime.Day == 1) currentDay += "st";
            else if (currentDateTime.Day == 2) currentDay += "nd";
            else if (currentDateTime.Day == 3) currentDay += "rd";
            else currentDay += "th";
            var dateStr = string.Format("{0} of {1} from Year {2}", currentDay, currentDateTime.Season, currentDateTime.Year);
            _dateTime.Text = dateStr;
            
            _compassPanel.RotationDayCycle = (_worldclock.ClockTime.ClockTimeNormalized) * MathHelper.TwoPi + MathHelper.Pi;

            if(_playerManager.Player.BindedSoulStone != null){
                Vector2 playerLookAtXZ = new Vector2(_playerManager.EntityRotations.LookAt.X, _playerManager.EntityRotations.LookAt.Z);

                Vector2 playerPosition = new Vector2((float)_playerManager.Player.Position.X, (float)_playerManager.Player.Position.Z);

                Vector2 SoulStoneLocation = new Vector2((float)_playerManager.Player.BindedSoulStone.Position.X, (float)_playerManager.Player.BindedSoulStone.Position.Z);

                Vector2 PlayerSoulStoneVector = playerPosition - SoulStoneLocation;
                PlayerSoulStoneVector.Normalize();

                float dotResult;
                Vector2.Dot(ref PlayerSoulStoneVector, ref playerLookAtXZ, out dotResult);
                dotResult *= -1;
                if (dotResult < 0)
                {
                    dotResult = 0;
                }
                _compassPanel.SoulStoneFacing = dotResult;
            }
            else _compassPanel.SoulStoneFacing = 0.0f; 

        }

        private static bool getLookAtYaw(Vector3 vector, out double yaw)
        {
            double dx = vector.X;
            double dz = vector.Z;
            yaw = 0;
            // Set yaw
            if (Math.Abs(dx) > 0.0001)
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
                return true;
            }
            return false;
        }

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
