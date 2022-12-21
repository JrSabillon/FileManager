using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models.POCO
{
    public class DocumentosTrituradosBusqueda
    {
        public bool SearchTestigo { get; set; }
        public bool SearchUser { get; set; }
        public bool SearchDate { get; set; }
        public bool SearchDocument { get; set; }
        public bool SearchAgency { get; set; }
        public bool SearchAct { get; set; }
        public string NombreTestigo { get; set; }
        public string Usuario { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string TipoDocumento { get; set; }
        public string Agencia { get; set; }
        public string Departamento { get; set; }
        public int? TrituraID { get; set; }
    }
}