using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using SD210_BugTracker_DGrouette.Models;
using SD210_BugTracker_DGrouette.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.GetOwinContext().Get<ApplicationDbContext>();
        public RoleManager<IdentityRole> RoleManager => new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbContext));

        [HttpGet]
        public ActionResult Index()
        {
            // Depending on role, we send different count
            // Admins + PM
            //      # of ALL projects
            //      tickets by status
            //          # of Open Tickets
            //          # of Resolved Tickets
            //          # of Closed Tickets
            // Developer
            //      # of Projects they're assigned too
            //      # of Tickets they're assigned too
            // Submitter
            //      # of Projects they're assigned too
            //      # of Tickets they've created
            var userId = User.Identity.GetUserId();
            DashboardViewModel dashboardData = new DashboardViewModel();

            if (ProjectHelper.IsAdminOrManager(User))
            {
                dashboardData.ProjectCount = DbContext.Projects
                    .Where(p => !p.IsArchived)
                    .Count();
                dashboardData.TicketCount = DbContext.Tickets.Count();

                dashboardData.OpenTicketCount = DbContext.Tickets.Where(p => p.TicketStatus.Name == ProjectConstants.TicketStatusOpen).Count();
                dashboardData.ResolvedTicketCount = DbContext.Tickets.Where(p => p.TicketStatus.Name == ProjectConstants.TicketStatusResolved).Count();
                dashboardData.RejectedTicketCount = DbContext.Tickets.Where(p => p.TicketStatus.Name == ProjectConstants.TicketStatusRejected).Count();
            }
            else if (User.IsInRole(ProjectConstants.DeveloperRole))
            {
                dashboardData.ProjectCount = DbContext.Projects
                    .Where(i => i.Users.Any(p => p.Id == userId) && !i.IsArchived) // where the projects contains this user, get all those projects, count it.
                    .Count();

                dashboardData.TicketCount = DbContext.Tickets
                    .Where(p => p.AssignedTo.Id == userId)
                            .Count();

            }
            else if (User.IsInRole(ProjectConstants.SubmitterRole))
            {
                dashboardData.ProjectCount = DbContext.Projects
                   .Where(i => i.Users.Any(p => p.Id == userId) && !i.IsArchived) // where the projects contains this user, get all those projects, count it.
                   .Count();

                dashboardData.TicketCount = DbContext.Tickets
                    .Where(p => p.CreatedBy.Id == userId)
                                .Count();
            }

            return View(dashboardData);
        }
    }
}