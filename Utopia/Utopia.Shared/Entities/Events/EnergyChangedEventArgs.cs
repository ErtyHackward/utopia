using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Events
{
    public class EnergyChangedEventArgs : EventArgs
    {
        public uint EntityOwner { get; set; }
        public float ValueChangedAmount { get; set; }
        public Energy EnergyChanged { get; set; }
    }
}
