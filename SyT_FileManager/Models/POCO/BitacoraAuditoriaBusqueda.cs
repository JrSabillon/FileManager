using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models.POCO
{
    public class BitacoraAuditoriaBusqueda
    {
        public bool SearchDate { get; set; } = true;
        public bool SearchActionType { get; set; } = true;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Accion { get; set; }
    }
}