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
    public class TicketController : Controller
    {
        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.GetOwinContext().Get<ApplicationDbContext>();
        public RoleManager<IdentityRole> RoleManager => new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbContext));

        // GET: CreateTicket
        [HttpGet]
        // Must be in submitter role to see the creation page.
        [Authorize(Roles = ProjectConstants.SubmitterRole)]
        public ActionResult CreateTicket(int? projectId)
        {
            // Get project id
            // Get Users to assign to the Ticket (Must be in Devs Role)
            // List of TicketTypes
            // List of TicketPriorities

            // Rest of CreateTicketViewModel props will be null/ empty

            if (!projectId.HasValue)
                return View();

            var project = DbContext.Projects.FirstOrDefault(proj => proj.Id == projectId);

            if (project is null)
                return View();

            var ticket = new CreateTicketViewModel()
            {
                ProjectId = projectId.Value,
                TicketTypes = DbContext.TicketTypes.Select(p =>
                    new SelectListItem()
                    {
                        Text = p.Name,
                        Value = p.Id.ToString()

                    }).ToList(),
                TicketPriorities = DbContext.TicketPriorities.Select(p =>
                    new SelectListItem()
                    {
                        Text = p.Name,
                        Value = p.Id.ToString()

                    }).ToList()
            };

            return View(ticket);
        }

        [HttpPost]
        // Must be in submitter role to submit a ticket creation
        [Authorize(Roles = ProjectConstants.SubmitterRole)]
        public ActionResult CreateTicket(CreateTicketViewModel newTicket)
        {

            if (!ModelState.IsValid)
            {
                newTicket.TicketTypes = DbContext.TicketTypes.Select(p =>
                    new SelectListItem()
                    {
                        Text = p.Name,
                        Value = p.Id.ToString()

                    }).ToList();

                newTicket.TicketPriorities = DbContext.TicketPriorities.Select(p =>
                    new SelectListItem()
                    {
                        Text = p.Name,
                        Value = p.Id.ToString()

                    }).ToList();

                return View(newTicket);
            }

            // Get project id
            var project = DbContext.Projects.FirstOrDefault(proj => proj.Id == newTicket.ProjectId);

            if (project is null)
                return View();

            // Get Users to assign to the Ticket (Must be in Devs Role) ++Q
            // set Creator to current User
            // set status to "Open"
            var ticket = new Tickets()
            {
                CreatedBy = UserManager.FindById(User.Identity.GetUserId()),
                DateCreated = DateTime.Now,
                Title = newTicket.Title,
                Description = newTicket.Description,
                ProjectId = newTicket.ProjectId,
                TicketPriorityId = newTicket.TicketPriorityId,
                TicketTypeId = newTicket.TicketTypeId,
                TicketStatus = DbContext.TicketStatuses.FirstOrDefault(p => p.Name == ProjectConstants.TicketStatusOpen),
            };
            // Rest of CreateTicketViewModel props will be null/ empty
            project.Tickets.Add(ticket);

            DbContext.SaveChanges();

            return RedirectToAction("AssignedProjects", "Projects");
        }


        // GET: Ticket
        [HttpGet]
        [Authorize] // Ensure only logged in users can access this.
        public ActionResult ListTickets()
        {
            // Ticket ViewModel
            //  Name
            //  Description
            //  Id
            //  ProjectName
            //  ProjectId
            //  DateCreated
            //  DateUpdated
            //  Type
            //  Status
            //  Priority
            //  CreatorName
            //  AssignedDevId
            //  AssignedDevName

            // Else -> show Assigned + Created

            // Assigned Tickets -> all users
            // User Created Tickets -> all users
            // All Tickets -> if Admin/ PM (Also send all related ID data here too.)

            // There's two of the same lists being mapped because of the users role.
            // If the user is an admin/ PM they have access to more functionallity
            // than regular users, such as edit ticket, which require Id's. So the 
            // MappedTicketsAdmin maps the ticket with related Id's where the 
            // 
            var listsOfTickets = new ListTicketsViewModel();
            var userId = User.Identity.GetUserId();

            if (ProjectHelper.UserIsAdminOrManager(User))
            {
                listsOfTickets.AllTickets = MapTickets(DbContext.Tickets.ToList());

                listsOfTickets.AssignedTickets = MapTickets(DbContext.Tickets
                    .Where(p => p.AssignedToId == userId)
                    .ToList());

                listsOfTickets.CreatedTickets = MapTickets(DbContext.Tickets
                    .Where(p => p.CreatedById == userId)
                    .ToList());
            }
            else
            {
                listsOfTickets.AssignedTickets = MapTickets(DbContext.Tickets
                    .Where(p => p.AssignedToId == userId)
                    .ToList());

                listsOfTickets.CreatedTickets = MapTickets(DbContext.Tickets
                    .Where(p => p.CreatedById == userId)
                    .ToList());
            }

            return View(listsOfTickets);
        }

        static List<TicketViewModel> MapTickets(List<Tickets> tickets)
        {
            return tickets.Select(ticket =>
                   new TicketViewModel()
                   {
                       Id = ticket.Id,
                       Title = ticket.Title,
                       Description = ticket.Description,
                       ProjectTitle = ticket.Project.Title,
                       ProjectId = ticket.ProjectId,
                       DateCreated = ticket.DateCreated,
                       DateUpdated = ticket.DateUpdated,
                       CreatedByDisplayName = ticket.CreatedBy.DisplayName,
                       CreatedById = ticket.CreatedBy.Id,
                       AssignedToDisplayName = ticket.AssignedTo?.DisplayName,
                       AssignedToId = ticket?.AssignedTo?.Id,
                       TicketPriority = ticket.TicketPriority,
                       TicketStatus = ticket.TicketStatus,
                       TicketType = ticket.TicketType,
                   }).ToList();
        }
    }
}