using System;
using System.IO;
using Utopia.Shared.Entities;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Creates network message class from binary stream by message Id.
    /// </summary>
    public class NetworkMessageFactory
    {
        public EntityFactory EntityFactory { get; set; }

        public NetworkMessageFactory(EntityFactory entityFactory)
        {
            EntityFactory = entityFactory;
        }

        /// <summary>
        /// Creates network message
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IBinaryMessage ReadMessage(MessageTypes id, BinaryReader reader)
        {
            switch (id)
            {
                case MessageTypes.BlockChange:      return BlocksChangedMessage.Read(reader);
                case MessageTypes.Chat:             return ChatMessage.Read(reader);
                case MessageTypes.ChunkData:        return ChunkDataMessage.Read(reader);
                case MessageTypes.DateTime:         return DateTimeMessage.Read(reader);
                case MessageTypes.Error:            return ErrorMessage.Read(reader);
                case MessageTypes.GameInformation:  return GameInformationMessage.Read(reader);
                case MessageTypes.GetChunks:        return GetChunksMessage.Read(reader);
                case MessageTypes.Login:            return LoginMessage.Read(reader);
                case MessageTypes.LoginResult:      return LoginResultMessage.Read(reader);
                case MessageTypes.EntityDirection:  return EntityHeadDirectionMessage.Read(reader);
                case MessageTypes.EntityIn:         return EntityInMessage.Read(reader, EntityFactory);
                case MessageTypes.EntityOut:        return EntityOutMessage.Read(reader);
                case MessageTypes.EntityPosition:   return EntityPositionMessage.Read(reader);
                case MessageTypes.EntityUse:        return EntityUseMessage.Read(reader);
                case MessageTypes.Ping:             return PingMessage.Read(reader);
                case MessageTypes.EntityVoxelModel: return EntityVoxelModelMessage.Read(reader);
                case MessageTypes.ItemTransfer:     return ItemTransferMessage.Read(reader);
                case MessageTypes.EntityEquipment:  return EntityEquipmentMessage.Read(reader, EntityFactory);
                case MessageTypes.Weather:          return WeatherMessage.Read(reader);
                case MessageTypes.EntityImpulse:    return EntityImpulseMessage.Read(reader);
                case MessageTypes.EntityLock:       return EntityLockMessage.Read(reader);
                case MessageTypes.EntityLockResult: return EntityLockResultMessage.Read(reader);
                case MessageTypes.UseFeedback:      return UseFeedbackMessage.Read(reader);
                default:
                    throw new ArgumentException("Invalid message id received");
            }
        }
    }
}
