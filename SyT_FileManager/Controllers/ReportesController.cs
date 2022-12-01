using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SyT_FileManager.AppCode;
using SyT_FileManager.Business;
using SyT_FileManager.DataAccess;
using SyT_FileManager.Models;
using SyT_FileManager.Models.POCO;

namespace SyT_FileManager.Controllers
{
    [AuthorizationHandler]
    [ExceptionHandler]
    public class ReportesController : Controller
    {
        AgenciaAccess AgenciaAccess;
        OrganizacionAccess OrganizacionAccess;
        RecursoAccess RecursoAccess;
        DocumentoBusiness DocumentoBusiness;
        UsuarioAccess UsuarioAccess;
       
        // GET: Reportes
        public ActionResult Index()
        {
            ViewBag.ReportType = new SelectList(ReportTypes(), "ReportCode", "ReportName");

            return View();
        }

        List<ReportType> ReportTypes()
        {
            List<ReportType> reports = new List<ReportType>
            {
                new ReportType { ReportCode = "USERS", ReportName = "Usuarios" },
                new ReportType { ReportCode = "DOCS", ReportName = "Documentos" },
                new ReportType { ReportCode = "DOC_PREST", ReportName = "Documentos prestados" },
                new ReportType { ReportCode = "ROLES", ReportName = "Roles" }
            };

            return reports;
        }

        public ActionResult LoadReportView(string ReportView)
        {
            AgenciaAccess = new AgenciaAccess();
            OrganizacionAccess = new OrganizacionAccess();
            RecursoAccess = new RecursoAccess();

            ViewBag.AgenciaID = new SelectList(AgenciaAccess.GetAgencias(), "AgenciaID", "AgenciaNombre");
            ViewBag.Departamento = new SelectList(OrganizacionAccess.GetEstructuraInicial(), "EstOrgaID", "EstOrgaNombre");
            ViewBag.PlazoRetencion = new SelectList(RecursoAccess.GetRecursoItems("DOCPLZR"), "RecursoItemID", "RecursoItemNombre");

            switch (ReportView)
            {
                case "DOC_PREST":
                    return PartialView("_DocumentosPrestados");
                case "DOCS":
                    return PartialView("_Documentos");
                case "USERS":
                    ViewBag.status = new SelectList(RecursoAccess.GetRecursoItems("USREST"), "RecursoItemID", "RecursoItemNombre");
                    return PartialView("_Usuarios");
                case "ROLES":
                    var model = new RoleAccess().GetRoles();
                    return PartialView("_Roles", model);
            }

            return null;
        }

        [HttpPost]
        public ActionResult _DocumentosPrestados(DocumentosPrestadosBusqueda busqueda)
        {
            DocumentoBusiness = new DocumentoBusiness();
            var model = DocumentoBusiness.GetDocumentosPrestados_RP(busqueda, Constants.GetUserData().UserId);

            return PartialView("./partials/_DocumentosPrestadosTable", model);
        }

        public ActionResult _Documentos(DocumentosBusqueda busqueda)
        {
            DocumentoBusiness = new DocumentoBusiness();
            var model = DocumentoBusiness.GetDocumentos_RP(busqueda, Constants.GetUserData().UserId);

            return PartialView("./partials/_DocumentosTable", model);
        }

        public ActionResult _Usuarios(UsuariosBusqueda busqueda)
        {
            UsuarioAccess = new UsuarioAccess();
            AgenciaAccess = new AgenciaAccess();
            var model = UsuarioAccess.GetUsuarios();
            var agencias = AgenciaAccess.GetAgencias();

            model.ForEach((UsuarioModel usuario) => usuario.Agencias = agencias);

            if (busqueda.searchStatus && !string.IsNullOrEmpty(busqueda.status))
                model = model.Where(x => x.UserStatus == busqueda.status).ToList();
            if (busqueda.searchName && !string.IsNullOrEmpty(busqueda.name))
                model = model.Where(x => x.UserNombre.ToLower().Contains(busqueda.name.ToLower())).ToList();
            if (busqueda.searchUserId && !string.IsNullOrEmpty(busqueda.userId))
                model = model.Where(x => x.UserId.ToLower().Contains(busqueda.userId.ToLower())).ToList();

            return PartialView("./partials/_UsuariosTable", model);
        }

        public ActionResult _UsuarioDetalles(string UserId)
        {
            ViewBag.Usuario = new UsuarioAccess().GetUsuario(UserId);
            ViewBag.Roles = new RoleAccess().GetRolesUsuarios(UserId).Where(x => x.Selected).ToList();
            ViewBag.Almacenes = new AlmacenAccess().GetAlmacenesByUsuario(UserId);

            return PartialView("./partials/_UsuarioDetalles");
        }

        public ActionResult _RolDetalles(string RolId)
        {
            ViewBag.Rol = new RoleAccess().GetRol(RolId);
            ViewBag.Privilegios = new PrivilegioAccess().GetPrivilegiosByRol(RolId);

            return PartialView("./partials/_RolDetalles");
        }
    }
}