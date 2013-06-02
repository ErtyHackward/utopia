using System;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Services.Interfaces;

namespace Utopia.Shared.Services
{
    public class PlayerCommandEventArgs : EventArgs
    {
        public IServerCommand Command { get; set; }
        public string[] Params { get; set; }

        public bool HaveParameters
        {
            get { return Params != null; }
        }

        public IDynamicEntity PlayerEntity { get; set; }
    }
}