using System.IO;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Structs
{
    public struct EntityLink : IBinaryStorable
    {
        private bool _isStatic;
        private Vector2I _chunkPosition;
        private uint _entityId;

        public bool IsStatic
        {
            get { return _isStatic; }
            set { _isStatic = value; }
        }
        
        public Vector2I ChunkPosition
        {
            get { return _chunkPosition; }
            set { _chunkPosition = value; }
        }
        
        public uint EntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }

        public EntityLink(uint dynamicId)
        {
            _entityId = dynamicId;
            _isStatic = false;
            _chunkPosition = Vector2I.Zero;
        }

        public EntityLink(Vector2I pos, uint staticId)
        {
            _chunkPosition = pos;
            _entityId = staticId;
            _isStatic = true;
        }

        public EntityLink(BinaryReader reader)
        {
            _isStatic = reader.ReadBoolean();
            _entityId = reader.ReadUInt32();
            _chunkPosition = reader.ReadVector2I();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(_isStatic);
            writer.Write(_entityId);
            writer.Write(_chunkPosition);
        }

        public void Load(BinaryReader reader)
        {
            _isStatic = reader.ReadBoolean();
            _entityId = reader.ReadUInt32();
            _chunkPosition = reader.ReadVector2I();
        }
    }
}
