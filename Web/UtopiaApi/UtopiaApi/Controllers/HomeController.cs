using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using ProtoBuf;
using UtopiaApi.Helpers;
using UtopiaApi.Models;
using UtopiaApi.Models.Repositories;

namespace UtopiaApi.Controllers
{
    public class HomeController : Controller
    {
        
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

        [HttpPost]
        public ActionResult Register(FormCollection formCollection)
        {
            var regModel = new RegisterModel();

            if (TryUpdateModel(regModel))
            {
                var loginRepo = new LoginRepository();

                if (loginRepo.IsRegistered(regModel.Email))
                {
                    ModelState.AddModelError("", "Such email is already registered.");
                    return View(regModel);
                }

                if(regModel.Password != regModel.RepeatPassword)
                {
                    ModelState.AddModelError("password", "Passwords does not match, please repeat the input");
                    return View(regModel);
                }

                var confirmToken = new Random((int)DateTime.Now.Ticks + regModel.Email.GetHashCode()).NextToken(40);

                loginRepo.Register(regModel.Email, regModel.Password, confirmToken);

                var client = new SmtpClient("smtpout.europe.secureserver.net");
                client.Credentials = new NetworkCredential("support@cubiquest.com", "vm6rFaqz");

                var mail = new MailMessage("support@cubiquest.com", regModel.Email);

                mail.Subject = "Registration on cubiquest";
                mail.Body = string.Format("Hello,\r\nYou or someone else want to register this email on cubiquest. If you want to register, please follow next url, or just ignore this message.\r\n{0}\r\n\r\nRegards,\r\nUtopia team.", "http://api.cubiquest.com/Confirm?token="+confirmToken);

                client.SendAsync(mail, null);

                ViewData["email"] = regModel.Email;

                return View("RegisterSuccess");
            }

            return View(regModel);
        }

        public ActionResult Confirm()
        {
            var token = ControllerContext.HttpContext.Request.Params["token"];

            var repo = new LoginRepository();

            if (repo.Confirm(token))
            {
                ViewData["message"] = "Your email is confirmed! Welcome to the utopia!";
            }

            return View();
        }

        public ActionResult Index()
        {
            return Redirect("http://cubiquest.com");
        }

    }
}
