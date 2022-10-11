using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using SyT_FileManager.Models;
using SyT_FileManager.AppCode;
using Dapper.Contrib.Extensions;
using System.Web.Mvc;

namespace SyT_FileManager.DataAccess
{
    public class AgenciaAccess
    {
        public AgenciaAccess()
        {

        }

        public List<AgenciaModel> GetAgencias()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var query = "SELECT a.*, b.RecursoItemNombre FROM Agencia a INNER JOIN RecursoItem b " +
                    "ON a.ZonaID = b.RecursoItemID AND b.RecursoID = 'ZONA' " +
                    "WHERE RecursoItemStatus = 'AC'";
                var data = context.Query<AgenciaModel>(query).ToList();

                return data;
            }
        }

        public AgenciaModel GetAgencia(int? id)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<AgenciaModel>(id);

                return data;
            }
        }

        public long Create(AgenciaModel agencia)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                agencia.AgenciaID = GetAgenciaID();
                var data = context.Insert(agencia);

                return data;
            }
        }

        public bool Update(AgenciaModel agencia)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Update(agencia);

                return data;
            }
        }

        public int GetAgenciaID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QueryFirst<int>("SELECT MAX(AgenciaID) + 1 FROM Agencia");

                return data;
            }
        }

        public List<SelectListItem> GetAgenciaStatus()
        {
            var status = new List<SelectListItem>
            {
                new SelectListItem { Value = "AC", Text = "Activo" },
                new SelectListItem { Value = "IN", Text = "Inactivo" }
            };

            return status;
        }

        public string GetZonaIDByAgenciaID(int AgenciaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT ZonaID FROM Agencia WHERE AgenciaID = @AgenciaID";
                var data = context.QueryFirst<string>(query, new { AgenciaID });

                return data;
            }
        }
    }
}