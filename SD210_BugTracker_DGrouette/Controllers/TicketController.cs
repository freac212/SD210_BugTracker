using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using SD210_BugTracker_DGrouette.Models;
using SD210_BugTracker_DGrouette.Models.Domain;
using SD210_BugTracker_DGrouette.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette.Controllers
{
    [Authorize] // Logged in users
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
                return RedirectToAction("AssignedProjects", "Projects");

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
        public ActionResult ListTickets()
        {
            // Assigned Tickets -> all users
            // User Created Tickets -> all users
            // All Tickets -> if Admin/ PM (Also send all related ID data here too.)

            // There's two of the same lists being mapped because of the users role.
            // If the user is an admin/ PM they have access to more functionallity
            // than regular users, such as edit ticket, which require Id's. So the 
            // MappedTicketsAdmin maps the ticket with related Id's where the 
           
            var listsOfTickets = new ListTicketsViewModel();
            var userId = User.Identity.GetUserId();

            if (ProjectHelper.IsAdminOrManager(User))
            {
                listsOfTickets.AllTickets = TicketHelper.MapTickets(DbContext.Tickets.ToList(), User);

                listsOfTickets.AssignedTickets = TicketHelper.MapTickets(DbContext.Tickets
                    .Where(p => p.AssignedToId == userId)
                    .ToList(), User);

                listsOfTickets.CreatedTickets = TicketHelper.MapTickets(DbContext.Tickets
                    .Where(p => p.CreatedById == userId)
                    .ToList(), User);
            }
            else
            {
                // All tickets can be the all the tickets from the projects the person is assigned too
                // get project Id's of the projects this user is assigned to, get the tickets that matches those project id's

                var tickets = DbContext.Projects
                    .Where(i => i.Users.Any(p => p.Id == userId))
                    .SelectMany(p => p.Tickets)
                    .ToList();

                listsOfTickets.AllTickets = TicketHelper.MapTickets(tickets, User);

                listsOfTickets.AssignedTickets = TicketHelper.MapTickets(DbContext.Tickets
                    .Where(p => p.AssignedToId == userId)
                    .ToList(), User);

                listsOfTickets.CreatedTickets = TicketHelper.MapTickets(DbContext.Tickets
                    .Where(p => p.CreatedById == userId)
                    .ToList(), User);
            }

            return View(listsOfTickets);
        }

        [HttpGet]
        public ActionResult DetailsTicket(int? ticketId)
        {
            if (ticketId is null)
                return RedirectToAction("AssignedProjects", "Projects");

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == ticketId);

            if (ticket is null)
                return RedirectToAction("AssignedProjects", "Projects");

            TicketViewModel viewModelTicket = null;

            if (TicketHelper.UserCanViewDetails(User, ticket))
                viewModelTicket = TicketHelper.MapTicket(ticket, User);
            else
                return RedirectToAction("AssignedProjects", "Projects");

            if (viewModelTicket is null)
                return RedirectToAction("AssignedProjects", "Projects");

            return View(viewModelTicket);
        }

        [HttpGet]
        public ActionResult EditTicket(int? ticketId)
        {
            if (ticketId is null)
                return RedirectToAction("AssignedProjects", "Projects");

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == ticketId);

            if (ticket is null)
                return RedirectToAction("AssignedProjects", "Projects");

            EditTicketViewModel viewModelTicket;

            if (TicketHelper.UserCanAccessTicket(User, ticket))
                viewModelTicket = TicketHelper.TicketDataByRole(ticket, User, RoleManager, UserManager, DbContext);
            else
                return RedirectToAction("AssignedProjects", "Projects");


            if (viewModelTicket is null)
                return RedirectToAction("AssignedProjects", "Projects");

            return View(viewModelTicket);
        }

        [HttpPost]
        public ActionResult EditTicket(EditTicketViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                EditTicketViewModel viewModelTicket = TicketHelper.TicketDataByRole(formData, User, RoleManager, UserManager, DbContext);
                return View(viewModelTicket);
            }

            if (formData is null)
                return RedirectToAction("AssignedProjects", "Projects");

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == formData.Id);

            if (ticket is null)
                return RedirectToAction("AssignedProjects", "Projects");

            // apply changes
            if (ProjectHelper.IsAdminOrManager(User))
            {
                ticket.Title = formData.Title;
                ticket.Description = formData.Description;
                ticket.DateUpdated = DateTime.Now;
                ticket.ProjectId = formData.ProjectId;
                ticket.TicketStatusId = (int)formData.TicketStatusId;
                ticket.TicketPriorityId = formData.TicketPriorityId;
                ticket.TicketTypeId = formData.TicketTypeId;
                ticket.AssignedToId = formData.AssignedToId;
            }
            else if (ProjectHelper.IsDevOrSubmitter(User))
            {
                ticket.Title = formData.Title;
                ticket.Description = formData.Description;
                ticket.DateUpdated = DateTime.Now;
                ticket.ProjectId = formData.ProjectId;
                ticket.TicketPriorityId = formData.TicketPriorityId;
                ticket.TicketTypeId = formData.TicketTypeId;
            }

            // save to db
            DbContext.SaveChanges();

            // return to details page of the ticket.
            return RedirectToAction("DetailsTicket", "Ticket", new { ticketId = ticket.Id });
        }

        [HttpGet]
        public ActionResult CommentTicket(int? ticketId)
        {
            if (ticketId is null)
                return RedirectToAction("AssignedProjects", "Projects");

            //var user = UserManager.FindById(User.Identity.GetUserId());
            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == ticketId);

            if (ticket is null)
                return RedirectToAction("AssignedProjects", "Projects");

            // Do user validation
            // Admin+PM -> ALL tickets
            // Dev -> Tickets they're assigned too
            // Submitter -> TIckets they've created
            // else return to home.

            if (TicketHelper.UserCanAccessTicket(User, ticket))
            {
                var comment = new CreateCommentTicketViewModel()
                {
                    TicketId = (int)ticketId
                };

                return View(comment);
            }
            else
            {
                return RedirectToAction("AssignedProjects", "Projects");
            }
        }

        [HttpPost]
        public ActionResult CommentTicket(CreateCommentTicketViewModel formData)
        {
            if (formData is null)
                return RedirectToAction("AssignedProjects", "Projects");

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == formData.TicketId);

            if (ticket is null)
                return RedirectToAction("AssignedProjects", "Projects");

            if (TicketHelper.UserCanAccessTicket(User, ticket))
            {
                ticket.Comments.Add(new Comment
                {
                    UserId = User.Identity.GetUserId(),
                    DateCreated = DateTime.Now,
                    CommentData = formData.Comment,
                });

                DbContext.SaveChanges();

                return RedirectToAction("DetailsTicket", "Ticket", new { ticketId = ticket.Id });
            }
            else
            {
                return RedirectToAction("AssignedProjects", "Projects");
            }
        }


        [HttpGet]
        public ActionResult FileUploadTicket(int? ticketId)
        {
            if (ticketId is null)
                return RedirectToAction("AssignedProjects", "Projects");

            //var user = UserManager.FindById(User.Identity.GetUserId());

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == ticketId);

            if (ticket is null)
                return RedirectToAction("AssignedProjects", "Projects");

            if (TicketHelper.UserCanAccessTicket(User, ticket))
            {
                var file = new UploadFileTicketViewModel()
                {
                    TicketId = (int)ticketId
                };

                return View(file);
            }
            else
            {
                return RedirectToAction("AssignedProjects", "Projects");
            }
        }


        [HttpPost]
        public ActionResult FileUploadTicket(UploadFileTicketViewModel formData)
        {
            if (formData is null)
                return RedirectToAction("AssignedProjects", "Projects");

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == formData.TicketId);

            if (ticket is null)
                return RedirectToAction("AssignedProjects", "Projects");

            if (TicketHelper.UserCanAccessTicket(User, ticket) && formData.Media != null)
            {
                var uploadFolder = Server.MapPath("~/Upload/");
                var user = UserManager.FindById(User.Identity.GetUserId());

                // If the Upload directory doesn't exist, create it.
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // New file name, random gen.
                var fileNameGen = Guid.NewGuid();

                // The GUID ensure no file will have the same name.
                var fullFilePathWName = uploadFolder + fileNameGen + Path.GetExtension(formData.Media.FileName);

                // Save file to Upload folder
                formData.Media.SaveAs(fullFilePathWName);

                // Set  media URL to later access the file. Then save to DB.

                // Add file url to list of urls.
                // Have an object storing the name of the file, and the path of the file.
                ticket.Files.Add(new TicketFile()
                {
                    MediaUrl = "~/Upload/" + fileNameGen + Path.GetExtension(formData.Media.FileName),
                    Title = formData.Media.FileName,
                    TicketId = ticket.Id,
                    UserId = user.Id
                });

                DbContext.SaveChanges();

                return RedirectToAction("DetailsTicket", "Ticket", new { ticketId = ticket.Id });
            }
            else
            {
                return RedirectToAction("AssignedProjects", "Projects");
            }
        }
    }
}