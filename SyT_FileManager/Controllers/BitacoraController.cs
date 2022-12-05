using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SyT_FileManager.AppCode;
using SyT_FileManager.DataAccess;
using SyT_FileManager.Models;
using PagedList;

namespace SyT_FileManager.Controllers
{
    public class BitacoraController : Controller
    {
        BitacoraAccess BitacoraAccess;
        // GET: Bitacora
        public ActionResult Index(int? page)
        {
            BitacoraAccess = new BitacoraAccess();
            var model = BitacoraAccess.GetAll();

            int pageSize = Constants.PaginationSize;
            int pageNumber = (page ?? 1);
            
            return View(model.OrderByDescending(x => x.Fecha).ToPagedList(pageNumber, pageSize));
        }
    }
}