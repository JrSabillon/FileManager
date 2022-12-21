using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class PrestarDocumentoBusqueda
    {
        public bool searchDate { get; set; } = true;
        public bool searchAgency { get; set; } = true;
        public bool searchBank { get; set; } = true;
        public bool searchBox { get; set; } = true;
        public bool searchByMe { get; set; }
        public int TipoDocumento { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? Agencia { get; set; }
        public int? Banco { get; set; }
        public int? CajaID { get; set; }
    }
}