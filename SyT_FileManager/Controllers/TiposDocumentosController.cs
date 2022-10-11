using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SyT_FileManager.AppCode;
using SyT_FileManager.DataAccess;
using SyT_FileManager.Models;

namespace SyT_FileManager.Controllers
{
    [ExceptionHandler]
    [AuthorizationHandler]
    public class TiposDocumentosController : Controller
    {
        TipoDocumentoAccess TipoDocumentoAccess;
        RecursoAccess RecursoAccess;

        public TiposDocumentosController()
        {
            TipoDocumentoAccess = new TipoDocumentoAccess();
            RecursoAccess = new RecursoAccess();
        }

        // GET: TiposDocumentos
        public ActionResult Index(int? page, string searchString = "")
        {
            var model = TipoDocumentoAccess.GetTipoDocumentos();

            ViewBag.searchString = searchString;
            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.Where(x => x.TipoDocNombre.ToLower().Contains(searchString.ToLower())).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _TipoDocumento(int TipoDocID)
        {
            var model = TipoDocumentoAccess.GetTipoDocumento(TipoDocID);

            ViewBag.ActionType = model == null ? Constants.ADD : Constants.EDIT;
            ViewBag.TipoDocPlazo = new SelectList(RecursoAccess.GetRecursoItems("DOCPLZR"), "RecursoItemID", "RecursoItemNombre", model == null ? "" : model.TipoDocPlazo.ToString());
            ViewBag.TipoDocStatus = new SelectList(TipoDocumentoAccess.GetTipoDocumentoStatus(), "Value", "Text", model == null ? "" : model.TipoDocStatus);

            return PartialView("_TipoDocumento", model);
        }

        [HttpPost]
        public ActionResult _TipoDocumento(TipoDocumentoModel tipoDocumento, string ActionType)
        {
            if (ActionType.Equals(Constants.ADD))
                TipoDocumentoAccess.Create(tipoDocumento);
            else
                TipoDocumentoAccess.Update(tipoDocumento);

            return RedirectToAction("Index");
        }
    }
}