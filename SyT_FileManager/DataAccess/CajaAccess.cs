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

namespace SyT_FileManager.DataAccess
{
    public class CajaAccess
    {
        public CajaAccess()
        {

        }

        public CajaModel GetCaja(int CajaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<CajaModel>(CajaID);

                return data;
            }
        }

        public int Create(CajaModel caja)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(caja);

                return caja.CajaID;
            }
        }

        public bool Update(CajaModel caja)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Update(caja);

                return data;
            }
        }

        public void SetCajaHistorica_AlmacenIDOrigen(int AlmacenIDOrigen, int CajaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                //Obtener Caja de CajaHistorica que no ha sido posicionada.
                string query = "SELECT * FROM CajaHistorica WHERE CajaID = @CajaID AND CajaEstante IS NULL";
                var caja = context.QueryFirstOrDefault<CajaHistoricaModel>(query, new { CajaID });

                if(caja != null)
                {
                    query = "UPDATE CajaHistorica SET AlmacenIDOrigen = @AlmacenIDOrigen WHERE CajaHistID = @CajaHistID";
                    context.Execute(query, new { AlmacenIDOrigen, caja.CajaHistID });
                }
            }
        }

        public int NextCajaID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QueryFirst<int>("SELECT ISNULL(MAX(CajaID), 0) + 1 FROM Caja");

                return data;
            }
        }

        public int NextCajaActivaID(int AlmacenID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QueryFirst<int>("SELECT ISNULL(MAX(CajaActivaID), 0) + 1 FROM Caja WHERE AlmacenID = @AlmacenID", new { AlmacenID });

                return data;
            }
        }

        public int NextCajaInactivaID(int AlmacenID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QueryFirst<int>("SELECT ISNULL(MAX(CajaInactivaID), 0) + 1 FROM Caja WHERE AlmacenID = @AlmacenID", new { AlmacenID });

                return data;
            }
        }

        public List<CajaModel> GetCajasByStatusAndAlmacenTipo(string CajaStatus, string AlmacenTipo)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<CajaModel>("SELECT * FROM Caja a INNER JOIN Almacen b ON a.AlmacenID = b.AlmacenID WHERE a.CajaStatus = @CajaStatus AND b.AlmacenTipo = @AlmacenTipo", new { CajaStatus, AlmacenTipo }).ToList();

                return data;
            }
        }

        public List<CajaModel> GetCajasByAlmacenID_Filtered(DateTime? FechaInicio, DateTime? FechaFin, int? AgenciaID, string AlmacenID, string TipoAlmacen, int CajaID = 0)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string procedure = "GetBoxes";
                var data = context.Query<CajaModel>(procedure, new { FechaInicio, FechaFin, AgenciaID, AlmacenID, CajaID, TipoAlmacen }, commandType: CommandType.StoredProcedure).ToList();

                return data;
            }
        }

        /// <summary>
        /// Obtener cajas con documentos vencidos.
        /// </summary>
        /// <param name="AlmacenTipo">ACT para almacenes de archivos activos, INA para almacenes de archivos inactivos</param>
        /// <returns></returns>
        public List<CajaModel> GetCajasOutdated(string AlmacenTipo)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT DISTINCT c.* FROM Caja c INNER JOIN Documento d " +
                    "ON c.CajaID = d.CajaID INNER JOIN Almacen a " +
                    "ON c.AlmacenID = a.AlmacenID " +
                    "WHERE d.DocFechaVencimiento <= @Fecha AND c.CajaStatus = 'ACT' AND a.AlmacenTipo = @AlmacenTipo";
                var data = context.Query<CajaModel>(query, new { Fecha = DateTime.Now.ToString("yyyy-MM-dd"), AlmacenTipo }).ToList();

                return data;
            }
        }

        internal List<GetCajasByAlmacenTipo_RP> GetCajasByAlmacenTipo_RP(string UserId, string AlmacenTipo)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { UserId, AlmacenTipo };
                var data = context.Query<GetCajasByAlmacenTipo_RP>("GetCajasByAlmacenTipo_RP", values, commandType: CommandType.StoredProcedure).ToList();

                return data;
            }
        }

        /// <summary>
        /// Obtener cajas vencidas segun parametros establecidos para documentos en almacen activo.
        /// </summary>
        /// <returns></returns>
        public List<CajaModel> GetCajasOutdated()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT DISTINCT c.* FROM Caja c INNER JOIN Documento d " +
                    "ON c.CajaID = d.CajaID INNER JOIN Almacen a " +
                    "ON c.AlmacenID = a.AlmacenID " +
                    "WHERE d.DocFechaInfo <= @Fecha AND c.CajaStatus = 'ACT' AND a.AlmacenTipo = @AlmacenTipo";
                var data = context.Query<CajaModel>(query, new { Fecha = new DocumentoModel().FechaExpiraAlmacenActivo, AlmacenTipo = "ACT" }).ToList();

                return data;
            }
        }

        public bool UpdatePosicionCajaAlmacen(CajaModel caja)
        {
            var cajaActual = GetCaja(caja.CajaID);
            StaticHelpers.MergeObjects<CajaModel>(caja, cajaActual);
            
            caja.CajaStatus = "ACT";
            caja.CajaUsuarioRegistro = Constants.GetUserData().UserId;
            caja.CajaFechaRegistro = DateTime.Now;

            var almacen = new AlmacenAccess().GetAlmacen(caja.AlmacenID);
            if (almacen.AlmacenTipo.Equals("ACT"))
                caja.CajaActivaID = NextCajaActivaID(caja.AlmacenID); //Ahora se agrega el id de la caja activa al posicionarla.
            else
                caja.CajaInactivaID = NextCajaInactivaID(caja.AlmacenID); //Ahora se agrega el id de la caja inactiva al posicionarla.

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Update(caja);

                return data;
            }
        }

        public int GetCajaIDByAlmacenIDAndCajaInactivaID(int AlmacenID, int CajaInactivaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT CajaID FROM Caja WHERE AlmacenID = @AlmacenID AND CajaInactivaID = @CajaInactivaID";
                var data = context.QueryFirst<int>(query, new { AlmacenID, CajaInactivaID });

                return data;
            }
        }

        /// <summary>
        /// Validar que el lugar donde se esta posicionando la caja esta disponible.
        /// </summary>
        /// <param name="caja">Modelo que contiene el AlmacenID y posicion de la caja.</param>
        /// <returns>Verdadero si el lugar esta disponible, en caso contrario falso.</returns>
        public bool ValidateBoxPosition(CajaModel caja)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT * FROM Caja WHERE AlmacenID = @AlmacenID AND CajaEstante = @CajaEstante " +
                    "AND CajaSeccion = @CajaSeccion AND CajaNivel = @CajaNivel AND CajaFila = @CajaFila AND CajaUbicacion = @CajaUbicacion AND CajaStatus = 'ACT'";
                var data = context.Query<CajaModel>(query, new { caja.CajaID, caja.AlmacenID, caja.CajaEstante, caja.CajaSeccion, caja.CajaNivel, caja.CajaFila, caja.CajaUbicacion }).ToList();

                return data.Count == 0;
            }
        }
    }
}