using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper.Contrib.Extensions;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using SyT_FileManager.AppCode;
using SyT_FileManager.Models;
using System.Web.Mvc;

namespace SyT_FileManager.DataAccess
{
    public class BitacoraAccess
    {
        public BitacoraAccess()
        {

        }

        public BitacoraModel Add(BitacoraModel bitacora)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(bitacora);

                if (data > 0)
                    return bitacora;

                return null;
            }
        }

        internal List<BitacoraModel> GetAll()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.GetAll<BitacoraModel>().ToList();

                return data;
            }
        }

        internal List<string> GetBitacoraAcciones()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<string>("SELECT DISTINCT Accion FROM Bitacora").ToList();

                return data;
            }
        }

        public int NextBitacoraID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT ISNULL(MAX(BitacoraID), 0) + 1 FROM Bitacora;";
                var data = context.QueryFirst<int>(query);

                return data;
            }
        }
    }
}