using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Data;
using System.Data.SqlClient;
using SyT_FileManager.AppCode;
using SyT_FileManager.Models;
using System.Web.Mvc;

namespace SyT_FileManager.DataAccess
{
    public class CatalogoAccess
    {
        public CatalogoAccess()
        {

        }

        public List<RecursoModel> GetRecursos()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.GetAll<RecursoModel>().ToList();

                return data;
            }
        }

        public RecursoModel GetRecurso(string RecursoID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<RecursoModel>(RecursoID);

                return data;
            }
        }

        public long Create(RecursoModel recurso)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(recurso);

                return data;
            }
        }

        public bool Update(RecursoModel recurso)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Update(recurso);

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
    }
}