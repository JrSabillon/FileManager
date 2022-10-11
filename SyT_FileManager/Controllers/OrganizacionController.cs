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
    public class OrganizacionController : Controller
    {
        OrganizacionAccess OrganizacionAccess;

        public OrganizacionController()
        {
            OrganizacionAccess = new OrganizacionAccess();
        }

        // GET: Organizacion
        public ActionResult Index()
        {
            var model = OrganizacionAccess.GetEstructuraInicial();
            GetEstructurasAnidadas(model);

            return View(model);
        }

        public void GetEstructurasAnidadas(List<EstructuraOrganizacionalModel> data)
        {
            foreach (var item in data)
            {
                item.Estructuras = OrganizacionAccess.GetNodosEstructura(item.EstOrgaID);

                if(item.Estructuras != null)
                {
                    GetEstructurasAnidadas(item.Estructuras);
                }
            }
        }

        public ActionResult _EstructuraOrganizacional(int EstOrgaID, int? EstOrgaIDPadre)
        {
            var model = OrganizacionAccess.GetEstructuraOrganizacional(EstOrgaID);
            ViewBag.ActionType = model == null ? Constants.ADD : Constants.EDIT;
            ViewBag.EstOrgaStatus = new SelectList(OrganizacionAccess.GetEstructuraStatus(), "Value", "Text", model == null ? "" : model.EstOrgaStatus);
            ViewBag.EstOrgaIDPadre = model == null ? EstOrgaIDPadre : model.EstOrgaIDPadre;

            return PartialView("_EstructuraOrganizacional", model);
        }

        [HttpPost]
        public ActionResult _EstructuraOrganizacional(EstructuraOrganizacionalModel estructuraOrganizacional, string ActionType)
        {
            if (ActionType.Equals(Constants.ADD))
                OrganizacionAccess.Create(estructuraOrganizacional);
            else
                OrganizacionAccess.Update(estructuraOrganizacional);

            return RedirectToAction("Index");
        }
    }
}