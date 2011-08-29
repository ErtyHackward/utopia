using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Server.Managers
{
    public class EntityManagerEventArgs : EventArgs
    {
        public IActiveEntity Entity { get; set; }
    }
}
