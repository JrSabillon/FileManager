using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class GetDocumentosPrestados_RP
    {
        public string PrestNombreSolicitante { get; set; }
        public string PrestEmailSolicitante { get; set; }
        public DateTime PrestFechaSolicitud { get; set; }
        public DateTime PrestFechaRetira { get; set; }
        public string PrestPersonaRetira { get; set; }
        public string PrestUsuarioEntrega { get; set; }
        public int PrestPlazoMaximoDevolucion { get; set; }
        public string PrestObservacion { get; set; }
        public string TipoDocNombre { get; set; }
        public string TipoDocPlazo { get; set; }
        public string DocAgenciaID { get; set; }
        public int Departamento { get; set; }
        public string DocDescripcion { get; set; }
        public string NombreDepartamento { get; set; }
        public int CajaID { get; set; }
    }
}