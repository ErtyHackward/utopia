using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Utopia.Shared.Net.Web
{
    public abstract class UtopiaWebApiBase : IDisposable
    {
        protected const string ServerUrl = "http://utopiarealms.com";

        /// <summary>
        /// Performs a post http request, use IAsyncResult.State as WebRequest class
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pars"></param>
        /// <param name="callback"></param>
        public static void PostRequestAsync<T>(string url, string pars, Action<T> callback) where T : WebEventArgs, new()
        {
            new ThreadStart(delegate {
                var postBytes = Encoding.UTF8.GetBytes(pars);

                var request = WebRequest.Create(url);
                request.Method = "POST";

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postBytes.Length;

                using (var requestStream = request.GetRequestStream())
                    requestStream.Write(postBytes, 0, postBytes.Length);

                request.BeginGetResponse(delegate(IAsyncResult result)
                {
                    var ea = new T();
                    try
                    {
                        using (var responce = request.EndGetResponse(result))
                        using (var respStream = responce.GetResponseStream())
                        using (var streamReader = new StreamReader(respStream))
                        {
                            var str = streamReader.ReadToEnd();
                            ea = JsonConvert.DeserializeObject<T>(str);
                        }
                    }
                    catch (Exception x)
                    {
                        ea.Exception = x;
                    }

                    callback(ea);

                }, request);
            }
            ).BeginInvoke(null, null);
        }

        /// <summary>
        /// Performs a post http request, use IAsyncResult.State as WebRequest class
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pars"></param>
        /// <param name="callback"></param>
        public static void GetRequestAsync<T>(string url, Action<T> callback) where T : WebEventArgs, new()
        {
            new ThreadStart(delegate
            {
                var request = WebRequest.Create(url);
                request.Method = "GET";

                request.BeginGetResponse(delegate(IAsyncResult result)
                {
                    var ea = new T();
                    try
                    {
                        using (var response = request.EndGetResponse(result))
                        using (var respStream = response.GetResponseStream())
                        using (var streamReader = new StreamReader(respStream))
                        {
                            var respString = streamReader.ReadToEnd();

                            ea = JsonConvert.DeserializeObject<T>(respString);
                        }
                    }
                    catch (Exception x)
                    {
                        ea.Exception = x;
                    }

                    callback(ea);

                }, request);
            }
            ).BeginInvoke(null, null);
        }

        /// <summary>
        /// Perfoms a post request in this thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public static T PostRequest<T>(string url, string pars)
        {
            var postBytes = Encoding.UTF8.GetBytes(pars);

            var request = WebRequest.Create(url);
            request.Method = "POST";

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;

            using (var requestStream = request.GetRequestStream())
                requestStream.Write(postBytes, 0, postBytes.Length);

            using (var response = request.GetResponse())
            using (var respStream = response.GetResponseStream())
            using (var streamReader = new StreamReader(respStream))
            {
                var str = streamReader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(str);
            }
        }

        public virtual void Dispose()
        {

        }
    }
}