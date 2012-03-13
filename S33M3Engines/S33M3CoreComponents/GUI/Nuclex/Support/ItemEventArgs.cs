using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.GUI.Nuclex.Support
{
    /// <summary>
    ///   Argument container used by collections to notify about changed items
    /// </summary>
    public class ItemEventArgs<TItem> : EventArgs
    {

        /// <summary>Initializes a new event arguments supplier</summary>
        /// <param name="item">Item to be supplied to the event handler</param>
        public ItemEventArgs(TItem item)
        {
            this.item = item;
        }

        /// <summary>Obtains the collection item the event arguments are carrying</summary>
        public TItem Item
        {
            get { return this.item; }
        }

        /// <summary>Item to be passed to the event handler</summary>
        private TItem item;

    }
}
