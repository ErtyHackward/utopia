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

        public static bool operator ==(ToolImpact left, ToolImpact right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null))
                return false;
            if (ReferenceEquals(right, null))
                return false;
            if (left.GetType() != right.GetType())
                return false;

            return left.Success == right.Success && left.Message == right.Message;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ToolImpact)obj);
        }

        public static bool operator !=(ToolImpact left, ToolImpact right)
        {
            return !( left == right );
        }

        protected bool Equals(ToolImpact other)
        {
            return Success.Equals(other.Success) && string.Equals(Message, other.Message);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Success.GetHashCode() * 397) ^ (Message != null ? Message.GetHashCode() : 0);
            }
        }
    }
}
