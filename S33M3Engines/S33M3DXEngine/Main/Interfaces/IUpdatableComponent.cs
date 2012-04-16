using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3DXEngine.Main.Interfaces
{
    public interface IUpdatableComponent : IGameComponent, IUpdatable
    {
        event EventHandler<EventArgs> UpdateOrderChanged;
        event EventHandler<EventArgs> UpdatableChanged;
        bool Updatable { get; set; }
        bool isEnabled { get; }
        int UpdateOrder { get; set; }
    }
}
