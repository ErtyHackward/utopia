using System;

namespace S33M3Engines.D3D
{
    public interface IUpdateableComponent : IGameComponent, IDisposable
    {
        void Update(ref GameTime timeSpent);
        void Interpolation(ref double interpolationHd, ref float interpolationLd);
        event EventHandler<EventArgs> UpdateOrderChanged;
        event EventHandler<EventArgs> EnabledChanged;
        bool Enabled { get; set; }
        int UpdateOrder { get; set; }
    }
}