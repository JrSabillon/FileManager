using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class PrivilegioModel
    {
        public string PrivId { get; set; }
        public string PrivNombre { get; set; }
        public string PrivDescripcion { get; set; }
        public bool PrivStatus { get; set; }
        public string PrivNivelOrden { get; set; }
        public string PrivAction { get; set; }
        public string PrivController { get; set; }
        public int PrivPosicion { get; set; }
        [JsonIgnore]
        public bool Selected { get; set; }
    }
}