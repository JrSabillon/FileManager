using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Newtonsoft.Json;

namespace SyT_FileManager.Models
{
    [Table("Documento")]
    public class DocumentoModel
    {
        [Required]
        [Display(Name = "Identidficador documento")]
        [ExplicitKey]
        public int DocID { get; set; }

        [Display(Name = "Caja")]
        public int CajaID { get; set; }
        [Computed]
        [JsonIgnore]
        public CajaModel Caja { get; set; } = new CajaModel();
        
        [Display(Name = "Fecha del documento")]
        public DateTime? DocFechaInfo { get; set; }
        
        [StringLength(2000)]
        [Display(Name = "Descripción del documento")]
        public string DocDescripcion { get; set; }
        
        [Display(Name = "Banco")]
        public int? DocBancoID { get; set; }
        
        [Computed]
        [JsonIgnore]
        public List<BancoModel> Bancos { get; set; }
        [Computed]
        public BancoModel SelectedBanco { get => Bancos.Where(x => x.BancoID == DocBancoID).First(); }
        
        [StringLength(10)]
        [Display(Name = "Zona")]
        public string DocZonaID { get; set; }
        [Computed]
        [JsonIgnore]
        public List<RecursoItem> Zonas { get; set; }
        [Computed]
        public RecursoItem SelectedZona { get => Zonas.Where(x => x.RecursoItemID == DocZonaID).First(); }
        
        [Display(Name = "Departamento")]
        public int? DocDepartamentoID { get; set; }
        
        [Computed]
        [JsonIgnore]
        public List<EstructuraOrganizacionalModel> Departamentos { get; set; }
        [Computed]
        [JsonIgnore]
        public EstructuraOrganizacionalModel SelectedDepartamento { get => Departamentos.Where(x => x.EstOrgaID == DocDepartamentoID).First(); }
        
        [Display(Name = "Centro de costo")]
        public int? DocCCCCID { get; set; }
        
        [Computed]
        [JsonIgnore]
        public List<EstructuraOrganizacionalModel> CentrosDeCosto { get; set; }
        [Computed]
        [JsonIgnore]
        public EstructuraOrganizacionalModel SelectedCentroDeCosto { get => CentrosDeCosto.Where(x => x.EstOrgaID == DocCCCCID).First(); }
        
        [Display(Name = "Tipo de documento")]
        public int? DocTipo { get; set; }
        
        [Computed]
        [JsonIgnore]
        public List<TipoDocumentoModel> TiposDocumentos { get; set; }
        [Computed]
        [JsonIgnore]
        public TipoDocumentoModel SelectedDocumento { get => TiposDocumentos.Where(x => x.TipoDocID == DocTipo).First(); }
        
        [StringLength(10)]
        [Display(Name = "Plazo de retención")]
        public string DocPlazoRetencion { get; set; }
        
        [Computed]
        [JsonIgnore]
        public List<RecursoItem> PlazosRetenciones { get; set; }
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedPlazoRetencion { get => PlazosRetenciones.Where(x => x.RecursoItemID == DocPlazoRetencion).First(); }
        
        [Display(Name = "Fecha de vencimiento")]
        public DateTime? DocFechaVencimiento { get; set; }
        
        [StringLength(10)]
        [Display(Name = "Estado")]
        public string DocStatus { get; set; }

        [Computed]
        [JsonIgnore]
        public List<RecursoItem> Estados { get; set; }
        [Computed]
        [JsonIgnore]
        public RecursoItem SelectedEstado { get => Estados.Where(x => x.RecursoItemID == DocStatus).First(); }
        
        [Display(Name = "Fecha de trituración")]
        public DateTime? DocFechaTrituracion { get; set; }
        [Required]
        [Display(Name = "Paquete de documentos")]
        public bool DocPaquete { get; set; }
        [Computed]
        [JsonIgnore]
        public bool Create { get; set; } = true;

        public int? DocAgenciaID { get; set; }
        [Computed]
        [JsonIgnore]
        public List<AgenciaModel> Agencias { get; set; }
        [Computed]
        [JsonIgnore]
        public AgenciaModel SelectedAgencia { get => Agencias.Where(x => x.AgenciaID == DocAgenciaID).First(); }

        [Computed]
        [JsonIgnore]
        public DateTime FechaExpiraAlmacenActivo
        {
            get
            {
                DateTime today = DateTime.Now;
                int yearsRange = Convert.ToInt32(ConfigurationManager.AppSettings["DocsActiveYears"] ?? "2"); //Si no esta configurado por defecto seran 2 años.
                //int startMonth = Convert.ToInt32(ConfigurationManager.AppSettings["DocsActiveMonth"] ?? today.Month.ToString()); //Si no esta configurado por defecto sera marzo.

                var startDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0); //Comienza a las 12 de la mañana del mes configurado para este año.
                var expirationDate = startDate.AddYears(yearsRange - (yearsRange * 2)); //Se le restan la cantidad de años configurados para que la fecha de expiracion sea el primer dia del mes configurado con el año de vencimiento.

                return expirationDate;
            }
        }
    }
}