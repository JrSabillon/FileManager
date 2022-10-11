using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SyT_FileManager.Models;
using SyT_FileManager.DataAccess;

namespace SyT_FileManager.Controllers
{
    public class LoginController : Controller
    {
        UsuarioAccess UsuarioAccess;

        public LoginController()
        {
            UsuarioAccess = new UsuarioAccess();
        }

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(UsuarioModel User)
        {
            //TODO: Agregar validacion de active directory
            var authUser = UsuarioAccess.GetUsuario(User.UserId);

            if(authUser != null)
            {
                CreateUserCookie(authUser);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Usuario no registrado";
            return View(User);
        }

        void CreateUserCookie(UsuarioModel User)
        {
            var CookieTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["CookieTimeout"]);

            FormsAuthenticationTicket ticket;
            string cookieStr;
            HttpCookie cookie;
            ticket = new FormsAuthenticationTicket(1, User.UserId, DateTime.Now, DateTime.Now.AddMinutes(CookieTimeout), false, JsonConvert.SerializeObject(User));
            cookieStr = FormsAuthentication.Encrypt(ticket);
            cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieStr)
            {
                Path = FormsAuthentication.FormsCookiePath
            };
            Response.Cookies.Add(cookie);
        }
    }
}