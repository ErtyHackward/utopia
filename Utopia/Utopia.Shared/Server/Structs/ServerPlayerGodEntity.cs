using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Connections;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Contains GodEntity server logic
    /// </summary>
    public class ServerPlayerGodEntity : ServerPlayerEntity
    {
        GodEntity GodEntity { get { return (GodEntity)DynamicEntity; } }

        public ServerPlayerGodEntity(ClientConnection connection, DynamicEntity entity, ServerCore server) : base(connection, entity, server)
        {

        }

        public override void Use(Shared.Net.Messages.EntityUseMessage entityUseMessage)
        {
            base.Use(entityUseMessage);
            
            GodEntity.ToolUse();
        }
    }
}