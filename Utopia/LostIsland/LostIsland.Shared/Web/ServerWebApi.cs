using System;
using LostIsland.Shared.Web.Responces;

namespace LostIsland.Shared.Web
{
    public class ServerWebApi : UtopiaWebApiBase
    {
        public event EventHandler<WebEventArgs<UserAuthenticationResponce>> UserAuthenticated;

        private void OnUserAuthenticated(WebEventArgs<UserAuthenticationResponce> e)
        {
            var handler = UserAuthenticated;
            if (handler != null) handler(this, e);
        }

        public void UserAuthenticateAsync(string login, string passwordHash)
        {
            PostRequestAsync(ServerUrl + "/userauthentication", string.Format("login={0}&pass={1}", Uri.EscapeDataString(login), passwordHash), UserAuthenticationCallback);
        }

        public void AliveUpdateAsync(string serverName, int port, uint usersCount)
        {
            PostRequestAsync(ServerUrl + "/serveralive", string.Format("name={0}&port={1}&usersCount={2}", serverName, port, usersCount), null);
        }

        private void UserAuthenticationCallback(IAsyncResult result)
        {
            var ea = ParseResult<UserAuthenticationResponce>(result);
            OnUserAuthenticated(ea);
        }


    }
}
