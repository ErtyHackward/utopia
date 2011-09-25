namespace Utopia.Net.Messages
{
    /// <summary>
    /// Enumeration of all network messages
    /// </summary>
    public enum MessageTypes : byte
    {
        /// <summary>
        /// First message that client should send
        /// </summary>
        Login = 1,
        /// <summary>
        /// Players can send chat messages by this command
        /// </summary>
        Chat = 2,
        /// <summary>
        /// Server indicates of error by this command, this command means that connection is broken
        /// </summary>
        Error = 3,
        /// <summary>
        /// Server notify clients about current game time using this command
        /// </summary>
        DateTime = 4,
        /// <summary>
        /// Player's request for chunk range
        /// </summary>
        GetChunks = 5,
        /// <summary>
        /// Game information
        /// </summary>
        GameInformation = 6,
        /// <summary>
        /// Occurs when block is changed
        /// </summary>
        BlockChange = 7,
        /// <summary>
        /// Entity position change
        /// </summary>
        EntityPosition = 8,
        /// <summary>
        /// Entity view direction
        /// </summary>
        EntityDirection = 9,
        /// <summary>
        /// Server responce to GetChunks with chunk data
        /// </summary>
        ChunkData = 10,
        /// <summary>
        /// New entity somewhere near
        /// </summary>
        EntityIn = 11,
        /// <summary>
        /// Entity get out of our view range
        /// </summary>
        EntityOut = 12,
        /// <summary>
        /// Result of login procedure
        /// </summary>
        LoginResult = 13,
        /// <summary>
        /// Entity impact
        /// </summary>
        EntityUse = 14,
        /// <summary>
        /// Ping command. To detect quality of the connection
        /// </summary>
        Ping = 15,
        /// <summary>
        /// Entity voxel model update
        /// </summary>
        EntityVoxelModel = 16,
        /// <summary>
        /// Item move from one place to another
        /// </summary>
        ItemTransfer = 17,
        /// <summary>
        /// Indicates that entity equipment was changed
        /// </summary>
        EntityEquipment = 18
    }
}
