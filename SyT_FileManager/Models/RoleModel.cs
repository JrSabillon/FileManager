using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("Rol")]
    public class RoleModel
    {
        [ExplicitKey]
        [Required]
        [StringLength(20)]
        [Display(Name = "Rol identificador *")]
        public string RolId { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Nombre del rol *")]
        public string RolNombre { get; set; }
        [StringLength(150)]
        [Display(Name = "Descripción del rol")]
        public string RolDescripcion { get; set; }
        [Computed]
        [JsonIgnore]
        public bool Selected { get; set; }
    }
}