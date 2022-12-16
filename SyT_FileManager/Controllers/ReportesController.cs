using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
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
        CajaBusiness CajaBusiness;
       
        // GET: Reportes
        public ActionResult Index()
        {
            ViewBag.ReportType = new SelectList(ReportTypes(), "ReportCode", "ReportName");

            return View();
        }

        List<ReportType> ReportTypes()
        {
            return Constants.Privilegios.Where(x => x.PrivId.StartsWith("MODV_REP_"))
                .Select(x => new ReportType
                {
                    ReportCode = x.PrivId,
                    ReportName = x.PrivNombre
                }).ToList();
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
                case "MODV_REP_DOCPREST":
                    return PartialView("_DocumentosPrestados");
                case "MODV_REP_DOCS":
                    return PartialView("_Documentos");
                case "MODV_REP_USR":
                    ViewBag.status = new SelectList(RecursoAccess.GetRecursoItems("USREST"), "RecursoItemID", "RecursoItemNombre");
                    return PartialView("_Usuarios");
                case "MODV_REP_ROLES":
                    var model = new RoleAccess().GetRoles();
                    return PartialView("_Roles", model);
                case "MODV_REP_DOCTRIT":
                    return PartialView("_DocumentosTriturados");
                case "MODV_REP_ARCACT":
                    return PartialView("_ArchivoActivo");
                case "MODV_REP_ARCINA":
                    return PartialView("_ArchivoInactivo");

            }

            return null;
        }

        public ActionResult _ArchivoActivo()
        {
            CajaBusiness = new CajaBusiness();
            var model = CajaBusiness.GetCajasByAlmacenTipo_RP(Constants.GetUserData().UserId, "ACT");
            
            return PartialView("./partials/_ArchivoActivoTable", model);
        }

        public ActionResult _DocumentosTriturados(DocumentosTrituradosBusqueda busqueda)
        {
            DocumentoBusiness = new DocumentoBusiness();
            var model = DocumentoBusiness.GetDocumentosTriturados_RP(busqueda, Constants.GetUserData().UserId);

            return PartialView("./partials/_DocumentosTrituradosTable", model);
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

        public FileResult PrintUsers(List<UsuarioModel> usuarios)
        {
            MemoryStream workStream = new MemoryStream();
            var today = DateTime.Now;

            string fileName = $"Usuarios_{today:yyyyMMddHHmmss}.pdf";
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 10f);

            string[] columnNames = new string[] { "Usuario", "Nombre", "Correo", "Estado", "Agencia", "Roles", "Almacenes" };
            float[] columnWidths = new float[] { 10, 15, 10, 10, 15, 20, 20 };
            PdfPTable tableLayout = new PdfPTable(columnWidths.Length);
            var writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;
            writer.PageEvent = new CustomPdfPageEventHandler();

            doc.Open();

            var title = new Paragraph("Reporte de usuarios", new Font() { Size = 15 }) { Alignment = Element.ALIGN_CENTER };
            doc.Add(title);
            doc.Add(new Chunk("\n"));
            doc.Add(new Paragraph($"Encargado de impresión: {Constants.GetUserData().UserNombre}", new Font() { Size = 10 }));
            doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:yyyy-MM-dd}", new Font() { Size = 10 }));
            var table = GenerateTable(tableLayout, "\n", columnNames, columnWidths);

            var agencias = new AgenciaAccess().GetAgencias();
            foreach (var item in usuarios)
            {
                AddCellToBody(tableLayout, item.UserId);
                AddCellToBody(tableLayout, item.UserNombre);
                AddCellToBody(tableLayout, item.UserEmail);
                AddCellToBody(tableLayout, item.UserStatus);
                AddCellToBody(tableLayout, agencias.Where(x => x.AgenciaID == item.AgenciaID).FirstOrDefault()?.AgenciaNombre ?? "N/D");
                AddCellToBody(tableLayout, string.Join(", ", new RoleAccess().GetRolesUsuarios(item.UserId).Where(x => x.Selected).Select(x => x.RolNombre)));
                AddCellToBody(tableLayout, string.Join(", ", new AlmacenAccess().GetAlmacenesByUsuario(item.UserId).Select(x => x.AlmacenNombre)));
            }

            doc.Add(table);
            doc.Add(new Paragraph($"Total de registros: {usuarios.Count}", new Font { Size = 10 }));

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf", fileName);
        }

        public FileResult PrintDocuments(List<GetDocumentos_RP> documentos)
        {
            MemoryStream workStream = new MemoryStream();
            var today = DateTime.Now;

            string fileName = $"Documentos_{today:yyyyMMddHHmmss}.pdf";
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 10f);

            string[] columnNames = new string[] { "Documento", "Descripción", "Fecha", "Plazo", "Vencimiento", "#Caja", "Agencia", "Departamento" };
            float[] columnWidths = new float[] { 15, 15, 10, 10, 10, 10, 15, 15 };
            PdfPTable tableLayout = new PdfPTable(columnWidths.Length);
            var writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;
            writer.PageEvent = new CustomPdfPageEventHandler();

            doc.Open();

            var title = new Paragraph("Reporte de documentos", new Font() { Size = 15 }) { Alignment = Element.ALIGN_CENTER };
            doc.Add(title);
            doc.Add(new Chunk("\n"));
            doc.Add(new Paragraph($"Encargado de impresión: {Constants.GetUserData().UserNombre}", new Font() { Size = 10 }));
            doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:yyyy-MM-dd}", new Font() { Size = 10 }));
            var table = GenerateTable(tableLayout, "\n", columnNames, columnWidths);

            var almacenes = new AlmacenAccess().GetAlmacenes();

            foreach (var item in documentos)
            {
                AddCellToBody(tableLayout, item.TipoDocNombre);
                AddCellToBody(tableLayout, item.DocDescripcion);
                AddCellToBody(tableLayout, item.DocFechaInfo.ToString("yyyy-MM-dd"));
                AddCellToBody(tableLayout, $"{item.TipoDocPlazo} año(s)");
                AddCellToBody(tableLayout, item.DocFechaVencimiento.ToString("yyyy-MM-dd"));
                AddCellToBody(tableLayout, (almacenes.Where(x => x.AlmacenID == item.AlmacenID).FirstOrDefault()?.AlmacenTipo ?? "ACT") == "ACT" ? item.CajaActivaID : item.CajaInactivaID);
                AddCellToBody(tableLayout, item.AgenciaNombre);
                AddCellToBody(tableLayout, item.Departamento);
            }

            doc.Add(table);
            doc.Add(new Paragraph($"Total de registros: {documentos.Count}", new Font { Size = 10 }));

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf", fileName);
        }

        public FileResult PrintDocumentosPrestados(List<GetDocumentosPrestados_RP> documentos)
        {
            MemoryStream workStream = new MemoryStream();
            var today = DateTime.Now;

            string fileName = $"DocumentosPrestados_{today:yyyyMMddHHmmss}.pdf";
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 10f);

            string[] columnNames = new string[] { "Documento", "Solicitante", "Correo", "Observación", "Fecha solicitó", "Fecha retiró", "Dias prestado", "Usuario entrego" };
            float[] columnWidths = new float[] { 15, 15, 15, 15, 10, 10, 10, 10 };
            PdfPTable tableLayout = new PdfPTable(columnWidths.Length);
            var writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;
            writer.PageEvent = new CustomPdfPageEventHandler();

            doc.Open();

            var title = new Paragraph("Reporte de documentos prestados", new Font() { Size = 15 }) { Alignment = Element.ALIGN_CENTER };
            doc.Add(title);
            doc.Add(new Chunk("\n"));
            doc.Add(new Paragraph($"Encargado de impresión: {Constants.GetUserData().UserNombre}", new Font() { Size = 10 }));
            doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:yyyy-MM-dd}", new Font() { Size = 10 }));
            var table = GenerateTable(tableLayout, "\n", columnNames, columnWidths);

            foreach (var item in documentos)
            {
                AddCellToBody(tableLayout, item.TipoDocNombre);
                AddCellToBody(tableLayout, item.PrestNombreSolicitante);
                AddCellToBody(tableLayout, item.PrestEmailSolicitante);
                AddCellToBody(tableLayout, item.PrestObservacion ?? "");
                AddCellToBody(tableLayout, item.PrestFechaSolicitud.ToString("yyyy-MM-dd"));
                AddCellToBody(tableLayout, item.PrestFechaRetira.ToString("yyyy-MM-dd"));
                AddCellToBody(tableLayout, $"{item.PrestPlazoMaximoDevolucion} dia(s)");
                AddCellToBody(tableLayout, item.PrestUsuarioEntrega);
            }

            doc.Add(table);
            doc.Add(new Paragraph($"Total de registros: {documentos.Count}", new Font { Size = 10 }));

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf", fileName);
        }

        public FileResult PrintDocumentosTriturados(List<GetDocumentosTriturados_RP> documentos)
        {
            MemoryStream workStream = new MemoryStream();
            var today = DateTime.Now;

            string fileName = $"DocumentosTriturados_{today:yyyyMMddHHmmss}.pdf";
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 10f);

            string[] columnNames = new string[] { "#Acta", "Documento", "Plazo", "Fecha trit.", "Usuario", "Testigo", "Agencia", "Departamento" };
            float[] columnWidths = new float[] { 10, 15, 10, 15, 10, 10, 15, 15 };
            PdfPTable tableLayout = new PdfPTable(columnWidths.Length);
            var writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;
            writer.PageEvent = new CustomPdfPageEventHandler();

            doc.Open();

            var title = new Paragraph("Reporte de documentos triturados", new Font() { Size = 15 }) { Alignment = Element.ALIGN_CENTER };
            doc.Add(title);
            doc.Add(new Chunk("\n"));
            doc.Add(new Paragraph($"Encargado de impresión: {Constants.GetUserData().UserNombre}", new Font() { Size = 10 }));
            doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:yyyy-MM-dd}", new Font() { Size = 10 }));
            var table = GenerateTable(tableLayout, "\n", columnNames, columnWidths);

            foreach (var item in documentos)
            {
                AddCellToBody(tableLayout, item.TrituraID);
                AddCellToBody(tableLayout, item.TipoDocNombre);
                AddCellToBody(tableLayout, $"{item.TipoDocPlazo} año(s)");
                AddCellToBody(tableLayout, item.TrituraFecha.ToString("yyyy-MM-dd"));
                AddCellToBody(tableLayout, item.TrituraUsuario);
                AddCellToBody(tableLayout, item.TrituraNombreTestigo);
                AddCellToBody(tableLayout, item.AgenciaNombre);
                AddCellToBody(tableLayout, item.Departamento);
            }

            doc.Add(table);
            doc.Add(new Paragraph($"Total de registros: {documentos.Count}", new Font { Size = 10 }));

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf", fileName);
        }

        public FileResult PrintRoles(List<RoleModel> roles)
        {
            MemoryStream workStream = new MemoryStream();
            var today = DateTime.Now;

            string fileName = $"Roles_{today:yyyyMMddHHmmss}.pdf";
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 10f);

            string[] columnNames = new string[] { "ID", "Rol", "Descripción", "Privilegios" };
            float[] columnWidths = new float[] { 15, 25, 25, 35 };
            PdfPTable tableLayout = new PdfPTable(columnWidths.Length);
            var writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;
            writer.PageEvent = new CustomPdfPageEventHandler();

            doc.Open();

            var title = new Paragraph("Reporte de roles", new Font() { Size = 15 }) { Alignment = Element.ALIGN_CENTER };
            doc.Add(title);
            doc.Add(new Chunk("\n"));
            doc.Add(new Paragraph($"Encargado de impresión: {Constants.GetUserData().UserNombre}", new Font() { Size = 10 }));
            doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:yyyy-MM-dd}", new Font() { Size = 10 }));
            var table = GenerateTable(tableLayout, "\n", columnNames, columnWidths);

            foreach (var item in roles)
            {
                var privilegios = new PrivilegioAccess().GetPrivilegiosByRol(item.RolId);

                AddCellToBody(tableLayout, item.RolId);
                AddCellToBody(tableLayout, item.RolNombre);
                AddCellToBody(tableLayout, item.RolDescripcion);
                AddCellToBody(tableLayout, string.Join(", ", privilegios.Select(x => x.PrivNombre)));
            }

            doc.Add(table);
            doc.Add(new Paragraph($"Total de registros: {roles.Count}", new Font { Size = 10 }));

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf", fileName);
        }

        /// <summary>
        /// Generar la plantilla de una tabla para agregarla al PDF
        /// </summary>
        /// <param name="tableLayout">Objeto de iText para anexar la tabla</param>
        /// <param name="tablePhrase">Frase que describe la tabla</param>
        /// <param name="columnNames">Nombres de las columnas</param>
        /// <param name="headers">Tamaños de cada columna</param>
        /// <returns></returns>
        private PdfPTable GenerateTable(PdfPTable tableLayout, string tablePhrase, string[] columnNames, float[] headers)
        {
            tableLayout.SetWidths(headers);
            tableLayout.WidthPercentage = 100;
            tableLayout.HeaderRows = 1;

            tableLayout.AddCell(new PdfPCell(new Phrase(tablePhrase))
            {
                Colspan = headers.Length,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            foreach (var item in columnNames)
            {
                AddCellToHeader(tableLayout, columnName: item);
            }

            return tableLayout;
        }

        private void AddCellToHeader(PdfPTable tableLayout, string columnName)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(columnName, new Font() { Color = BaseColor.WHITE }))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                BackgroundColor = new BaseColor(134, 4, 20)
            });
        }

        private void AddCellToBody(PdfPTable tableLayout, object value)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(value.ToString(), new Font(Font.FontFamily.HELVETICA, 8f)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                BackgroundColor = new BaseColor(255, 255, 255)
            });
        }
    }
}