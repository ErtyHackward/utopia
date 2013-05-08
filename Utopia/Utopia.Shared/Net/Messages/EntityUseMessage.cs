using ProtoBuf;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Net.Interfaces;

namespace Utopia.Shared.Net.Messages
{
    /// <summary>
    /// Defines a message that informs server about client tool using
    /// </summary>
    [ProtoContract]
    public class EntityUseMessage : IBinaryMessage
    {
        /// <summary>
        /// Identification number of entity that performs use operation (player or NPC)
        /// </summary>
        [ProtoMember(1)]
        public uint DynamicEntityId { get; set; }

        /// <summary>
        /// Supplied entity state at the use moment
        /// </summary>
        [ProtoMember(2)]
        public DynamicEntityState State { get; set; }
        
        /// <summary>
        /// Gets or sets Tool Entity Id that performs action
        /// </summary>
        [ProtoMember(3)]
        public uint ToolId { get; set; }

        /// <summary>
        /// Identification token of the use operation
        /// </summary>
        [ProtoMember(4)]
        public int Token { get; set; }

        /// <summary>
        /// Gets the use type (put, or use)
        /// </summary>
        [ProtoMember(5)]
        public UseType UseType { get; set; }

        /// <summary>
        /// Get config recipe index to craft
        /// </summary>
        [ProtoMember(6)]
        public int RecipeIndex { get; set; }

        /// <summary>
        /// Gets message id (cast to MessageTypes enumeration)
        /// </summary>
        public byte MessageId
        {
            get { return (byte)MessageTypes.EntityUse; }
        }

        public EntityUseMessage()
        {
            
        }

        public EntityUseMessage(EntityUseEventArgs e)
        {
            State = e.State;
            DynamicEntityId = e.Entity.DynamicId;
            ToolId = e.Tool == null ? 0 : e.Tool.StaticId;
            UseType = e.UseType;
            RecipeIndex = e.RecipeIndex;
        }
    }
}
