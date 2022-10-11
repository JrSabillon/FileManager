using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SyT_FileManager.AppCode;
using SyT_FileManager.DataAccess;
using PagedList;
using SyT_FileManager.Models;

namespace SyT_FileManager.Controllers
{
    [ExceptionHandler]
    [AuthorizationHandler]
    public class BancosController : Controller
    {
        BancoAccess BancoAccess;

        public BancosController()
        {
            BancoAccess = new BancoAccess();
        }

        // GET: Banco
        public ActionResult Index(int? page, string searchString = "")
        {
            var model = BancoAccess.GetBancos();

            ViewBag.searchString = searchString;
            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.Where(x => x.BancoNombre.ToLower().Contains(searchString.ToLower())).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _Bank(int BancoID)
        {
            var model = BancoAccess.GetBanco(BancoID);
            ViewBag.BancoStatus = new SelectList(BancoAccess.GetBancoStatus(), "Value", "Text", model == null ? "" : model.BancoStatus);
            ViewBag.ActionType = model == null ? Constants.ADD : Constants.EDIT;

            return PartialView("_Bank", model);
        }

        [HttpPost]
        public ActionResult _Bank(BancoModel Banco, string ActionType)
        {
            if (ActionType.Equals(Constants.ADD))
                BancoAccess.Create(Banco);
            else
                BancoAccess.Update(Banco);

            return RedirectToAction("Index");
        }
    }
}