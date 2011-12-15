using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;

namespace Utopia.Server.Managers
{

    public class ChatManager
    {
        private readonly Server _server;

        public string ServerName { get; set; }

        public ChatManager(Server server)
        {
            _server = server;
            _server.ConnectionManager.ConnectionAdded += ConnectionManagerConnectionAdded;
            _server.ConnectionManager.ConnectionRemoved += ConnectionManagerConnectionRemoved;

            ServerName = "server";
        }

        private void ConnectionManagerConnectionAdded(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessageChat += ConnectionMessageChat;
        }

        private void ConnectionManagerConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            e.Connection.MessageChat -= ConnectionMessageChat;
        }

        private void ConnectionMessageChat(object sender, ProtocolMessageEventArgs<ChatMessage> e)
        {
            var connection = (ClientConnection)sender;
            if (e.Message.DisplayName == connection.DisplayName)
            {
                var msg = e.Message.Message;

                if (string.IsNullOrWhiteSpace(msg))
                    return;

                if (_server.CommandsManager.TryExecute(connection, msg))
                    return;

                _server.ConnectionManager.Broadcast(e.Message);
            }
        }

        public void SendMessage(ClientConnection connection, string message)
        {
            SendMessage(connection, message, ServerName);
        }

        public void SendMessage(ClientConnection connection, string message, string nick)
        {
            SendMessage(connection, message, nick, true);
        }

        public void SendMessage(ClientConnection connection, string message, string nick, bool isOperator)
        {
            connection.SendAsync(new ChatMessage { DisplayName = nick, Message = message, Operator = isOperator });
        }

        public void Broadcast(string message)
        {
            Broadcast(ServerName, message);
        }

        public void Broadcast(string message, string nick)
        {
            Broadcast(nick, message, true);
        }

        public void Broadcast(string message, string nick, bool isOperator)
        {
            _server.ConnectionManager.Broadcast(new ChatMessage { DisplayName = nick, Message = message, Operator = isOperator });
        }
    }
}
