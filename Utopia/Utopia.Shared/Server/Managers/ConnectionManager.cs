using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Events;

namespace Utopia.Shared.Server.Managers
{
    /// <summary>
    /// Provides server connection management
    /// </summary>
    public class ConnectionManager : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, ClientConnection> _connections;
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Get or sets server mode, Local mode allows to accept only local connections
        /// </summary>
        public bool LocalMode { get; set; }

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
        /// Occurs before connection is removed from connection manager
        /// </summary>
        public event EventHandler<ConnectionEventArgs> BeforeConnectionRemoved;

        protected void OnBeforeConnectionRemoved(ConnectionEventArgs e)
        {
            var handler = BeforeConnectionRemoved;
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
            logger.Info("Listening at {0} port", Listener.Port);
        }

        void ListenerIncomingConnection(object sender, IncomingConnectionEventArgs e)
        {
            var ipEndPoint = (IPEndPoint)e.Socket.RemoteEndPoint;

            if (LocalMode && !IPAddress.IsLoopback(ipEndPoint.Address))
            {
                e.Socket.Dispose();
                return;
            }
            
            var conn = new ClientConnection(e.Socket);
            
            logger.Info("{0} connected", e.Socket.RemoteEndPoint);

            e.Handled = Add(conn);

            conn.Listen();

            if (!e.Handled)
            {
                logger.Warn("Disconnecting non handled connection {0}", e.Socket.RemoteEndPoint);
                conn.Dispose();
            }
        }

        void ConnectionMessagePing(object sender, ProtocolMessageEventArgs<PingMessage> e)
        {
            var connection = (ClientConnection)sender;
            // we need respond as fast as possible
            if (e.Message.Request)
            {
                var msg = e.Message;
                msg.Request = false;
                connection.Send(msg);
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
                        connection.Send(message);
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

            connection.StatusChanged += ConnectionConnectionStatusChanged;
            return true;
        }

        void ConnectionConnectionStatusChanged(object sender, TcpConnectionStatusEventArgs e)
        {
            var connection = sender as ClientConnection;

            if (e.Status == TcpConnectionStatus.Disconnected)
            {
                var ea = new ConnectionEventArgs { Connection = connection };
                OnBeforeConnectionRemoved(ea);
                lock (_syncRoot)
                {
                    if (connection != null) _connections.Remove(connection.Id);
                }
                OnConnectionRemoved(ea);
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

                var disposeList = _connections.Values.ToList();

                foreach (var connection in disposeList)
                {
                    connection.Dispose();
                }
            }

            while (_connections.Count > 0)
                Thread.Yield();
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

        public IEnumerable<ClientConnection> Connections()
        {
            lock (_syncRoot)
            {
                foreach (var connection in _connections.Values)
                {
                    yield return connection;
                }
            }
        }
    }
}
