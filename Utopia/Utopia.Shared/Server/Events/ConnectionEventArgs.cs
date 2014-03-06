using System;
using Utopia.Shared.Net.Connections;

namespace Utopia.Shared.Server.Events
{
    public interface IConnectionEventArgs
    {
        ClientConnection Connection { get; set; }
    }

    public class ConnectionEventArgs : EventArgs, IConnectionEventArgs
    {
        public ClientConnection Connection { get; set; }
    }
}