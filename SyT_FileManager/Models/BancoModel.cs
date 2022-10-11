using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("Banco")]
    public class BancoModel
    {
        [ExplicitKey]
        [Required]
        [Display(Name = "Identificador")]
        public int BancoID { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre")]
        public string BancoNombre { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Estado")]
        public string BancoStatus { get; set; }
    }
}