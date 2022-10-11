using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SyT_FileManager.DataAccess;
using PagedList;
using SyT_FileManager.AppCode;
using SyT_FileManager.Models;

namespace SyT_FileManager.Controllers
{
    [ExceptionHandler]
    [AuthorizationHandler]
    public class CatalogosController : Controller
    {
        CatalogoAccess CatalogoAccess;
        RecursoAccess RecursoAccess;

        public CatalogosController()
        {
            CatalogoAccess = new CatalogoAccess();
            RecursoAccess = new RecursoAccess();
        }

        // GET: Catalogos
        public ActionResult Index(int? page, string searchString = "")
        {
            var model = CatalogoAccess.GetRecursos();

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.Where(x => x.RecursoNombre.ToLower().Contains(searchString.ToLower())).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _Recursos(string RecursoID)
        {
            var model = RecursoAccess.GetRecursoItems(RecursoID);
            ViewBag.RecursoID = RecursoID;

            return PartialView("_Recursos", model);
        }

        public ActionResult _Recurso(string RecursoID, string RecursoItemID)
        {
            var model = RecursoAccess.GetRecursoItem(RecursoID, RecursoItemID);
            ViewBag.RecursoItemStatus = new SelectList(RecursoAccess.GetRecursoStatus(), "Value", "Text", model == null ? "" : model.RecursoItemStatus);
            ViewBag.RecursoID = RecursoID;
            ViewBag.ActionType = model == null ? Constants.ADD : Constants.EDIT;

            return PartialView("_Recurso", model);
        }

        [HttpPost]
        public ActionResult _Recurso(RecursoItem recursoItem, string ActionType)
        {
            if (ActionType.Equals(Constants.ADD))
                RecursoAccess.Create(recursoItem);
            else
                RecursoAccess.Update(recursoItem);

            return RedirectToAction("Index");
        }

        public ActionResult _Catalogo(string RecursoID)
        {
            var model = CatalogoAccess.GetRecurso(RecursoID);
            ViewBag.RecursoStatus = new SelectList(CatalogoAccess.GetRecursoStatus(), "Value", "Text", model == null ? "" : model.RecursoStatus);
            ViewBag.ActionType = model == null ? Constants.ADD : Constants.EDIT;

            return PartialView("_Catalogo", model);
        }

        [HttpPost]
        public ActionResult _Catalogo(RecursoModel recurso, string ActionType)
        {
            if (ActionType.Equals(Constants.ADD))
                CatalogoAccess.Create(recurso);
            else
                CatalogoAccess.Update(recurso);

            return RedirectToAction("Index");
        }
    }
}