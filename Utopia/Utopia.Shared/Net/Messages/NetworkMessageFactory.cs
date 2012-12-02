using System;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
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
            var length = reader.ReadInt32();
            var bytes = reader.ReadBytes(length);
            Type msgType;
            switch (id)
            {
                case MessageTypes.BlockChange:      msgType = typeof(BlocksChangedMessage); break;
                case MessageTypes.Chat:             msgType = typeof(ChatMessage); break;
                case MessageTypes.ChunkData:        msgType = typeof(ChunkDataMessage); break;
                case MessageTypes.DateTime:         msgType = typeof(DateTimeMessage); break;
                case MessageTypes.Error:            msgType = typeof(ErrorMessage); break;
                case MessageTypes.GameInformation:  msgType = typeof(GameInformationMessage); break;
                case MessageTypes.GetChunks:        msgType = typeof(GetChunksMessage); break;
                case MessageTypes.Login:            msgType = typeof(LoginMessage); break;
                case MessageTypes.LoginResult:      msgType = typeof(LoginResultMessage); break;
                case MessageTypes.EntityDirection:  msgType = typeof(EntityHeadDirectionMessage); break;
                case MessageTypes.EntityIn:         msgType = typeof(EntityInMessage); break; 
                case MessageTypes.EntityOut:        msgType = typeof(EntityOutMessage); break; 
                case MessageTypes.EntityPosition:   msgType = typeof(EntityPositionMessage); break; 
                case MessageTypes.EntityUse:        msgType = typeof(EntityUseMessage); break;
                case MessageTypes.Ping:             msgType = typeof(PingMessage); break;
                case MessageTypes.EntityVoxelModel: msgType = typeof(EntityVoxelModelMessage); break;
                case MessageTypes.ItemTransfer:     msgType = typeof(ItemTransferMessage); break;
                case MessageTypes.EntityEquipment:  msgType = typeof(EntityEquipmentMessage); break;
                case MessageTypes.Weather:          msgType = typeof(WeatherMessage); break;
                case MessageTypes.EntityImpulse:    msgType = typeof(EntityImpulseMessage); break;
                case MessageTypes.EntityLock:       msgType = typeof(EntityLockMessage); break;
                case MessageTypes.EntityLockResult: msgType = typeof(EntityLockResultMessage); break;
                case MessageTypes.UseFeedback:      msgType = typeof(UseFeedbackMessage); break;
                default:
                    throw new ArgumentException("Invalid message id received");
            }

            var ms = new MemoryStream(bytes);
            return (IBinaryMessage)RuntimeTypeModel.Default.Deserialize(ms, null, msgType);
        }
    }
}
