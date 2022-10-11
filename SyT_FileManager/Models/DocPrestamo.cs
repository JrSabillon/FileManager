using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("DocPrestamo")]
    public class DocPrestamo
    {
        [ExplicitKey]
        public int PrestamoID { get; set; }
        public int DocID { get; set; }
        public int CajaID { get; set; }
        [Display(Name = "Departamento solicitante")]
        public int PrestDepartamentoSolicitanteID { get; set; }
        [Computed]
        public List<EstructuraOrganizacionalModel> Departamentos { get; set; }
        [Computed]
        public EstructuraOrganizacionalModel SelectedDepartamento { get => Departamentos.Where(x => x.EstOrgaID == PrestDepartamentoSolicitanteID).FirstOrDefault(); }
        [Display(Name = "Nombre solicitante")]
        public string PrestNombreSolicitante { get; set; }
        [Display(Name = "Correo")]
        public string PrestEmailSolicitante { get; set; }
        [Display(Name = "Fecha de solicitud")]
        public DateTime PrestFechaSolicitud { get; set; }
        [Display(Name = "Fecha de retiro")]
        public DateTime PrestFechaRetira { get; set; }
        [Display(Name = "Nombre de quien retira")]
        public string PrestPersonaRetira { get; set; }
        [Display(Name = "Otra persona retira")]
        [Computed]
        public bool OtraPersonaRetira { get; set; } = false;
        public string PrestUsuarioEntrega { get; set; }
        [Display(Name = "Fecha de devolución")]
        public DateTime? PrestFechaDevuelve { get; set; }
        [Display(Name = "Nombre de quien devolvio")]
        public string PrestPersonaDevuelve { get; set; }
        public string PrestUsuarioRecibe { get; set; }
        [Display(Name = "Plazo maximo de devolución")]
        public int PrestPlazoMaximoDevolucion { get; set; }
        [Display(Name = "Devolver documento antes de")]
        public DateTime PrestFechaMaximaDevolucion { get; set; }

        [Computed]
        public int? DocTipo { get; set; }
        [Computed]
        public List<TipoDocumentoModel> TiposDocumentos { get; set; }
        [Computed]
        public TipoDocumentoModel SelectedDocumento { get => TiposDocumentos.Where(x => x.TipoDocID == DocTipo).FirstOrDefault(); }
        [Computed]
        public int CajaActivaID { get; set; }
        [Computed]
        public int CajaInactivaID { get; set; }
    }
}