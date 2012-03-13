using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main.Interfaces;

namespace S33M3DXEngine.Main
{
    public class DrawOrders
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private IDrawableComponent _drawableGameComponent;
        private List<DrawOrder> _drawOrdersCollection = new List<DrawOrder>();
        private int _componentHashCode;
        #endregion

        #region Public Variables/Properties
        public List<DrawOrder> DrawOrdersCollection
        {
            get { return _drawOrdersCollection; }
        }
        #endregion

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="drawableGameComponent">The GameComponent whom I'm attached to</param>
        public DrawOrders(IDrawableComponent drawableGameComponent)
        {
            _drawableGameComponent = drawableGameComponent;
            _componentHashCode = _drawableGameComponent.GetHashCode();
            AddIndex(10, string.Empty);
        }

        #region Public Methods
        /// <summary>
        /// Add a New draw call.
        /// </summary>
        /// <param name="index">New draw call ID, this ID must be unique for the IDrawableComponent</param>
        /// <param name="Order">The order the will be used when the Component wil be sorted</param>
        /// <returns>return the newcly created index ID</returns>
        public int AddIndex(int Order, string indexName)
        {
            int index = _drawOrdersCollection.Count;
            //The count of the draworder is equal to the new create index id (before the add into the collection)
            _drawOrdersCollection.Add(new DrawOrder(index, Order, indexName, _componentHashCode));
            _drawableGameComponent.OnDrawOrderChanged(_drawableGameComponent, EventArgs.Empty);
            return index;
        }

        /// <summary>
        /// Remove an existing draw call
        /// </summary>
        /// <param name="index">The draw call ID</param>
        /// <returns>True of the operation succed</returns>
        public bool RemoveIndex(int index)
        {
            try
            {
                _drawOrdersCollection.RemoveAt(index);
                _drawableGameComponent.OnDrawOrderChanged(_drawableGameComponent, EventArgs.Empty);
            }
            catch (Exception)
            {
                logger.Error("Trying to remove an index : {0} not existing into the draworder collection !", index);
                return false;    
            }
            return true;
        }

        /// <summary>
        /// Update a Draw call Order
        /// </summary>
        /// <param name="index">The draw call ID</param>
        /// <param name="Order">The new draw call Order</param>
        /// <returns>True of the operation succed</returns>
        public bool UpdateIndex(int index, int Order, string name = null)
        {
            try
            {
                _drawOrdersCollection[index].Order = Order;
                 if(name != null) _drawOrdersCollection[index].Name = name;
                _drawableGameComponent.OnDrawOrderChanged(_drawableGameComponent, EventArgs.Empty);
            }
            catch (Exception)
            {
                logger.Error("Trying to update DrawOrder from an index : {0} not existing into the draworder collection !", index);
                return false;
            }

            return true;
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
            public string Name;
            //The Hash code from the owner IDrawableComponent
            private int _componentHashCode;

            /// <summary>
            /// Create a new Draw call
            /// </summary>
            /// <param name="drawID">The draw call ID</param>
            /// <param name="order">The draw call Order</param>
            /// <param name="componentHashCode">The IDrawable owner hashcode</param>
            public DrawOrder(int drawID, int order, string name, int componentHashCode)
            {
                DrawID = drawID;
                Order = order;
                Name = name;
                _componentHashCode = componentHashCode;
            }

            public override int GetHashCode()
            {
                return _componentHashCode;
            }
        }
    }
}
