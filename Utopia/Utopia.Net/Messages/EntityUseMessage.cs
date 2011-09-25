using System.IO;
using SharpDX;
using Utopia.Net.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Net.Messages
{
    /// <summary>
    /// Defines a message that informs server about client tool using
    /// </summary>
    public struct EntityUseMessage : IBinaryMessage
    {
        private Vector3I _pickedBlockPosition;
        private Vector3I _newBlockPosition;
        private uint _pickedEntityId;
        private uint _toolId;
        private uint _entityId;
        private bool _isBlockPicked;

        /// <summary>
        /// Identification number of entity that performs use operation (player or NPC)
        /// </summary>
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }

        public Vector3I PickedBlockPosition
        {
            get { return _pickedBlockPosition; }
            set { _pickedBlockPosition = value; }
        }
        
        public Vector3I NewBlockPosition
        {
            get { return _newBlockPosition; }
            set { _newBlockPosition = value; }
        }

        /// <summary>
        /// Gets or sets Tool Entity Id that performs action
        /// </summary>
        public uint ToolId
        {
            get { return _toolId; }
            set { _toolId = value; }
        }
        
        /// <summary>
        /// Picked entity id (optional)
        /// </summary>
        public uint PickedEntityId
        {
            get { return _pickedEntityId; }
            set { _pickedEntityId = value; }
        }

        
        public bool IsBlockPicked
        {
            get { return _isBlockPicked; }
            set { _isBlockPicked = value; }
        }

        /// <summary>
        /// Gets message id (cast to MessageTypes enumeration)
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityUse; }
        }
        
        public static EntityUseMessage Read(BinaryReader reader)
        {
            EntityUseMessage msg;

            msg._entityId = reader.ReadUInt32();
            msg._pickedBlockPosition = reader.ReadVector3I();
            msg._newBlockPosition = reader.ReadVector3I();
            msg._pickedEntityId = reader.ReadUInt32();
            msg._toolId = reader.ReadUInt32();
            msg._isBlockPicked = reader.ReadBoolean();

            return msg;
        }

        /// <summary>
        /// Writes all necessary instance members
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(_entityId);
            writer.Write(_pickedBlockPosition);
            writer.Write(_newBlockPosition);
            writer.Write(_pickedEntityId);
            writer.Write(_toolId);
            writer.Write(_isBlockPicked);
        }
    }
}
