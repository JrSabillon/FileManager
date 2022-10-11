using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SyT_FileManager.AppCode;
using SyT_FileManager.DataAccess;

namespace SyT_FileManager.Controllers
{
    public class NavController : Controller
    {
        PrivilegioAccess access;

        public NavController()
        {
            access = new PrivilegioAccess();
        }

        // GET: Nav
        public ActionResult NavBar()
        {
            var data = access.GetPrivilegios(Constants.GetUserData().UserId);
            Constants.Privilegios = data;

            return PartialView("_navbar", data);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
    }
}