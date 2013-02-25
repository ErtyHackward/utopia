using System;
using Utopia.Shared.Net.Web.Responses;

namespace Utopia.Shared.Net.Web
{
    public class ServerWebApi : UtopiaWebApiBase
    {
        public UserAuthenticateResponse UserAuthenticate(string login, string passwordHash)
        {
            return PostRequest<UserAuthenticateResponse>(ServerUrl + "/userauthentication", string.Format("login={0}&pass={1}", Uri.EscapeDataString(login), passwordHash));
        }

        public void AliveUpdateAsync(string serverName, int port, uint usersCount, Action<WebEventArgs> callback)
        {
            PostRequestAsync<WebEventArgs>(ServerUrl + "/api/servers", string.Format("name={0}&port={1}&count={2}", Uri.EscapeDataString(serverName), port, usersCount), callback);
        }
    }
}
