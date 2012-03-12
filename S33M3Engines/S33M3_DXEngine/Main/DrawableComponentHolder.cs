using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main.Interfaces;

namespace S33M3_DXEngine.Main
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
