using System;
using System.IO;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Creates network message class from binary stream by message Id.
    /// </summary>
    public class NetworkMessageFactory
    {
        static NetworkMessageFactory()
        {
            Instance = new NetworkMessageFactory();
        }

        /// <summary>
        /// Gets factory instance
        /// </summary>
        public static NetworkMessageFactory Instance { get; private set; }

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
                case MessageTypes.BlockChange: return BlockChangeMessage.Read(reader);
                case MessageTypes.Chat: return ChatMessage.Read(reader);
                case MessageTypes.ChunkData: return ChunkDataMessage.Read(reader);
                case MessageTypes.DateTime: return DateTimeMessage.Read(reader);
                case MessageTypes.Error: return ErrorMessage.Read(reader);
                case MessageTypes.GameInformation: return GameInformationMessage.Read(reader);
                case MessageTypes.GetChunks: return GetChunksMessage.Read(reader);
                case MessageTypes.Login: return LoginMessage.Read(reader);
                case MessageTypes.LoginResult: return LoginResultMessage.Read(reader);
                case MessageTypes.PlayerDirection: return PlayerDirectionMessage.Read(reader);
                case MessageTypes.PlayerIn: return PlayerInMessage.Read(reader);
                case MessageTypes.PlayerOut: return PlayerOutMessage.Read(reader);
                case MessageTypes.PlayerPosition: return PlayerPositionMessage.Read(reader);
                case MessageTypes.EntityUse: return EntityUseMessage.Read(reader);
                case MessageTypes.ToolUseMessage: return ToolUseMessage.Read(reader);
                default:
                    throw new ArgumentException("");
            }
        }
    }
}
