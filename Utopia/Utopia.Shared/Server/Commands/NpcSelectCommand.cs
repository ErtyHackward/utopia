using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;

namespace Utopia.Shared.Server.Commands
{
    public class NpcSelectCommand : AdministratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "npcselect"; }
        }

        public override string Description
        {
            get { return "prepares selected npc for edit"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            var state =  connection.ServerEntity.DynamicEntity.EntityState;

            if (!state.IsEntityPicked)
            {
                connection.SendChat("No npc is selected");
                return;
            }

            var charEntity = state.PickedEntityLink.Resolve(server.EntityFactory) as CharacterEntity;

            if (charEntity == null)
            {
                connection.SendChat("It is not the character entity");
                return;
            }

            connection.SelectedNpc = charEntity;
            connection.SendChat(charEntity.Name + " is selected");
        }
    }
}