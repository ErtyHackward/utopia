using System;

namespace S33M3Engines.D3D
{
    public interface IDrawableComponent : IDrawable, IUpdateableComponent
    {
        event EventHandler<EventArgs> DrawOrderChanged;
        event EventHandler<EventArgs> VisibleChanged;
        void OnDrawOrderChanged(object sender, EventArgs args);
        bool Visible { get; set; }
        DrawOrders DrawOrders { get; }
    }
}