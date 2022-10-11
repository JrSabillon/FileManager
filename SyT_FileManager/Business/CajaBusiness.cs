using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SyT_FileManager.AppCode;
using SyT_FileManager.DataAccess;
using SyT_FileManager.Models;

namespace SyT_FileManager.Business
{
    public class CajaBusiness
    {
        CajaAccess CajaAccess;
        DocumentoAccess DocumentAccess;
        public CajaBusiness()
        {
            CajaAccess = new CajaAccess();
            DocumentAccess = new DocumentoAccess();
        }

        public long Create(int AlmacenID, string CajaPersonaEntrega)
        {
            var caja = new CajaModel()
            {
                AlmacenID = AlmacenID,
                CajaPersonaEntrega = CajaPersonaEntrega,
                CajaFechaRecepcion = DateTime.Now,
                CajaStatus = "PEND",
                CajaID = CajaAccess.NextCajaID(),
                //CajaActivaID = CajaAccess.NextCajaActivaID(AlmacenID)
                CajaActivaID = 0
            };
            
            return CajaAccess.Create(caja);
        }

        /// <summary>
        /// Crear una nueva caja inactiva debido a que solo algunos documentos de una caja activa pasaron al almacen inactivo.
        /// </summary>
        /// <param name="AlmacenID">Almacen de archivo inactivo destino</param>
        /// <param name="AlmacenIDOrigen">Almacen de donde provienen los documentos</param>
        /// <param name="CajaActivaID">Id de la caja del almacen activo</param>
        /// <returns></returns>
        public int CreateCajaInactiva(int AlmacenID, int CajaID)
        {
            var caja = CajaAccess.GetCaja(CajaID);

            //Asignar el valor de la nueva caja en almacen inactivo
            CajaID = CajaAccess.Create(new CajaModel()
            {
                AlmacenID = AlmacenID,
                CajaPersonaEntrega = Constants.GetUserData().UserId,
                CajaFechaRecepcion = DateTime.Now,
                CajaStatus = "PEND",
                CajaID = CajaAccess.NextCajaID(),
                CajaActivaID = caja.CajaActivaID,
                CajaInactivaID = 0
            });

            CajaAccess.SetCajaHistorica_AlmacenIDOrigen(caja.AlmacenID, CajaID);

            return CajaID;
        }

        public CajaModel SetCajaInactiva(int CajaID, int AlmacenID)
        {
            var caja = CajaAccess.GetCaja(CajaID);

            caja.CajaInactivaID = 0;
            caja.CajaFechaRecepcion = DateTime.Now;
            caja.CajaPersonaEntrega = Constants.GetUserData().UserId;
            caja.AlmacenID = AlmacenID;
            caja.CajaEstante = null;
            caja.CajaSeccion = null;
            caja.CajaNivel = null;
            caja.CajaFila = null;
            caja.CajaUbicacion = null;
            caja.CajaUsuarioRegistro = null;
            caja.CajaFechaRegistro = null;
            caja.CajaStatus = "PEND";
            CajaAccess.Update(caja);

            return caja;
        }

        public CajaModel Transfer(int AlmacenIDDestino, int CajaIDOrigen)
        {
            var caja = CajaAccess.GetCaja(CajaIDOrigen);

            int AlmacenIDOrigen = caja.AlmacenID;

            //Disable(caja.CajaID);

            //Ingresar caja en el nuevo almacen
            //caja.CajaID = CajaAccess.NextCajaID();
            //caja.CajaActivaID = CajaAccess.NextCajaActivaID(AlmacenIDDestino);
            caja.CajaActivaID = 0;
            caja.CajaInactivaID = 0;
            caja.CajaFechaRecepcion = DateTime.Now;
            caja.CajaPersonaEntrega = Constants.GetUserData().UserId;
            caja.AlmacenID = AlmacenIDDestino;
            caja.CajaEstante = null;
            caja.CajaSeccion = null;
            caja.CajaNivel = null;
            caja.CajaFila = null;
            caja.CajaUbicacion = null;
            caja.CajaUsuarioRegistro = null;
            caja.CajaFechaRegistro = null;
            caja.CajaStatus = "PEND";
            CajaAccess.Update(caja);
            //int CajaID = CajaAccess.Create(caja);
            //caja.CajaID = CajaID;

            //Transferir documentos de una caja a otra
            //DocumentAccess.TransferDocuments_BoxToBox(CajaIDOrigen, CajaID);
            //Actualizar almacen de origen en caja historica
            //CajaAccess.SetCajaHistorica_AlmacenIDOrigen(AlmacenIDOrigen, caja.CajaID);

            return caja;
        }

        /// <summary>
        /// Deshabilitar una caja y sus documentos
        /// </summary>
        /// <param name="CajaID"></param>
        public void Disable(int CajaID)
        {
            var caja = CajaAccess.GetCaja(CajaID);
            
            caja.CajaStatus = "INA";
            CajaAccess.Update(caja);
        }
    }
}