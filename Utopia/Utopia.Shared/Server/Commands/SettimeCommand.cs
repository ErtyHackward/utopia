using System;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Commands
{
    public class SettimeCommand : ModeratorCommand, IServerChatCommand
    {
        public override string Id
        {
            get { return "settime"; }
        }

        public override string Description
        {
            get { return "Changes current game time. Examples: settime 9:00, settime 23:00"; }
        }

        public void Execute(ServerCore server, ClientConnection connection, string[] arguments)
        {
            try
            {
                if (arguments == null || arguments.Length == 0)
                    return;

                var time = UtopiaTimeSpan.Parse(arguments[0]);

                server.Clock.SetCurrentTimeOfDay(time);
                server.ConnectionManager.Broadcast(new DateTimeMessage { DateTime = server.Clock.Now, TimeFactor = server.Clock.TimeFactor });
                server.ChatManager.Broadcast("Time updated by " + connection.DisplayName);
            }
            catch (Exception ex)
            {
                if (ex is OverflowException || ex is FormatException)
                    connection.Send(new ChatMessage { IsServerMessage = true, DisplayName = "server", Message = "wrong time value, try 9:00 or 21:00" });
                else throw;
            }
        }
    }
}