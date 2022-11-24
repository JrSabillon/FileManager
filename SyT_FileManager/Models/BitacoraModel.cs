using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("Bitacora")]
    public class BitacoraModel
    {
        [ExplicitKey]
        public int BitacoraID { get; set; }
        public string Entidad { get; set; }
        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }
        public string Accion { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorActual { get; set; }
    }
}