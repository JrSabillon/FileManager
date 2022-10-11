using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class CajaHistoricaModel
    {
        public int CajaHistID { get; set; }
        public int CajaID { get; set; }
        public int AlmacenIDOrigen { get; set; }
        public int AlmacenIDDestino { get; set; }
        public DateTime CajaHistFechaMovimiento { get; set; }
        public string CajaHistUsuarioMovimiento { get; set; }
    }
}