using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models.POCO
{
    public class UsuariosBusqueda
    {
        public bool searchStatus { get; set; }
        public bool searchName { get; set; }
        public bool searchUserId { get; set; }
        public string status { get; set; }
        public string name { get; set; }
        public string userId { get; set; }
    }
}