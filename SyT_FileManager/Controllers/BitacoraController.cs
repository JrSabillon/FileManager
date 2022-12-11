using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SyT_FileManager.AppCode;
using SyT_FileManager.DataAccess;
using SyT_FileManager.Models.POCO;
using PagedList;

namespace SyT_FileManager.Controllers
{
    [AuthorizationHandler]
    [ExceptionHandler]
    public class BitacoraController : Controller
    {
        BitacoraAccess BitacoraAccess;
        // GET: Bitacora
        public ActionResult Index(int? page, BitacoraAuditoriaBusqueda busqueda)
        {
            BitacoraAccess = new BitacoraAccess();
            var model = BitacoraAccess.GetAll();

            ViewBag.SearchDate = busqueda.SearchDate;
            ViewBag.SearchActionType = busqueda.SearchActionType;
            ViewBag.Accion = new SelectList(model.Select(x => x.Accion).Distinct());
            ViewBag.SelectedAccion = busqueda.Accion;
            ViewBag.FechaInicio = busqueda.FechaInicio;
            ViewBag.FechaFin = busqueda.FechaFin;

            if (busqueda.SearchDate && busqueda.FechaInicio.HasValue && busqueda.FechaFin.HasValue)
                model = model.Where(x => x.Fecha >= busqueda.FechaInicio && x.Fecha <= busqueda.FechaFin).ToList();
            if (busqueda.SearchActionType && !string.IsNullOrEmpty(busqueda.Accion))
                model = model.Where(x => x.Accion == busqueda.Accion).ToList();

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);
            
            return View(model.OrderByDescending(x => x.Fecha).ToPagedList(pageNumber, pageSize));
        }
    }
}