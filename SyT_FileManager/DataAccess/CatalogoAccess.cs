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
using Newtonsoft.Json;

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
                var oldRecurso = context.Get<RecursoModel>(recurso.RecursoID);
                var updated = context.Update(recurso);

                if (updated)
                {
                    BitacoraAccess bitacoraAccess = new BitacoraAccess();
                    BitacoraModel bitacora = new BitacoraModel()
                    {
                        BitacoraID = bitacoraAccess.NextBitacoraID(),
                        Entidad = "Recurso",
                        Usuario = Constants.GetUserData().UserId,
                        Fecha = DateTime.Now,
                        Accion = Constants.EDIT,
                        ValorAnterior = JsonConvert.SerializeObject(oldRecurso),
                        ValorActual = JsonConvert.SerializeObject(recurso)
                    };

                    bitacoraAccess.Add(bitacora);

                }

                return updated;
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