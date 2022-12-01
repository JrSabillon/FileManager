using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    public class GetDocumentos_RP
    {
        public string DocAgenciaID { get; set; }
        public DateTime DocFechaInfo { get; set; }
        public string DocDescripcion { get; set; }
        public DateTime DocFechaVencimiento { get; set; }
        public int AlmacenID { get; set; }
        public int CajaActivaID { get; set; }
        public int CajaInactivaID { get; set; }
        public string CajaEstante { get; set; }
        public string CajaFila { get; set; }
        public string CajaNivel { get; set; }
        public string CajaSeccion { get; set; }
        public string CajaUbicacion { get; set; }
        public string Departamento { get; set; }
        public string CodigoDepartamento { get; set; }
        public string CentroCosto { get; set; }
        public string CodigoCentroCosto { get; set; }
        public string TipoDocPlazo { get; set; }
        public string TipoDocNombre { get; set; }
        public string AgenciaNombre { get; set; }
        public List<AlmacenModel> Almacenes { get; set; }
        public AlmacenModel SelectedAlmacen { 
            get
            {
                return Almacenes.Where(x => x.AlmacenID == AlmacenID).FirstOrDefault();
            }
        }
    }
}