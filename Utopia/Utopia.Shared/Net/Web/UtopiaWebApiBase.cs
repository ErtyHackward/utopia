using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Utopia.Shared.Net.Web
{
    public abstract class UtopiaWebApiBase : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

                    if (callback != null)
                        callback(ea);

                }, request);
            }
            ).BeginInvoke(null, null);
        }

        /// <summary>
        /// Performs a post http request, use IAsyncResult.State as WebRequest class
        /// </summary>
        /// <param name="url"></param>
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

                    if (callback != null)
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

        public struct UploadFileInfo
        {
            public string FilePath;
            public string ParamName;
            public string ContentType;
        }

        public static void HttpUploadFiles(string url, IEnumerable<UploadFileInfo> files, NameValueCollection fields)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            var wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in fields.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, fields[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }

            foreach (var uploadFileInfo in files)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);

                string headerTemplate =
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, uploadFileInfo.ParamName, uploadFileInfo.FilePath, uploadFileInfo.ContentType);
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);

                using (var fileStream = new FileStream(uploadFileInfo.FilePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    while (( bytesRead = fileStream.Read(buffer, 0, buffer.Length) ) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                    }
                }
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                var stream2 = wresp.GetResponseStream();
                var reader2 = new StreamReader(stream2);
                var resp = reader2.ReadToEnd();
                Trace.WriteLine(resp);

            }
            catch (Exception ex)
            {

                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                throw;
            }
        }

        public static void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            var wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while (( bytesRead = fileStream.Read(buffer, 0, buffer.Length) ) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
            }
            
            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                var resp = reader2.ReadToEnd();
                Trace.WriteLine(resp);

            }
            catch (Exception ex)
            {
                
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                throw;
            }
        }

        public virtual void Dispose()
        {

        }
    }
}