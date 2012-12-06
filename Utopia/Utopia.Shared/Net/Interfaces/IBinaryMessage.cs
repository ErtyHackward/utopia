namespace Utopia.Shared.Net.Interfaces
{
    /// <summary>
    /// Indicates that this instance can be sent in binary mode
    /// </summary>
    public interface IBinaryMessage
    {
        /// <summary>
        /// Gets a message identification number
        /// </summary>
        byte MessageId { get; }
    }
}
