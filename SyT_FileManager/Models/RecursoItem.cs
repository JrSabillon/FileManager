using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("RecursoItem")]
    public class RecursoItem
    {
        [ExplicitKey]
        [StringLength(10)]
        public string RecursoID { get; set; }
        [ExplicitKey]
        [StringLength(10)]
        [Display(Name = "Identificador")]
        public string RecursoItemID { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre")]
        public string RecursoItemNombre { get; set; }
        [StringLength(10)]
        [Display(Name = "Estado")]
        public string RecursoItemStatus { get; set; }
    }
}