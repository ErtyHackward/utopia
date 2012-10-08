using System.Drawing;
using Utopia.Entities.Managers;
using Utopia.Network;
using Utopia.Shared.World.PlanGenerator;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Sprites;
using SharpDX.Direct3D11;
using Utopia.Action;
using S33M3CoreComponents.Inputs;

namespace Utopia.GUI.Map
{
    /// <summary>
    /// Component that shows current map and player current position
    /// </summary>
    public class MapComponent : GameComponent
    {
        private readonly D3DEngine _engine;
        private readonly InputsManager _inputManager;
        private readonly MainScreen _screen;
        private readonly ServerComponent _server;
        private readonly WindowControl _mapWindow;
        private readonly WorldPlan _planGenerator;
        private readonly PlayerEntityManager _playerManager;
        private Bitmap _mapImage;
        private MapControl _mapControl;

        public MapComponent(D3DEngine engine, InputsManager inputManager, MainScreen screen, ServerComponent server, WorldPlan plan, PlayerEntityManager playerManager)
        {
            this.IsDefferedLoadContent = true;

            _playerManager = playerManager;
            _engine = engine;
            _inputManager = inputManager;
            _screen = screen;
            _server = server;

            _mapWindow = new WindowControl();
            _mapWindow.Name = "Map";
            _mapWindow.Title = "World map";
            _mapWindow.Bounds = new UniRectangle(100, 100, 711, 400);
            

            _planGenerator = plan;
            _playerManager = playerManager;
            //_planGenerator.Parameters = _server.GameInformations.PlanGenerationParameters;
        }

        public override void Initialize()
        {
            // TODO: need to make it async
            _planGenerator.Generate();
            _mapImage = _planGenerator.Render();
        }

        public override void LoadContent(DeviceContext context)
        {
            if (_mapImage == null) return;

            var playerMarker = new Bitmap(16, 16);

            using (var g = Graphics.FromImage(playerMarker))
            {
                g.DrawLine(Pens.Black, 0, 8, 16, 8);
                g.DrawLine(Pens.Black, 8, 0, 8, 16);
            }

            var innerBounds = new UniRectangle(4, 24, _mapWindow.Bounds.Size.X - 4 - 3, _mapWindow.Bounds.Size.Y - 24 - 3);
            _mapControl = new MapControl
            {
                MarkerPosition = new Point(),
                PlayerMarker = ToDispose(new SpriteTexture(_engine.Device, playerMarker, new SharpDX.Vector2(), _engine.IsB8G8R8A8_UNormSupport)),
                MapTexture = ToDispose(new SpriteTexture(_engine.Device, _mapImage, new SharpDX.Vector2(), _engine.IsB8G8R8A8_UNormSupport)),
                Bounds = innerBounds
            };

            _mapWindow.Children.Add(_mapControl);
        }


        public override void Update(GameTime timeSpend)
        {
            if (_mapImage == null) return;

            _mapControl.MarkerPosition = new Point((int)_playerManager.Player.Position.X, (int)_playerManager.Player.Position.Z);

            if (_inputManager.ActionsManager.isTriggered(UtopiaActions.OpenMap))
            {
                if (_screen.Desktop.Children.Contains(_mapWindow))
                    _screen.Desktop.Children.Remove(_mapWindow);
                else 
                    _screen.Desktop.Children.Add(_mapWindow);
            }
        }
    }

}
