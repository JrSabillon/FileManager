using iTextSharp.text;
using iTextSharp.text.pdf;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SyT_FileManager.AppCode;
using SyT_FileManager.Business;
using SyT_FileManager.DataAccess;
using SyT_FileManager.Models;

namespace SyT_FileManager.Controllers
{
    [ExceptionHandler]
    [AuthorizationHandler]
    public class ArchivoInactivoController : Controller
    {
        BancoAccess BancoAccess;
        RecursoAccess RecursoAccess;
        OrganizacionAccess OrganizacionAccess;
        TipoDocumentoAccess TipoDocumentoAccess;
        AlmacenAccess AlmacenAccess;
        DocumentoAccess DocumentoAccess;
        CajaBusiness CajaBusiness;
        DocumentoBusiness DocumentoBusiness;
        AgenciaAccess AgenciaAccess;
        CajaAccess CajaAccess;

        public ArchivoInactivoController()
        {
            BancoAccess = new BancoAccess();
            RecursoAccess = new RecursoAccess();
            OrganizacionAccess = new OrganizacionAccess();
            TipoDocumentoAccess = new TipoDocumentoAccess();
            AlmacenAccess = new AlmacenAccess();
            CajaBusiness = new CajaBusiness();
            DocumentoAccess = new DocumentoAccess();
            DocumentoBusiness = new DocumentoBusiness();
            AgenciaAccess = new AgenciaAccess();
            CajaAccess = new CajaAccess();
        }

        // GET: ArchivoInactivo
        public ActionResult Posicionar(int? page)
        {
            //Solo obtener cajas pendientes por posicionar y del tipo de almacen de archivos activos
            var model = CajaAccess.GetCajasByStatusAndAlmacenTipo("PEND", "INA");
            var AlmacenesID = AlmacenAccess.GetAlmacenIDByUserID(Constants.GetUserData().UserId);
            var almacenes = AlmacenAccess.GetAlmacenes();
            model.ForEach((CajaModel caja) =>
            {
                caja.Almacenes = almacenes;
            });

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.Where(x => AlmacenesID.Any(id => x.AlmacenID.Equals(id))).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _CajaDocumentos(int CajaID, string ActionType)
        {
            var documentos = DocumentoAccess.GetDocumentosByCajaID(CajaID);
            var caja = CajaAccess.GetCaja(CajaID);
            var UsuarioEntrego = new UsuarioAccess().GetUsuario(caja.CajaPersonaEntrega);

            var Bancos = BancoAccess.GetBancos();
            var Zonas = RecursoAccess.GetRecursoItems("ZONA");
            var Estados = RecursoAccess.GetRecursoItems("DOCSTTS");
            var CentrosDeCosto = OrganizacionAccess.GetCCDropdown();
            var TiposDocumentos = TipoDocumentoAccess.GetTipoDocumentos().Where(x => x.TipoDocStatus.Equals("AC")).ToList();
            var Agencias = AgenciaAccess.GetAgencias().Where(x => x.AgenciaStatus == "AC").ToList();

            documentos.ForEach((DocumentoModel documento) => {
                documento.Bancos = Bancos;
                documento.Zonas = Zonas;
                documento.CentrosDeCosto = CentrosDeCosto;
                documento.TiposDocumentos = TiposDocumentos;
                documento.Agencias = Agencias;
                documento.Estados = Estados;
            });

            ViewBag.CajaID = caja.CajaID;
            ViewBag.CajaInactivaID = caja.CajaInactivaID;
            ViewBag.AlmacenID = caja.AlmacenID;
            ViewBag.AlmacenNombre = AlmacenAccess.GetAlmacen(caja.AlmacenID).AlmacenNombre;
            ViewBag.UsuarioEntrego = $"{UsuarioEntrego.UserId} - {UsuarioEntrego.UserNombre}";
            ViewBag.AgenciaEnvia = UsuarioEntrego.AgenciaID.HasValue ? AgenciaAccess.GetAgencia(UsuarioEntrego.AgenciaID).AgenciaNombre : AlmacenAccess.GetAlmacen(caja.AlmacenID).AlmacenLabel;
            ViewBag.ActionType = ActionType;


            return PartialView("./Partials/_CajaDocumentos", documentos);
        }

        public ActionResult _UbicacionCaja(int CajaID, int AlmacenID)
        {
            var model = new CajaModel()
            {
                CajaID = CajaID,
                Estantes = RecursoAccess.GetRecursoItems("STNTNUM"),
                Secciones = RecursoAccess.GetRecursoItems("STNTSEC"),
                Niveles = RecursoAccess.GetRecursoItems("STNTNIV"),
                Filas = RecursoAccess.GetRecursoItems("STNTFIL"),
                Ubicaciones = RecursoAccess.GetRecursoItems("STNTUBI")
            };

            ViewBag.CajaID = CajaID;
            ViewBag.AlmacenID = AlmacenID;

            return PartialView("_UbicacionCaja", model);
        }

        [HttpPost]
        public ActionResult _UbicacionCaja(CajaModel caja)
        {
            CajaAccess.UpdatePosicionCajaAlmacen(caja);
            return RedirectToAction("CajaPosicionada", new { caja.CajaID });
        }

        public ActionResult CajaPosicionada(int CajaID)
        {
            ViewBag.CajaID = CajaID;
            ViewBag.ActionType = "BoxPositioned";

            return View();
        }

        public ActionResult EnviarCaja(int? page, EnviarCajaBusqueda busqueda)
        {
            var almacenes = AlmacenAccess.GetAlmacenes();
            var estantes = RecursoAccess.GetRecursoItems("STNTNUM");
            var secciones = RecursoAccess.GetRecursoItems("STNTSEC");
            var niveles = RecursoAccess.GetRecursoItems("STNTNIV");
            var filas = RecursoAccess.GetRecursoItems("STNTFIL");
            var ubicaciones = RecursoAccess.GetRecursoItems("STNTUBI");
            var AlmacenesID = AlmacenAccess.GetAlmacenIDByUserID(Constants.GetUserData().UserId);

            var model = CajaAccess.GetCajasByAlmacenID_Filtered(busqueda.FechaInicio, busqueda.FechaFin, busqueda.Agencia, string.Join(",", AlmacenesID), "INA");

            model.ForEach((CajaModel caja) =>
            {
                caja.Almacenes = almacenes;
                caja.Estantes = estantes;
                caja.Secciones = secciones;
                caja.Niveles = niveles;
                caja.Filas = filas;
                caja.Ubicaciones = ubicaciones;
            });

            ViewBag.AlmacenID = new SelectList(almacenes.Where(x => x.AlmacenTipo.Equals("INA")), "AlmacenID", "AlmacenLabel");
            ViewBag.searchDate = busqueda.searchDate;
            ViewBag.searchAgency = busqueda.searchAgency;
            ViewBag.Agencia = new SelectList(AgenciaAccess.GetAgencias(), "AgenciaID", "AgenciaNombre");
            ViewBag.FechaInicio = busqueda.FechaInicio;
            ViewBag.FechaFin = busqueda.FechaFin;

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.Where(x => AlmacenesID.Any(id => x.AlmacenID.Equals(id))).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult TransferBox(CajaModel caja)
        {
            int AlmacenIDDestino = caja.AlmacenID; //Almacen donde sera enviada la caja.
            var boxData = CajaAccess.GetCaja(caja.CajaID);

            //Crear caja inactiva para el nuevo almacen
            int CajaID = CajaBusiness.CreateCajaInactiva(AlmacenIDDestino, boxData.CajaID);
            //Luego de crear la caja enviar los documentos
            DocumentoAccess.TransferDocuments_BoxToBox(caja.CajaID, CajaID);
            //Deshabilitar la caja de donde se enviaron los documentos
            CajaBusiness.Disable(caja.CajaID);

            return RedirectToAction("CajaEnviada", new { CajaID });
        }

        public ActionResult CajaEnviada(int CajaID)
        {
            var model = CajaAccess.GetCaja(CajaID);

            return View(model);
        }

        public JsonResult ValidateBoxPosition(CajaModel caja)
        {
            bool isAvailable = CajaAccess.ValidateBoxPosition(caja);

            return Json(isAvailable, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Prestar(int? page, PrestarDocumentoBusqueda busqueda, string id = "INA")
        {
            List<AlmacenModel> usuarioAlmacen = AlmacenAccess.GetAlmacenesByUsuarioAndTipoAlmacen(id, Constants.GetUserData().UserId);
            var model = DocumentoAccess.GetDocumentosByDocTipoAndDocStatusAnd_CajaAlmacenID(busqueda.TipoDocumento, "ACT", usuarioAlmacen.Select(x => x.AlmacenID).ToArray());

            ViewBag.TipoDocumento = new SelectList(TipoDocumentoAccess.GetTipoDocumentos(), "TipoDocID", "TipoDocNombre", busqueda.TipoDocumento);
            ViewBag.Agencia = new SelectList(AgenciaAccess.GetAgencias(), "AgenciaID", "AgenciaNombre", busqueda.Agencia);
            ViewBag.Banco = new SelectList(BancoAccess.GetBancos(), "BancoID", "BancoNombre", busqueda.Banco);
            ViewBag.id = id;
            ViewBag.searchDate = busqueda.searchDate;
            ViewBag.searchAgency = busqueda.searchAgency;
            ViewBag.searchBank = busqueda.searchBank;

            ///Filtrar modelo segun criterios de busqueda
            if (busqueda.searchAgency)
                model = model.Where(x => x.DocAgenciaID.Equals(busqueda.Agencia)).ToList();
            if (busqueda.searchBank)
                model = model.Where(x => x.DocBancoID.Equals(busqueda.Banco)).ToList();
            if (busqueda.searchDate)
                model = model.Where(x => x.DocFechaInfo >= busqueda.FechaInicio && x.DocFechaInfo <= busqueda.FechaFin).ToList();

            //Llenar relaciones del documento
            var agencias = AgenciaAccess.GetAgencias();
            var tipoDocumentos = TipoDocumentoAccess.GetTipoDocumentos();
            var bancos = BancoAccess.GetBancos();

            model.ForEach((DocumentoModel document) =>
            {
                document.Agencias = agencias;
                document.TiposDocumentos = tipoDocumentos;
                document.Bancos = bancos;
            });

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _PrestarDocumento(int CajaID, int DocID, string NombreDocumento, int? CajaInactivaID)
        {
            var model = new DocPrestamo();
            model.Departamentos = OrganizacionAccess.GetEstructuraInicial();
            model.PrestFechaSolicitud = DateTime.Now;
            model.CajaID = CajaID;
            model.DocID = DocID;

            ViewBag.NombreDocumento = NombreDocumento;
            ViewBag.CajaInactivaID = CajaInactivaID;

            return PartialView("_PrestarDocumento", model);
        }

        [HttpPost]
        public ActionResult _PrestarDocumento(DocPrestamo prestamo)
        {
            DocumentoBusiness.PrestarDocumento(prestamo);

            return RedirectToAction("Prestar", new { id = "INA" });
        }

        public ActionResult RecibirDocumento(int? page, PrestarDocumentoBusqueda busqueda, string id = "INA")
        {
            List<AlmacenModel> usuarioAlmacen = AlmacenAccess.GetAlmacenesByUsuarioAndTipoAlmacen(id, Constants.GetUserData().UserId);
            var model = DocumentoAccess.GetDocPrestamosByDocStatusAndDocTipoAnd_AlmacenID("PRS", busqueda.TipoDocumento, usuarioAlmacen.Select(x => x.AlmacenID).ToArray());

            ViewBag.TipoDocumento = new SelectList(TipoDocumentoAccess.GetTipoDocumentos(), "TipoDocID", "TipoDocNombre", busqueda.TipoDocumento);
            ViewBag.Agencia = new SelectList(AgenciaAccess.GetAgencias(), "AgenciaID", "AgenciaNombre", busqueda.Agencia);
            ViewBag.Banco = new SelectList(BancoAccess.GetBancos(), "BancoID", "BancoNombre", busqueda.Banco);
            ViewBag.id = id;
            ViewBag.searchDate = busqueda.searchDate;
            ViewBag.searchByMe = busqueda.searchByMe;

            ///Filtrar modelo segun criterios de busqueda
            if (busqueda.searchDate)
                model = model.Where(x => x.PrestFechaSolicitud >= busqueda.FechaInicio && x.PrestFechaSolicitud <= busqueda.FechaFin).ToList();
            if (busqueda.searchByMe)
                model = model.Where(x => x.PrestUsuarioEntrega.Equals(Constants.GetUserData().UserId)).ToList();

            //Llenar relaciones del documento
            var tipoDocumentos = TipoDocumentoAccess.GetTipoDocumentos();

            model.ForEach((DocPrestamo document) =>
            {
                document.TiposDocumentos = tipoDocumentos;
            });

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _RecibirDocumento(int CajaID, int DocID, string NombreDocumento, int? CajaInactivaID)
        {
            var model = DocumentoAccess.GetDocumentoPrestado(CajaID, DocID);
            model.Departamentos = OrganizacionAccess.GetEstructuraInicial();
            model.OtraPersonaRetira = !model.PrestNombreSolicitante.Equals(model.PrestPersonaRetira);

            ViewBag.NombreDocumento = NombreDocumento;
            ViewBag.CajaInactivaID = CajaInactivaID;

            return PartialView("_RecibirDocumento", model);
        }

        [HttpPost]
        public ActionResult _RecibirDocumento(DocPrestamo prestamo)
        {
            DocumentoBusiness.RecibirDocumento(prestamo);

            return RedirectToAction("RecibirDocumento", new { id = "INA" });
        }

        public ActionResult ConsultarCaja(int? page, EnviarCajaBusqueda busqueda)
        {
            var almacenes = AlmacenAccess.GetAlmacenes();
            var estantes = RecursoAccess.GetRecursoItems("STNTNUM");
            var secciones = RecursoAccess.GetRecursoItems("STNTSEC");
            var niveles = RecursoAccess.GetRecursoItems("STNTNIV");
            var filas = RecursoAccess.GetRecursoItems("STNTFIL");
            var ubicaciones = RecursoAccess.GetRecursoItems("STNTUBI");
            var cajaStatus = RecursoAccess.GetRecursoItems("CAJASTTS");
            var AlmacenesID = AlmacenAccess.GetAlmacenIDByUserID(Constants.GetUserData().UserId);

            var model = CajaAccess.GetCajasByAlmacenID_Filtered(busqueda.FechaInicio, busqueda.FechaFin, busqueda.Agencia, string.Join(",", AlmacenesID), "INA", busqueda.Caja);

            model.ForEach((CajaModel caja) =>
            {
                caja.Almacenes = almacenes;
                caja.Estantes = estantes;
                caja.Secciones = secciones;
                caja.Niveles = niveles;
                caja.Filas = filas;
                caja.Ubicaciones = ubicaciones;
                caja.Status = cajaStatus;
            });

            ViewBag.AlmacenID = new SelectList(almacenes.Where(x => x.AlmacenTipo.Equals("ACT")), "AlmacenID", "AlmacenLabel");
            ViewBag.searchDate = busqueda.searchDate;
            ViewBag.searchAgency = busqueda.searchAgency;
            ViewBag.searchBox = busqueda.searchBox;
            //ViewBag.Agencia = new SelectList(AgenciaAccess.GetAgencias(), "AgenciaID", "AgenciaNombre");
            ViewBag.FechaInicio = busqueda.FechaInicio;
            ViewBag.FechaFin = busqueda.FechaFin;

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.Where(x => AlmacenesID.Any(id => x.AlmacenID.Equals(id))).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Triturar(int? page, int? lote, bool searchOutdated = true)
        {
            var model = searchOutdated ? CajaAccess.GetCajasOutdated("INA") : CajaAccess.GetCajasByStatusAndAlmacenTipo("ACT", "INA");
            var almacenes = AlmacenAccess.GetAlmacenes();
            var estantes = RecursoAccess.GetRecursoItems("STNTNUM");
            var secciones = RecursoAccess.GetRecursoItems("STNTSEC");
            var niveles = RecursoAccess.GetRecursoItems("STNTNIV");
            var filas = RecursoAccess.GetRecursoItems("STNTFIL");
            var ubicaciones = RecursoAccess.GetRecursoItems("STNTUBI");
            var AlmacenesID = AlmacenAccess.GetAlmacenIDByUserID(Constants.GetUserData().UserId);

            model.ForEach((CajaModel caja) =>
            {
                caja.Almacenes = almacenes;
                caja.Estantes = estantes;
                caja.Secciones = secciones;
                caja.Niveles = niveles;
                caja.Filas = filas;
                caja.Ubicaciones = ubicaciones;
            });

            ViewBag.searchOutdated = searchOutdated;
            ViewBag.lote = lote;

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.Where(x => AlmacenesID.Any(id => x.AlmacenID.Equals(id))).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _SelectDocuments(int CajaID, int lote)
        {
            var caja = CajaAccess.GetCaja(CajaID);
            var documentos = DocumentoAccess.GetDocumentosByCajaID(CajaID).ToList();
            var Bancos = BancoAccess.GetBancos();
            var Zonas = RecursoAccess.GetRecursoItems("ZONA");
            var CentrosDeCosto = OrganizacionAccess.GetCCDropdown();
            var TiposDocumentos = TipoDocumentoAccess.GetTipoDocumentos().Where(x => x.TipoDocStatus.Equals("AC")).ToList();
            var EstadosDocumentos = RecursoAccess.GetRecursoItems("DOCSTTS");

            documentos.ForEach((DocumentoModel documento) => {
                documento.Bancos = Bancos;
                documento.Zonas = Zonas;
                documento.CentrosDeCosto = CentrosDeCosto;
                documento.TiposDocumentos = TiposDocumentos;
                documento.Create = documento.DocFechaVencimiento <= DateTime.Now;
                documento.Estados = EstadosDocumentos;
            });

            var almacenes = AlmacenAccess.GetAlmacenes();

            ViewBag.CajaID = caja.CajaID;
            ViewBag.CajaInactivaID = caja.CajaInactivaID;
            ViewBag.lote = lote;
            
            return PartialView("_SelectDocuments", documentos);
        }

        /// <summary>
        /// Enviar documentos a almacen inactivo
        /// </summary>
        /// <param name="Documentos">Listado de documentos de la caja, solo los seleccionados con el atributo create seran enviados a menos que el usuario seleccionara enviarlos todos.</param>
        /// <param name="SendAll">Parametro que indica si enviara todos los documentos o no.</param>
        /// <param name="lote">ID de lote, si viene en 0 se crea un nuevo lote si no se anexan al lote enviado.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SelectDocuments(List<DocumentoModel> Documentos, bool SendAll, int lote)
        {
            lote = DocumentoBusiness.TriturarDocumentos(Documentos, SendAll, lote);

            return RedirectToAction("DocumentosTriturados", new { lote });
        }

        public ActionResult DocumentosTriturados(int? lote)
        {
            if (!lote.HasValue)
            {
                ViewBag.Message = "No se selecciono un No. de lote";
                return View();
            }
            
            var model = DocumentoAccess.GetDocTrituraByTrituraID(lote);
            ViewBag.Message = model.Count == 0 ? $"No hay documentos en este lote No. de lote [{lote}]" : null;
            ViewBag.lote = lote;

            model.ForEach((DocTrituraModel docTritura) =>
            {
                docTritura.Documento = DocumentoAccess.GetDocumento(docTritura.DocID);
                docTritura.Documento.TiposDocumentos = TipoDocumentoAccess.GetTipoDocumentos();
            });

            return View(model);
        }

        public ActionResult ArmarCaja(int? CajaID)
        {
            var model = new DocumentoModel()
            {
                Bancos = BancoAccess.GetBancos(),
                Zonas = RecursoAccess.GetRecursoItems("ZONA"),
                CentrosDeCosto = OrganizacionAccess.GetCCDropdown(),
                TiposDocumentos = TipoDocumentoAccess.GetTipoDocumentos().Where(x => x.TipoDocStatus.Equals("AC")).ToList()
            };
            var user = Constants.GetUserData();
            List<AlmacenModel> almacenes = new List<AlmacenModel>();
            var almacenesUsuario = AlmacenAccess.GetAlmacenIDByUserID(user.UserId);

            almacenes = AlmacenAccess.GetAlmacenesActivos().Where(x => almacenesUsuario.Any(y => x.AlmacenID.Equals(y))).ToList();

            ViewBag.AlmacenID = new SelectList(almacenes, "AlmacenID", "AlmacenLabel");
            ViewBag.Agencias = new SelectList(AgenciaAccess.GetAgencias(), "AgenciaID", "AgenciaNombre", user.AgenciaID);
            ViewBag.ZonaID = user.AgenciaID.HasValue ? AgenciaAccess.GetAgencia(user.AgenciaID).ZonaID : almacenes.First().ZonaId;
            ViewBag.AgenciaID = user.AgenciaID;
            ViewBag.CajaID = CajaID ?? 0;
            ViewBag.AlmacenGuardado = CajaID.HasValue ? CajaAccess.GetCaja(CajaID.Value).AlmacenID : 0;

            return View(model);
        }

        public JsonResult GetZonaIDByAgenciaID(int AgenciaID)
        {
            string ZonaID = AgenciaAccess.GetZonaIDByAgenciaID(AgenciaID);

            return Json(ZonaID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DocumentGrid(DocumentoModel documento)
        {
            documento.Bancos = BancoAccess.GetBancos();
            documento.Agencias = AgenciaAccess.GetAgencias();
            documento.Zonas = RecursoAccess.GetRecursoItems("ZONA");
            documento.CentrosDeCosto = OrganizacionAccess.GetCCDropdown();
            documento.TiposDocumentos = TipoDocumentoAccess.GetTipoDocumentos().Where(x => x.TipoDocStatus.Equals("AC")).ToList();
            //documento.DocFechaInfo = documento.DocFechaInfo;
            //documento.DocFechaVencimiento = documento.DocFechaInfo.Value.AddYears(0);

            return PartialView("./Partials/_DocumentGrid", documento);
        }

        [HttpPost]
        public ActionResult Enviar(List<DocumentoModel> documentos, int AlmacenID, string Source)
        {
            //Obtener solo los documentos que se ingresaran al final.
            var documents = documentos.Where(x => x.Create).ToList();

            //Ingresar registro de la caja
            var CajaID = CajaBusiness.Create(AlmacenID, Constants.GetUserData().UserId);

            //Ingresar documentos
            DocumentoBusiness.CreateDocuments(documents, Convert.ToInt32(CajaID));

            return RedirectToAction(Source ?? "CajaEnviada", new { CajaID });
        }

        public ActionResult ConsolidarTrituracion(string TrituraNombreTestigo, int lote)
        {
            bool updated = DocumentoAccess.UpdateDocTrituraSetTrituraNombreTestigo(TrituraNombreTestigo, lote);

            return RedirectToAction("DocumentosTriturados", new { lote });
        }

        public FileResult ImprimirActaTrituracion(int lote, string TrituraNombreTestigo)
        {
            DocumentoAccess.UpdateDocTrituraSetTrituraNombreTestigo(TrituraNombreTestigo, lote);

            var pdfData = DocumentoAccess.GetDocTrituraByTrituraID(lote);

            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            var today = DateTime.Now;

            string fileName = $"DocumentosTriturados_{today:yyyyMMddHHmmss}.pdf";
            Document doc = new Document(PageSize.A4);
            doc.SetMargins(25f, 25f, 10f, 10f);

            PdfPTable tableLayout = new PdfPTable(5);
            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            var title = new Paragraph("Acta de Trituración", new Font() { Size = 15 }) { Alignment = Element.ALIGN_CENTER };
            doc.Add(title);
            doc.Add(new Chunk("\n"));
            doc.Add(new Paragraph($"Nombre del testigo: {TrituraNombreTestigo}", new Font() { Size = 10 }));
            doc.Add(new Paragraph($"Persona que tritura: {Constants.GetUserData().UserNombre}", new Font() { Size = 10 }));
            doc.Add(new Paragraph($"Fecha de generación del acta: {DateTime.Now:yyyy-MM-dd}", new Font() { Size = 10 }));
            doc.Add(new Chunk("\n"));

            PdfPTable table = AddTableContent(pdfData, tableLayout);
            doc.Add(table);

            doc.Add(new Chunk("\n"));
            doc.Add(new Chunk("\n"));
            doc.Add(new Chunk("\n"));
            doc.Add(AddSignatures());

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf", fileName);
        }

        private PdfPTable AddSignatures()
        {
            PdfPTable signaturesLayout = new PdfPTable(2);
            float[] widths = { 50, 50 };
            signaturesLayout.SetWidths(widths);
            signaturesLayout.WidthPercentage = 100;
            signaturesLayout.HeaderRows = 0;

            signaturesLayout.AddCell(new PdfPCell(new Phrase("__________________________________ \nFirma del testigo", new Font(Font.FontFamily.HELVETICA, 10f)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = new BaseColor(255, 255, 255),
                Border = Rectangle.NO_BORDER
            });

            signaturesLayout.AddCell(new PdfPCell(new Phrase("__________________________________ \nFirma del encargado de trituración", new Font(Font.FontFamily.HELVETICA, 10f)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = new BaseColor(255, 255, 255),
                Border = Rectangle.NO_BORDER
            });

            return signaturesLayout;
        }

        private PdfPTable AddTableContent(List<DocTrituraModel> pdfData, PdfPTable tableLayout)
        {
            float[] headers = { 25, 40, 30, 20, 30 };
            tableLayout.SetWidths(headers);
            tableLayout.WidthPercentage = 100;
            tableLayout.HeaderRows = 1;

            tableLayout.AddCell(new PdfPCell(new Phrase("Documentos triturados"))
            {
                Colspan = 5,
                Border = 0,
                PaddingBottom = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            AddCellToHeader(tableLayout, columnName: "Documento");
            AddCellToHeader(tableLayout, columnName: "Descripcion");
            AddCellToHeader(tableLayout, columnName: "Banco");
            AddCellToHeader(tableLayout, columnName: "Agencia");
            AddCellToHeader(tableLayout, columnName: "Fecha trituración");

            var bancos = BancoAccess.GetBancos();
            var documentos = TipoDocumentoAccess.GetTipoDocumentos();
            var agencias = AgenciaAccess.GetAgencias();

            foreach (var item in pdfData)
            {
                var documentoTriturado = DocumentoAccess.GetDocumento(item.DocID); 

                AddCellToBody(tableLayout, value: documentos.Find(x => x.TipoDocID.Equals(documentoTriturado.DocTipo)).TipoDocNombre); //Nombre del documento.
                AddCellToBody(tableLayout, value: documentoTriturado.DocDescripcion);
                AddCellToBody(tableLayout, value: bancos.Find(x => x.BancoID.Equals(documentoTriturado.DocBancoID)).BancoNombre); //Tipo de banco.
                AddCellToBody(tableLayout, value: agencias.Find(x => x.AgenciaID.Equals(documentoTriturado.DocAgenciaID)).AgenciaNombre); //Agencia de origen del documento.
                AddCellToBody(tableLayout, value: documentoTriturado.DocFechaTrituracion.Value.ToString("yyyy-MM-dd"));
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