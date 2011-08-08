using System;

namespace Utopia.Net.Connections
{
    /// <summary>
    /// Represents a container for a network message arguments
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProtocolMessageEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets or sets the actual message
        /// </summary>
        public T Message { get; set; }
    }
}