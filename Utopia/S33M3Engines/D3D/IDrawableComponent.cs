using System;

namespace S33M3Engines.D3D
{
    public interface IDrawableComponent : IDrawable, IUpdateableComponent
    {
        event EventHandler<EventArgs> DrawOrderChanged;
        event EventHandler<EventArgs> VisibleChanged;
        bool Visible { get; set; }
        int DrawOrder { get; set; }
    }
}