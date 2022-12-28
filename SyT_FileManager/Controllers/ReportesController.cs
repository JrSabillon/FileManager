using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
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

        public ActionResult _ArchivoActivo(CajasByAlmacenBusqueda busqueda)
        {
            CajaBusiness = new CajaBusiness();
            var model = CajaBusiness.GetCajasByAlmacenTipo_RP(busqueda, Constants.GetUserData().UserId, "ACT");
            
            return PartialView("./partials/_ArchivoActivoTable", model);
        }

        public ActionResult _ArchivoInactivo(CajasByAlmacenBusqueda busqueda)
        {
            CajaBusiness = new CajaBusiness();
            var model = CajaBusiness.GetCajasByAlmacenTipo_RP(busqueda, Constants.GetUserData().UserId, "INA");

            return PartialView("./partials/_ArchivoInactivoTable", model);
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
            OrganizacionAccess = new OrganizacionAccess();
            var model = DocumentoBusiness.GetDocumentosPrestados_RP(busqueda, Constants.GetUserData().UserId);
            var departamentos = OrganizacionAccess.GetEstructuraInicial();

            model.ForEach(item => item.NombreDepartamento = departamentos.Where(x => x.EstOrgaID == item.Departamento).FirstOrDefault().EstOrgaNombre);

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

        private byte[] PrintUsersExcel(List<UsuarioModel> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();

            ExcelWorksheet sheet = Ep.Workbook.Worksheets.Add("Usuarios");
            sheet.Cells["A1:G1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Value = "Usuario";
            sheet.Cells["B1"].Value = "Nombre";
            sheet.Cells["C1"].Value = "Correo";
            sheet.Cells["D1"].Value = "Estado";
            sheet.Cells["E1"].Value = "Agencia";
            sheet.Cells["F1"].Value = "Roles";
            sheet.Cells["G1"].Value = "Almacenes";

            var agencias = new AgenciaAccess().GetAgencias();
            int row = 2;
            foreach (var item in data)
            {
                sheet.Cells[$"A{row}"].Value = item.UserId;
                sheet.Cells[$"B{row}"].Value = item.UserNombre;
                sheet.Cells[$"C{row}"].Value = item.UserEmail;
                sheet.Cells[$"D{row}"].Value = item.UserStatus;
                sheet.Cells[$"E{row}"].Value = agencias.Where(x => x.AgenciaID == item.AgenciaID).FirstOrDefault()?.AgenciaNombre ?? "N/D";
                sheet.Cells[$"F{row}"].Value = string.Join(", ", new RoleAccess().GetRolesUsuarios(item.UserId).Where(x => x.Selected).Select(x => x.RolNombre));
                sheet.Cells[$"G{row}"].Value = string.Join(", ", new AlmacenAccess().GetAlmacenesByUsuario(item.UserId).Select(x => x.AlmacenNombre));

                row++;
            }
            sheet.Cells["A:AZ"].AutoFitColumns();

            return Ep.GetAsByteArray();
        }

        public FileResult PrintUsers(List<UsuarioModel> usuarios, string extension)
        {
            var today = DateTime.Now;
            string fileName = $"Usuarios_{today:yyyyMMddHHmmss}{extension}";

            if (extension.Equals(Constants.ExcelExtension))
            {
                var excelFile = PrintUsersExcel(usuarios);

                return File(excelFile, "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            MemoryStream workStream = new MemoryStream();

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

        private byte[] PrintDocumentsExcel(List<GetDocumentos_RP> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();

            ExcelWorksheet sheet = Ep.Workbook.Worksheets.Add("Documentos");
            sheet.Cells["A1:I1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Value = "Documento";
            sheet.Cells["B1"].Value = "Descripción";
            sheet.Cells["C1"].Value = "Fecha";
            sheet.Cells["D1"].Value = "Plazo";
            sheet.Cells["E1"].Value = "Vencimiento";
            sheet.Cells["F1"].Value = "#Caja";
            sheet.Cells["G1"].Value = "Agencia";
            sheet.Cells["H1"].Value = "Departamento";
            sheet.Cells["I1"].Value = "Almacen";

            var almacenes = new AlmacenAccess().GetAlmacenes();
            int row = 2;
            foreach (var item in data)
            {
                var almacen = almacenes.Where(x => x.AlmacenID == item.AlmacenID).FirstOrDefault();
                sheet.Cells[$"A{row}"].Value = item.TipoDocNombre;
                sheet.Cells[$"B{row}"].Value = item.DocDescripcion;
                sheet.Cells[$"C{row}"].Value = item.DocFechaInfo;
                sheet.Cells[$"C{row}"].Style.Numberformat.Format = "yyyy-MM-dd";
                sheet.Cells[$"D{row}"].Value = item.TipoDocPlazo;
                sheet.Cells[$"E{row}"].Value = item.DocFechaVencimiento;
                sheet.Cells[$"E{row}"].Style.Numberformat.Format = "yyyy-MM-dd";
                sheet.Cells[$"F{row}"].Value = (almacen?.AlmacenTipo ?? "ACT") == "ACT" ? item.CajaActivaID : item.CajaInactivaID;
                sheet.Cells[$"G{row}"].Value = item.AgenciaNombre;
                sheet.Cells[$"H{row}"].Value = item.Departamento;
                sheet.Cells[$"I{row}"].Value = almacen.AlmacenNombre;

                row++;
            }
            sheet.Cells["A:AZ"].AutoFitColumns();

            return Ep.GetAsByteArray();
        }

        public FileResult PrintDocuments(List<GetDocumentos_RP> documentos, string extension)
        {
            var today = DateTime.Now;
            string fileName = $"Documentos_{today:yyyyMMddHHmmss}{extension}";

            if (extension.Equals(Constants.ExcelExtension))
            {
                var excelFile = PrintDocumentsExcel(documentos);

                return File(excelFile, "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            MemoryStream workStream = new MemoryStream();
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 10f);

            string[] columnNames = new string[] { "Documento", "Descripción", "Fecha", "Plazo", "Vencimiento", "#Caja", "Agencia", "Departamento", "Almacen" };
            float[] columnWidths = new float[] { 15, 15, 10, 10, 10, 10, 15, 15, 15 };
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
                var almacen = almacenes.Where(x => x.AlmacenID == item.AlmacenID).FirstOrDefault();
                AddCellToBody(tableLayout, item.TipoDocNombre);
                AddCellToBody(tableLayout, item.DocDescripcion);
                AddCellToBody(tableLayout, item.DocFechaInfo.ToString("yyyy-MM-dd"));
                AddCellToBody(tableLayout, $"{item.TipoDocPlazo} año(s)");
                AddCellToBody(tableLayout, item.DocFechaVencimiento.ToString("yyyy-MM-dd"));
                AddCellToBody(tableLayout, (almacen?.AlmacenTipo ?? "ACT") == "ACT" ? item.CajaActivaID : item.CajaInactivaID);
                AddCellToBody(tableLayout, item.AgenciaNombre);
                AddCellToBody(tableLayout, item.Departamento);
                AddCellToBody(tableLayout, almacen.AlmacenNombre);
            }

            doc.Add(table);
            doc.Add(new Paragraph($"Total de registros: {documentos.Count}", new Font { Size = 10 }));

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf", fileName);
        }

        private byte[] PrintDocumentosPrestadosExcel(List<GetDocumentosPrestados_RP> data)
        {
            OrganizacionAccess = new OrganizacionAccess();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();

            ExcelWorksheet sheet = Ep.Workbook.Worksheets.Add("Documentos prestados");
            sheet.Cells["A1:J1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Value = "Documento";
            sheet.Cells["B1"].Value = "Descripción";
            sheet.Cells["C1"].Value = "Departamento";
            sheet.Cells["D1"].Value = "Solicitante";
            sheet.Cells["E1"].Value = "Correo";
            sheet.Cells["F1"].Value = "Observación";
            sheet.Cells["G1"].Value = "Fecha solicitó";
            sheet.Cells["H1"].Value = "Fecha retiró";
            sheet.Cells["I1"].Value = "Dias prestados";
            sheet.Cells["J1"].Value = "Usuario entrego";

            int row = 2;
            //var departamentos = OrganizacionAccess.GetEstructuraInicial();
            foreach (var item in data)
            {
                //item.NombreDepartamento = departamentos.Where(x => x.EstOrgaID == item.Departamento).FirstOrDefault()?.EstOrgaNombre;
                sheet.Cells[$"A{row}"].Value = item.TipoDocNombre;
                sheet.Cells[$"B{row}"].Value = item.DocDescripcion;
                sheet.Cells[$"C{row}"].Value = item.NombreDepartamento;
                sheet.Cells[$"D{row}"].Value = item.PrestNombreSolicitante;
                sheet.Cells[$"E{row}"].Value = item.PrestEmailSolicitante;
                sheet.Cells[$"F{row}"].Value = item.PrestObservacion ?? "";
                sheet.Cells[$"G{row}"].Value = item.PrestFechaSolicitud;
                sheet.Cells[$"G{row}"].Style.Numberformat.Format = "yyyy-MM-dd";
                sheet.Cells[$"H{row}"].Value = item.PrestFechaRetira;
                sheet.Cells[$"H{row}"].Style.Numberformat.Format = "yyyy-MM-dd";
                sheet.Cells[$"I{row}"].Value = item.PrestPlazoMaximoDevolucion;
                sheet.Cells[$"J{row}"].Value = item.PrestUsuarioEntrega;

                row++;
            }
            sheet.Cells["A:AZ"].AutoFitColumns();

            return Ep.GetAsByteArray();
        }

        public FileResult PrintDocumentosPrestados(List<GetDocumentosPrestados_RP> documentos, string extension)
        {
            OrganizacionAccess = new OrganizacionAccess();
            var today = DateTime.Now;
            string fileName = $"DocumentosPrestados_{today:yyyyMMddHHmmss}{extension}";

            if (extension.Equals(Constants.ExcelExtension))
            {
                var excelFile = PrintDocumentosPrestadosExcel(documentos);

                return File(excelFile, "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            MemoryStream workStream = new MemoryStream();
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 10f);

            string[] columnNames = new string[] { "Documento", "Descripción", "Depto.", "Solicitante", "Correo", "Observación", "Solicitó", "Retiró", "Dias prest.", "Usuario" };
            float[] columnWidths = new float[] { 15, 15, 15, 15, 15, 15, 10, 10, 10, 10 };
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

            //var departamentos = OrganizacionAccess.GetEstructuraInicial();
            foreach (var item in documentos)
            {
                //item.NombreDepartamento = departamentos.Where(x => x.EstOrgaID == item.Departamento).FirstOrDefault()?.EstOrgaNombre;
                AddCellToBody(tableLayout, item.TipoDocNombre);
                AddCellToBody(tableLayout, item.DocDescripcion);
                AddCellToBody(tableLayout, item.NombreDepartamento);
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

        private byte[] PrintDocumentosTrituradosExcel(List<GetDocumentosTriturados_RP> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();

            ExcelWorksheet sheet = Ep.Workbook.Worksheets.Add("Documentos triturados");
            sheet.Cells["A1:I1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Value = "#Acta";
            sheet.Cells["B1"].Value = "Documentos";
            sheet.Cells["C1"].Value = "Descripción";
            sheet.Cells["D1"].Value = "Plazo";
            sheet.Cells["E1"].Value = "Fecha trit.";
            sheet.Cells["F1"].Value = "Usuario";
            sheet.Cells["G1"].Value = "Testigo";
            sheet.Cells["H1"].Value = "Agencia";
            sheet.Cells["I1"].Value = "Departamento";

            int row = 2;
            foreach (var item in data)
            {
                sheet.Cells[$"A{row}"].Value = item.TrituraID;
                sheet.Cells[$"B{row}"].Value = item.TipoDocNombre;
                sheet.Cells[$"C{row}"].Value = item.DocDescripcion;
                sheet.Cells[$"D{row}"].Value = item.TipoDocPlazo;
                sheet.Cells[$"E{row}"].Value = item.TrituraFecha;
                sheet.Cells[$"E{row}"].Style.Numberformat.Format = "yyyy-MM-dd";
                sheet.Cells[$"F{row}"].Value = item.TrituraUsuario;
                sheet.Cells[$"G{row}"].Value = item.TrituraNombreTestigo;
                sheet.Cells[$"H{row}"].Value = item.AgenciaNombre;
                sheet.Cells[$"I{row}"].Value = item.Departamento;

                row++;
            }
            sheet.Cells["A:AZ"].AutoFitColumns();

            return Ep.GetAsByteArray();
        }

        public FileResult PrintDocumentosTriturados(List<GetDocumentosTriturados_RP> documentos, string extension)
        {
            var today = DateTime.Now;
            string fileName = $"DocumentosTriturados_{today:yyyyMMddHHmmss}{extension}";

            if (extension.Equals(Constants.ExcelExtension))
            {
                var excelFile = PrintDocumentosTrituradosExcel(documentos);

                return File(excelFile, "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            MemoryStream workStream = new MemoryStream();
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 10f);

            string[] columnNames = new string[] { "#Acta", "Documento", "Descripción", "Plazo", "Fecha trit.", "Usuario", "Testigo", "Agencia", "Depto." };
            float[] columnWidths = new float[] { 10, 15, 15, 10, 15, 10, 10, 15, 15 };
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
                AddCellToBody(tableLayout, item.DocDescripcion);
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

        private byte[] PrintRolesExcel(List<RoleModel> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();

            ExcelWorksheet sheet = Ep.Workbook.Worksheets.Add("Roles");
            sheet.Cells["A1:D1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Value = "ID";
            sheet.Cells["B1"].Value = "Rol";
            sheet.Cells["C1"].Value = "Descripción";
            sheet.Cells["D1"].Value = "Privilegios";

            int row = 2;
            foreach (var item in data)
            {
                sheet.Cells[$"A{row}"].Value = item.RolId;
                sheet.Cells[$"B{row}"].Value = item.RolNombre;
                sheet.Cells[$"C{row}"].Value = item.RolDescripcion;
                sheet.Cells[$"D{row}"].Value = string.Join(", ", new PrivilegioAccess().GetPrivilegiosByRol(item.RolId).Select(x => x.PrivNombre));
                
                row++;
            }
            sheet.Cells["A:AZ"].AutoFitColumns();

            return Ep.GetAsByteArray();
        }

        public FileResult PrintRoles(List<RoleModel> roles, string extension)
        {
            var today = DateTime.Now;
            string fileName = $"Roles_{today:yyyyMMddHHmmss}{extension}";

            if (extension.Equals(Constants.ExcelExtension))
            {
                var excelFile = PrintRolesExcel(roles);

                return File(excelFile, "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            MemoryStream workStream = new MemoryStream();
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

        private byte[] PrintCajasAlmacenExcel(List<GetCajasByAlmacenTipo_RP> data, string TipoAlmacen)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Ep = new ExcelPackage();
            TipoDocumentoAccess tipoDocumentoAccess = new TipoDocumentoAccess();
            DocumentoAccess documentoAccess = new DocumentoAccess();
            OrganizacionAccess = new OrganizacionAccess();

            ExcelWorksheet sheet = Ep.Workbook.Worksheets.Add(TipoAlmacen.Equals("ACT") ? "Cajas archivo act." : "Cajas archivo ina.");
            sheet.Cells["A1:J1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Value = "#Caja";
            sheet.Cells["B1"].Value = "Fecha";
            sheet.Cells["C1"].Value = "Usuario envío";
            sheet.Cells["D1"].Value = "Almacen";
            sheet.Cells["E1"].Value = "Estante";
            sheet.Cells["F1"].Value = "Sección";
            sheet.Cells["G1"].Value = "Nivel";
            sheet.Cells["H1"].Value = "Fila";
            sheet.Cells["I1"].Value = "Ubicación";
            //sheet.Cells["J1"].Value = "Documentos";

            ExcelWorksheet sheetDocuments = Ep.Workbook.Worksheets.Add("Documentos");
            sheetDocuments.Cells["A1:D1"].Style.Font.Bold = true;
            sheetDocuments.Cells["A1"].Value = "#Caja";
            sheetDocuments.Cells["B1"].Value = "Documento";
            sheetDocuments.Cells["C1"].Value = "Descripción";
            sheetDocuments.Cells["D1"].Value = "Departamento";

            var tiposDocumentos = tipoDocumentoAccess.GetTipoDocumentos();
            var estructuraOrganizacional = OrganizacionAccess.GetAllEstructuraOrganizacional();
            int row = 2;
            int rowDocuments = 2;
            foreach (var item in data)
            {
                var documentos = documentoAccess.GetDocumentosByCajaID(item.CajaID);
                documentos.ForEach(x => {
                    var centroCosto = estructuraOrganizacional.Where(y => y.EstOrgaID == x.DocCCCCID).FirstOrDefault();
                    x.TiposDocumentos = tiposDocumentos;
                    sheetDocuments.Cells[$"A{rowDocuments}"].Value = TipoAlmacen.Equals("ACT") ? item.CajaActivaID : item.CajaInactivaID;
                    sheetDocuments.Cells[$"B{rowDocuments}"].Value = x.SelectedDocumento.TipoDocNombre;
                    sheetDocuments.Cells[$"C{rowDocuments}"].Value = x.DocDescripcion;
                    sheetDocuments.Cells[$"D{rowDocuments}"].Value = estructuraOrganizacional.Where(y => y.EstOrgaID == centroCosto.EstOrgaIDPadre).FirstOrDefault().EstOrgaNombre;

                    rowDocuments++;
                });

                sheet.Cells[$"A{row}"].Value = TipoAlmacen.Equals("ACT") ? item.CajaActivaID : item.CajaInactivaID;
                sheet.Cells[$"B{row}"].Value = item.CajaFechaRecepcion;
                sheet.Cells[$"B{row}"].Style.Numberformat.Format = "yyyy-MM-dd";
                sheet.Cells[$"C{row}"].Value = item.CajaPersonaEntrega;
                sheet.Cells[$"D{row}"].Value = item.AlmacenNombre;
                sheet.Cells[$"E{row}"].Value = item.Estante;
                sheet.Cells[$"F{row}"].Value = item.Seccion;
                sheet.Cells[$"G{row}"].Value = item.Nivel;
                sheet.Cells[$"H{row}"].Value = item.Fila;
                sheet.Cells[$"I{row}"].Value = item.Ubicacion;
                //sheet.Cells[$"J{row}"].Value = string.Join(", ", documentos.Select(x => x.SelectedDocumento.TipoDocNombre));

                row++;
            }
            sheet.Cells["A:AZ"].AutoFitColumns();
            sheetDocuments.Cells["A:AZ"].AutoFitColumns();
            
            return Ep.GetAsByteArray();
        }

        public FileResult PrintCajasAlmacen(List<GetCajasByAlmacenTipo_RP> cajas, string TipoAlmacen, string extension)
        {
            var today = DateTime.Now;
            string fileName = TipoAlmacen.Equals("ACT") ? $"CajasArchivoActivo_{today:yyyyMMddHHmmss}{extension}" : $"CajasArchivoInactivo_{today:yyyyMMddHHmmss}{extension}";

            if (extension.Equals(Constants.ExcelExtension))
            {
                var excelFile = PrintCajasAlmacenExcel(cajas, TipoAlmacen);

                return File(excelFile, "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            DocumentoAccess documentoAccess = new DocumentoAccess();
            OrganizacionAccess = new OrganizacionAccess();
            TipoDocumentoAccess tipoDocumentoAccess = new TipoDocumentoAccess();
            MemoryStream workStream = new MemoryStream();
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 50f);

            //string[] columnNames = new string[] { "#Caja", "Fecha", "Usuario envío", "Almacen", "Estante", "Sección", "Nivel", "Fila", "Ubicación" };
            //float[] columnWidths = new float[] { 8, 15, 10, 10, 15, 12, 10, 10, 10 };
            var writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;
            writer.PageEvent = new CustomPdfPageEventHandler();

            doc.Open();

            string tipoReporte = TipoAlmacen.Equals("ACT") ? "Almacen Activo" : "Almacen Inactivo";
            var title = new Paragraph($"Reporte de cajas - {tipoReporte}", new Font() { Size = 15 }) { Alignment = Element.ALIGN_CENTER };
            doc.Add(title);
            doc.Add(new Chunk("\n"));
            doc.Add(new Paragraph($"Encargado de impresión: {Constants.GetUserData().UserNombre}", new Font() { Size = 10 }));
            doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:yyyy-MM-dd}", new Font() { Size = 10 }));
            //doc.Add(new Chunk("\n"));
            
            var tiposDocumentos = tipoDocumentoAccess.GetTipoDocumentos();
            var estructuraOrganizacional = OrganizacionAccess.GetAllEstructuraOrganizacional();
            foreach (var item in cajas)
            {
                var documentos = documentoAccess.GetDocumentosByCajaID(item.CajaID);
                PdfPTable tableLayout = new PdfPTable(2);
                var table = GenerateTable(tableLayout, "", new string[] { TipoAlmacen.Equals("ACT") ? $"#Caja: {item.CajaActivaID}" : $"#Caja: {item.CajaInactivaID}", $"Fecha: {item.CajaFechaRecepcion:yyyy-MM-dd}"}, new float[] { 50, 50 });
                doc.Add(new Chunk("\n"));
                doc.Add(table);
                tableLayout = new PdfPTable(5);
                table = GenerateTable(tableLayout, "", new string[] { "Estante", "Sección", "Nivel", "Fila", "Ubicación" }, new float[] { 20, 20, 20, 20, 20 });
                AddCellToBody(tableLayout, item.Estante);
                AddCellToBody(tableLayout, item.Seccion);
                AddCellToBody(tableLayout, item.Nivel);
                AddCellToBody(tableLayout, item.Fila);
                AddCellToBody(tableLayout, item.Ubicacion);
                doc.Add(table);
                //tableLayout = new PdfPTable(1);
                //table = GenerateTable(tableLayout, "", new string[] { "Documentos" }, new float[] { 100 });
                //AddCellToBody(tableLayout, string.Join("\n", documentos.Select(x => x.SelectedDocumento.TipoDocNombre + " - " + x.DocDescripcion)));
                //doc.Add(table);
                tableLayout = new PdfPTable(3);
                table = GenerateTable(tableLayout, "", new string[] { "Documento", "Descripción", "Departamento" }, new float[] { 40, 30, 30 });
                documentos.ForEach(x => {
                    var centroCosto = estructuraOrganizacional.Where(y => y.EstOrgaID == x.DocCCCCID).FirstOrDefault();
                    x.TiposDocumentos = tiposDocumentos;
                    AddCellToBody(tableLayout, x.SelectedDocumento.TipoDocNombre);
                    AddCellToBody(tableLayout, x.DocDescripcion);
                    AddCellToBody(tableLayout, estructuraOrganizacional.Where(y => y.EstOrgaID == centroCosto.EstOrgaIDPadre).FirstOrDefault().EstOrgaNombre);
                });
                doc.Add(table);

            }

            //doc.Add(table);
            doc.Add(new Paragraph($"Total de registros: {cajas.Count}", new Font { Size = 10 }));

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
            tableLayout.AddCell(new PdfPCell(new Phrase(columnName, new Font() { Color = BaseColor.WHITE, Size = 10 }))
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