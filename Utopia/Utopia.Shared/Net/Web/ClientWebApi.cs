using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Utopia.Shared.Net.Web.Responses;

namespace Utopia.Shared.Net.Web
{
    /// <summary>
    /// Class responds to handle client to web API interaction
    /// </summary>
    public class ClientWebApi : UtopiaWebApiBase
    {
        public static string ClientId;
        public static string ClientSecret;

        /// <summary>
        /// Gets a token received from a login procedure
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Occurs when new token is received
        /// </summary>
        public event EventHandler<TokenResponse> TokenReceived;

        private void OnTokenReceived(TokenResponse e)
        {
            Token = e != null ? e.AccessToken : null;

            var handler = TokenReceived;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when server list is received
        /// </summary>
        public event EventHandler<ServerListResponse> ServerListReceived;

        private void OnServerListReceived(ServerListResponse e)
        {
            var handler = ServerListReceived;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when token verify operation is finished
        /// </summary>
        public event EventHandler<VerifyResponse> TokenVerified;

        protected virtual void OnTokenVerified(VerifyResponse e)
        {
            if (e.Active == 0)
                Token = null;

            var handler = TokenVerified;
            if (handler != null) handler(this, e);
        }


        /// <summary>
        /// Sends a token request to the server and fires TokenReceived event when done
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        public void OauthTokenAsync(string email, string passwordHash)
        {
            GetRequestAsync<TokenResponse>(ServerUrl + "/oauth/token?" + string.Format("username={0}&password={1}&client_id={2}&client_secret={3}&grant_type=password&mode=login", Uri.EscapeDataString(email), passwordHash, ClientId, ClientSecret), OnTokenReceived);
        }

        /// <summary>
        /// Sends verify token request to perform authorization
        /// </summary>
        /// <param name="token"></param>
        public void OauthVerifyTokenAsync(string token)
        {
            Token = token;
            GetRequestAsync<VerifyResponse>(ServerUrl + string.Format("/oauth/verify?access_token={0}", token), OnTokenVerified);
        }

        private void CheckToken()
        {
            if (string.IsNullOrEmpty(Token))
                throw new InvalidOperationException("Token check operation failed because login procedure was not completed");
        }

        public void GetServersListAsync()
        {
            CheckToken();
            GetRequestAsync<ServerListResponse>(ServerUrl + string.Format("/api/servers?access_token={0}", Token), OnServerListReceived);
        }

        public void UploadModel(string filePath)
        {
            CheckToken();

            var nvc = new NameValueCollection();

            nvc.Add("name", Path.GetFileNameWithoutExtension(filePath));

            List<UploadFileInfo> files = new List<UploadFileInfo>();

            files.Add(new UploadFileInfo
                {
                    FilePath = filePath,
                    ContentType = "application/octet-stream",
                    ParamName = "File"
                });

            files.Add(new UploadFileInfo
            {
                FilePath = Path.ChangeExtension(filePath, ".png"),
                ContentType = "image/png",
                ParamName = "Screen"
            });

            HttpUploadFiles(ServerUrl + string.Format("/api/models?access_token={0}", Token), files, nvc);
        }
    }
}
