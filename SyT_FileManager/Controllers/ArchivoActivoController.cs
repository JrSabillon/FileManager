using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SyT_FileManager.AppCode;
using SyT_FileManager.Business;
using SyT_FileManager.DataAccess;
using SyT_FileManager.Models;
using PagedList;

namespace SyT_FileManager.Controllers
{
    [ExceptionHandler]
    [AuthorizationHandler]
    public class ArchivoActivoController : Controller
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

        public ArchivoActivoController()
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

        // GET: ArchivoActivo
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Enviar()
        {
            var model = new DocumentoModel()
            {
                Bancos = BancoAccess.GetBancos(),
                Zonas = RecursoAccess.GetRecursoItems("ZONA"),
                CentrosDeCosto = OrganizacionAccess.GetCCDropdown(),
                TiposDocumentos = TipoDocumentoAccess.GetTipoDocumentos().Where(x => x.TipoDocStatus.Equals("AC")).ToList()
            };

            var userAgency = Constants.GetUserData().AgenciaID;
            List<AlmacenModel> almacenes = new List<AlmacenModel>();

            if (userAgency.HasValue)
                almacenes = AlmacenAccess.GetAlmacenesActivosByAgenciaID(userAgency).Where(x => x.AlmacenStatus.Equals("AC")).ToList();
            else
                almacenes = AlmacenAccess.GetAlmacenesByUsuarioAndTipoAlmacen("ACT", Constants.GetUserData().UserId);

            ViewBag.AlmacenID = new SelectList(almacenes, "AlmacenID", "AlmacenLabel");
            ViewBag.Agencias = new SelectList(AgenciaAccess.GetAgencias(), "AgenciaID", "AgenciaNombre", userAgency);
            ViewBag.ZonaID = userAgency.HasValue ? AgenciaAccess.GetAgencia(userAgency).ZonaID : almacenes.First().ZonaId;
            ViewBag.AgenciaID = userAgency;

            return View(model);
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

        public ActionResult CajaEnviada(int CajaID)
        {
            var model = CajaAccess.GetCaja(CajaID);

            return View(model);
        }

        public ActionResult Posicionar(int? page)
        {
            //Solo obtener cajas pendientes por posicionar y del tipo de almacen de archivos activos
            var model = CajaAccess.GetCajasByStatusAndAlmacenTipo("PEND", "ACT");
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
            ViewBag.CajaActivaID = caja.CajaActivaID;
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

        /// <summary>
        /// Enviar documentos de archivo activo a archivo inactivo
        /// </summary>
        /// <returns></returns>
        public ActionResult EnviarDocumentos(int? page, bool searchOutdated = true)
        {
            var model = searchOutdated ? CajaAccess.GetCajasOutdated() : CajaAccess.GetCajasByStatusAndAlmacenTipo("ACT", "ACT");
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

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);

            return View(model.Where(x => AlmacenesID.Any(id => x.AlmacenID.Equals(id))).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult _SelectDocuments(int CajaID)
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
                documento.Create = documento.DocFechaInfo <= documento.FechaExpiraAlmacenActivo;
                documento.Estados = EstadosDocumentos;
            });

            var almacenes = AlmacenAccess.GetAlmacenes();

            ViewBag.CajaID = CajaID;
            //var cajasInactivas = CajaAccess.GetCajasByStatusAndAlmacenTipo("ACT", "INA");

            //ViewBag.CajaInactivaID = cajasInactivas;
            ViewBag.CajaActivaID = caja.CajaActivaID;
            ViewBag.AlmacenIDOrigen = CajaAccess.GetCaja(CajaID).AlmacenID;
            //Obtener almacenes de tipo almacen inactivo
            ViewBag.AlmacenesInactivos = AlmacenAccess.GetAlmacenesByAlmacenTipo("INA");

            return PartialView("_SelectDocuments", documentos);
        }

        /// <summary>
        /// Enviar documentos a almacen inactivo
        /// </summary>
        /// <param name="Documentos">Listado de documentos de la caja, solo los seleccionados con el atributo create seran enviados a menos que el usuario seleccionara enviarlos todos</param>
        /// <param name="AlmacenID">ID del almacen inactivo donde seran enviados los documentos</param>
        /// <param name="SendAll">Parametro que indica si enviara todos los documentos o no</param>
        /// <param name="CajaID">ID de la caja actual</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SelectDocuments(List<DocumentoModel> Documentos, int AlmacenID, bool SendAll, int CajaID)
        {
            //Si envia todos los documentos entonces la caja actual pasa al almacen inactivo
            if(SendAll)
            {
                var caja = CajaBusiness.SetCajaInactiva(CajaID, AlmacenID);

                return RedirectToAction("CajaEnviada", new { caja.CajaID });
            }

            //Si no envia toda la caja y solo envia algunos documentos, entonces se crea una nueva caja y los documentos pasan a esa nueva caja.
            int CajaIDOrigen = CajaID; //ID de la caja original
            CajaID = CajaBusiness.CreateCajaInactiva(AlmacenID, CajaID); //ID de la nueva caja 
            
            foreach (var documento in Documentos)
            {
                //Actualizar la nueva caja de cada documento enviado
                if (documento.Create)
                {
                    DocumentoBusiness.UpdateDocumentoSetCajaID(documento, CajaID);
                    //Crear el movimiento del documento en caja historica.
                    DocumentoBusiness.CreateDocumentoHistorico(documento.DocID, CajaIDOrigen, CajaID);
                }
            }

            return RedirectToAction("CajaEnviada", new { CajaID });
        }

        public JsonResult ValidateBoxPosition(CajaModel caja)
        {
            bool isAvailable = CajaAccess.ValidateBoxPosition(caja);

            return Json(isAvailable, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetZonaIDByAgenciaID(int AgenciaID)
        {
            string ZonaID = AgenciaAccess.GetZonaIDByAgenciaID(AgenciaID);

            return Json(ZonaID, JsonRequestBehavior.AllowGet);
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

            var model = CajaAccess.GetCajasByAlmacenID_Filtered(busqueda.FechaInicio, busqueda.FechaFin, busqueda.Agencia, string.Join(",", AlmacenesID), "ACT");

            model.ForEach((CajaModel caja) =>
            {
                caja.Almacenes = almacenes;
                caja.Estantes = estantes;
                caja.Secciones = secciones;
                caja.Niveles = niveles;
                caja.Filas = filas;
                caja.Ubicaciones = ubicaciones;
            });

            ViewBag.AlmacenID = new SelectList(almacenes.Where(x => x.AlmacenTipo.Equals("ACT")), "AlmacenID", "AlmacenLabel");
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
            var TransferedBox = CajaBusiness.Transfer(caja.AlmacenID, caja.CajaID);

            return RedirectToAction("CajaEnviada", new { TransferedBox.CajaID });
        }

        public ActionResult Prestar(int? page, PrestarDocumentoBusqueda busqueda, string id = "ACT")
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

        public ActionResult _PrestarDocumento(int CajaID, int DocID, string NombreDocumento, int? CajaActivaID)
        {
            var model = new DocPrestamo();
            model.Departamentos = OrganizacionAccess.GetEstructuraInicial();
            model.PrestFechaSolicitud = DateTime.Now;
            model.CajaID = CajaID;
            model.DocID = DocID;

            ViewBag.NombreDocumento = NombreDocumento;
            ViewBag.CajaActivaID = CajaActivaID;

            return PartialView("_PrestarDocumento", model);
        }

        [HttpPost]
        public ActionResult _PrestarDocumento(DocPrestamo prestamo)
        {
            DocumentoBusiness.PrestarDocumento(prestamo);

            return RedirectToAction("Prestar", new { id = "ACT" });
        }

        public ActionResult RecibirDocumento(int? page, PrestarDocumentoBusqueda busqueda, string id = "ACT")
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

        public ActionResult _RecibirDocumento(int CajaID, int DocID, string NombreDocumento, int? CajaActivaID)
        {
            var model = DocumentoAccess.GetDocumentoPrestado(CajaID, DocID);
            model.Departamentos = OrganizacionAccess.GetEstructuraInicial();
            model.OtraPersonaRetira = !model.PrestNombreSolicitante.Equals(model.PrestPersonaRetira);

            ViewBag.NombreDocumento = NombreDocumento;
            ViewBag.CajaActivaID = CajaActivaID;

            return PartialView("_RecibirDocumento", model);
        }

        [HttpPost]
        public ActionResult _RecibirDocumento(DocPrestamo prestamo)
        {
            DocumentoBusiness.RecibirDocumento(prestamo);

            return RedirectToAction("RecibirDocumento", new { id = "ACT" });
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

            var model = CajaAccess.GetCajasByAlmacenID_Filtered(busqueda.searchDate ? busqueda.FechaInicio : null, busqueda.searchDate ? busqueda.FechaFin : null, busqueda.Agencia, string.Join(",", AlmacenesID), "ACT", busqueda.searchBox ? busqueda.Caja : 0);

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

        [HttpGet]
        public JsonResult GetZonaByAgencia(int AgenciaID)
        {
            var ZonaID = AgenciaAccess.GetZonaIDByAgenciaID(AgenciaID);

            return Json(ZonaID, JsonRequestBehavior.AllowGet);
        }

        public FileResult GenerateCodebar(string code, int width, int height, int size)
        {
            return File(StaticHelpers.GenerateCodeBar(code, width, height, size), "image/jpg");
        }
    }
}