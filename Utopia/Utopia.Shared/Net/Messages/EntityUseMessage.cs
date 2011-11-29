using System.IO;
using Utopia.Shared.Entities;
using Utopia.Shared.Net.Interfaces;
using Utopia.Shared.Structs;
using S33M3Engines.Shared.Math;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that informs server about client tool using
    /// </summary>
    public struct EntityUseMessage : IBinaryMessage
    {
        private Vector3I _pickedBlockPosition;
        private Vector3I _newBlockPosition;
        private Vector3D _pickedEntityPosition;
        private EntityLink _pickedEntityId;
        private uint _toolId;
        private uint _entityId;
        private bool _isBlockPicked;
        private bool _isEntityPicked;
        private ToolUseMode _useMode;

        private int _token;

        /// <summary>
        /// Identification number of entity that performs use operation (player or NPC)
        /// </summary>
        public uint DynamicEntityId
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
        /// Picked entity position (optional)
        /// </summary>
        public Vector3D PickedEntityPosition
        {
            get { return _pickedEntityPosition; }
            set { _pickedEntityPosition = value; }
        }

        /// <summary>
        /// Picked entity id (optional)
        /// </summary>
        public EntityLink PickedEntityId
        {
            get { return _pickedEntityId; }
            set { _pickedEntityId = value; }
        }

        
        public bool IsBlockPicked
        {
            get { return _isBlockPicked; }
            set { _isBlockPicked = value; }
        }

        public bool IsEntityPicked
        {
            get { return _isEntityPicked; }
            set { _isEntityPicked = value; }
        }
        
        /// <summary>
        /// Identification token of the use operation
        /// </summary>
        public int Token
        {
            get { return _token; }
            set { _token = value; }
        }

        /// <summary>
        /// Indicates use mode (simple case - left ot right mouse buttons)
        /// </summary>
        public ToolUseMode UseMode
        {
            get { return _useMode; }
            set { _useMode = value; }
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
            msg._pickedEntityPosition = new Vector3D();
            msg._pickedEntityPosition.X = reader.ReadDouble();
            msg._pickedEntityPosition.Y = reader.ReadDouble();
            msg._pickedEntityPosition.Z = reader.ReadDouble();
            msg._toolId = reader.ReadUInt32();
            msg._isBlockPicked = reader.ReadBoolean();
            msg._isEntityPicked = reader.ReadBoolean();
            msg._pickedEntityId = reader.ReadEntityLink();
            msg._token = reader.ReadInt32();
            msg._useMode = (ToolUseMode)reader.ReadByte();

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
            writer.Write(_pickedEntityPosition.X);
            writer.Write(_pickedEntityPosition.Y);
            writer.Write(_pickedEntityPosition.Z);
            writer.Write(_toolId);
            writer.Write(_isBlockPicked);
            writer.Write(_isEntityPicked);
            writer.Write(_pickedEntityId);
            writer.Write(_token);
            writer.Write((byte)_useMode);
        }
    }
}
