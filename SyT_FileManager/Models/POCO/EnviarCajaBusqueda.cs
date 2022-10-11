using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class EnviarCajaBusqueda
    {
        public bool searchDate { get; set; } = true;
        public bool searchAgency { get; set; } = true;
        public bool searchBox { get; set; } = false;
        private DateTime? _fechaInicio = DateTime.Now;
        public DateTime? FechaInicio { 
            get {
                return searchDate ? _fechaInicio : null; 
            } 
            set {
                _fechaInicio = value;    
            } 
        }
        public DateTime? FechaFin { get; set; }
        public int Agencia { get; set; }
        public int Caja { get; set; }
    }
}