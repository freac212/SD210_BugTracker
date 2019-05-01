using SD210_BugTracker_DGrouette.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette.Models.Filters
{
    public class IdAuthenticationAttribute : ActionFilterAttribute
    {
        private List<string> Parameters { get; set; }

        public IdAuthenticationAttribute(params string[] tests)
        {
            Parameters = new List<string>(tests);
        }

        // Ensure specified strings are not null
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = (TicketController)filterContext.Controller;

            foreach (var parameter in Parameters)
            {
                var item = filterContext.HttpContext.Request[parameter];

                if (String.IsNullOrEmpty(item))
                {
                    Debug.WriteLine("Item was null, redirecting.");
                    filterContext.Controller.TempData["ErrorMessage"] = "That data either doesn't exist or you don't have access to it.";
                    filterContext.Result = controller.RedirectToAction("Index", "Dashboard");
                }
            }
        }
    }
}