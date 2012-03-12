using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3_CoreComponents.GUI.Nuclex.Support
{
    /// <summary>
    ///   Argument container used by collections to notify about replaced items
    /// </summary>
    public class ItemReplaceEventArgs<TItem> : EventArgs
    {

        /// <summary>Initializes a new event arguments supplier</summary>
        /// <param name="oldItem">Item that has been replaced by another item</param>
        /// <param name="newItem">Replacement item that is now part of the collection</param>
        public ItemReplaceEventArgs(TItem oldItem, TItem newItem)
        {
            this.oldItem = oldItem;
            this.newItem = newItem;
        }

        /// <summary>Item that has been replaced by another item</summary>
        public TItem OldItem
        {
            get { return this.oldItem; }
        }

        /// <summary>Replacement item that is now part of the collection</summary>
        public TItem NewItem
        {
            get { return this.newItem; }
        }

        /// <summary>Item that was removed from the collection</summary>
        private TItem oldItem;
        /// <summary>Item that was added to the collection</summary>
        private TItem newItem;

    }
}
