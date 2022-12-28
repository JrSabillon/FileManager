using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class GetDocumentosTriturados_RP
    {
        public string TrituraNombreTestigo { get; set; }
        public int TrituraID { get; set; }
        public string TrituraUsuario { get; set; }
        public DateTime TrituraFecha { get; set; }
        public string TipoDocNombre { get; set; }
        public int TipoDocPlazo { get; set; }
        public string AgenciaNombre { get; set; }
        public string Departamento { get; set; }
        public string DocDescripcion { get; set; }
    }
}