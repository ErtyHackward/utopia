using System;
using System.Collections.Generic;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Provides server connection management
    /// </summary>
    public class ConnectionManager : IDisposable
    {
        private readonly Dictionary<string, ClientConnection> _connections;
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Occurs when connection is added to connection manager
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionAdded;

        protected void OnConnectionAdded(ConnectionEventArgs e)
        {
            e.Connection.MessagePing += ConnectionMessagePing;
            var handler = ConnectionAdded;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when connection is removed from connection manager
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionRemoved;
        
        protected void OnConnectionRemoved(ConnectionEventArgs e)
        {
            e.Connection.MessagePing -= ConnectionMessagePing;
            var handler = ConnectionRemoved;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Gets connection listener. Allows to accept client connections
        /// </summary>
        public TcpConnectionListener Listener { get; private set; }

        /// <summary>
        /// Gets total connections count
        /// </summary>
        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public ConnectionManager()
        {
            _connections = new Dictionary<string, ClientConnection>();
        }

        public ConnectionManager(int portToListen) : this()
        {
            Listener = new TcpConnectionListener(portToListen);
            Listener.IncomingConnection += ListenerIncomingConnection;
        }

        public void Listen()
        {
            Listener.Start();
            TraceHelper.Write("Listening at {0} port", Listener.Port);
        }

        void ListenerIncomingConnection(object sender, IncomingConnectionEventArgs e)
        {
            var conn = new ClientConnection(e.Socket);

            TraceHelper.Write("{0} connected", e.Socket.RemoteEndPoint);

            e.Handled = Add(conn);

            conn.Listen();

            if (!e.Handled)
                conn.BeginDispose();
        }

        void ConnectionMessagePing(object sender, ProtocolMessageEventArgs<PingMessage> e)
        {
            var connection = (ClientConnection)sender;
            // we need respond as fast as possible
            if (e.Message.Request)
            {
                var msg = e.Message;
                msg.Request = false;
                connection.SendAsync(msg);
            }
        }

        /// <summary>
        /// Sends a message to every connected and authorized client
        /// </summary>
        /// <param name="message">a message to send</param>
        public void Broadcast(IBinaryMessage message)
        {
            lock (_syncRoot)
            {
                foreach (var connection in _connections.Values)
                {
                    if(connection.Authorized)
                        connection.SendAsync(message);
                }
            }
        }

        /// <summary>
        /// Sends a message to every connected and authorized client
        /// </summary>
        /// <param name="buffer">a byte array to send</param>
        public void Broadcast(byte[] buffer)
        {
            Broadcast(buffer, buffer.Length);
        }

        /// <summary>
        /// Sends a message to every connected and authorized client
        /// </summary>
        /// <param name="buffer">a byte array to send</param>
        /// <param name="length">length of the message</param>
        public void Broadcast(byte[] buffer, int length)
        {
            lock (_syncRoot)
            {
                foreach (var connection in _connections.Values)
                {
                    if (connection.Authorized)
                        connection.Send(buffer, length);
                }
            }
        }

        /// <summary>
        /// Adds a connection to manager
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool Add(ClientConnection connection)
        {
            lock (_syncRoot)
            {
                if (_connections.ContainsKey(connection.Id))
                    return false;

                _connections.Add(connection.Id, connection);
            }

            OnConnectionAdded(new ConnectionEventArgs { Connection = connection });

            connection.ConnectionStatusChanged += ConnectionConnectionStatusChanged;
            return true;
        }

        void ConnectionConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            var connection = sender as ClientConnection;

            if (e.Status == ConnectionStatus.Disconnected)
            {
                lock (_syncRoot)
                {
                    if (connection != null) _connections.Remove(connection.Id);
                }
                OnConnectionRemoved(new ConnectionEventArgs { Connection = connection });
            }
        }

        /// <summary>
        /// Performs an action on every authorized connection
        /// </summary>
        /// <param name="action"></param>
        public void Foreach(Action<ClientConnection> action)
        {
            lock (_syncRoot)
            {
                foreach (var connection in _connections.Values)
                {
                    if (connection.Authorized)
                        action(connection);
                }
            }
        }

        /// <summary>
        /// Disposes all contained connections
        /// </summary>
        public void Dispose()
        {
            lock (_syncRoot)
            {
                Listener.Dispose();
                foreach (var connection in _connections.Values)
                {
                    connection.BeginDispose();
                }
            }
        }

        public ClientConnection Find(Predicate<ClientConnection> condition)
        {
            lock (_syncRoot)
            {
                foreach (var connection in _connections.Values)
                {
                    if (condition(connection))
                        return connection;
                }
            }
            return null;
        }
    }
}
