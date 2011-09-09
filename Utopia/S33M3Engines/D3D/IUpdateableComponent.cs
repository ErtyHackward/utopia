using System;

namespace S33M3Engines.D3D
{
    public interface IUpdateableComponent : IGameComponent, IUpdatable, IDisposable
    {
        event EventHandler<EventArgs> UpdateOrderChanged;
        event EventHandler<EventArgs> EnabledChanged;
        bool Enabled { get; set; }
        int UpdateOrder { get; set; }
    }
}