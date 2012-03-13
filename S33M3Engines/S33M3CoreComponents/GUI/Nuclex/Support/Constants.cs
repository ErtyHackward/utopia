using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace S33M3CoreComponents.GUI.Nuclex.Support
{
    /// <summary>Contains fixed constants used by some collections</summary>
    public static class Constants
    {
        /// <summary>Fixed event args used to notify that the collection has reset</summary>
        public static readonly NotifyCollectionChangedEventArgs NotifyCollectionResetEventArgs =
          new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
    }
}
