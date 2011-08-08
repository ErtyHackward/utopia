using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message used to inform player that some other player left view range
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerOutMessage : IBinaryMessage
    {
        public byte MessageId;
        /// <summary>
        /// Identification number of the player
        /// </summary>
        public int UserId;

        public static PlayerOutMessage Read(BinaryReader reader)
        {
            PlayerOutMessage msg;
            msg.MessageId = reader.ReadByte();
            msg.UserId = reader.ReadInt32();
            return msg;
        }

        public static void Write(BinaryWriter writer, PlayerOutMessage msg)
        {
            writer.Write(msg.MessageId);
            writer.Write(msg.UserId);
        }


        public void Write(BinaryWriter writer)
        {
            MessageId = (byte)MessageTypes.PlayerOut;
            Write(writer, this);
        }
    }
}
