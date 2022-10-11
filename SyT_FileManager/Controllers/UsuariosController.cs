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
    public class UsuariosController : Controller
    {
        RoleAccess RoleAccess;
        UsuarioAccess UsuarioAccess;
        RecursoAccess RecursoAccess;
        AgenciaAccess AgenciaAccess;

        public UsuariosController()
        {
            RoleAccess = new RoleAccess();
            UsuarioAccess = new UsuarioAccess();
            RecursoAccess = new RecursoAccess();
            AgenciaAccess = new AgenciaAccess();
        }

        // GET: Usuarios
        public ActionResult Index(int? page, string usuarioStatus, string searchString = "")
        {
            var model = UsuarioAccess.GetUsuarios().Where(x => (x.UserId.ToLower().Contains(searchString.ToLower()) || x.UserNombre.ToLower().Contains(searchString.ToLower())) && x.UserStatus.Contains(usuarioStatus ?? ""));

            ViewBag.searchString = searchString;
            ViewBag.usuarioStatus = new SelectList(RecursoAccess.GetRecursoItems("USREST"), "RecursoItemID", "RecursoItemNombre", usuarioStatus ?? "");
            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _UserRole(string UserId)
        {
            var data = RoleAccess.GetRolesUsuarios(UserId);
            ViewBag.UserId = UserId;

            return PartialView("_UserRole", data);
        }

        [HttpPost]
        public ActionResult UserRole(List<RoleModel> roles, string UserId)
        {
            RoleAccess.SaveRolesUsuarios(roles, UserId);

            return RedirectToAction("Index");
        }

        public ActionResult _User(string UserId)
        {
            var model = UsuarioAccess.GetUsuario(UserId) ?? new UsuarioModel();
            model.Agencias = AgenciaAccess.GetAgencias();
            ViewBag.EstatusUsuario = new SelectList(RecursoAccess.GetRecursoItems("USREST"), "RecursoItemID", "RecursoItemNombre");
            ViewBag.UserId = UserId;

            return PartialView("_User", model);
        }

        [HttpPost]
        public ActionResult _User(UsuarioModel User)
        {
            UsuarioAccess.SaveUsuario(User);

            return RedirectToAction("Index", "Usuarios");
        }

        public bool ValidateUserId(string UserId)
        {
            return UsuarioAccess.ValidateUserId(UserId);
        }

        public ActionResult _UsuarioAlmacen(string UserId)
        {
            ViewBag.AlmacenesAsignados = new MultiSelectList(UsuarioAccess.GetAlmacenesAsignados(UserId), "AlmacenID", "AlmacenLabel");
            ViewBag.AlmacenesDisponibles = new MultiSelectList(UsuarioAccess.GetAlmacenesDisponibles(UserId), "AlmacenID", "AlmacenLabel");
            ViewBag.UserId = UserId;

            return PartialView("_UsuarioAlmacen");
        }

        [HttpPost]
        public ActionResult _UsuarioAlmacen(string UserId, int[] AlmacenesAsignados)
        {
            UsuarioAccess.CreateUsuarioAlmacen(UserId, AlmacenesAsignados);
            
            return RedirectToAction("Index");
        }
    }
}