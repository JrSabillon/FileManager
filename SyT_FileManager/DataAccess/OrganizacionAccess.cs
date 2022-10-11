using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Data;
using System.Data.SqlClient;
using SyT_FileManager.Models;
using SyT_FileManager.AppCode;
using System.Web.Mvc;

namespace SyT_FileManager.DataAccess
{
    public class OrganizacionAccess
    {
        public OrganizacionAccess()
        {

        }

        public List<EstructuraOrganizacionalModel> GetEstructuraInicial()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<EstructuraOrganizacionalModel>("SELECT * FROM EstructuraOrganizacional WHERE EstOrgaIDPadre IS NULL").ToList();

                return data;
            }
        }

        public List<EstructuraOrganizacionalModel> GetNodosEstructura(int? EstOrgaId) 
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT * FROM EstructuraOrganizacional WHERE EstOrgaIDPadre = @EstOrgaId";
                var values = new { EstOrgaId };

                var data = context.Query<EstructuraOrganizacionalModel>(query, values).ToList();

                return data;
            }
        }

        public bool TieneNodos(int EstOrgaId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QuerySingle<int>("SELECT COUNT(*) FROM EstructuraOrganizacional WHERE EstOrgaIDPadre = @EstOrgaId", new { EstOrgaId });

                return data > 0;
            }
        }

        public EstructuraOrganizacionalModel GetEstructuraOrganizacional(int EstOrgaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<EstructuraOrganizacionalModel>(EstOrgaID);

                return data;
            }
        }

        public List<SelectListItem> GetEstructuraStatus()
        {
            var status = new List<SelectListItem>
            {
                new SelectListItem { Value = "AC", Text = "Activo" },
                new SelectListItem { Value = "IN", Text = "Inactivo" }
            };

            return status;
        }

        public long Create(EstructuraOrganizacionalModel estructuraOrganizacional)
        {
            estructuraOrganizacional.EstOrgaID = GetNextID();

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(estructuraOrganizacional);

                return data;
            }
        }

        public bool Update(EstructuraOrganizacionalModel estructuraOrganizacional)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Update(estructuraOrganizacional);

                return data;
            }
        }

        private int GetNextID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QuerySingle<int>("SELECT MAX(EstOrgaID) + 1 FROM EstructuraOrganizacional");

                return data;
            }
        }

        public List<EstructuraOrganizacionalModel> GetCCDropdown()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<EstructuraOrganizacionalModel>("SELECT * FROM(" +
                    "SELECT ISNULL(EstOrgaIDPadre, EstOrgaID) Orden, EstOrgaID, EstOrgaIDPadre, IIF(EstOrgaIDPadre IS NULL, EstOrgaNombre, '   ' + EstOrgaNombre) EstOrgaNombre, EstOrgaAbreviatura, EstOrgaStatus " +
                    "FROM EstructuraOrganizacional WHERE EstOrgaIDPadre IS NULL OR EstOrgaIDPadre IN (SELECT EstOrgaID FROM EstructuraOrganizacional WHERE EstOrgaIDPadre IS NULL)" +
                    ") t1 ORDER BY t1.Orden, t1.EstOrgaNombre DESC").ToList();

                return data;
            }
        }

        public List<EstructuraOrganizacionalModel> GetAllEstructuraOrganizacional()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.GetAll<EstructuraOrganizacionalModel>().ToList();

                return data;
            }
        }
    }
}