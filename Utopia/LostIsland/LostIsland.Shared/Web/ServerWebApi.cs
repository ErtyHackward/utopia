using System;
using Sandbox.Shared.Web.Responces;

namespace Sandbox.Shared.Web
{
    public class ServerWebApi : UtopiaWebApiBase
    {
        public UserAuthenticationResponce UserAuthenticate(string login, string passwordHash)
        {
            return PostRequest<UserAuthenticationResponce>(ServerUrl + "/userauthentication", string.Format("login={0}&pass={1}", Uri.EscapeDataString(login), passwordHash));
        }

        public void AliveUpdateAsync(string serverName, int port, uint usersCount)
        {
            PostRequestAsync(ServerUrl + "/serveralive", string.Format("name={0}&port={1}&usersCount={2}", serverName, port, usersCount), null);
        }
    }
}
