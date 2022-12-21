using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SyT_FileManager.DataAccess;

namespace SyT_FileManager.Models
{
    [Table("DocTritura")]
    public class DocTrituraModel
    {
        public int TrituraID { get; set; }
        [ExplicitKey] //Llave explicita solo para borrar documentos(reversarlos de la trituración)
        public int DocID { get; set; }
        [Computed]
        [JsonIgnore]
        public DocumentoModel Documento { get; set; }
        public int CajaID { get; set; }
        public int AlmacenID { get; set; }
        public string TrituraNombreTestigo { get; set; }
        public DateTime TrituraFecha { get; set; }
        public string TrituraUsuario { get; set; }
    }
}