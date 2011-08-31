using System;
using Utopia.Net.Connections;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Interfaces;

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

        public void Listen(IDynamicEntity entity)
        {
            entity.PositionChanged += entity_PositionChanged;
            entity.ViewChanged += entity_ViewChanged;
            entity.Use += entity_Use;
        }

        void entity_Use(object sender, EntityUseEventArgs e)
        {
            
        }

        void entity_ViewChanged(object sender, EntityViewEventArgs e)
        {
            
        }

        void entity_PositionChanged(object sender, EntityMoveEventArgs e)
        {
            
        }

        public void Unlisten(IDynamicEntity entity)
        {
            entity.PositionChanged -= entity_PositionChanged;
            entity.ViewChanged -= entity_ViewChanged;
            entity.Use -= entity_Use;
        }
    }
}
