using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SyT_FileManager.AppCode
{
    public class ExceptionHandler : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            Exception exc = filterContext.Exception;
            
            if (filterContext.ExceptionHandled || filterContext.HttpContext.IsCustomErrorEnabled)
            {
                WriteLog(exc, $"{filterContext.RouteData.Values["controller"]}/{filterContext.RouteData.Values["action"]}");
                return;
            }

            WriteLog(exc, $"{filterContext.RouteData.Values["controller"]}/{filterContext.RouteData.Values["action"]}");
            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult() { ViewName = "Error" };
        }

        public void WriteLog(Exception exc, string Source)
        {
            string logFile = $"~/Logs/ErrorLog_{DateTime.Now:yyyy-MM-dd}.txt";
            logFile = HttpContext.Current.Server.MapPath(logFile);

            StreamWriter sw = new StreamWriter(logFile, true);
            sw.WriteLine($"********** {DateTime.Now:HH:mm:ss} **********");
            if (exc.InnerException != null)
            {
                sw.WriteLine($"Inner Exception Type - {exc.InnerException.GetType()}");
                sw.WriteLine($"Inner Exception - {exc.InnerException.Message}");
                sw.WriteLine($"Inner Source - {exc.InnerException.Source}");

                if(exc.InnerException.StackTrace != null)
                {
                    sw.WriteLine($"Inner Exception Trace - {exc.InnerException.StackTrace}");
                }
            }

            sw.WriteLine($"Exception Type - {exc.GetType()}");
            sw.WriteLine($"Exception - {exc.Message}");
            sw.WriteLine($"Source - {Source}");
            sw.WriteLine($"Stack Trace - {exc.StackTrace ?? "NONE"}");

            sw.Close();
        }
    }
}