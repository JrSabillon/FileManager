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
using System.DirectoryServices.AccountManagement;

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
        public ActionResult Index(UsuarioModel User, string UserPassword)
        {
            string ActiveDomain = ConfigurationManager.AppSettings["domain"] ?? string.Empty;

            try
            {
                if (ActiveDomain.Equals(string.Empty))
                {
                    var authUser = UsuarioAccess.GetUsuario(User.UserId);

                    if (authUser != null)
                    {
                        CreateUserCookie(authUser);

                        return RedirectToAction("Index", "Home");
                    }

                    ViewBag.Message = "Usuario NO Registrado o Credenciales incorrectas [0]";
                    return View(User);
                }
                else
                {
                    using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, ActiveDomain))
                    {
                        if (pc.ValidateCredentials(User.UserId, UserPassword))
                        {
                            var authUser = UsuarioAccess.GetUsuario(User.UserId);

                            if (authUser != null)
                            {
                                CreateUserCookie(authUser);

                                return RedirectToAction("Index", "Home");
                            }

                            ViewBag.Message = "Usuario NO Registrado o Credenciales incorrectas [2]";
                            return View(User);
                        }

                        ViewBag.Message = "Usuario NO Registrado o Credenciales incorrectas [1]";
                        return View(User);
                    }
                }
            }
            catch(PrincipalServerDownException LDAPexc)
            {
                ViewBag.Message = "Servidor de Active Directory no disponible.";
                return View(User);
            }
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