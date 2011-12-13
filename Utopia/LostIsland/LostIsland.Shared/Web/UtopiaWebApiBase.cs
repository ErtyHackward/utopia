using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using ProtoBuf;

namespace LostIsland.Shared.Web
{
    public abstract class UtopiaWebApiBase : IDisposable
    {
        protected const string ServerUrl = "http://api.cubiquest.com"; // "http://localhost:20753";

        protected WebEventArgs<T> ParseResult<T>(IAsyncResult result)
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

        /// <summary>
        /// Performs a post http request, use IAsyncResult.State as WebRequest class
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pars"></param>
        /// <param name="callback"></param>
        public static void PostRequestAsync(string url, string pars, AsyncCallback callback)
        {
            new ThreadStart(delegate {
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
            ).BeginInvoke(null, null);
        }

        public virtual void Dispose()
        {

        }
    }
}