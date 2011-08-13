using System.IO;
using System.Runtime.InteropServices;
using Utopia.Net.Interfaces;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message that informs player that another player somewhere near
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerInMessage : IBinaryMessage
    {
        /// <summary>
        /// Player identification number
        /// </summary>
        private int _userId;

        /// <summary>
        /// Login of the player
        /// </summary>
        private string _login;

        /// <summary>
        /// Gets message id
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.PlayerIn; }
        }

        /// <summary>
        /// Gets or sets a player identification number
        /// </summary>
        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        /// <summary>
        /// Gets or sets a login of the player
        /// </summary>
        public string Login
        {
            get { return _login; }
            set { _login = value; }
        }

        public static PlayerInMessage Read(BinaryReader reader)
        {
            PlayerInMessage msg;
            msg._userId = reader.ReadInt32();
            msg._login = reader.ReadString();
            return msg;
        }

        public static void Write(BinaryWriter writer, PlayerInMessage msg)
        {
            writer.Write(msg._userId);
            writer.Write(msg._login);
        }
        
        public void Write(BinaryWriter writer)
        {
            Write(writer, this);
        }
    }
}
