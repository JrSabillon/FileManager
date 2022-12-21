using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SyT_FileManager.AppCode;
using SyT_FileManager.DataAccess;
using SyT_FileManager.Models;
using SyT_FileManager.Models.POCO;

namespace SyT_FileManager.Business
{
    public class DocumentoBusiness
    {
        DocumentoAccess DocumentoAccess;
        OrganizacionAccess OrganizacionAccess;
        TipoDocumentoAccess TipoDocumentoAccess;
        AlmacenAccess AlmacenAccess;

        public DocumentoBusiness()
        {
            DocumentoAccess = new DocumentoAccess();
            OrganizacionAccess = new OrganizacionAccess();
            TipoDocumentoAccess = new TipoDocumentoAccess();
            AlmacenAccess = new AlmacenAccess();
        }

        public long CreateDocuments(List<DocumentoModel> documentos, int CajaID)
        {
            var TiposDocumentos = TipoDocumentoAccess.GetTipoDocumentos();
            var estructuraOrganizacional = OrganizacionAccess.GetAllEstructuraOrganizacional();
            long insertedRows = 0;

            foreach (var documento in documentos)
            {
                int DocPlazoRetencion = TiposDocumentos.Where(x => x.TipoDocID == documento.DocTipo.Value).First().TipoDocPlazo;
                int DocDepartamentoID = estructuraOrganizacional.Where(x => x.EstOrgaID == documento.DocCCCCID.Value).First().EstOrgaIDPadre.Value;
                
                documento.DocID = DocumentoAccess.GetNextDocID();
                documento.CajaID = CajaID;
                //Obtener ID de departamento segun centro de costo
                documento.DocDepartamentoID = DocDepartamentoID;
                documento.DocPlazoRetencion = DocPlazoRetencion.ToString();
                //Sumar plazo de retencion a la fecha actual.
                documento.DocFechaVencimiento = documento.DocFechaInfo.Value.AddYears(DocPlazoRetencion);
                documento.DocStatus = "ACT";
                documento.DocFechaTrituracion = null;
                
                insertedRows += DocumentoAccess.CreateDocument(documento);
                CreateDocumentoHistorico(documento.DocID, null, documento.CajaID);
            }


            return insertedRows;
        }

        public bool SendDocumentsToArchivoInactivo(List<DocumentoModel> documentos, int CajaID, int AlmacenID, bool SendAll)
        {
            foreach (var documento in documentos)
            {
                var document = DocumentoAccess.GetDocumento(documento.DocID);
                var CajaIDOrigen = document.CajaID; //CajaID donde se encuentra actualmente el documento
                var CajaIDDestino = CajaID; //CajaID donde se posicionara el documento
                var DocIDActual = document.DocID; //DocID original de la caja a la que pertenece

                if (documento.Create || SendAll)
                {
                    //Actualizar el estado del documento para inhabilitarlo de la caja actual solo si esta activo.
                    if (document.DocStatus == "ACT")
                    {
                        document.DocStatus = "INA";
                        DocumentoAccess.Update(document);
                    }
                }

                document.CajaID = CajaID;
                //document.DocID = DocumentoAccess.GetNextDocIDByCajaID(CajaID);
                document.DocFechaInfo = DateTime.Now;
                document.DocFechaVencimiento = document.DocFechaInfo.Value.AddYears(Convert.ToInt32(document.DocPlazoRetencion));
                document.DocZonaID = AlmacenAccess.GetAlmacen(AlmacenID).ZonaId;
                document.DocStatus = "ACT";

                if (!SendAll && documento.Create)
                {
                    DocumentoAccess.CreateDocument(document);
                    CreateDocumentoHistorico(DocIDActual, CajaIDOrigen, CajaIDDestino);
                }

                if (SendAll)
                {
                    if(document.DocStatus == "ACT")
                    {
                        DocumentoAccess.CreateDocument(document);
                        CreateDocumentoHistorico(DocIDActual, CajaIDOrigen, CajaIDDestino);
                    }
                }

            }

            return true;
        }

        public int CreateDocumentoHistorico(int DocID, int? CajaIDOrigen, int CajaIDDestino)
        {
            var DocCajaHistorica = new DocCajaHistoricaModel()
            {
                DocID = DocID,
                CajaIDOrigen = CajaIDOrigen,
                CajaIDDestino = CajaIDDestino,
                DocCajaHistUsuarioMovimiento = Constants.GetUserData().UserId,
                DocCajaHistFechaMovimiento = DateTime.Now
            };

            return DocumentoAccess.CreateDocumentoHistorico(DocCajaHistorica);
        }

        public long PrestarDocumento(DocPrestamo prestamo)
        {
            prestamo.PrestamoID = DocumentoAccess.GetNextPrestamoID();
            prestamo.PrestPersonaRetira = prestamo.OtraPersonaRetira ? prestamo.PrestPersonaRetira : prestamo.PrestNombreSolicitante;
            prestamo.PrestUsuarioEntrega = Constants.GetUserData().UserId;
            prestamo.PrestFechaMaximaDevolucion = prestamo.PrestFechaRetira.AddDays(prestamo.PrestPlazoMaximoDevolucion);

            var documento = DocumentoAccess.GetDocumento(prestamo.DocID);
            documento.DocStatus = "PRS";
            DocumentoAccess.Update(documento);

            return DocumentoAccess.PrestarDocumento(prestamo);
        }

        internal bool RecibirDocumento(DocPrestamo prestamo)
        {
            var DocPrestamo = DocumentoAccess.GetDocPrestamo(prestamo.PrestamoID);
            DocPrestamo.PrestFechaDevuelve = prestamo.PrestFechaDevuelve;
            DocPrestamo.PrestPersonaDevuelve = prestamo.PrestPersonaDevuelve;
            DocPrestamo.PrestUsuarioRecibe = Constants.GetUserData().UserId;

            bool Recibido = DocumentoAccess.RecibirDocumento(DocPrestamo);

            var documento = DocumentoAccess.GetDocumento(prestamo.DocID);
            documento.DocStatus = "ACT";
            DocumentoAccess.Update(documento);

            return Recibido;
        }

        /// <summary>
        /// Actualizar caja donde estara el documento(pasa en caso de que solo algunos documentos cambien de caja)
        /// </summary>
        /// <param name="documento">Objeto que contiene la informacion del documento</param>
        /// <param name="CajaID">Id de la caja donde se enviaran los documentos</param>
        /// <returns></returns>
        public bool UpdateDocumentoSetCajaID(DocumentoModel documento, int CajaID)
        {
            documento = DocumentoAccess.GetDocumento(documento.DocID);
            documento.CajaID = CajaID;

            return DocumentoAccess.Update(documento);
        }

        /// <summary>
        /// Enviar documentos a trituracion por lotes.
        /// </summary>
        /// <param name="documentos">Documentos de la caja que pueden ser triturados</param>
        /// <param name="sendAll">Variable que indica si todos los documentos de la caja seran triturados(true para enviarlos todos)</param>
        /// <returns>ID del lote de trituracion para los documentos seleccionados.</returns>
        public int TriturarDocumentos(List<DocumentoModel> documentos, bool sendAll, int lote)
        {
            //Enviar los documentos a triturar por lotes, conseguir numero de lote.
            int TrituraID = lote != 0 ? lote : DocumentoAccess.GetNextTrituraID();
            //Inicializar lote que se triturara.
            List<DocTrituraModel> docTrituras = new List<DocTrituraModel>();
            DocTrituraModel docTritura = null;
            var today = DateTime.Now;

            documentos.ForEach((DocumentoModel documento) =>
            {
                //Si tritura todos los documentos y el documento se encuentra disponible para triturar entonces setear la bandera a true.
                documento.Create = sendAll ? sendAll : documento.Create && documento.DocStatus.Equals("ACT");

                if (documento.Create)
                {
                    docTritura = new DocTrituraModel
                    {
                        TrituraID = TrituraID,
                        DocID = documento.DocID,
                        CajaID = documento.CajaID,
                        TrituraFecha = today,
                        TrituraUsuario = Constants.GetUserData().UserId
                    };

                    //Agregar documento al lote de trituracion.
                    docTrituras.Add(docTritura);
                }
            });

            //Enviar a trituracion por lotes.
            DocumentoAccess.TriturarDocumentos(docTrituras);

            return TrituraID;
        }

        public List<GetDocumentosTriturados_RP> GetDocumentosTriturados_RP(DocumentosTrituradosBusqueda busqueda, string UserId)
        {
            List<GetDocumentosTriturados_RP> data = DocumentoAccess.GetDocumentosTriturados_RP(busqueda, UserId);

            if (busqueda.SearchTestigo && !string.IsNullOrEmpty(busqueda.NombreTestigo))
                data = data.Where(x => x.TrituraNombreTestigo.ToLower().Contains(busqueda.NombreTestigo.ToLower())).ToList();
            if (busqueda.SearchUser && !string.IsNullOrEmpty(busqueda.Usuario))
                data = data.Where(x => x.TrituraUsuario.ToLower().Contains(busqueda.Usuario.ToLower())).ToList();
            if (busqueda.SearchDocument && !string.IsNullOrEmpty(busqueda.TipoDocumento))
                data = data.Where(x => x.TipoDocNombre.ToLower().Contains(busqueda.TipoDocumento.ToLower())).ToList();
            if (busqueda.SearchAct && busqueda.TrituraID.HasValue)
                data = data.Where(x => x.TrituraID == busqueda.TrituraID).ToList();

            return data;
        }

        public List<GetDocumentosPrestados_RP> GetDocumentosPrestados_RP(DocumentosPrestadosBusqueda busqueda, string UserId)
        {
            if (!busqueda.searchDates)
            {
                busqueda.FechaInicio = null;
                busqueda.FechaFin = null;
            }
            var data = DocumentoAccess.GetDocumentosPrestados_RP(busqueda, UserId);

            if (busqueda.searchAgency && !string.IsNullOrEmpty(busqueda.AgenciaID))
                data = data.Where(x => x.DocAgenciaID == busqueda.AgenciaID).ToList();
            if (busqueda.searchDepartment && !string.IsNullOrEmpty(busqueda.Departamento))
                data = data.Where(x => x.Departamento == busqueda.Departamento).ToList();
            if (busqueda.searchTerm && !string.IsNullOrEmpty(busqueda.PlazoRetencion))
                data = data.Where(x => x.TipoDocPlazo == busqueda.PlazoRetencion).ToList();

            return data;
        }

        public List<GetDocumentos_RP> GetDocumentos_RP(DocumentosBusqueda busqueda, string UserId)
        {
            if (!busqueda.searchDates)
            {
                busqueda.FechaInicio = null;
                busqueda.FechaFin = null;
            }

            var data = DocumentoAccess.GetDocumentos_RP(busqueda, UserId);
            var almacenes = new AlmacenAccess().GetAlmacenes();

            data.ForEach((GetDocumentos_RP item) => item.Almacenes = almacenes);

            if (busqueda.searchAgency && !string.IsNullOrEmpty(busqueda.AgenciaID))
                data = data.Where(x => x.DocAgenciaID == busqueda.AgenciaID).ToList();
            if (busqueda.searchDepartment && !string.IsNullOrEmpty(busqueda.Departamento))
                data = data.Where(x => x.CodigoDepartamento == busqueda.Departamento).ToList();
            if (busqueda.searchTerm && !string.IsNullOrEmpty(busqueda.PlazoRetencion))
                data = data.Where(x => x.CodigoCentroCosto == busqueda.PlazoRetencion).ToList();

            return data;
        }

        internal DocumentoModel AddDocumentToBox(DocumentoModel documento)
        {
            var documentos = DocumentoAccess.GetDocumentosByCajaID(documento.CajaID);
            int DocPlazo = TipoDocumentoAccess.GetTipoDocumentos().Where(x => x.TipoDocID == documento.DocTipo.Value).First().TipoDocPlazo;
            documento.DocID = DocumentoAccess.GetNextDocID();
            documento.DocZonaID = documentos.Select(x => x.DocZonaID).FirstOrDefault();
            documento.DocDepartamentoID = OrganizacionAccess.GetAllEstructuraOrganizacional()
                .Where(x => x.EstOrgaID == documento.DocCCCCID.Value).First().EstOrgaIDPadre.Value;
            documento.DocPlazoRetencion = DocPlazo.ToString();
            documento.DocFechaVencimiento = documento.DocFechaInfo.Value.AddYears(DocPlazo);
            documento.DocStatus = "ACT";
            documento.DocAgenciaID = documentos.Select(x => x.DocAgenciaID).FirstOrDefault();

            DocumentoAccess.CreateDocument(documento);

            //Llenar dependencias
            RecursoAccess RecursoAccess = new RecursoAccess();
            documento.TiposDocumentos = TipoDocumentoAccess.GetTipoDocumentos();
            documento.Estados = RecursoAccess.GetRecursoItems("DOCSTTS");
            documento.Bancos = new BancoAccess().GetBancos();
            documento.CentrosDeCosto = OrganizacionAccess.GetCCDropdown();
            documento.Agencias = new AgenciaAccess().GetAgencias();
            documento.Zonas = RecursoAccess.GetRecursoItems("ZONA");

            return documento;
        }
    }
}