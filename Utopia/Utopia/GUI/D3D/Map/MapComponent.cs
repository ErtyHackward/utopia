using System.Drawing;
using System.Drawing.Imaging;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.D3D;
using Utopia.Action;
using Utopia.Entities.Managers;
using Utopia.Network;
using Utopia.Shared.World.PlanGenerator;
using S33M3Engines;

namespace Utopia.GUI.D3D.Map
{
    /// <summary>
    /// Control that shows current map and player current position
    /// </summary>
    public class MapComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly ActionsManager _actionManager;
        private readonly Screen _screen;
        private readonly Server _server;
        private readonly WindowControl _mapWindow;
        private readonly WorldPlan _planGenerator;
        private readonly PlayerEntityManager _playerManager;
        private Bitmap _mapImage;
        private MapControl _mapControl;

        public MapComponent(D3DEngine engine, ActionsManager actionManager, Screen screen, Server server, WorldPlan plan,PlayerEntityManager playerManager)
        {
            _playerManager = playerManager;
            _engine = engine;
            _actionManager = actionManager;
            _screen = screen;
            _server = server;

            _mapWindow = new WindowControl();
            _mapWindow.Name = "Map";
            _mapWindow.Title = "World map";
            _mapWindow.Bounds = new UniRectangle(100, 100, 711, 400);
            var innerBounds = new UniRectangle(4, 24, _mapWindow.Bounds.Size.X - 4- 3, _mapWindow.Bounds.Size.Y - 24 -3);

            _planGenerator = plan;
            _playerManager = playerManager;
            _planGenerator.Parameters = _server.GameInformations.PlanGenerationParameters;

            var playerMarker = new Bitmap(16, 16);

            using (var g = Graphics.FromImage(playerMarker))
            {
                g.DrawLine(Pens.Black, 0, 8, 16, 8);
                g.DrawLine(Pens.Black, 8, 0, 8, 16);
            }


            // TODO: need to make it async
            _planGenerator.Generate();
            _mapImage = _planGenerator.Render();
            _mapControl = new MapControl
                              {
                                  MarkerPosition = new Point(),
                                  PlayerMarker = new S33M3Engines.Shared.Sprites.SpriteTexture(_engine.Device, playerMarker, new SharpDX.Vector2(), SharpDX.DXGI.Format.B8G8R8A8_UNorm),
                                  MapTexture = new S33M3Engines.Shared.Sprites.SpriteTexture(_engine.Device, _mapImage, new SharpDX.Vector2(), SharpDX.DXGI.Format.B8G8R8A8_UNorm),
                                  Bounds = innerBounds
                              };

            _mapWindow.Children.Add(_mapControl);

            _mapImage.Save("map.png", ImageFormat.Png);
        }



        public override void Update(ref GameTime timeSpent)
        {
            _mapControl.MarkerPosition = new Point((int)_playerManager.Player.Position.X, (int)_playerManager.Player.Position.Z);

            if (_actionManager.isTriggered(Actions.OpenMap))
            {
                if (_screen.Desktop.Children.Contains(_mapWindow))
                    _screen.Desktop.Children.Remove(_mapWindow);
                else 
                    _screen.Desktop.Children.Add(_mapWindow);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

}
