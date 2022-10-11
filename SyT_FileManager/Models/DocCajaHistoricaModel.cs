using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class DocCajaHistoricaModel
    {
        public int DocCajaHistID { get; set; }
        public int DocID { get; set; }
        public int? CajaIDOrigen { get; set; }
        public int CajaIDDestino { get; set; }
        public DateTime DocCajaHistFechaMovimiento { get; set; }
        public string DocCajaHistUsuarioMovimiento { get; set; }
    }
}