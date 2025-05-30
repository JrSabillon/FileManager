﻿using System;
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
using Newtonsoft.Json;

namespace SyT_FileManager.DataAccess
{
    public class BancoAccess
    {
        public BancoAccess()
        {

        }

        public List<BancoModel> GetBancos()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.GetAll<BancoModel>().ToList();

                return data;
            }
        }

        public BancoModel GetBanco(int BancoID)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Get<BancoModel>(BancoID);

                return data;
            }
        }

        public List<SelectListItem> GetBancoStatus()
        {
            var status = new List<SelectListItem>
            {
                new SelectListItem { Value = "AC", Text = "Activo" },
                new SelectListItem { Value = "IN", Text = "Inactivo" }
            };

            return status;
        }

        public long Create(BancoModel banco)
        {
            banco.BancoID = GetNextID();

            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.Insert(banco);

                return data;
            }
        }

        public bool Update(BancoModel banco)
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var oldBanco = context.Get<BancoModel>(banco.BancoID);
                var updated = context.Update(banco);

                if (updated)
                {
                    BitacoraAccess bitacoraAccess = new BitacoraAccess();
                    BitacoraModel bitacora = new BitacoraModel()
                    {
                        BitacoraID = bitacoraAccess.NextBitacoraID(),
                        Entidad = "Banco",
                        Usuario = Constants.GetUserData().UserId,
                        Fecha = DateTime.Now,
                        Accion = Constants.EDIT,
                        ValorAnterior = JsonConvert.SerializeObject(oldBanco),
                        ValorActual = JsonConvert.SerializeObject(banco)
                    };

                    bitacoraAccess.Add(bitacora);

                }

                return updated;
            }
        }

        public int GetNextID()
        {
            using (IDbConnection context = new SqlConnection(Constants.ConnectionString))
            {
                var data = context.QuerySingle<int>("SELECT MAX(BancoID) + 1 FROM Banco");

                return data;
            }
        }
    }
}