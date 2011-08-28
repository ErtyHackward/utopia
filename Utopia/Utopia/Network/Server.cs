using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Net.Connections;
using Utopia.Shared.ClassExt;
using S33M3Engines.D3D;

namespace Utopia.Network
{
    public class Server : GameComponent
    {
        #region Private variables
        #endregion

        #region Public properties/variables
        public string Address { get; set; }
        public int Port { get; set; }
        public ServerConnection ServerConnection { get; set; }
        #endregion

        public Server()
        {
            this.CallDraw = false; //Disable Draw calls
        }

        public void BindingServer(string address, int port)
        {
            if (Address == address && Port == port) return;
            if (ServerConnection != null && ServerConnection.ConnectionStatus == ConnectionStatus.Connected) ServerConnection.Disconnect();

            Address = address;
            Port = port;

            ServerConnection = new ServerConnection(address, port);
            //Register Login Events
            ServerConnection.MessageLoginResult += _server_MessageLoginResult;
            ServerConnection.ConnectionStatusChanged += _server_ConnectionStatusChanged;
            ServerConnection.MessageBlockChange += _server_MessageBlockChange;
            ServerConnection.MessageChat += _server_MessageChat;
            ServerConnection.MessageChunkData += _server_MessageChunkData;
            ServerConnection.MessageDateTime += _server_MessageDateTime;
            ServerConnection.MessageDirection += _server_MessageDirection;
            ServerConnection.MessageError += _server_MessageError;
            ServerConnection.MessageGameInformation += _server_MessageGameInformation;
            ServerConnection.MessagePlayerIn += _server_MessagePlayerIn;
            ServerConnection.MessagePlayerOut += _server_MessagePlayerOut;
            ServerConnection.MessagePosition += _server_MessagePosition;
        }

        #region Events Handlings Methods
        void _server_MessageLoginResult(object sender, ProtocolMessageEventArgs<Net.Messages.LoginResultMessage> e)
        {
            if (e.Message.Logged)
            {
                Console.WriteLine("I'm Logged");
            }
        }

        void _server_MessagePosition(object sender, ProtocolMessageEventArgs<Net.Messages.PlayerPositionMessage> e)
        {
            throw new NotImplementedException();
        }

        void _server_MessagePlayerOut(object sender, ProtocolMessageEventArgs<Net.Messages.PlayerOutMessage> e)
        {
            throw new NotImplementedException();
        }

        void _server_MessagePlayerIn(object sender, ProtocolMessageEventArgs<Net.Messages.PlayerInMessage> e)
        {
            Console.WriteLine("_server_MessagePlayerIn : " + e.Message.Login);
        }

        void _server_MessageGameInformation(object sender, ProtocolMessageEventArgs<Net.Messages.GameInformationMessage> e)
        {
            Console.WriteLine("_server_MessageGameInformation : " + e.Message.ChunkSize.ToString());
        }

        void _server_MessageError(object sender, ProtocolMessageEventArgs<Net.Messages.ErrorMessage> e)
        {
            Console.WriteLine("_server_MessageGameInformation : " + e.Message.Message.ToString());
        }

        void _server_MessageDirection(object sender, ProtocolMessageEventArgs<Net.Messages.PlayerDirectionMessage> e)
        {
            throw new NotImplementedException();
        }

        void _server_MessageDateTime(object sender, ProtocolMessageEventArgs<Net.Messages.DateTimeMessage> e)
        {
            throw new NotImplementedException();
        }

        void _server_MessageChunkData(object sender, ProtocolMessageEventArgs<Net.Messages.ChunkDataMessage> e)
        {
            throw new NotImplementedException();
        }

        void _server_MessageChat(object sender, ProtocolMessageEventArgs<Net.Messages.ChatMessage> e)
        {
            throw new NotImplementedException();
        }

        void _server_MessageBlockChange(object sender, ProtocolMessageEventArgs<Net.Messages.BlockChangeMessage> e)
        {
            throw new NotImplementedException();
        }

        void _server_ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            Console.WriteLine("_server_ConnectionStatusChanged : " + e.Reason + " status : " + e.Status);
            //throw new NotImplementedException();
        }

        #endregion

        #region Public Methods
        public void ConnectToServer(string UserName, string Password, bool withRegistering)
        {
            ServerConnection.Login = UserName;
            ServerConnection.Password = Password.GetMd5Hash();
            ServerConnection.ClientVersion = 1;
            ServerConnection.Register = withRegistering;

            if (ServerConnection.ConnectionStatus != ConnectionStatus.Connected)
            {
                ServerConnection.ConnectAsync(new AsyncCallback(ConnectAsyncCallBack), null);
            }
            else
            {
                ServerConnection.Authenticate();
            }
        }

        private bool oneShot = true;
        public override void Update(ref GameTime TimeSpend)
        {
            ServerConnection.FetchPendingMessages(1);

            //Ask once a chunks via the server ! ==> Testing !
            if(oneShot)
            {
            ServerConnection.SendAsync(new Utopia.Net.Messages.GetChunksMessage() { StartPosition = new Shared.Structs.IntVector2(0,0), EndPosition = new Shared.Structs.IntVector2(32,0), Flag = Net.Messages.GetChunksMessageFlag.AlwaysSendChunkData });
                oneShot = false;
            }

        }

        #endregion

        #region Private Methods
        private void ConnectAsyncCallBack(IAsyncResult result)
        {
            ServerConnection.FetchPendingMessages(1);
        }
        #endregion
    }
}
