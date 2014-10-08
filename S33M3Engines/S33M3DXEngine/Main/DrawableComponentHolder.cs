using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main.Interfaces;

namespace S33M3DXEngine.Main
{
    public class DrawableComponentHolder
    {
        public IDrawableComponent DrawableComponent;
        public DrawOrders.DrawOrder DrawOrder;

        public DrawableComponentHolder(IDrawableComponent _drawableComponent, DrawOrders.DrawOrder _drawOrder)
        {
            DrawableComponent = _drawableComponent;
            DrawOrder = _drawOrder;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", DrawableComponent, DrawOrder.DrawID, DrawOrder.Order);
        }
    }
}
