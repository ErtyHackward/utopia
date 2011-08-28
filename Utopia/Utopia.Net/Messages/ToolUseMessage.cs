using System.IO;
using SharpDX;
using Utopia.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message that informs server about client tool using
    /// </summary>
    public struct ToolUseMessage : IBinaryMessage
    {
        private Vector3 _spaceVector;
        private Location3<int> _pickedBlockPosition;
        private Location3<int> _newBlockPosition;
        private uint _pickedEntityId;
        //TODO should ToolUseMessage contain a ToolId ? wich tool is the player using  

        /// <summary>
        /// Look vector at tool using moment
        /// </summary>
        public Vector3 SpaceVector
        {
            get { return _spaceVector; }
            set { _spaceVector = value; }
        }
        
        public Location3<int> PickedBlockPosition
        {
            get { return _pickedBlockPosition; }
            set { _pickedBlockPosition = value; }
        }
        
        public Location3<int> NewBlockPosition
        {
            get { return _newBlockPosition; }
            set { _newBlockPosition = value; }
        }
        
        /// <summary>
        /// Picked entity id (optional)
        /// </summary>
        public uint PickedEntityId
        {
            get { return _pickedEntityId; }
            set { _pickedEntityId = value; }
        }

        public byte MessageId
        {
            get { return (byte)MessageTypes.ToolUseMessage; }
        }

        public static ToolUseMessage Read(BinaryReader reader)
        {
            ToolUseMessage msg;

            msg._spaceVector = reader.ReadVector3();
            msg._pickedBlockPosition = reader.ReadIntLocation3();
            msg._newBlockPosition = reader.ReadIntLocation3();
            msg._pickedEntityId = reader.ReadUInt32();

            return msg;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_spaceVector);
            writer.Write(_pickedBlockPosition);
            writer.Write(_newBlockPosition);
            writer.Write(_pickedEntityId);
        }
    }
}
