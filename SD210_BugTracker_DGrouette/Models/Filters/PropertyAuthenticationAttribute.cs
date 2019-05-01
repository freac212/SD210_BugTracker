using SD210_BugTracker_DGrouette.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Routing;

namespace SD210_BugTracker_DGrouette.Models.Filters
{
    public class PropertyAuthenticationAttribute : ActionFilterAttribute
    {
        // Ensures all parameters for the controller action are not null.
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            foreach (var item in filterContext.ActionParameters)
            {
                var controller = (TicketController)filterContext.Controller;

                if (item.Value is null)
                {
                    Debug.WriteLine("Item was null, redirecting.");
                    filterContext.Result = controller.RedirectToAction("Index", "Dashboard");
                }
            }
        }
    }
}