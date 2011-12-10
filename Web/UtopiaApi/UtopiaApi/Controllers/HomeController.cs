using System;
using System.IO;
using System.Web.Mvc;
using ProtoBuf;
using UtopiaApi.Helpers;
using UtopiaApi.Models;
using UtopiaApi.Models.Repositories;

namespace UtopiaApi.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /
        
        public ActionResult Login()
        {
            var login = ControllerContext.HttpContext.Request.Params["login"];
            var password = ControllerContext.HttpContext.Request.Params["pass"];

            var repo = new LoginRepository();
            var user = repo.Auth(login, password);

            LoginResponce loginResponce;

            if (user != null)
            {
                var r = new Random(DateTime.Now.Millisecond + login.GetHashCode());
                var token = r.NextToken();
                loginResponce = new LoginResponce { Logged = true, Token = token };
                repo.WriteToken(user.id, token);
            }
            else
            {
                loginResponce = new LoginResponce { Logged = false, Error = "Invalid login/password combination" };
            }

            // add protobuf serialize 
            var ms = new MemoryStream();
            
            Serializer.Serialize(ms, loginResponce);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/octet-stream");
        }

        public ActionResult LogOff()
        {
            var token = ControllerContext.HttpContext.Request.Params["token"];

            var repo = new LoginRepository();
            repo.DeleteToken(token);

            return new EmptyResult();
        }

        public ActionResult Register()
        {

            return View();
        }

        public ActionResult Index()
        {


            string result = "";

            //if (!string.IsNullOrEmpty(password))
            //{

            //    var encoded = Encoding.UTF8.GetBytes(password);

            //    var sha1 = SHA1.Create();

            //    byte[] hash = sha1.ComputeHash(encoded, 0, encoded.Length);
            //    StringBuilder formatted = new StringBuilder(hash.Length);
            //    foreach (byte b in hash)
            //    {
            //        formatted.AppendFormat("{0:X2}", b);
            //    }

            //    //var client = new SmtpClient("smtpout.europe.secureserver.net");
            //    //client.Credentials = new NetworkCredential("support@cubiquest.com", "vm6rFaqz");

            //    //var mail = new MailMessage("support@cubiquest.com", "hackward@gmail.com");

            //    //mail.Subject = "Registration on cubiquest";
            //    //mail.Body = string.Format("Hello,\r\nYou or someone else want to register this email on cubiquest. If you want to register, please follow next url, or just ignore this message.\r\n{0}\r\n\r\nRegards,\r\nUtopia team.","http://api.cubiquest.com/Confirm?code=ABCDEFGHIJKLMNOPQRSTUVW");

            //    //client.SendAsync(mail, null);

            //    result += formatted;
            //}



            return Content(result + " Length:"+result.Length);
        }

    }
}
