using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("Caja")]
    public class CajaModel
    {
        [ExplicitKey]
        public int CajaID { get; set; }
        public int CajaActivaID { get; set; }
        public int CajaInactivaID { get; set; }
        public DateTime? CajaFechaRecepcion { get; set; }
        public string CajaPersonaEntrega { get; set; }
        public int AlmacenID { get; set; }
        [Computed]
        public List<AlmacenModel> Almacenes { get; set; }
        [Computed]
        public AlmacenModel SelectedAlmacen { get => Almacenes.Where(x => x.AlmacenID == AlmacenID).First(); }
        [Display(Name = "Estante")]
        public string CajaEstante { get; set; }
        [Computed]
        public List<RecursoItem> Estantes { get; set; }
        [Computed]
        public RecursoItem SelectedEstante { get => Estantes.Where(x => x.RecursoItemID == CajaEstante).First(); }
        [Display(Name = "Sección")]
        public string CajaSeccion { get; set; }
        [Computed]
        public List<RecursoItem> Secciones { get; set; }
        [Computed]
        public RecursoItem SelectedSeccion { get => Secciones.Where(x => x.RecursoItemID == CajaSeccion).First(); }
        [Display(Name = "Nivel")]
        public string CajaNivel { get; set; }
        [Computed]
        public List<RecursoItem> Niveles { get; set; }
        [Computed]
        public RecursoItem SelectedNivel { get => Niveles.Where(x => x.RecursoItemID == CajaNivel).First(); }
        [Display(Name = "Fila")]
        public string CajaFila { get; set; }
        [Computed]
        public List<RecursoItem> Filas { get; set; }
        [Computed]
        public RecursoItem SelectedFila { get => Filas.Where(x => x.RecursoItemID == CajaFila).First(); }
        [Display(Name = "Ubicación")]
        public string CajaUbicacion { get; set; }
        [Computed]
        public List<RecursoItem> Ubicaciones { get; set; }
        [Computed]
        public RecursoItem SelectedUbicacion { get => Ubicaciones.Where(x => x.RecursoItemID == CajaUbicacion).First(); }
        public string CajaUsuarioRegistro { get; set; }
        public DateTime? CajaFechaRegistro { get; set; }
        public string CajaStatus { get; set; }
        [Computed]
        public List<RecursoItem> Status { get; set; }
        [Computed]
        public RecursoItem SelectedStatus { get => Status.Where(x => x.RecursoItemID == CajaStatus).First(); }
        [Computed]
        public string CajaLabel { get => $"Caja #{CajaInactivaID}"; }
    }
}