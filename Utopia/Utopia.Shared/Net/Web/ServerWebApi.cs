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

        public void AliveUpdateAsync(string serverName, int port, uint usersCount)
        {
            PostRequestAsync<WebEventArgs>(ServerUrl + "/serveralive", string.Format("name={0}&port={1}&usersCount={2}", serverName, port, usersCount), null);
        }
    }
}
