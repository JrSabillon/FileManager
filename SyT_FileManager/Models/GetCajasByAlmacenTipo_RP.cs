using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class GetCajasByAlmacenTipo_RP
    {
        public int CajaID { get; set; }
        public int CajaActivaID { get; set; }
        public int CajaInactivaID { get; set; }
        public DateTime CajaFechaRecepcion { get; set; }
        public string CajaPersonaEntrega { get; set; }
        public string AlmacenNombre { get; set; }
        public string Estante { get; set; }
        public string Seccion { get; set; }
        public string Nivel { get; set; }
        public string Fila { get; set; }
        public string Ubicacion { get; set; }
    }
}