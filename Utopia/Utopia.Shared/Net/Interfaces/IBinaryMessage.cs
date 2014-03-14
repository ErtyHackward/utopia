using ProtoBuf;
using Utopia.Shared.Net.Messages;

namespace Utopia.Shared.Net.Interfaces
{
    /// <summary>
    /// Indicates that this instance can be sent in binary mode
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(BlocksChangedMessage))]
    [ProtoInclude(101, typeof(ChatMessage))]
    [ProtoInclude(102, typeof(ChunkDataMessage))]
    [ProtoInclude(103, typeof(DateTimeMessage))]
    [ProtoInclude(104, typeof(ErrorMessage))]
    [ProtoInclude(105, typeof(GameInformationMessage))]
    [ProtoInclude(106, typeof(GetChunksMessage))]
    [ProtoInclude(107, typeof(LoginMessage))]
    [ProtoInclude(108, typeof(LoginResultMessage))]
    [ProtoInclude(109, typeof(EntityHeadDirectionMessage))]
    [ProtoInclude(110, typeof(EntityInMessage))]
    [ProtoInclude(111, typeof(EntityOutMessage))]
    [ProtoInclude(112, typeof(EntityPositionMessage))]
    [ProtoInclude(113, typeof(EntityUseMessage))]
    [ProtoInclude(114, typeof(PingMessage))]
    [ProtoInclude(115, typeof(EntityVoxelModelMessage))]
    [ProtoInclude(116, typeof(ItemTransferMessage))]
    [ProtoInclude(117, typeof(EntityEquipmentMessage))]
    [ProtoInclude(118, typeof(WeatherMessage))]
    [ProtoInclude(119, typeof(EntityImpulseMessage))]
    [ProtoInclude(120, typeof(EntityLockMessage))]
    [ProtoInclude(121, typeof(EntityLockResultMessage))]
    [ProtoInclude(122, typeof(UseFeedbackMessage))]
    [ProtoInclude(123, typeof(RequestDateTimeSyncMessage))]
    [ProtoInclude(124, typeof(GetEntityMessage))]
    [ProtoInclude(125, typeof(EntityDataMessage))]
    [ProtoInclude(126, typeof(EntityHealthMessage))]
    [ProtoInclude(127, typeof(EntityHealthStateMessage))]
    [ProtoInclude(128, typeof(EntityAfflictionStateMessage))]
    
    public interface IBinaryMessage
    {
        /// <summary>
        /// Gets a message identification number
        /// </summary>
        byte MessageId { get; }
    }
}
