using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SyT_FileManager.AppCode
{
    public class ExceptionHandler : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || filterContext.HttpContext.IsCustomErrorEnabled)
                return;

            Exception exc = filterContext.Exception;
            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult() { ViewName = "Error" };
        }
    }
}