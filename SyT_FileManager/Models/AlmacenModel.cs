using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("Almacen")]
    public class AlmacenModel
    {
        [ExplicitKey]
        [Display(Name = "Identificador")]
        public int AlmacenID { get; set; }
        [Display(Name = "Nombre")]
        [StringLength(200)]
        public string AlmacenNombre { get; set; }
        [Display(Name = "Tipo de almacen")]
        [StringLength(10)]
        public string AlmacenTipo { get; set; }

        [Computed]
        [JsonIgnore]
        public List<RecursoItem> TipoAlmacenes { get; set; } = new List<RecursoItem>();
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedTipoAlmacen { get => TipoAlmacenes.Where(x => x.RecursoItemID.Equals(AlmacenTipo)).First(); }
        
        [Display(Name = "Dirección")]
        [StringLength(500)]
        public string AlmacenDireccion { get; set; }
        [Display(Name = "Estado")]
        [StringLength(10)]
        public string AlmacenStatus { get; set; }

        [Display(Name = "Agencia")]
        public int? AgenciaID { get; set; }

        [Computed]
        [JsonIgnore]
        public List<AgenciaModel> Agencias { get; set; } = new List<AgenciaModel>();
        [Computed]
        [JsonIgnore]
        public AgenciaModel SelectedAgencia { get => Agencias.Where(x => x.AgenciaID.Equals(AgenciaID)).First(); }
        
        [Display(Name = "Zona")]
        [StringLength(10)]
        public string ZonaId { get; set; }

        [Computed]
        [JsonIgnore]
        public List<RecursoItem> Zonas { get; set; } = new List<RecursoItem>();
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedZona { get => Zonas.Where(x => x.RecursoItemID.Equals(ZonaId)).First(); }
        
        [Computed]
        [JsonIgnore]
        public string AlmacenLabel { get => AlmacenNombre + " - " + AlmacenDireccion; }
    }
}