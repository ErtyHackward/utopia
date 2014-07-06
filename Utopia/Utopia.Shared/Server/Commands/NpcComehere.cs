using System;
using S33M3Resources.Structs;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Server.Structs;
using Utopia.Shared.Services;

namespace Utopia.Shared.Server.Commands
{
    public class NpcComehere : AdministratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "npccomehere"; }
        }

        public override string Description
        {
            get { return "order the npc to go to your current location"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            if (connection.SelectedNpc == null)
            {
                connection.SendChat("No npc is selected");
                return;
            }

            try
            {
                ServerDynamicEntity serverEntity;

                if (!server.AreaManager.TryFind(connection.SelectedNpc.DynamicId, out serverEntity))
                {
                    connection.SendChat("Can't find the server entity");
                    return;
                }

                var serverNpc = serverEntity as ServerNpc;

                if (serverNpc == null)
                {
                    connection.SendChat("Invalid type of the server entity");
                    return;
                }

                var humanAi = serverNpc.GeneralAI as HumanAI;

                if (humanAi == null)
                {
                    connection.SendChat("Invalid type of the AI");
                    return;
                }

                humanAi.Movement.Goto(connection.ServerEntity.DynamicEntity.Position.ToCubePosition());

                connection.SendChat("Command is sent!");
            }
            catch (Exception x)
            {
                connection.SendChat("Error: " + x);
            }
        }
    }
}