using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("UsuarioAlmacen")]
    public class UsuarioAlmacenModel
    {
        [ExplicitKey]
        public string UserId { get; set; }
        public int AlmacenID { get; set; }
    }
}