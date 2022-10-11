using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SyT_FileManager.Models
{
    [Table("EstructuraOrganizacional")]
    public class EstructuraOrganizacionalModel
    {
        [ExplicitKey]
        [Required]
        [Display(Name = "Identidad")]
        public int EstOrgaID { get; set; }
        [Display(Name = "Identidad padre")]
        public int? EstOrgaIDPadre { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre")]
        public string EstOrgaNombre { get; set; }
        [StringLength(50)]
        [Display(Name = "Abreviatura")]
        public string EstOrgaAbreviatura { get; set; }
        [StringLength(10)]
        [Display(Name = "Estado")]
        public string EstOrgaStatus { get; set; }
        [Computed]
        public List<EstructuraOrganizacionalModel> Estructuras { get; set; }

        public EstructuraOrganizacionalModel(int EstOrgaID, int EstOrgaIDPadre, string EstOrgaNombre, string EstOrgaAbreviatura, string EstOrgaStatus, List<EstructuraOrganizacionalModel> Estructuras)
        {
            this.EstOrgaID = EstOrgaID;
            this.EstOrgaIDPadre = EstOrgaIDPadre;
            this.EstOrgaNombre = EstOrgaNombre;
            this.EstOrgaAbreviatura = EstOrgaAbreviatura;
            this.EstOrgaStatus = EstOrgaStatus;
            this.Estructuras = Estructuras;
        }

        public EstructuraOrganizacionalModel()
        {

        }
    }
}