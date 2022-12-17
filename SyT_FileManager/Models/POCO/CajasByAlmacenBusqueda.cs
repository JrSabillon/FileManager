using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models.POCO
{
    public class CajasByAlmacenBusqueda
    {
        public bool SearchBox { get; set; }
        public bool SearchDate { get; set; }
        public bool SearchUser { get; set; }
        public int? CajaID { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string User { get; set; }
    }
}