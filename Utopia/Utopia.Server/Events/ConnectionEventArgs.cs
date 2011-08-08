using System;
using Utopia.Net.Connections;

namespace Utopia.Server
{
    public class ConnectionEventArgs : EventArgs
    {
        public ClientConnection Connection { get; set; }
    }
}