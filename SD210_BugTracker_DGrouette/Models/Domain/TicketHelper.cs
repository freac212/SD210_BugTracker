using Microsoft.AspNet.Identity;
using SD210_BugTracker_DGrouette.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public class TicketHelper
    {
        public static string PriorityChecker(string ticketPriority)
        {
            string color;

            switch (ticketPriority)
            {
                case "Low":
                    color = "rgba(50, 255, 0, 0.60)";
                    break;
                case "Medium":
                    color = "rgba(255, 216, 0, 0.60)";
                    break;
                case "High":
                    color = "rgba(255, 0, 0, 0.60);";
                    break;
                default: throw new Exception("status drawing method missing a status");
            }

            return $"<i class='fas fa-circle priority-icon' style='color: {color};'></i>";
        }

        public static List<TicketViewModel> MapTickets(List<Tickets> tickets)
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
                       Comments = ticket.Comments.Select(cmt => new CommentTicketViewModel()
                       {
                           Comment = cmt.CommentData,
                           CreatorDisplayName = cmt.User.DisplayName,
                           DateCreated = cmt.DateCreated,
                       }).ToList()
                   }).ToList();
        }

        public static List<TicketViewModel> MapTickets(List<Tickets> tickets, IPrincipal user)
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
                       Comments = ticket.Comments.Select(cmt => new CommentTicketViewModel()
                       {
                           Comment = cmt.CommentData,
                           CreatorDisplayName = cmt.User.DisplayName,
                           DateCreated = cmt.DateCreated,
                       }).ToList(),
                       CanEdit = UserCanAccessTicket(user, ticket),
                       CanAddFile = UserCanAccessTicket(user, ticket),
                       CanComment = UserCanAccessTicket(user, ticket)
                   }).ToList();
        }

        public static TicketViewModel MapTicket(Tickets ticket, IPrincipal user)
        {
            return new TicketViewModel()
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
                Comments = ticket.Comments.Select(cmt => new CommentTicketViewModel()
                {
                    Comment = cmt.CommentData,
                    CreatorDisplayName = cmt.User.DisplayName,
                    DateCreated = cmt.DateCreated,
                }).ToList(),
                Files = ticket.Files.Select(file => new FileTicketViewModel()
                {
                    MediaUrl = file.MediaUrl, // Removes the "~"
                    MediaTitle = file.Title
                }).ToList(),
                CanEdit = UserCanAccessTicket(user, ticket),
                CanAddFile = UserCanAccessTicket(user, ticket),
                CanComment = UserCanAccessTicket(user, ticket)
            };
        }


        public static EditTicketViewModel TicketDataByRole(Tickets ticket, IPrincipal User, RoleManager<Microsoft.AspNet.Identity.EntityFramework.IdentityRole> RoleManager, ApplicationUserManager UserManager, ApplicationDbContext DbContext)
        {
            EditTicketViewModel viewModelTicket;
            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var devRole = RoleManager.FindByName(ProjectConstants.DeveloperRole);

            if (ProjectHelper.IsAdminOrManager(User))
            {
                viewModelTicket = new EditTicketViewModel()
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    ProjectId = ticket.ProjectId,
                    TicketStatusId = ticket.TicketStatusId,
                    TicketPriorityId = ticket.TicketPriorityId,
                    TicketTypeId = ticket.TicketTypeId,
                    AssignedToId = ticket?.AssignedToId,
                    Projects = DbContext.Projects.Select(p =>
                        new SelectListItem()
                        {
                            Text = p.Title,
                            Value = p.Id.ToString()

                        }).ToList(),
                    DevUsers = DbContext.Users.Where(user => user.Roles.Any(role => role.RoleId == devRole.Id)).Select(p =>
                        new SelectListItem()
                        {
                            Text = p.DisplayName,
                            Value = p.Id.ToString()

                        }).ToList(),
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

                        }).ToList(),
                    TicketStatuses = DbContext.TicketStatuses.Select(p =>
                        new SelectListItem()
                        {
                            Text = p.Name,
                            Value = p.Id.ToString()

                        }).ToList(),
                };
            }
            else if (ProjectHelper.IsDevOrSubmitter(User))
            {
                viewModelTicket = new EditTicketViewModel()
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    ProjectId = ticket.ProjectId,
                    TicketPriorityId = ticket.TicketPriorityId,
                    TicketTypeId = ticket.TicketTypeId,
                    // This works for both submitter and devs, as they HAVE to be assigned to the project.
                    Projects = DbContext.Projects.Where(user => user.Users.Any(usr => usr.Id == currentUser.Id)).Select(p =>
                        new SelectListItem()
                        {
                            Text = p.Title,
                            Value = p.Id.ToString()

                        }).ToList(),
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

                        }).ToList(),
                };
            }
            else
            {
                throw new Exception("Error in Ticket Data Method. Missing Role.");
            }

            return viewModelTicket;
        }

        public static EditTicketViewModel TicketDataByRole(EditTicketViewModel ticket, IPrincipal User, RoleManager<Microsoft.AspNet.Identity.EntityFramework.IdentityRole> RoleManager, ApplicationUserManager UserManager, ApplicationDbContext DbContext)
        {
            EditTicketViewModel viewModelTicket;
            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var devRole = RoleManager.FindByName(ProjectConstants.DeveloperRole);

            if (ProjectHelper.IsAdminOrManager(User))
            {
                viewModelTicket = new EditTicketViewModel()
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    ProjectId = ticket.ProjectId,
                    TicketStatusId = ticket.TicketStatusId,
                    TicketPriorityId = ticket.TicketPriorityId,
                    TicketTypeId = ticket.TicketTypeId,
                    AssignedToId = ticket?.AssignedToId,
                    Projects = DbContext.Projects.Select(p =>
                        new SelectListItem()
                        {
                            Text = p.Title,
                            Value = p.Id.ToString()

                        }).ToList(),
                    DevUsers = DbContext.Users.Where(user => user.Roles.Any(role => role.RoleId == devRole.Id)).Select(p =>
                        new SelectListItem()
                        {
                            Text = p.DisplayName,
                            Value = p.Id.ToString()

                        }).ToList(),
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

                        }).ToList(),
                    TicketStatuses = DbContext.TicketStatuses.Select(p =>
                        new SelectListItem()
                        {
                            Text = p.Name,
                            Value = p.Id.ToString()

                        }).ToList(),
                };
            }
            else if (ProjectHelper.IsDevOrSubmitter(User))
            {
                // get the ticket because we need the ticketStatusId
                var ticketStatusId = DbContext.Tickets.FirstOrDefault(tckt => tckt.Id == ticket.Id).TicketStatusId;

                viewModelTicket = new EditTicketViewModel()
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    ProjectId = ticket.ProjectId,
                    TicketPriorityId = ticket.TicketPriorityId,
                    TicketStatusId = ticketStatusId,
                    TicketTypeId = ticket.TicketTypeId,
                    // This works for both submitter and devs, as they HAVE to be assigned to the project.
                    Projects = DbContext.Projects.Where(user => user.Users.Any(usr => usr.Id == currentUser.Id)).Select(p =>
                        new SelectListItem()
                        {
                            Text = p.Title,
                            Value = p.Id.ToString()

                        }).ToList(),
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

                        }).ToList(),
                };
            }
            else
            {
                throw new Exception("Error in Ticket Data Method. Missing Role.");
            }

            return viewModelTicket;
        }

        internal static bool UserCanAccessTicket(IPrincipal user, Tickets ticket)
        {
            var userId = user.Identity.GetUserId();

            if (ProjectHelper.IsAdminOrManager(user))
            {
                // Return ticket because user is a project manager or Admin
                return true;
            }
            if (user.IsInRole(ProjectConstants.DeveloperRole))
            {
                // Find the tickets this user is assigned too, if this ticket is in that list, return that ticket.
                //ticket = tickets.Where(t => t.AssignedToId == userId).FirstOrDefault(tckt => tckt.Id == ticketId);
                if (ticket.AssignedToId == userId)
                    return true;
            }
            if (user.IsInRole(ProjectConstants.SubmitterRole))
            {
                // Find the tickets this user created, if this ticket is in that list, return that ticket.
                //ticket = tickets.Where(t => t.CreatedById == userId).FirstOrDefault(tckt => tckt.Id == ticketId);
                if (ticket.CreatedById == userId)
                    return true;
            }
            return false;
        }

        internal static bool UserCanViewDetails(IPrincipal user, Tickets ticket)
        {
            var userId = user.Identity.GetUserId();

            if (ProjectHelper.IsAdminOrManager(user))
            {
                // Return ticket because user is a project manager or Admin
                return true;
            }
            if (user.IsInRole(ProjectConstants.DeveloperRole))
            {
                // Find the tickets this user is assigned too, if this ticket is in that list, return that ticket.
                //ticket = tickets.Where(t => t.AssignedToId == userId).FirstOrDefault(tckt => tckt.Id == ticketId);
                if (ticket.AssignedToId == userId)
                    return true;
            }
            if (user.IsInRole(ProjectConstants.SubmitterRole))
            {
                // Find the tickets this user created, if this ticket is in that list, return that ticket.
                //ticket = tickets.Where(t => t.CreatedById == userId).FirstOrDefault(tckt => tckt.Id == ticketId);
                if (ticket.CreatedById == userId)
                    return true;
            }
            if(ticket.Project.Users.Any(usr => usr.Id == user.Identity.GetUserId()))
            {
                // if user is assigned to project; They're allowed to access details
                return true;
            }
            return false;
        }
    }
}