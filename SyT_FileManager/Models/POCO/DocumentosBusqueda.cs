using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models.POCO
{
    public class DocumentosBusqueda
    {
        public bool searchAgency { get; set; }
        public bool searchDepartment { get; set; }
        public bool searchTerm { get; set; }
        public bool searchDates { get; set; }
        public string AgenciaID { get; set; }
        public string Departamento { get; set; }
        public string PlazoRetencion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}