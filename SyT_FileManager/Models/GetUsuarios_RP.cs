using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class GetUsuarios_RP
    {
        public string UserId { get; set; }
        public string UserNombre { get; set; }
        public string UserEmail { get; set; }
        public string UserStatus { get; set; }
        public string AgenciaNombre { get; set; }
    }
}