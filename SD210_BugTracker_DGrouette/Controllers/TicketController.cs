using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using SD210_BugTracker_DGrouette.Models;
using SD210_BugTracker_DGrouette.Models.Domain;
using SD210_BugTracker_DGrouette.Models.Filters;
using SD210_BugTracker_DGrouette.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette.Controllers
{
    [Authorize] // Logged in users only
    public class TicketController : Controller
    {
        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.GetOwinContext().Get<ApplicationDbContext>();
        public RoleManager<IdentityRole> RoleManager => new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbContext));


        [HttpGet]
        [Authorize(Roles = ProjectConstants.SubmitterRole)] // Must be in submitter role to see the creation page.
        [IdAuthentication("projectId")]
        public ActionResult CreateTicket(int? projectId)
        {
            var project = ProjectHelper.GetProjectById(DbContext, (int)projectId);

            if (project is null)
                return RedirectToDashError();

            // Check if user is assigned to project && is in submitter role (part of auth already)
            var user = UserManager.FindById(User.Identity.GetUserId());

            if (!project.Users.Any(p => p.Id == user.Id))
                return RedirectToDashError();

            var ticket = new CreateTicketViewModel()
            {
                ProjectId = projectId.Value,
                TicketTypes = DbContext.TicketTypes.Select(p => // Potentially turn these into methods ++Q
                    new SelectListItem()
                    {
                        Text = p.Name,
                        Value = p.Id.ToString()

                    }).ToList(),
                TicketPriorities = DbContext.TicketPriorities.Select(p => // Potentially turn these into methods ++Q
                    new SelectListItem()
                    {
                        Text = p.Name,
                        Value = p.Id.ToString()

                    }).ToList()
            };

            return View(ticket);
        }

        [HttpPost]
        [Authorize(Roles = ProjectConstants.SubmitterRole)] // Must be in submitter role to submit a ticket creation
        public ActionResult CreateTicket(CreateTicketViewModel newTicket)
        {
            // Get project id
            var project = ProjectHelper.GetProjectById(DbContext, newTicket.ProjectId);

            if (project is null)
                return RedirectToDashError();

            // Check if user is assigned to project && is in submitter role (part of auth already)
            var user = UserManager.FindById(User.Identity.GetUserId());

            if (!project.Users.Any(p => p.Id == user.Id))
                return RedirectToDashError();


            if (!ModelState.IsValid)
            {
                newTicket.TicketTypes = DbContext.TicketTypes.Select(p => // Potentially turn these into methods ++Q
                    new SelectListItem()
                    {
                        Text = p.Name,
                        Value = p.Id.ToString()

                    }).ToList();

                newTicket.TicketPriorities = DbContext.TicketPriorities.Select(p => // Potentially turn these into methods ++Q
                    new SelectListItem()
                    {
                        Text = p.Name,
                        Value = p.Id.ToString()

                    }).ToList();

                return View(newTicket);
            }

            // Get Users to assign to the Ticket (Must be in Devs Role) ++Q
            // set Creator to current User
            // set status to "Open"
            var ticket = new Ticket()
            {
                CreatedBy = UserManager.FindById(User.Identity.GetUserId()),
                DateCreated = DateTime.Now,
                Title = newTicket.Title,
                Description = newTicket.Description,
                ProjectId = newTicket.ProjectId,
                TicketPriorityId = newTicket.TicketPriorityId,
                TicketTypeId = newTicket.TicketTypeId,
                TicketStatus = DbContext.TicketStatuses.FirstOrDefault(p => p.Name == ProjectConstants.TicketStatusOpen)
            };

            // Rest of CreateTicketViewModel props will be null/ empty
            project.Tickets.Add(ticket);

            DbContext.SaveChanges();

            // ++Q -> Redirect to ListTickets
            return RedirectToDash();
        }

        [HttpGet]
        public ActionResult ListTickets()
        {
            // Assigned Tickets -> all users
            // User Created Tickets -> all users
            // All Tickets -> if Admin/ PM (Also send all related ID data here too.)
            // (Except the archived projects)

            // There's two of the same lists being mapped because of the users role.
            // If the user is an admin/ PM they have access to more functionallity

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
                // All tickets can be the all the tickets from the projects the person is assigned too minus the archived projects.
                // (Repurposing a list todo two things depending on roles.)
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
        [IdAuthentication("ticketId")]
        public ActionResult DetailsTicket(int? ticketId)
        {
            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == ticketId);

            if (ticket is null)
                return RedirectToDashError();

            TicketViewModel viewModelTicket = null;

            if (TicketHelper.UserCanViewDetails(User, ticket))
                viewModelTicket = TicketHelper.MapTicket(ticket, User);
            else
                return RedirectToDashError();

            if (viewModelTicket is null)
                return RedirectToDashError();

            return View(viewModelTicket);
        }

        [HttpGet]
        [IdAuthentication("ticketId")]
        public ActionResult EditTicket(int? ticketId)
        {
            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == ticketId);

            if (ticket is null)
                return RedirectToDashError();

            EditTicketViewModel viewModelTicket;

            if (TicketHelper.UserCanAccessTicket(User, ticket))
                viewModelTicket = TicketHelper.TicketDataByRole(ticket, User, RoleManager, UserManager, DbContext);
            else
                return RedirectToDashError();

            if (viewModelTicket is null)
                return RedirectToDashError();

            return View(viewModelTicket);
        }

        [HttpPost]
        public ActionResult EditTicket(EditTicketViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                // Redirect back to the form with the inputed from data, since something is missing or incorrect.
                EditTicketViewModel viewModelTicket = TicketHelper.TicketDataByRole(formData, User, RoleManager, UserManager, DbContext);
                return View(viewModelTicket);
            }

            if (formData is null)
                return RedirectToDashError();

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == formData.Id);

            if (ticket is null)
                return RedirectToDashError();

            // Apply changes
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

            // Generate history
            var user = UserManager.FindById(User.Identity.GetUserId());
            var ticketHistory = TicketHelper.GenerateTicketHistory(DbContext, ticket, user);

            if (ticketHistory != null)
            {
                ticket.TicketHistories.Add(ticketHistory);
                // Notify users (Only if there's a change...)
                TicketHelper.SendNotificationsToUsers(ticket, "Ticket changes");

                // If a new dev has been assigned to this ticket,
                // >Remove the old dev from the mailing list,
                // >Add the new dev,
                // >Then notify the user they've been assigned to this ticket.
                if (TicketHelper.IsNewDevAssigned(ticketHistory))
                {
                    var history = ticketHistory.TicketHistoryDetails.FirstOrDefault(p => p.Property == "Dev Assigned");

                    var devUserOld = UserManager.Users.FirstOrDefault(p => p.DisplayName == history.OldValue);
                    var devUserNew = UserManager.Users.FirstOrDefault(p => p.DisplayName == history.NewValue);

                    TicketHelper.ManageDevNotificationAssignment(devUserOld, devUserNew, ticket);
                    TicketHelper.NotifyUserByEmail(devUserNew, ticket.Title);
                }
            }

            // Save to db
            DbContext.SaveChanges();

            // Return to details page of the ticket.
            return RedirectToAction("DetailsTicket", "Ticket", new
            {
                ticketId = ticket.Id
            });
        }

        [HttpGet]
        [IdAuthentication("ticketId")]
        public ActionResult CreateComment(int? ticketId)
        {
            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == ticketId);

            if (ticket is null)
                return RedirectToDashError();

            // Do user validation
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
                return RedirectToDashError();
            }
        }

        [HttpPost]
        public ActionResult CreateComment(CreateCommentTicketViewModel formData)
        {
            if (!ModelState.IsValid)
                return View(formData);

            if (formData is null)
                return RedirectToDashError();

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == formData.TicketId);

            if (ticket is null)
                return RedirectToDashError();

            if (TicketHelper.UserCanAccessTicket(User, ticket))
            {
                ticket.Comments.Add(new Comment
                {
                    UserId = User.Identity.GetUserId(),
                    DateCreated = DateTime.Now,
                    CommentData = formData.Comment,
                });

                // Notify users of a comment
                TicketHelper.SendNotificationsToUsers(ticket, "comment");

                DbContext.SaveChanges();

                return RedirectToAction("DetailsTicket", "Ticket", new { ticketId = ticket.Id });
            }
            else
            {
                return RedirectToDashError();
            }
        }

        [HttpGet]
        [IdAuthentication("ticketId")]
        public ActionResult FileUploadTicket(int? ticketId)
        {
            if (ticketId is null)
                return RedirectToDashError();

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == ticketId);

            if (ticket is null)
                return RedirectToDashError();

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
                return RedirectToDashError();
            }
        }

        [HttpPost]
        public ActionResult FileUploadTicket(UploadFileTicketViewModel formData)
        {
            if (formData is null)
                return RedirectToDashError();

            var ticket = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == formData.TicketId);

            if (ticket is null)
                return RedirectToDashError();

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

                // Set media URL to later access the file. Then save to DB.
                // Have an object storing the name of the file, short URL to the file, and the path of the file.
                ticket.Files.Add(new TicketFile()
                {
                    MediaUrl = "~/Upload/" + fileNameGen + Path.GetExtension(formData.Media.FileName),
                    FileName = fileNameGen.ToString() + Path.GetExtension(formData.Media.FileName),
                    Title = formData.Media.FileName,
                    TicketId = ticket.Id,
                    UserId = user.Id
                });

                // Notify users of added file
                TicketHelper.SendNotificationsToUsers(ticket, "File added");

                DbContext.SaveChanges();

                return RedirectToAction("DetailsTicket", "Ticket", new { ticketId = ticket.Id });
            }
            else
            {
                return RedirectToDashError();
            }
        }

        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult TicketNotifications()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var notifications = DbContext.Tickets
                .Select(p => new TicketNotificationViewModel()
                {
                    TicketTitle = p.Title,
                    TicketId = p.Id,
                    // If user has a notification with this ticketId it's selected
                    Selected = p.SubscribedUsers.Any(prop => prop.Id == user.Id)

                }).ToList();

            return View(notifications);
        }

        [HttpPost]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult TicketNotifications(List<TicketNotificationViewModel> formData)
        {
            if (formData is null)
                return RedirectToDashError();

            var user = UserManager.FindById(User.Identity.GetUserId());

            foreach (var notification in formData)
            {
                var ticket = DbContext.Tickets
                    .FirstOrDefault(p => p.Id == notification.TicketId);

                if (ticket is null)
                    return RedirectToDashError();

                if (notification.Selected)
                    ticket.SubscribedUsers.Add(user);
                else
                    ticket.SubscribedUsers.Remove(user);
            }

            DbContext.SaveChanges();

            return RedirectToAction("Index", "Manage");
        }

        [HttpGet]
        [IdAuthentication("commentId")]
        public ActionResult EditComment(int? commentId)
        {
            // Get comment being edited
            var comment = DbContext.Comments.FirstOrDefault(cmt => cmt.Id == commentId);

            if (comment is null)
                return RedirectToDashError();

            // Do user validation
            if (TicketHelper.UserCanAccessComment(User, comment))
            {
                var commentViewModel = new EditCommentTicketViewModel()
                {
                    Id = comment.Id,
                    TicketId = comment.TicketId,
                    Comment = comment.CommentData,
                };

                return View(commentViewModel);
            }
            else
            {
                return RedirectToDashError();
            }
        }

        [HttpPost]
        public ActionResult EditComment(EditCommentTicketViewModel formData)
        {
            if (!ModelState.IsValid)
                return View(formData);
            
            if (formData is null)
                return RedirectToDashError();

            var comment = DbContext.Comments.FirstOrDefault(cmt => cmt.Id == formData.Id);

            if (comment is null)
                return RedirectToDashError();

            if (TicketHelper.UserCanAccessComment(User, comment))
            {
                comment.CommentData = formData.Comment;

                DbContext.SaveChanges();

                return RedirectToAction("DetailsTicket", "Ticket", new { ticketId = formData.TicketId});
            }
            else
            {
                return RedirectToDashError();
            }
        }

        [HttpPost]
        [IdAuthentication("itemId", "ticketId")]
        public ActionResult DeleteComment(int? itemId, int? ticketId)
        {
            var ticketComment = DbContext.Comments.FirstOrDefault(p => p.Id == itemId);

            if (ticketComment is null)
                return RedirectToDashError();

            var user = UserManager.FindById(User.Identity.GetUserId());

            // Ensure the user is either Admin/ PM/ Creator
            if (ProjectHelper.IsAdminOrManager(User) || ticketComment.UserId == user.Id)
            {
                DbContext.Comments.Remove(ticketComment);
                DbContext.SaveChanges();
            }

            return RedirectToAction("DetailsTicket", "Ticket", new { ticketId = (int)ticketId });
        }

        [HttpPost]
        [IdAuthentication("itemId", "ticketId", "fileName")]
        public ActionResult DeleteFile(int? itemId, int? ticketId, string fileName)
        {
            var ticketFile = DbContext.Files.FirstOrDefault(p => p.Id == itemId);

            if (ticketFile is null)
                return RedirectToDashError();

            var user = UserManager.FindById(User.Identity.GetUserId());

            // Ensure the user is either Admin/ PM/ Creator
            if (ProjectHelper.IsAdminOrManager(User) || ticketFile.UserId == user.Id)
            {
                var uploadFolder = Server.MapPath("~/Upload/");
                var fileWithPath = uploadFolder + fileName;

                System.IO.File.Delete(Path.Combine(uploadFolder, fileWithPath));

                DbContext.Files.Remove(ticketFile);
                DbContext.SaveChanges();
            }

            return RedirectToAction("DetailsTicket", "Ticket", new { ticketId = (int)ticketId });
        }

        private ActionResult RedirectToDashError()
        {
            return RedirectToAction("Index", "Dashboard", TempData["ErrorMessage"] = "That data either doesn't exist or you don't have access to it.");
        }
        private ActionResult RedirectToDash()
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}