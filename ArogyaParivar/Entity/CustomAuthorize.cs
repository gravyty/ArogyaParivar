using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ArogyaParivar
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        public string Name
        {
            get;
            set;
        }

        public CustomAuthorize(string name)
        {
            Name = name;
        }
        // Custom property
        public string ValidRole { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session["UserID"] == null)// || Convert.ToString(httpContext.Session["Role"]) != Name)
            {
                //User is not logged-in so redirect to login page
                return false;
            }
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
            new RouteValueDictionary(
                new
                {
                    controller = "Login",
                    action = "Login"
                })
            );
        }
    }

}