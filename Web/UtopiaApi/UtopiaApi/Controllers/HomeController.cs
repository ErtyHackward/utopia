using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
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

            var auth = repo.Auth(login, password);

            // add protobuf serialize 


            return Content("login");
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
