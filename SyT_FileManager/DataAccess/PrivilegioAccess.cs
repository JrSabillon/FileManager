using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using SyT_FileManager.AppCode;
using SyT_FileManager.Models;
using Dapper;

namespace SyT_FileManager.DataAccess
{
    public class PrivilegioAccess
    {
        public PrivilegioAccess()
        {
            
        }

        public List<PrivilegioModel> GetPrivilegios()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<PrivilegioModel>("SELECT * FROM Privilegio").ToList();

                return data.Where(x => x.PrivStatus).ToList();
            }
        }

        public List<PrivilegioModel> GetPrivilegios(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { UserId };

                var data = context.Query<PrivilegioModel>("SELECT DISTINCT a.* FROM Privilegio a INNER JOIN RolPrivilegio b " +
                    "ON a.PrivId = b.PrivId INNER JOIN UsuarioRol c " +
                    "ON b.RolId = c.RolId " +
                    "WHERE c.UserId = @UserId", values).ToList();

                return data.Where(x => x.PrivStatus).ToList();
            }
        }

        public List<PrivilegioModel> GetPrivilegiosByRol(string RolId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { RolId };

                var data = context.Query<PrivilegioModel>("SELECT a.* FROM Privilegio a INNER JOIN RolPrivilegio b " +
                    "ON a.PrivId = b.PrivId " +
                    "WHERE b.RolId = @RolId", values).ToList();

                return data.Where(x => x.PrivStatus).ToList();
            }
        }

        public List<PrivilegioModel> GetPrivilegiosByRolSelected(string RolId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { RolId };

                var data = context.Query<PrivilegioModel>("select c.*, case when b.PrivId is not null then 1 else 0 end as selected " +
                    "from Rol a right join RolPrivilegio b " +
                    "on a.RolId = b.RolId right join Privilegio c " +
                    "on b.PrivId = c.PrivId and a.RolId = @RolId " +
                    "WHERE c.PrivStatus = 1", values).ToList();

                return data.Where(x => x.PrivStatus).ToList();
            }
        }
    }
}