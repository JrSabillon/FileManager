using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using Dapper.Contrib.Extensions;
using SyT_FileManager.AppCode;
using SyT_FileManager.Models;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace SyT_FileManager.DataAccess
{
    public class TipoDocumentoAccess
    {
        public TipoDocumentoAccess()
        {

        }

        public List<TipoDocumentoModel> GetTipoDocumentos()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.GetAll<TipoDocumentoModel>().ToList();

                return data;
            }
        }

        public TipoDocumentoModel GetTipoDocumento(int TipoDocID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<TipoDocumentoModel>(TipoDocID);

                return data;
            }
        }

        public long Create(TipoDocumentoModel tipoDocumento)
        {
            tipoDocumento.TipoDocID = GetNextID();

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(tipoDocumento);

                return data;
            }
        }

        public int GetNextID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QueryFirst<int>("SELECT MAX(TipoDocID) + 1 FROM TipoDocumento");

                return data;
            }
        }

        public bool Update(TipoDocumentoModel tipoDocumento)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var oldTipoDocumento = context.Get<TipoDocumentoModel>(tipoDocumento.TipoDocID);
                var updated = context.Update(tipoDocumento);

                if (updated)
                {
                    BitacoraAccess bitacoraAccess = new BitacoraAccess();
                    BitacoraModel bitacora = new BitacoraModel()
                    {
                        BitacoraID = bitacoraAccess.NextBitacoraID(),
                        Entidad = "TipoDocumento",
                        Usuario = Constants.GetUserData().UserId,
                        Fecha = DateTime.Now,
                        Accion = Constants.EDIT,
                        ValorAnterior = JsonConvert.SerializeObject(oldTipoDocumento),
                        ValorActual = JsonConvert.SerializeObject(tipoDocumento)
                    };

                    bitacoraAccess.Add(bitacora);
                }

                return updated;
            }
        }

        public List<SelectListItem> GetTipoDocumentoStatus()
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