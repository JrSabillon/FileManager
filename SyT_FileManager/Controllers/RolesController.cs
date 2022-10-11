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
    public class RolesController : Controller
    {
        PrivilegioAccess PrivilegioAccess;
        RoleAccess RoleAccess;

        public RolesController()
        {
            PrivilegioAccess = new PrivilegioAccess();
            RoleAccess = new RoleAccess();
        }

        // GET: Roles
        public ActionResult Index()
        {
            var model = RoleAccess.GetRoles();

            return View(model);
        }

        public JsonResult GetPrivilegiosByRol(string RolId)
        {
            var data = PrivilegioAccess.GetPrivilegiosByRol(RolId).Where(x => x.PrivStatus).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _RolPrivilege(string RolId)
        {
            var model = PrivilegioAccess.GetPrivilegiosByRolSelected(RolId).Where(x => x.PrivStatus).ToList();
            ViewBag.RolId = RolId;

            return PartialView("_RolPrivilege", model);
        }

        [HttpPost]
        public ActionResult _RolPrivilege(List<PrivilegioModel> privilegios, string RolId)
        {
            RoleAccess.SaveRolePrivilege(privilegios, RolId);
            Constants.Privilegios = PrivilegioAccess.GetPrivilegios(Constants.GetUserData().UserId).Where(x => x.PrivStatus).ToList();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult _Rol(RoleModel rol)
        {
            RoleAccess.SaveRol(rol);

            return RedirectToAction("Index");
        }
    }
}