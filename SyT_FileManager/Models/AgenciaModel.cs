using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("Agencia")]
    public class AgenciaModel
    {
        [ExplicitKey]
        [Required]
        [Display(Name = "Identificador de agencia")]
        public int AgenciaID { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre de agencia")]
        public string AgenciaNombre { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Identificador de la zona")]
        public string ZonaID { get; set; }
        [StringLength(20)]
        [Display(Name = "Código alterno de agencia")]
        public string AgenciaCodigo { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Estado de la agencia")]
        public string AgenciaStatus { get; set; }
        [Computed]
        [Display(Name = "Nombre de la zona")]
        public string RecursoItemNombre { get; set; }
    }
}