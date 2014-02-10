using System;
using ProtoBuf;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Basic tool impact
    /// </summary>
    [ProtoContract]
    public class ToolImpact : IToolImpact
    {
        /// <summary>
        /// Indicates if tool use was succeed
        /// </summary>
        [ProtoMember(1)]
        public bool Success { get; set; }

        /// <summary>
        /// Describes why tool can not be used
        /// </summary>
        [ProtoMember(2)]
        public String Message { get; set; }
    }
}
