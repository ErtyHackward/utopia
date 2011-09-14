using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3Engines.D3D
{
    /// <summary>
    /// Class responsible for management various Draw call for a single gamecomponent
    /// </summary>
    public class DrawOrders
    {
        #region Private Variables
        private IDrawableComponent _drawableGameComponent;
        private Dictionary<int, DrawOrder> _drawOrders = new Dictionary<int,DrawOrder>();
        private int _componentHashCode;
        #endregion

        #region Public Variables/Properties
        #endregion

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="drawableGameComponent">The GameComponent whom I'm attached to</param>
        public DrawOrders(IDrawableComponent drawableGameComponent)
        {
            _drawableGameComponent = drawableGameComponent;
            _componentHashCode = _drawableGameComponent.GetHashCode();
            AddIndex(0, 10);
        }

        #region Public Methods
        /// <summary>
        /// Add a New draw call.
        /// </summary>
        /// <param name="index">New draw call ID, this ID must be unique for the IDrawableComponent</param>
        /// <param name="Order">The order the will be used when the Component wil be sorted</param>
        /// <returns>True of the operation succed</returns>
        public bool AddIndex(int index, int Order)
        {
            if (_drawOrders.ContainsKey(index)) return false;
            _drawOrders.Add(index, new DrawOrder(index, Order, _componentHashCode));
            _drawableGameComponent.OnDrawOrderChanged(_drawableGameComponent, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Remove an existing draw call
        /// </summary>
        /// <param name="index">The draw call ID</param>
        /// <returns>True of the operation succed</returns>
        public bool RemoveIndex(int index)
        {
            if (!_drawOrders.ContainsKey(index)) return false;
            _drawOrders.Remove(index);
            _drawableGameComponent.OnDrawOrderChanged(_drawableGameComponent, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Update a Draw call Order
        /// </summary>
        /// <param name="index">The draw call ID</param>
        /// <param name="Order">The new draw call Order</param>
        /// <returns>True of the operation succed</returns>
        public bool UpdateIndex(int index, int Order)
        {
            if (!_drawOrders.ContainsKey(index)) return false;
            _drawOrders[index].Order = Order;
            _drawableGameComponent.OnDrawOrderChanged(_drawableGameComponent, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Get a list of all registered Draw call for the IDrawwableComponent
        /// </summary>
        /// <returns>The Draw call lists as DrawOrder objects</returns>
        public IEnumerable<DrawOrder> GetAllDrawOrder()
        {
            return _drawOrders.Values;
        }

        public override int GetHashCode()
        {
            return _componentHashCode;
        }
        #endregion

        #region Private Methods
        #endregion

        public class DrawOrder
        {
            /// <summary>
            /// The Draw call ID
            /// </summary>
            public int DrawID;
            /// <summary>
            /// The Draw call Order
            /// </summary>
            public int Order;
            //The Hash code from the owner IDrawableComponent
            private int _componentHashCode;

            /// <summary>
            /// Create a new Draw call
            /// </summary>
            /// <param name="drawID">The draw call ID</param>
            /// <param name="order">The draw call Order</param>
            /// <param name="componentHashCode">The IDrawable owner hashcode</param>
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


    }
}
