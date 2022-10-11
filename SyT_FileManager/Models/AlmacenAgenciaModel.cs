using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("AlmacenAgencia")]
    public class AlmacenAgenciaModel
    {
        [ExplicitKey]
        [Display(Name = "Almacen")]
        public int AlmacenID { get; set; }
        [Computed]
        public List<AlmacenModel> Almacenes { get; set; }
        [ExplicitKey]
        [Display(Name = "Agencia")]
        public int AgenciaID { get; set; }
        [Computed]
        public List<AgenciaModel> Agencias { get; set; }
    }
}