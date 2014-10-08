using System;
using ProtoBuf;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents a link to a entity. First level of the link can be dynamic entity or chunk position
    /// </summary>
    [ProtoContract]
    public struct EntityLink
    {
        private bool _isDynamic;
        private Vector3I _chunkPosition;
        private uint _entityId;
        private uint[] _tail;
        
        /// <summary>
        /// Indicates whether the entity link has dynamic entity as root otherwise root is world chunk
        /// </summary>
        [ProtoMember(1)]
        public bool IsDynamic
        {
            get { return _isDynamic; }
            set { _isDynamic = value; }
        }

        /// <summary>
        /// Gets world chunk position
        /// </summary>
        [ProtoMember(2)]
        public Vector3I ChunkPosition
        {
            get { return _chunkPosition; }
            set { _chunkPosition = value; }
        }

        /// <summary>
        /// Gets dynamic entity id
        /// </summary>
        [ProtoMember(3)]
        public uint DynamicEntityId
        {
            get { return _entityId; }
            set { _entityId = value; }
        }

        /// <summary>
        /// Gets chain of static entities (example: chunk pos (root) -> chest (Tail[0]) -> shovel (Tail[1])
        /// </summary>
        [ProtoMember(4)]
        public uint[] Tail
        {
            get { return _tail; }
            set { _tail = value; }
        }

        public bool IsStatic
        {
            get { return !_isDynamic; }
        }
        
        /// <summary>
        /// Returns whether the link is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return (!_isDynamic && _tail == null) || (_isDynamic && _entityId == 0); }
        }

        /// <summary>
        /// Gets empty link
        /// </summary>
        public static EntityLink Empty
        {
            get
            {
                EntityLink link;
                link._chunkPosition = new Vector3I();
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
            _chunkPosition = Vector3I.Zero;
            _tail = tail;
        }

        /// <summary>
        /// Creates new chunk-based entity link
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tail"></param>
        public EntityLink(Vector3I pos, params uint[] tail)
        {
            _chunkPosition = pos;
            _entityId = 0;
            _isDynamic = false;
            _tail = tail;
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

        public override int GetHashCode()
        {
            if (IsDynamic)
            {
                return (int)_entityId;
            }

            var hash = _chunkPosition.GetHashCode();

            if (_tail != null)
            {
                for (int i = 0; i < _tail.Length; i++)
                {
                    hash += (int)_tail[i] << (int)(32 * ((float)i + 1) / _tail.Length);
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

        /// <summary>
        /// Finds exact static entity that links points to
        /// Supports container-in-container extraction
        /// </summary>
        /// <param name="landscapeManager"></param>
        /// <returns></returns>
        public IStaticEntity ResolveStatic(ILandscapeManager landscapeManager)
        {
            if (IsDynamic)
            {
                throw new InvalidOperationException();
            }

            var chunk = landscapeManager.GetChunk(ChunkPosition);

            if (chunk == null)
                return null;

            var collection = (IStaticContainer)chunk.Entities;
            IStaticEntity sEntity = null;

            for (int i = 0; i < Tail.Length; i++)
            {

                if (!collection.ContainsId(Tail[i], out sEntity))
                    return null;

                if (sEntity is IStaticContainer)
                    collection = sEntity as IStaticContainer;
            }

            return sEntity;
        }

        /// <summary>
        /// Resolves entity object using factory fields: 
        /// DynamicEntityManager for dynamic entity
        /// LandscapeManager for static ones
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public T Resolve<T>(EntityFactory factory) where T : class, IEntity 
        {
            if (IsDynamic)
                return factory.DynamicEntityManager.FindEntity(this) as T;
            return ResolveStatic(factory.LandscapeManager) as T;
        }

        public IEntity Resolve(EntityFactory factory)
        {
            return Resolve<IEntity>(factory);
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return "[EL:Empty]";
            }

            if (IsDynamic)
            {
                return string.Format("[EL:{0}]", DynamicEntityId);
            }
            return string.Format("[EL:{0}:{1}]", ChunkPosition.ToString().Replace(" ",""), string.Join(",",_tail));
        }
    }

}
