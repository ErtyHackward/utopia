using System;
using Utopia.Net.Connections;

namespace Utopia.Server
{
    public class PlayerCommandEventArgs : EventArgs
    {
        public ClientConnection Connection { get; set; }
        public string Command { get; set; }
    }
}