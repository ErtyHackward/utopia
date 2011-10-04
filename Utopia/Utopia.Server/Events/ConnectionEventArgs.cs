using System;

namespace Utopia.Server
{
    public class ConnectionEventArgs : EventArgs
    {
        public ClientConnection Connection { get; set; }
    }
}