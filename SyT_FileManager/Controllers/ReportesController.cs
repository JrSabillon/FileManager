using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SyT_FileManager.AppCode;

namespace SyT_FileManager.Controllers
{
    public class ReportesController : Controller
    {
        // GET: Reportes
        public ActionResult Index()
        {
            string message = StaticHelpers.UserType() == UserLevel.Agency ? Constants.AGENCY_USER : Constants.STORE_USER;

            if (Constants.Privilegios.Any(x => x.PrivId.Equals("MODV_ACCT")))
                message += " - Visualizar todos los reportes";

            ViewBag.Message = message;
            return View();
        }
    }
}