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
        /// <summary>
        /// Identification number of the player
        /// </summary>
        private int _userId;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.PlayerOut; }
        }

        /// <summary>
        /// Gets or sets an identification number of the player
        /// </summary>
        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public static PlayerOutMessage Read(BinaryReader reader)
        {
            PlayerOutMessage msg;
            msg._userId = reader.ReadInt32();
            return msg;
        }

        public static void Write(BinaryWriter writer, PlayerOutMessage msg)
        {
            writer.Write(msg._userId);
        }


        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
