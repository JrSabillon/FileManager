using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
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
        [JsonIgnore]
        public List<AlmacenModel> Almacenes { get; set; }
        [Computed]
        [JsonIgnore]
        public AlmacenModel SelectedAlmacen { get => Almacenes.Where(x => x.AlmacenID == AlmacenID).FirstOrDefault(); }
        [Display(Name = "Estante")]
        public string CajaEstante { get; set; }
        [Computed]
        [JsonIgnore]
        public List<RecursoItem> Estantes { get; set; }
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedEstante { get => Estantes.Where(x => x.RecursoItemID == CajaEstante).FirstOrDefault(); }
        [Display(Name = "Sección")]
        public string CajaSeccion { get; set; }
        [Computed]
        [JsonIgnore]
        public List<RecursoItem> Secciones { get; set; }
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedSeccion { get => Secciones.Where(x => x.RecursoItemID == CajaSeccion).FirstOrDefault(); }
        [Display(Name = "Nivel")]
        public string CajaNivel { get; set; }
        [Computed]
        [JsonIgnore]
        public List<RecursoItem> Niveles { get; set; }
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedNivel { get => Niveles.Where(x => x.RecursoItemID == CajaNivel).FirstOrDefault(); }
        [Display(Name = "Fila")]
        public string CajaFila { get; set; }
        [Computed]
        [JsonIgnore]
        public List<RecursoItem> Filas { get; set; }
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedFila { get => Filas.Where(x => x.RecursoItemID == CajaFila).FirstOrDefault(); }
        [Display(Name = "Ubicación")]
        public string CajaUbicacion { get; set; }
        [Computed]
        [JsonIgnore]
        public List<RecursoItem> Ubicaciones { get; set; }
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedUbicacion { get => Ubicaciones.Where(x => x.RecursoItemID == CajaUbicacion).FirstOrDefault(); }
        public string CajaUsuarioRegistro { get; set; }
        public DateTime? CajaFechaRegistro { get; set; }
        public string CajaStatus { get; set; }
        [Computed]
        [JsonIgnore]
        public List<RecursoItem> Status { get; set; }
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedStatus { get => Status.Where(x => x.RecursoItemID == CajaStatus).FirstOrDefault(); }
        [Computed]
        [JsonIgnore]
        public string CajaLabel { get => $"Caja #{CajaInactivaID}"; }
    }
}