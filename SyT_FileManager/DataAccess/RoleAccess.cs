using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using SyT_FileManager.Models;
using Dapper;
using System.Data.SqlClient;
using SyT_FileManager.AppCode;
using Dapper.Contrib.Extensions;

namespace SyT_FileManager.DataAccess
{
    public class RoleAccess
    {
        public RoleAccess()
        {

        }

        public List<RoleModel> GetRoles()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<RoleModel>("SELECT * FROM Rol").ToList();

                return data;
            }
        }

        public List<RoleModel> GetRolesUsuarios(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { UserId };
                var data = context.Query<RoleModel>("SELECT a.*, CASE WHEN b.UserId IS NULL THEN 0 ELSE 1 END Selected " +
                    "FROM Rol a LEFT JOIN UsuarioRol b ON a.RolId = b.RolId AND b.UserId = @UserId", values).ToList();

                return data;
            }
        }

        public int SaveRolesUsuarios(List<RoleModel> roles, string UserId)
        {
            string insertValues = "('{0}', '{1}')";
            var insertList = new List<string>();
            
            foreach (var item in roles)
            {
                if (item.Selected)
                {
                    insertList.Add(string.Format(insertValues, UserId, item.RolId));
                }
            }

            DeleteRolesUsuarios(UserId);

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "INSERT INTO UsuarioRol VALUES " + string.Join(",", insertList);

                return context.Execute(query);
            }
        }

        public int DeleteRolesUsuarios(string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { UserId };
                var data = context.Execute("DELETE UsuarioRol WHERE UserId = @UserId", values);

                return data;
            }
        }

        public int SaveRolePrivilege(List<PrivilegioModel> privilegios, string RolId)
        {
            string insertValues = "('{0}', '{1}')";
            var insertList = new List<string>();

            foreach (var item in privilegios)
            {
                if (item.Selected)
                {
                    insertList.Add(string.Format(insertValues, RolId, item.PrivId));
                }
            }

            DeleteRolePrivilege(RolId);

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "INSERT INTO RolPrivilegio VALUES " + string.Join(",", insertList);

                return context.Execute(query);
            }
        }

        public int DeleteRolePrivilege(string RolId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { RolId };
                var data = context.Execute("DELETE FROM RolPrivilegio WHERE RolId = @RolId", values);

                return data;
            }
        }

        public long SaveRol(RoleModel rol)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(rol);

                return data;
            }
        }
    }
}