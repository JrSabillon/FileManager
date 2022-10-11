using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("Recurso")]
    public class RecursoModel
    {
        [ExplicitKey]
        [Required]
        [StringLength(10)]
        [Display(Name = "Identificador")]
        public string RecursoID { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre")]
        public string RecursoNombre { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Estado")]
        public string RecursoStatus { get; set; }
    }
}