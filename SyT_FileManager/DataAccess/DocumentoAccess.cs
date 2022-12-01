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
using SyT_FileManager.Business;
using SyT_FileManager.Models.POCO;

namespace SyT_FileManager.DataAccess
{
    public class DocumentoAccess
    {
        public DocumentoAccess()
        {
        }

        public long CreateDocuments(List<DocumentoModel> documentos)
        {
            long insertedRows = 0;

            foreach (var documento in documentos)
            {
                using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
                {
                    insertedRows += context.Insert(documento);
                }

                CreateDocumentoHistorico(new DocCajaHistoricaModel()
                {
                    DocCajaHistID = GetNextDocCajaHistID(),
                    DocID = documento.DocID,
                    CajaIDOrigen = null,
                    CajaIDDestino= documento.CajaID,
                    DocCajaHistFechaMovimiento = DateTime.Now,
                    DocCajaHistUsuarioMovimiento = Constants.GetUserData().UserId
                });
            }

            return insertedRows;
        }

        public long CreateDocument(DocumentoModel documento)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                return context.Insert(documento);
            }
        }

        public bool Update(DocumentoModel documento)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                return context.Update(documento);
            }
        }

        public List<DocumentoModel> GetDocumentosByCajaID(int CajaID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Query<DocumentoModel>("SELECT * FROM Documento WHERE CajaID = @CajaID AND DocStatus IN('ACT', 'PRS')", new { CajaID }).ToList();

                return data;
            }
        }

        public DocumentoModel GetDocumento(int DocID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<DocumentoModel>(DocID);

                return data;
            }
        }

        public int GetNextDocID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT ISNULL(MAX(DocID), 0) + 1 FROM Documento";
                var data = context.QueryFirst<int>(query);

                return data;
            }
        }

        public int CreateDocumentoHistorico(DocCajaHistoricaModel CajaHistorica)
        {
            CajaHistorica.DocCajaHistID = GetNextDocCajaHistID();

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "INSERT INTO [dbo].[DocCajaHistorica](" +
                    "[DocCajaHistID], [DocID], [CajaIDOrigen], [CajaIDDestino], [DocCajaHistFechaMovimiento], [DocCajaHistUsuarioMovimiento]) VALUES (" +
                    "@DocCajaHistID, @DocID, @CajaIDOrigen, @CajaIDDestino, @DocCajaHistFechaMovimiento, @DocCajaHistUsuarioMovimiento)";
                var data = context.Execute(query, new { CajaHistorica.DocCajaHistID, CajaHistorica.DocID, CajaHistorica.CajaIDOrigen, CajaHistorica.CajaIDDestino, CajaHistorica.DocCajaHistFechaMovimiento, CajaHistorica.DocCajaHistUsuarioMovimiento });

                return data;
            }
        }

        public int GetNextDocCajaHistID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT ISNULL(MAX(DocCajaHistID), 0) + 1 FROM DocCajaHistorica";
                var data = context.QueryFirst<int>(query);

                return data;
            }
        }

        public void TransferDocuments_BoxToBox(int CajaIDOrigen, int CajaIDDestino)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string procedure = "TransferDocuments_BoxToBox";
                context.Execute(procedure, new { CajaIDOrigen, CajaIDDestino }, commandType: CommandType.StoredProcedure);
            }

            var documents = GetDocumentosByCajaID(CajaIDDestino);
            var today = DateTime.Now;
            var user = Constants.GetUserData();

            documents.ForEach((DocumentoModel document) =>
            {
                DocCajaHistoricaModel DocHistorico = new DocCajaHistoricaModel
                {
                    DocID = document.DocID,
                    CajaIDOrigen = CajaIDOrigen,
                    CajaIDDestino = CajaIDDestino,
                    DocCajaHistFechaMovimiento = today,
                    DocCajaHistUsuarioMovimiento = user.UserId
                };

                CreateDocumentoHistorico(DocHistorico);
            });
        }

        public List<DocumentoModel> GetDocumentosByDocTipoAndDocStatusAnd_CajaAlmacenID(int DocTipo, string DocStatus, int[] AlmacenID)
        {
            List<DocumentoModel> data = new List<DocumentoModel>();

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT * FROM Documento d INNER JOIN Caja c " +
                    "ON d.CajaID = c.CajaID " +
                    "WHERE d.DocStatus = @DocStatus AND d.DocTipo = @DocTipo AND c.AlmacenID IN @AlmacenID AND c.CajaStatus = 'ACT'";
                data = context.Query<DocumentoModel>(query, new { DocTipo, DocStatus, AlmacenID }).ToList();
            }

            var CajaAccess = new CajaAccess();
            data.ForEach((DocumentoModel document) =>
            {
                document.Caja = CajaAccess.GetCaja(document.CajaID);
            });

            return data;
        }

        public long PrestarDocumento(DocPrestamo prestamo)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(prestamo);

                return data;
            }
        }

        public int GetNextPrestamoID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT ISNULL(MAX(PrestamoID), 0) + 1 FROM DocPrestamo";
                var data = context.QueryFirst<int>(query);

                return data;
            }
        }

        public DocPrestamo GetDocumentoPrestado(int CajaID, int DocID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT * FROM DocPrestamo WHERE DocID = @DocID AND CajaID = @CajaID AND PrestFechaDevuelve IS NULL";
                var data = context.QueryFirstOrDefault<DocPrestamo>(query, new { DocID, CajaID });

                return data;
            }
        }

        public List<DocPrestamo> GetDocPrestamosByDocStatusAndDocTipoAnd_AlmacenID(string DocStatus, int DocTipo, int[] AlmacenID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT dp.*, d.DocTipo, c.CajaActivaID, c.CajaInactivaID FROM Documento d INNER JOIN DocPrestamo dp " +
                    "ON d.CajaID = dp.CajaID AND d.DocID = dp.DocID " +
                    "INNER JOIN Caja c ON d.CajaID = c.CajaID " +
                    "WHERE d.DocStatus = @DocStatus AND c.AlmacenID IN @AlmacenID AND c.CajaStatus = 'ACT' AND dp.PrestFechaDevuelve IS NULL";
                query += DocTipo == 0 ? "" : " AND d.DocTipo = @DocTipo"; //Filtrar por documento si selecciono uno

                var data = context.Query<DocPrestamo>(query, new { DocStatus, DocTipo, AlmacenID }).ToList();

                return data;
            }
        }

        public DocPrestamo GetDocPrestamo(int PrestamoID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<DocPrestamo>(PrestamoID);

                return data;
            }
        }

        public bool RecibirDocumento(DocPrestamo prestamo)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Update(prestamo);

                return data;
            }
        }

        public int GetNextTrituraID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT ISNULL(MAX(TrituraID), 0) + 1 FROM DocTritura";
                var data = context.QuerySingle<int>(query);

                return data;
            }
        }

        public long TriturarDocumentos(List<DocTrituraModel> docTrituras)
        {
            long affectedRows = 0L;

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "INSERT INTO [dbo].[DocTritura] ([TrituraID], [DocID], [CajaID], [AlmacenID], [TrituraNombreTestigo], [TrituraFecha], [TrituraUsuario]) " +
                    "VALUES (@TrituraID, @DocID, @CajaID, 0, '', @TrituraFecha, @TrituraUsuario)";

                docTrituras.ForEach((DocTrituraModel docTritura) =>
                {
                    var values = new { docTritura.TrituraID, docTritura.DocID, docTritura.CajaID, docTritura.TrituraFecha, docTritura.TrituraUsuario };

                    affectedRows += context.Execute(query, values);
                });

                return affectedRows;
            }
        }

        internal List<DocTrituraModel> GetDocTrituraByTrituraID(int? lote)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "SELECT * FROM DocTritura WHERE TrituraID = @TrituraID";
                var data = context.Query<DocTrituraModel>(query, new { TrituraID = lote }).ToList();

                return data;
            }
        }

        internal bool UpdateDocTrituraSetTrituraNombreTestigo(string TrituraNombreTestigo, int lote)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                string query = "UPDATE DocTritura SET TrituraNombreTestigo = @TrituraNombreTestigo WHERE TrituraID = @lote";
                var data = context.Execute(query, new { TrituraNombreTestigo, lote });

                return data > 0;
            }
        }

        public List<GetDocumentosPrestados_RP> GetDocumentosPrestados_RP(DocumentosPrestadosBusqueda busqueda, string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { busqueda.FechaInicio, busqueda.FechaFin, UserId };
                var data = context.Query<GetDocumentosPrestados_RP>("GetDocumentosPrestados_RP", values, commandType: CommandType.StoredProcedure).ToList();

                return data;
            }
        }

        public List<GetDocumentos_RP> GetDocumentos_RP(DocumentosBusqueda busqueda, string UserId)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var values = new { busqueda.FechaInicio, busqueda.FechaFin, UserId };
                var data = context.Query<GetDocumentos_RP>("GetDocumentos_RP", values, commandType: CommandType.StoredProcedure).ToList();

                return data;
            }
        }
    }
}