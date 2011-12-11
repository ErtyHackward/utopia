using System;
using System.Net;
using System.Text;

namespace LostIsland.Shared
{
    public class UtopiaWebApi
    {
        private const string ServerUrl = "http://api.cubiquest.com/";

        public event EventHandler<LoginCompletedEventArgs> LoginCompleted;

        public void PostRequestAsync(string url, string pars, AsyncCallback callback)
        {
            var postBytes = Encoding.UTF8.GetBytes(pars);

            var request = WebRequest.Create(url);
            request.Method = "POST";

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;

            var requestStream = request.GetRequestStream();
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            request.BeginGetResponse(PostComplete, request);
        }

        private void PostComplete(IAsyncResult result)
        {

        }

        public void UserLogin(string email, string passwordHash)
        {
            PostRequestAsync(ServerUrl + "/login", string.Format("login={0}&pass={1}", email, passwordHash), LoginCompleteCallback);


        }

        private void LoginCompleteCallback(IAsyncResult result)
        {

        }
    }

    public class LoginCompletedEventArgs : EventArgs
    {
        public bool Error { get; set; }
    }
}
