using System;
using System.IO;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents a link to a entity. First level of the link can be dynamic entity or chunk position
    /// </summary>
    public struct EntityLink : IBinaryStorable
    {
        private bool _isDynamic;
        private Vector2I _chunkPosition;
        private uint _entityId;
        private uint[] _tail;

        /// <summary>
        /// Gets chain of static entities (example: chunk pos (root) -> chest (Tail[0]) -> shovel (Tail[1])
        /// </summary>
        public uint[] Tail
        {
            get { return _tail; }
        }

        /// <summary>
        /// Indicates whether the entity link has dynamic entity as root otherwise root is world chunk
        /// </summary>
        public bool IsDynamic
        {
            get { return _isDynamic; }
        }
        
        /// <summary>
        /// Gets world chunk position
        /// </summary>
        public Vector2I ChunkPosition
        {
            get { return _chunkPosition; }
        }
        
        /// <summary>
        /// Gets dynamic entity id
        /// </summary>
        public uint DynamicEntityId
        {
            get { return _entityId; }
        }

        /// <summary>
        /// Returns whether the link is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return _isDynamic && _entityId == 0; }
        }

        /// <summary>
        /// Gets empty link
        /// </summary>
        public static EntityLink Empty
        {
            get
            {
                EntityLink link;
                link._chunkPosition = new Vector2I();
                link._entityId = 0;
                link._isDynamic = true;
                link._tail = null;
                return link;
            }
        }

        /// <summary>
        /// Creates new dynamic-entity based link
        /// </summary>
        /// <param name="dynamicId"></param>
        /// <param name="tail"></param>
        public EntityLink(uint dynamicId, params uint[] tail)
        {
            _entityId = dynamicId;
            _isDynamic = true;
            _chunkPosition = Vector2I.Zero;
            _tail = tail;
        }

        /// <summary>
        /// Creates new chunk-based entity link
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tail"></param>
        public EntityLink(Vector2I pos, params uint[] tail)
        {
            _chunkPosition = pos;
            _entityId = 0;
            _isDynamic = false;
            _tail = tail;
        }

        public EntityLink(BinaryReader reader)
        {
            _isDynamic = false;
            _entityId = 0;
            _chunkPosition = new Vector2I();
            _tail = null;

            Load(reader);
        }

        /// <summary>
        /// Determines whether the link points to dynamicEntity specified
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsPointsTo(IDynamicEntity entity)
        {
            return _isDynamic && _entityId == entity.DynamicId;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(_isDynamic);
            if(_isDynamic)
                writer.Write(_entityId);
            else
                writer.Write(_chunkPosition);

            if (_tail == null || _tail.Length == 0)
                writer.Write((byte)0);
            else writer.Write((byte)_tail.Length);

            if (_tail != null)
            {
                foreach (uint u in _tail)
                {
                    writer.Write(u);
                }
            }
        }

        public void Load(BinaryReader reader)
        {
            _isDynamic = reader.ReadBoolean();
            if(_isDynamic)
                _entityId = reader.ReadUInt32();
            else 
                _chunkPosition = reader.ReadVector2I();

            var count = reader.ReadByte();

            if (count > 0)
            {
                _tail = new uint[count];
                for (int i = 0; i < count; i++)
                {
                    _tail[i] = reader.ReadUInt32();
                }
            }
            else _tail = null;
        }

        public override int GetHashCode()
        {
            if(IsDynamic)
            {
                return (int)_entityId;
            }

            var hash = _chunkPosition.GetHashCode();

            if (_tail != null)
            {
                for (int i = 0; i < _tail.Length; i++)
                {
                    hash += (int)_tail[i] << (32 * (i + 1) / _tail.Length);
                }
            }

            return hash;
        }

        public static bool operator ==(EntityLink one, EntityLink two)
        {
            if (one._isDynamic != two._isDynamic) return false;
            if (one._isDynamic)
            {
                return one._entityId == two._entityId;
            }

            if (one._chunkPosition != two._chunkPosition)
                return false;

            if (one._tail.Length != two._tail.Length)
                return false;

            for (int i = 0; i < one._tail.Length; i++)
            {
                if (one._tail[i] != two._tail[i])
                    return false;
            }

            return true;
        }

        public static bool operator !=(EntityLink one, EntityLink two)
        {
            return !(one == two);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(EntityLink))
                return false;

            return (EntityLink)obj == this;
        }

        public IStaticEntity ResolveStatic(ILandscapeManager2D landscapeManager)
        {
            if (IsDynamic)
            {
                throw new InvalidOperationException();
            }

            var chunk = landscapeManager.GetChunk(ChunkPosition);

            var collection = (IStaticContainer)chunk.Entities;
            IStaticEntity sEntity = null;

            for (int i = 0; i < Tail.Length; i++)
            {
                sEntity = collection.GetStaticEntity(Tail[i]);
                if (sEntity is IStaticContainer)
                    collection = sEntity as IStaticContainer;
            }

            return sEntity;
        
        }
    }
}
