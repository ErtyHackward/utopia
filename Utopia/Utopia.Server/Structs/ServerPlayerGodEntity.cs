﻿using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Contains GodEntity server logic
    /// </summary>
    public class ServerPlayerGodEntity : ServerPlayerEntity
    {
        public ServerPlayerGodEntity(ClientConnection connection, DynamicEntity entity, Server server) : base(connection, entity, server)
        {

        }
    }
}