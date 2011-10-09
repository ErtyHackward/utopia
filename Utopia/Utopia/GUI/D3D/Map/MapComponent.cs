using System.Drawing;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using S33M3Engines.D3D;
using SharpDX.Direct3D11;
using Utopia.Action;
using Utopia.Network;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.World.PlanGenerator;

namespace Utopia.GUI.D3D.Map
{
    /// <summary>
    /// Control that shows current map and player current position
    /// </summary>
    public class MapComponent : GameComponent
    {
        private readonly Device _device;
        private readonly ActionsManager _actionManager;
        private readonly Screen _screen;
        private readonly Server _server;
        private readonly WindowControl _mapWindow;
        private readonly WorldPlan _planGenerator;
        private Bitmap _mapImage;

        public MapComponent(Device device, ActionsManager actionManager, Screen screen, Server server)
        {
            _device = device;
            _actionManager = actionManager;
            _screen = screen;
            _server = server;

            _mapWindow = new WindowControl();
            _mapWindow.Name = "Map";
            _mapWindow.Title = "World map";
            _mapWindow.Bounds = new UniRectangle(100, 100, 300, 200);
            
            
            

            _server.ServerConnection.MessageGameInformation += ServerConnectionMessageGameInformation;

            _planGenerator = new WorldPlan();
        }

        void ServerConnectionMessageGameInformation(object sender, ProtocolMessageEventArgs<GameInformationMessage> e)
        {
            _planGenerator.Parameters = e.Message.PlanGenerationParameters;

            // TODO: need to make it async
            _planGenerator.Generate();
            _mapImage = _planGenerator.Render();
            _mapWindow.Children.Add(new MapControl() { MapTexture = new S33M3Engines.Shared.Sprites.SpriteTexture(_device, _mapImage, new SharpDX.Vector2()),  Bounds = _mapWindow.Bounds});
        }

        public override void Update(ref GameTime timeSpent)
        {
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
            _server.ServerConnection.MessageGameInformation -= ServerConnectionMessageGameInformation;
            
            base.Dispose();
        }
    }

}
