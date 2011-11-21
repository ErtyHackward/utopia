using System;
using Utopia.Server.Structs;

namespace Utopia.Server.Events
{
    public class PlayerCommandEventArgs : EventArgs
    {
        public ClientConnection Connection { get; set; }
        public IServerCommand Command { get; set; }
        public string[] Params { get; set; }

        public bool HaveParameters
        {
            get { return Params != null; }
        }
    }
}