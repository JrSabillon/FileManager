using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using SyT_FileManager.Models;
using SyT_FileManager.AppCode;
using System.Web.Mvc;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;

namespace SyT_FileManager.DataAccess
{
    public class RecursoAccess
    {
        public RecursoAccess()
        {

        }

        public List<RecursoItem> GetRecursoItems(string RecursoID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { RecursoID };
                string query = "SELECT * FROM RecursoItem WHERE RecursoID = @RecursoID";

                var data = context.Query<RecursoItem>(query, values).ToList();

                return data;
            }
        }

        public RecursoItem GetRecursoItem(string RecursoID, string RecursoItemID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QueryFirstOrDefault<RecursoItem>("SELECT * FROM RecursoItem WHERE RecursoID = @RecursoID AND RecursoItemID = @RecursoItemID", new { RecursoID, RecursoItemID });

                return data;
            }
        }

        public List<SelectListItem> GetRecursoStatus()
        {
            var status = new List<SelectListItem>
            {
                new SelectListItem { Value = "AC", Text = "Activo" },
                new SelectListItem { Value = "IN", Text = "Inactivo" }
            };

            return status;
        }

        public long Create(RecursoItem recursoItem)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(recursoItem);

                return data;
            }
        }

        public bool Update(RecursoItem recursoItem)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var oldRecursoItem = GetRecursoItem(recursoItem.RecursoID, recursoItem.RecursoItemID);
                var updated = context.Update(recursoItem);

                if (updated)
                {
                    BitacoraAccess bitacoraAccess = new BitacoraAccess();
                    BitacoraModel bitacora = new BitacoraModel()
                    {
                        BitacoraID = bitacoraAccess.NextBitacoraID(),
                        Entidad = "RecursoItem",
                        Usuario = Constants.GetUserData().UserId,
                        Fecha = DateTime.Now,
                        Accion = Constants.EDIT,
                        ValorAnterior = JsonConvert.SerializeObject(oldRecursoItem),
                        ValorActual = JsonConvert.SerializeObject(recursoItem)
                    };

                    bitacoraAccess.Add(bitacora);
                }

                return updated;
            }
        }
    }
}