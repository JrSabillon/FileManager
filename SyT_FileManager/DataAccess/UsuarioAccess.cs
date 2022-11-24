using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using Dapper;
using SyT_FileManager.Models;
using System.Data;
using SyT_FileManager.AppCode;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;

namespace SyT_FileManager.DataAccess
{
    public class UsuarioAccess
    {
        public UsuarioAccess()
        {

        }

        public List<UsuarioModel> GetUsuarios()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT * FROM Usuario";
                var data = context.Query<UsuarioModel>(query).ToList();

                return data;
            }
        }

        public UsuarioModel GetUsuario(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<UsuarioModel>(UserId);

                return data;
            }
        }

        public long SaveUsuario(UsuarioModel User)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var user = this.GetUsuario(User.UserId);

                if(user == null)
                    return context.Insert(User);
                else
                {
                    var updated = this.UpdateUsuario(User);

                    return updated ? 1 : 0;
                }
            }
        }

        public bool UpdateUsuario(UsuarioModel User)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var oldUser = GetUsuario(User.UserId);
                var updated = context.Update(User);

                if (updated)
                {
                    BitacoraAccess bitacoraAccess = new BitacoraAccess();
                    BitacoraModel bitacora = new BitacoraModel()
                    {
                        BitacoraID = bitacoraAccess.NextBitacoraID(),
                        Entidad = "Rol",
                        Usuario = Constants.GetUserData().UserId,
                        Fecha = DateTime.Now,
                        Accion = Constants.DELETE,
                        ValorAnterior = JsonConvert.SerializeObject(oldUser),
                        ValorActual = JsonConvert.SerializeObject(User)
                    };

                    bitacoraAccess.Add(bitacora);
                }

                return updated;
            }
        }

        /// <summary>
        /// Validar si un id de usuario esta disponible
        /// </summary>
        /// <param name="UserId">ID de usuario a ingresar</param>
        /// <returns>Verdadero si el ID esta disponible, falso en caso contrario.</returns>
        public bool ValidateUserId(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<UsuarioModel>(UserId);

                return data == null;
            }
        }

        /// <summary>
        /// Almacenes que tiene asignados un usuario determinado
        /// </summary>
        /// <returns></returns>
        public List<AlmacenModel> GetAlmacenesAsignados(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<AlmacenModel>("SELECT a.* FROM Almacen a " +
                    "INNER JOIN UsuarioAlmacen b " +
                    "ON a.AlmacenID = b.AlmacenID " +
                    "WHERE b.UserId = @UserId AND a.AlmacenStatus = 'AC'", new { UserId })
                    .ToList();

                return data;
            }
        }

        /// <summary>
        /// Almacenes que no estan asignados a un usuario determinado
        /// </summary>
        /// <returns></returns>
        public List<AlmacenModel> GetAlmacenesDisponibles(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<AlmacenModel>("select * FROM Almacen a " +
                    "WHERE a.AlmacenStatus = 'AC' AND " +
                    "NOT EXISTS(SELECT 1 FROM UsuarioAlmacen b WHERE b.AlmacenID = a.AlmacenID AND b.UserId = @UserId)", new { UserId })
                    .ToList();

                return data;
            }
        }

        public long CreateUsuarioAlmacen(string UserId, int[] AlmacenID)
        {
            DeleteUsuarioAlmacen(UserId);
            long affectedRows = 0;

            if (AlmacenID == null || AlmacenID.Length == 0)
                return affectedRows;

            for (int i = 0; i < AlmacenID.Length; i++)
            {
                using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
                {
                    affectedRows += context.Execute("INSERT INTO UsuarioAlmacen VALUES (@UserId, @AlmacenID)", new { UserId, AlmacenID = AlmacenID[i] });
                }
            }

            return affectedRows;
        }

        public int DeleteUsuarioAlmacen(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Execute("DELETE UsuarioAlmacen WHERE UserId = @UserId", new { UserId });

                return data;
            }
        }

        public bool Delete(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var user = context.Get<UsuarioModel>(UserId);
                var deleted = context.Delete(user);

                if (deleted)
                {
                    BitacoraAccess bitacoraAccess = new BitacoraAccess();
                    BitacoraModel bitacora = new BitacoraModel()
                    {
                        BitacoraID = bitacoraAccess.NextBitacoraID(),
                        Entidad = "Rol",
                        Usuario = Constants.GetUserData().UserId,
                        Fecha = DateTime.Now,
                        Accion = Constants.DELETE,
                        ValorAnterior = JsonConvert.SerializeObject(user),
                        ValorActual = ""
                    };

                    bitacoraAccess.Add(bitacora);
                }

                return deleted;
            }
        }
    }
}