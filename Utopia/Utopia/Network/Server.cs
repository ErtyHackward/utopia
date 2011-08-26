using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Net.Connections;
using Utopia.Shared.ClassExt;

namespace Utopia.Network
{
    public class Server
    {
        #region Private variables
        private ServerConnection _server;
        #endregion 

        #region Public properties/variables
        #endregion

        public Server(string address, int port)
        {
            _server = new ServerConnection(address, port);
            //Register Login Events
            _server.MessageLoginResult += _server_MessageLoginResult;
            _server.ConnectionStatusChanged += _server_ConnectionStatusChanged;
            _server.MessageBlockChange += _server_MessageBlockChange;
            _server.MessageChat += _server_MessageChat;
            _server.MessageChunkData += _server_MessageChunkData;
            _server.MessageDateTime += _server_MessageDateTime;
            _server.MessageDirection += _server_MessageDirection;
            _server.MessageError += _server_MessageError;
            _server.MessageGameInformation += _server_MessageGameInformation;
            _server.MessagePlayerIn += _server_MessagePlayerIn;
            _server.MessagePlayerOut += _server_MessagePlayerOut;
            _server.MessagePosition += _server_MessagePosition;
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
            throw new NotImplementedException();
        }

        void _server_MessageGameInformation(object sender, ProtocolMessageEventArgs<Net.Messages.GameInformationMessage> e)
        {
            throw new NotImplementedException();
        }

        void _server_MessageError(object sender, ProtocolMessageEventArgs<Net.Messages.ErrorMessage> e)
        {
            throw new NotImplementedException();
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
            _server.Login = UserName;
            _server.Password = Password.GetMd5Hash();
            _server.ClientVersion = 1;
            _server.Register = withRegistering;

            if (_server.ConnectionStatus != ConnectionStatus.Connected)
            {
                _server.ConnectAsync();
            }
            else
            {
                _server.Authenticate();
            }
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
