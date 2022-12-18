using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;

namespace SyT_FileManager.Models
{
    [Table("Usuario")]
    public class UsuarioModel
    {
        [Required]
        [StringLength(20, ErrorMessage = "Maximo 20 caracteres")]
        [Display(Name = "Usuario")]
        [ExplicitKey]
        public string UserId { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Maximo 50 caracteres")]
        [Display(Name = "Nombre de usuario")]
        public string UserNombre { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Maximo 50 caracteres")]
        [Display(Name = "Correo de usuario")]
        public string UserEmail { get; set; }
        [Display(Name = "Estatus")]
        public string UserStatus { get; set; }
        [Display(Name = "Agencia")]
        public int? AgenciaID { get; set; }
        [Computed]
        [JsonIgnore]
        public List<AgenciaModel> Agencias { get; set; }
        [Computed]
        [JsonIgnore]
        public AgenciaModel SelectedAgencia {
            get
            {
                return Agencias.Where(x => x.AgenciaID == AgenciaID).FirstOrDefault();
            }
        }
    }
}