using System;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Server.Structs;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Commands
{
    public class NpcAddActivity : AdministratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "npcaddactivity"; }
        }

        public override string Description
        {
            get { return "adds new activity for the selected npc. Example: npcaddactivity Work 12:00"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            if (connection.SelectedNpc == null)
            {
                connection.SendChat("No npc is selected");
                return;
            }

            if (arguments == null || arguments.Length != 2)
            {
                connection.SendChat("Wrong parameters");
                return;
            }

            try
            {
                var time = UtopiaTimeSpan.Parse(arguments[1]);

                var activity = new Activity { 
                    Name = arguments[0], 
                    StartAt = time 
                };

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

                humanAi.Activities.Add(activity);

                serverNpc.Character.OnNeedSave();
                connection.SendChat("New activity was added");
            }
            catch (Exception x)
            {
                connection.SendChat("Error: " + x);
            }
        }
    }
}