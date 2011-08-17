using System;
using System.Collections.Generic;
using Utopia.Net.Connections;
using Utopia.Net.Interfaces;

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

        protected void OnConnectionAdded(ConnectionEventArgs ea)
        {
            if (ConnectionAdded != null)
                ConnectionAdded(this, ea);
        }

        /// <summary>
        /// Occurs when connection is removed from connection manager
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionRemoved;

        protected void OnConnectionRemoved(ConnectionEventArgs ea)
        {
            if (ConnectionRemoved != null)
                ConnectionRemoved(this, ea);
        }

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
        /// Disposes all contained collection
        /// </summary>
        public void Dispose()
        {
            lock (_syncRoot)
            {
                foreach (var connection in _connections.Values)
                {
                    connection.BeginDispose();
                }
            }
        }
    }
}
