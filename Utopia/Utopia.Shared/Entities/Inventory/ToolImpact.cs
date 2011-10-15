using System;
using System.IO;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Basic tool impact
    /// </summary>
    public class ToolImpact : IToolImpact
    {
        /// <summary>
        /// Describes why tool can not be used
        /// </summary>
        public String Message { get; set; }
        
        /// <summary>
        /// Indicates if tool use was succeed
        /// </summary>
        public bool Success { get; set; }


        public void Save(BinaryWriter writer)
        {
            writer.Write(Success);
            if (Message == null) Message = string.Empty;
            writer.Write(Message);
        }

        public void Load(BinaryReader reader)
        {
            Success = reader.ReadBoolean();
            Message = reader.ReadString();
        }
    }
}
