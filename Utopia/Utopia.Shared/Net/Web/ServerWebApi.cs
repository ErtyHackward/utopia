using System;
using Utopia.Shared.Net.Web.Responses;

namespace Utopia.Shared.Net.Web
{
    public class ServerWebApi : UtopiaWebApiBase
    {
        public UserAuthenticateResponse UserAuthenticate(string login, string passwordHash)
        {
            return PostRequest<UserAuthenticateResponse>(ServerUrl + "/api/servers/verifyuser", string.Format("login={0}&hash={1}", Uri.EscapeDataString(login), passwordHash));
        }

        public void AliveUpdateAsync(string serverName, string description, int port, uint usersCount, Action<WebEventArgs> callback)
        {
            PostRequestAsync(ServerUrl + "/api/servers", string.Format("name={0}&port={1}&count={2}&description={3}", Uri.EscapeDataString(serverName), port, usersCount, description), callback);
        }
    }
}
