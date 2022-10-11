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
    public class AgenciasController : Controller
    {
        AgenciaAccess AgenciaAccess;
        RecursoAccess RecursoAccess;
        AlmacenAccess AlmacenAccess;

        public AgenciasController()
        {
            AgenciaAccess = new AgenciaAccess();
            RecursoAccess = new RecursoAccess();
            AlmacenAccess = new AlmacenAccess();
        }

        // GET: Agencias
        public ActionResult Index(int? page, string searchString = "")
        {
            var model = AgenciaAccess.GetAgencias().OrderBy(x => x.AgenciaID).ToList();

            ViewBag.searchString = searchString;
            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.Where(x => x.AgenciaNombre.ToLower().Contains(searchString.ToLower())).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _Agency(int? id)
        {
            var model = AgenciaAccess.GetAgencia(id);
            ViewBag.ActionType = model == null ? Constants.ADD : Constants.EDIT;
            ViewBag.ZonaID = new SelectList(RecursoAccess.GetRecursoItems("ZONA"), "RecursoItemID", "RecursoItemNombre", model == null ? "" : model.ZonaID);
            ViewBag.AgenciaStatus = new SelectList(AgenciaAccess.GetAgenciaStatus(), "Value", "Text", model == null ? "" : model.AgenciaStatus);
            
            return PartialView("_Agency", model);
        }

        [HttpPost]
        public ActionResult _Agency(AgenciaModel agencia, string ActionType)
        {
            if (ActionType.Equals(Constants.ADD))
                AgenciaAccess.Create(agencia);
            else
                AgenciaAccess.Update(agencia);

            return RedirectToAction("Index");
        }

        public ActionResult _AlmacenAgencia(int AgenciaID)
        {
            ViewBag.AlmacenesAsignados = new MultiSelectList(AlmacenAccess.GetAlmacenesAsignados(AgenciaID), "AlmacenID", "AlmacenLabel");
            ViewBag.AlmacenesDisponibles = new MultiSelectList(AlmacenAccess.GetAlmacenesDisponibles(AgenciaID), "AlmacenID", "AlmacenLabel");
            ViewBag.AgenciaID = AgenciaID;

            return PartialView("_AlmacenAgencia");
        }

        [HttpPost]
        public ActionResult _AlmacenAgencia(int AgenciaID, int[] AlmacenesAsignados)
        {
            AlmacenAccess.CreateAlmacenAgencia(AgenciaID, AlmacenesAsignados);

            return RedirectToAction("Index");
        }
    }
}