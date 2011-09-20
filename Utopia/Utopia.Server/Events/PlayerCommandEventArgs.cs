using System;

namespace Utopia.Server.Events
{
    public class PlayerCommandEventArgs : EventArgs
    {
        public ClientConnection Connection { get; set; }
        public string Command { get; set; }
    }
}