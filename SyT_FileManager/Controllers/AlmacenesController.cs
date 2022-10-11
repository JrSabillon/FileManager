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
    public class AlmacenesController : Controller
    {
        AlmacenAccess AlmacenAccess;
        AgenciaAccess AgenciaAccess;
        RecursoAccess RecursoAccess;
        public AlmacenesController()
        {
            AlmacenAccess = new AlmacenAccess();
            AgenciaAccess = new AgenciaAccess();
            RecursoAccess = new RecursoAccess();
        }

        // GET: Almacenes
        public ActionResult Index()
        {
            var model = AlmacenAccess.GetAlmacenes();

            return View(model);
        }

        public ActionResult _Almacen(int AlmacenID)
        {
            var model = AlmacenAccess.GetAlmacen(AlmacenID) ?? new AlmacenModel();

            model.Agencias = AgenciaAccess.GetAgencias();
            model.Zonas = RecursoAccess.GetRecursoItems("ZONA");
            model.TipoAlmacenes = RecursoAccess.GetRecursoItems("ALMTIPO");

            ViewBag.ActionType = model.AlmacenID == 0 ? Constants.ADD : Constants.EDIT;
            ViewBag.AlmacenStatus = new SelectList(AlmacenAccess.GetAlmacenStatus(), "Value", "Text");

            return PartialView("_Almacen", model);
        }

        [HttpPost]
        public ActionResult _Almacen(AlmacenModel almacen, string ActionType)
        {
            if (ActionType.Equals(Constants.ADD))
                AlmacenAccess.Create(almacen);
            else
                AlmacenAccess.Update(almacen);

            return RedirectToAction("Index");
        }
    }
}