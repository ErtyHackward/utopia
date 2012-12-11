using System;
using Utopia.Shared.Net.Web.Responses;

namespace Utopia.Shared.Net.Web
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
        public event EventHandler<WebEventArgs<TokenResponse>> LoginCompleted;

        private void OnLoginCompleted(WebEventArgs<TokenResponse> e)
        {
            Token = e.Response != null ? e.Response.AccessToken : null;

            var handler = LoginCompleted;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when server list is received
        /// </summary>
        public event EventHandler<WebEventArgs<ServerListResponse>> ServerListReceived;

        private void OnServerListReceived(WebEventArgs<ServerListResponse> e)
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
            PostRequestAsync<WebEventArgs<TokenResponse>>(ServerUrl + "/login", string.Format("login={0}&pass={1}", email, passwordHash), OnLoginCompleted);
        }
        
        private void CheckToken()
        {
            if (string.IsNullOrEmpty(Token))
                throw new InvalidOperationException("Token check operation failed because login procedure was not completed");
        }

        public void GetServersListAsync()
        {
            CheckToken();
            PostRequestAsync<WebEventArgs<ServerListResponse>>(ServerUrl + "/servers", "", OnServerListReceived);
        }
    }
}
