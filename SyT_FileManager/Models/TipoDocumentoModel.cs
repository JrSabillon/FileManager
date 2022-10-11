using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("TipoDocumento")]
    public class TipoDocumentoModel
    {
        [ExplicitKey]
        [Required]
        [Display(Name = "Identificador")]
        public int TipoDocID { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre")]
        public string TipoDocNombre { get; set; }
        [Display(Name = "Plazo")]
        public int TipoDocPlazo { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Estado")]
        public string TipoDocStatus { get; set; }
    }
}