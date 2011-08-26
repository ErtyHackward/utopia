using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Net.Connections;
using Utopia.Shared.Chunks.Entities;

namespace Utopia.Network
{
    public class EntityMessageTranslator
    {
        private readonly ServerConnection _connection;

        public EntityMessageTranslator(ServerConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            _connection = connection;
        }

        public void Listen(LivingEntity entity)
        {
            entity.EntityUse += entity_EntityUse;
            entity.LeftToolUse += entity_LeftToolUse;
            entity.RightToolUse += entity_RightToolUse;
        }

        void entity_RightToolUse(object sender, LivingEntityUseEventArgs e)
        {
            


        }

        void entity_LeftToolUse(object sender, LivingEntityUseEventArgs e)
        {
            
        }

        void entity_EntityUse(object sender, LivingEntityUseEventArgs e)
        {
            if (e.PickedEntityId == 0)
                return;

            
        }

        public void Unlisten(LivingEntity entity)
        {
            entity.EntityUse -= entity_EntityUse;
            entity.LeftToolUse -= entity_LeftToolUse;
            entity.RightToolUse -= entity_RightToolUse;
        }
    }
}
