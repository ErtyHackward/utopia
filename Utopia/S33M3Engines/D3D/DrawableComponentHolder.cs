using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D
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
    }
}
