using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using SyT_FileManager.Models;
using System.Web.Security;
using Newtonsoft.Json;
using SyT_FileManager.DataAccess;

namespace SyT_FileManager.AppCode
{
    public static class Constants
    {
        #region Variables dinamicas
        public static string ConnectionString
        {
            get => ConfigurationManager.ConnectionStrings["FileContext"].ConnectionString;
        }

        public static int PaginationSize
        {
            get
            {
                var size = ConfigurationManager.AppSettings["PaginationSize"];

                return size == null ? 10 : Convert.ToInt32(size);
            }
        }

        public static List<PrivilegioModel> Privilegios
        {
            get 
            {
                if(HttpContext.Current.Session["Privilegios"] != null)
                    return (List<PrivilegioModel>)HttpContext.Current.Session["Privilegios"];

                //Si se borraron los privilegios de la sesion entonces hay que restaurarlos
                PrivilegioAccess privilegioAccess = new PrivilegioAccess();
                var privilegios = privilegioAccess.GetPrivilegios(GetUserData().UserId);

                return privilegios;
            } 
            set
            {
                HttpContext.Current.Session["Privilegios"] = value;
            }
        }

        public static UsuarioModel GetUserData()
        {
            HttpCookie userCookie = HttpContext.Current.Request.Cookies.Get(FormsAuthentication.FormsCookieName);

            if (userCookie != null)
            {
                string value = FormsAuthentication.Decrypt(userCookie.Value).UserData;
                UsuarioModel user = JsonConvert.DeserializeObject<UsuarioModel>(value);

                return user;
            }

            return new UsuarioModel();
        }

        #endregion

        #region Constantes fijas
        public const string ADD = "Agregar";
        public const string EDIT = "Editar";
        public const string DELETE = "Eliminar";
        public const string AGENCY_USER = "Usuario de agencia";
        public const string STORE_USER = "Usuario de almacen";
        public const string OTHER = "Otro tipo de usuario";
        public const string ExcelExtension = ".xlsx";
        public const string PDFExtension = ".pdf";
        #endregion
    }

    public enum CodeBar
    {
        [Integer(200)]
        Width,
        [Integer(60)]
        Height,
        [Integer(60)]
        Size
    }

    public enum UserLevel
    {
        Agency,
        Store
    }
}