using System;
using Sandbox.Shared.Web.Responces;

namespace Sandbox.Shared.Web
{
    /// <summary>
    /// Class responds to handle client to web API interaction
    /// </summary>
    public class ClientWebApi : UtopiaWebApiBase
    {
        /// <summary>
        /// Gets a token received from a login procedure
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Occurs when login procedure is completed
        /// </summary>
        public event EventHandler<WebEventArgs<LoginResponce>> LoginCompleted;

        private void OnLoginCompleted(WebEventArgs<LoginResponce> e)
        {
            var handler = LoginCompleted;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when server list is received
        /// </summary>
        public event EventHandler<WebEventArgs<ServerListResponce>> ServerListReceived;

        private void OnServerListReceived(WebEventArgs<ServerListResponce> e)
        {
            var handler = ServerListReceived;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// Sends a login request to the server and fires LoginCompleted event when done
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        public void UserLoginAsync(string email, string passwordHash)
        {
            PostRequestAsync(ServerUrl + "/login", string.Format("login={0}&pass={1}", email, passwordHash), LoginCompleteCallback);
        }

        /// <summary>
        /// Sends a log off request to the server
        /// </summary>
        public void UserLogOffAsync()
        {
            CheckToken();
            PostRequestAsync(ServerUrl + "/logoff", string.Format("token={0}", Token), null);
            Token = null;
        }

        /// <summary>
        /// Sends a get-servers request and fires ServerListReceived event when done
        /// </summary>
        public void GetServersListAsync()
        {
            CheckToken();
            PostRequestAsync(ServerUrl + "/serverlist", string.Format("token={0}", Token), ServerListCallback);
        }
        
        private void CheckToken()
        {
            if (string.IsNullOrEmpty(Token))
                throw new InvalidOperationException("Token check operation failed because login procedure was not completed");
        }

        private void LoginCompleteCallback(IAsyncResult result)
        {
            var ea = ParseResult<LoginResponce>(result);

            Token = ea.Responce != null ? ea.Responce.Token : null;
            
            OnLoginCompleted(ea);
        }

        private void ServerListCallback(IAsyncResult result)
        {
            var ea = ParseResult<ServerListResponce>(result);

            OnServerListReceived(ea);
        }

        public override void Dispose()
        {
            if (Token != null)
                UserLogOffAsync();
        }
    }
}
