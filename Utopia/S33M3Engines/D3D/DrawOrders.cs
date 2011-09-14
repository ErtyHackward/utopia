using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D
{
    public class DrawOrders
    {
        private IDrawableComponent _drawableGameComponent;
        private Dictionary<int, DrawOrder> _drawOrders = new Dictionary<int,DrawOrder>();
        private int _componentHashCode;

        public bool AddIndex(int index, int Order)
        {
            if (_drawOrders.ContainsKey(index)) return false;
            _drawOrders.Add(index, new DrawOrder(index, Order, _componentHashCode));
            _drawableGameComponent.OnDrawOrderChanged(_drawableGameComponent, EventArgs.Empty);
            return true;
        }

        public bool RemoveIndex(int index)
        {
            if (!_drawOrders.ContainsKey(index)) return false;
            _drawOrders.Remove(index);
            _drawableGameComponent.OnDrawOrderChanged(_drawableGameComponent, EventArgs.Empty);
            return true;
        }

        public bool UpdateIndex(int index, int Order)
        {
            if (!_drawOrders.ContainsKey(index)) return false;
            _drawOrders[index].Order = Order;
            _drawableGameComponent.OnDrawOrderChanged(_drawableGameComponent, EventArgs.Empty);
            return true;
        }

        public IEnumerable<DrawOrder> GetAllDrawOrder()
        {
            foreach (var item in _drawOrders.Values)
            {
                yield return item;
            }
        }

        public DrawOrders(IDrawableComponent drawableGameComponent)
        {
            _drawableGameComponent = drawableGameComponent;
            _componentHashCode = _drawableGameComponent.GetHashCode();
            AddIndex(0, 10);
        }

        public class DrawOrder
        {
            public int DrawID;
            public int Order;
            private int _componentHashCode;

            public DrawOrder(int drawID, int order, int componentHashCode)
            {
                DrawID = drawID;
                Order = order;
                _componentHashCode = componentHashCode;
            }

            public override int GetHashCode()
            {
                return _componentHashCode;
            }
        }

        public override int GetHashCode()
        {
            return _componentHashCode;
        }
    }
}
