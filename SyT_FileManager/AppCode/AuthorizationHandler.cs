using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SyT_FileManager.AppCode
{
    public class AuthorizationHandler : AuthorizeAttribute
    {
        private readonly bool RequiresAuthorization;

        public AuthorizationHandler()
        {

        }

        public AuthorizationHandler(bool RequiresAuthorization)
        {
            this.RequiresAuthorization = RequiresAuthorization;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var request = httpContext.Request;
            var response = httpContext.Response;
            var user = httpContext.User;

            if (request.IsAjaxRequest())
            {
                response.StatusCode = user.Identity.IsAuthenticated ? (int)HttpStatusCode.Forbidden : (int)HttpStatusCode.Unauthorized;
                response.SuppressFormsAuthenticationRedirect = true;
                response.End();
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}