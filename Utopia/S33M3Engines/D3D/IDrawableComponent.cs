using System;

namespace S33M3Engines.D3D
{
    public interface IDrawableComponent : IUpdateableComponent
    {
        void Draw();
        event EventHandler<EventArgs> DrawOrderChanged;
        event EventHandler<EventArgs> VisibleChanged;
        bool Visible { get; set; }
        int DrawOrder { get; set; }
    }
}