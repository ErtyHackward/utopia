using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3DXEngine.Main.Interfaces
{
    public interface IUpdatableComponent : IGameComponent, IUpdatable, IDisposable
    {
        event EventHandler<EventArgs> UpdateOrderChanged;
        event EventHandler<EventArgs> UpdatableChanged;
        bool Updatable { get; set; }
        int UpdateOrder { get; set; }
    }
}
