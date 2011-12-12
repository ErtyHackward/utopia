using System;
using System.IO;
using System.Net;
using System.Text;
using ProtoBuf;
using UtopiaApi.Models;

namespace LostIsland.Shared.Web
{
    /// <summary>
    /// Class responds to handle client to web API interaction
    /// </summary>
    public class UtopiaWebApi : IDisposable
    {
        private const string ServerUrl = "http://api.cubiquest.com";

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
        /// Performs a post http request, use IAsyncResult.State as WebRequest class
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pars"></param>
        /// <param name="callback"></param>
        public static void PostRequestAsync(string url, string pars, AsyncCallback callback)
        {
            var postBytes = Encoding.UTF8.GetBytes(pars);

            var request = WebRequest.Create(url);
            request.Method = "POST";

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;

            var requestStream = request.GetRequestStream();
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            request.BeginGetResponse(callback, request);
        }

        /// <summary>
        /// Sends a login request to the server and fires LoginCompleted event when done
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passwordHash"></param>
        public void UserLogin(string email, string passwordHash)
        {
            PostRequestAsync(ServerUrl + "/login", string.Format("login={0}&pass={1}", email, passwordHash), LoginCompleteCallback);
        }

        /// <summary>
        /// Sends a log off request to the server
        /// </summary>
        public void UserLogOff()
        {
            CheckToken();
            PostRequestAsync(ServerUrl + "/logoff", string.Format("token={0}", Token), null);
            Token = null;
        }

        /// <summary>
        /// Sends a get-servers request and fires ServerListReceived event when done
        /// </summary>
        public void GetServersList()
        {
            CheckToken();
            PostRequestAsync(ServerUrl + "/serverlist", string.Format("token={0}", Token), ServerListCallback);
        }

        private void CheckToken()
        {
            if (string.IsNullOrEmpty(Token))
                throw new InvalidOperationException("Unable to send a logoff request because login procedure was not completed");
        }

        private WebEventArgs<T> ParseResult<T>(IAsyncResult result)
        {
            WebResponse responce = null;
            Stream respStream = null;
            var request = (WebRequest)result.AsyncState;
            var ea = new WebEventArgs<T>();
            try
            {
                responce = request.EndGetResponse(result);
                respStream = responce.GetResponseStream();
                ea.Responce = Serializer.Deserialize<T>(respStream);
            }
            catch (Exception x)
            {
                ea.Exception = x;
            }
            finally
            {
                if (respStream != null)
                    respStream.Close();
                if (responce != null)
                    responce.Close();
            }

            return ea;
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

        public void Dispose()
        {
            if (Token != null)
                UserLogOff();
        }
    }

    public class WebEventArgs<T> : EventArgs
    {
        public T Responce { get; set; }
        public Exception Exception { get; set; }
    }
}
