using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3_DXEngine.Main.Interfaces
{
    public interface IDrawableComponent : IDrawable, IUpdatableComponent
    {
        event EventHandler<EventArgs> DrawOrderChanged;
        event EventHandler<EventArgs> VisibleChanged;
        void OnDrawOrderChanged(object sender, EventArgs args);
        bool Visible { get; set; }
        DrawOrders DrawOrders { get; }
    }
}
