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
    public class AlmacenAccess
    {
        public AlmacenAccess()
        {

        }

        public List<AlmacenModel> GetAlmacenes()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.GetAll<AlmacenModel>().ToList();

                return data;
            }
        }

        public List<AlmacenModel> GetAlmacenesByAlmacenTipo(string AlmacenTipo)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<AlmacenModel>("SELECT * FROM Almacen WHERE AlmacenTipo = @AlmacenTipo", new { AlmacenTipo }).ToList();

                return data;
            }
        }

        public AlmacenModel GetAlmacen(int AlmacenID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<AlmacenModel>(AlmacenID);

                return data;
            }
        }

        public List<SelectListItem> GetAlmacenStatus()
        {
            var status = new List<SelectListItem>
            {
                new SelectListItem { Value = "AC", Text = "Activo" },
                new SelectListItem { Value = "IN", Text = "Inactivo" }
            };

            return status;
        }

        public long Create(AlmacenModel almacen)
        {
            almacen.AlmacenID = NextAlmacenID();

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(almacen);

                return data;
            }
        }

        public bool Update(AlmacenModel almacen)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Update(almacen);

                return data;
            }
        }

        public int NextAlmacenID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QueryFirst<int>("SELECT ISNULL(MAX(AlmacenID), 0) + 1 FROM Almacen");

                return data;
            }
        }

        /// <summary>
        /// Almacenes que tiene asignados una agencia determinada
        /// </summary>
        /// <returns></returns>
        public List<AlmacenModel> GetAlmacenesAsignados(int AgenciaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<AlmacenModel>("SELECT a.* FROM Almacen a " +
                    "INNER JOIN AlmacenAgencia b " +
                    "ON a.AlmacenID = b.AlmacenID " +
                    "WHERE b.AgenciaID = @AgenciaID AND a.AlmacenStatus = 'AC'", new { AgenciaID })
                    .ToList();

                return data;
            }
        }

        /// <summary>
        /// Almacenes que no estan asignados a una agencia determinada
        /// </summary>
        /// <returns></returns>
        public List<AlmacenModel> GetAlmacenesDisponibles(int AgenciaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<AlmacenModel>("select * FROM Almacen a " +
                    "WHERE a.AlmacenStatus = 'AC' AND " +
                    "NOT EXISTS(SELECT 1 FROM AlmacenAgencia b WHERE b.AlmacenID = a.AlmacenID AND b.AgenciaID = @AgenciaID)", new { AgenciaID })
                    .ToList();

                return data;
            }
        }

        public long CreateAlmacenAgencia(int AgenciaID, int[] AlmacenID)
        {
            DeleteAlmacenAgencia(AgenciaID);
            long affectedRows = 0;

            if (AlmacenID == null || AlmacenID.Length == 0)
                return affectedRows;

            for (int i = 0; i < AlmacenID.Length; i++)
            {
                using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
                {
                    affectedRows += context.Execute("INSERT INTO AlmacenAgencia VALUES (@AlmacenID, @AgenciaID)", new { AlmacenID = AlmacenID[i], AgenciaID });
                }
            }

            return affectedRows;
        }

        public int DeleteAlmacenAgencia(int AgenciaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Execute("DELETE AlmacenAgencia WHERE AgenciaID = @AgenciaID", new { AgenciaID });

                return data;
            }
        }

        /// <summary>
        /// Obtener los almacenes de archivos activos de determinada agencia
        /// </summary>
        /// <returns></returns>
        public List<AlmacenModel> GetAlmacenesActivosByAgenciaID(int? AgenciaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT b.* FROM AlmacenAgencia a INNER JOIN Almacen b " +
                    "ON a.AlmacenID = b.AlmacenID " +
                    "WHERE a.AgenciaID = @AgenciaID";

                var data = context.Query<AlmacenModel>(query, new { AgenciaID }).ToList();

                return data;
            }
        }

        /// <summary>
        /// Obtener los almacenes de archivos activos de determinada agencia
        /// </summary>
        /// <returns></returns>
        public List<AlmacenModel> GetAlmacenesActivos()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT b.* FROM AlmacenAgencia a INNER JOIN Almacen b " +
                    "ON a.AlmacenID = b.AlmacenID AND AlmacenStatus = 'AC'";

                var data = context.Query<AlmacenModel>(query).ToList();

                return data;
            }
        }

        public List<AlmacenModel> GetAlmacenesByUsuarioAndTipoAlmacen(string AlmacenTipo, string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT a.* FROM Almacen a INNER JOIN UsuarioAlmacen b ON a.AlmacenID = b.AlmacenID WHERE a.AlmacenTipo = @AlmacenTipo AND b.UserId = @UserId";
                var data = context.Query<AlmacenModel>(query, new { AlmacenTipo, UserId }).ToList();

                return data;
            }
        }

        public List<AlmacenModel> GetAlmacenesByUsuario(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT a.* FROM Almacen a INNER JOIN UsuarioAlmacen b ON a.AlmacenID = b.AlmacenID WHERE b.UserId = @UserId";
                var data = context.Query<AlmacenModel>(query, new { UserId }).ToList();

                return data;
            }
        }

        public List<int> GetAlmacenIDByUserID(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<int>("SELECT AlmacenID FROM UsuarioAlmacen WHERE UserId = @UserId", new { UserId }).ToList();

                return data;
            }
        }
    }
}