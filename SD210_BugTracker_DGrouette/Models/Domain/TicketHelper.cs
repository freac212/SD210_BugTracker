using Microsoft.AspNet.Identity;
using SD210_BugTracker_DGrouette.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public static class TicketHelper
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

        public static List<TicketViewModel> MapTickets(List<Ticket> tickets, IPrincipal user)
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
                           Id = cmt.Id,
                           Comment = cmt.CommentData,
                           CreatorDisplayName = cmt.User.DisplayName,
                           CreatorId = cmt.UserId,
                           CanModifyComment = UserCanModifyComment(user, cmt),
                           DateCreated = cmt.DateCreated,
                       }).ToList(),
                       CanEdit = UserCanAccessTicket(user, ticket),
                       CanAddFile = UserCanAccessTicket(user, ticket),
                       CanComment = UserCanAccessTicket(user, ticket)
                   }).ToList();
        }

        public static TicketViewModel MapTicket(Ticket ticket, IPrincipal user)
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
                CanEdit = UserCanAccessTicket(user, ticket),
                CanAddFile = UserCanAccessTicket(user, ticket),
                CanComment = UserCanAccessTicket(user, ticket),
                Comments = ticket.Comments.Select(cmt => new CommentTicketViewModel()
                {
                    Id = cmt.Id,
                    Comment = cmt.CommentData,
                    CreatorDisplayName = cmt.User.DisplayName,
                    CreatorId = cmt.UserId,
                    CanModifyComment = UserCanModifyComment(user, cmt),
                    DateCreated = cmt.DateCreated,
                }).ToList(),
                Files = ticket.Files.Select(file => new FileTicketViewModel()
                {
                    MediaUrl = file.MediaUrl, // Removes the "~"
                    MediaFileName = file.FileName,
                    MediaTitle = file.Title,
                    CanDeleteFile = UserCanDeleteFile(user, file),
                    Id = file.Id
                }).ToList(),
                Histories = ticket.TicketHistories.Select(history => new TicketHistoryViewModel()
                {
                    DateUpdated = history.DateUpdated,
                    UserDisplayName = history.User.DisplayName,
                    HistoryDetails = history.TicketHistoryDetails.Select(details => new TicketHistoryDetailsViewModel()
                    {
                        Property = details.Property,
                        NewValue = details.NewValue,
                        OldValue = details.OldValue
                    }).ToList()
                }).ToList()
            };
        }

        public static EditTicketViewModel TicketDataByRole(Ticket ticket, IPrincipal User, RoleManager<Microsoft.AspNet.Identity.EntityFramework.IdentityRole> RoleManager, ApplicationUserManager UserManager, ApplicationDbContext DbContext)
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
                    Projects = DbContext.Projects.Where(p => !p.IsArchived).Select(p =>
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
                    Projects = DbContext.Projects.Where(user => user.Users.Any(usr => usr.Id == currentUser.Id) && !user.IsArchived).Select(p =>
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
                    Projects = DbContext.Projects.Where(p => !p.IsArchived).Select(p =>
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
                    Projects = DbContext.Projects.Where(user => user.Users.Any(usr => usr.Id == currentUser.Id) && !user.IsArchived).Select(p =>
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

        internal static bool UserCanAccessTicket(IPrincipal user, Ticket ticket)
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

        internal static bool UserCanAccessComment(IPrincipal user, Comment comment)
        {
            var userId = user.Identity.GetUserId();

            if (ProjectHelper.IsAdminOrManager(user))
            {
                // Return ticket because user is a project manager or Admin
                return true;
            }
            if (ProjectHelper.IsDevOrSubmitter(user))
            {
                if (comment.UserId == userId)
                    return true;
            }
            return false;
        }

        internal static bool UserCanViewDetails(IPrincipal user, Ticket ticket)
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
            if (ticket.Project.Users.Any(usr => usr.Id == user.Identity.GetUserId()))
            {
                // if user is assigned to project; They're allowed to access details
                return true;
            }
            return false;
        }

        private static bool UserCanModifyComment(IPrincipal user, Comment comment)
        {
            var userId = user.Identity.GetUserId();

            if (ProjectHelper.IsAdminOrManager(user))
            {
                // project manager or Admin can delete all comments.
                return true;
            }
            if (user.IsInRole(ProjectConstants.DeveloperRole) || user.IsInRole(ProjectConstants.SubmitterRole))
            {
                // If the Dev/ Submitter created this comment -> they can delete it.
                if (comment.UserId == userId)
                    return true;
            }

            return false;
        }

        private static bool UserCanDeleteFile(IPrincipal user, TicketFile file)
        {
            var userId = user.Identity.GetUserId();

            if (ProjectHelper.IsAdminOrManager(user))
            {
                // project manager or Admin can delete all comments.
                return true;
            }
            if (user.IsInRole(ProjectConstants.DeveloperRole) || user.IsInRole(ProjectConstants.SubmitterRole))
            {
                // If the Dev/ Submitter created this comment -> they can delete it.
                if (file.UserId == userId)
                    return true;
            }

            return false;
        }

        public static TicketHistory GenerateTicketHistory(ApplicationDbContext DbContext, Ticket ticket, ApplicationUser user)
        {
            // Get the props that have been modified, add to new TicketHistory, return it.
            DbChangeTracker changeTracker = DbContext.ChangeTracker;
            var entries = changeTracker.Entries();
            var history = new TicketHistory();
            history.User = user;

            if (changeTracker.HasChanges())
            {
                var modifiedEntries = (from prop in entries
                                       where prop.State == System.Data.Entity.EntityState.Modified
                                       select prop).FirstOrDefault();

                var historyDetails = (from propName in modifiedEntries.OriginalValues.PropertyNames
                                      let valA = modifiedEntries.OriginalValues[propName]?.ToString()
                                      let valB = modifiedEntries.CurrentValues[propName]?.ToString()
                                      where valA != valB && (valA == null || !valA.Equals(valB)) && propName != "DateUpdated"
                                      select new TicketHistoryDetails()
                                      {
                                          OldValue = valA,
                                          NewValue = valB,
                                          Property = propName
                                      }).ToList();

                history.DateUpdated = (from propName in modifiedEntries.OriginalValues.PropertyNames
                                       where propName == "DateUpdated"
                                       select (DateTime)modifiedEntries.CurrentValues[propName]).FirstOrDefault();

                // Converting Ticket status, priority, and types from their id's to more the more user friendly names.
                for (int i = 0; i < historyDetails.Count(); i++)
                {
                    if (historyDetails[i].Property == "TicketStatusId")
                    {
                        var statuses = DbContext.TicketStatuses.ToList();
                        historyDetails[i].Property = "Ticket Status";

                        var parsedIdOld = int.Parse(historyDetails[i].OldValue);
                        historyDetails[i].OldValue = statuses.Find(p => p.Id == parsedIdOld).Name;

                        var parsedIdNew = int.Parse(historyDetails[i].NewValue);
                        historyDetails[i].NewValue = statuses.Find(p => p.Id == parsedIdNew).Name;
                    }
                    else if (historyDetails[i].Property == "TicketPriorityId")
                    {
                        var priorities = DbContext.TicketPriorities.ToList();
                        historyDetails[i].Property = "Ticket Priority";

                        var parsedIdOld = int.Parse(historyDetails[i].OldValue);
                        historyDetails[i].OldValue = priorities.Find(p => p.Id == parsedIdOld).Name;

                        var parsedIdNew = int.Parse(historyDetails[i].NewValue);
                        historyDetails[i].NewValue = priorities.Find(p => p.Id == parsedIdNew).Name;
                    }
                    else if (historyDetails[i].Property == "TicketTypeId")
                    {
                        var types = DbContext.TicketTypes.ToList();
                        historyDetails[i].Property = "Ticket Type";

                        var parsedIdOld = int.Parse(historyDetails[i].OldValue);
                        historyDetails[i].OldValue = types.Find(p => p.Id == parsedIdOld).Name;

                        var parsedIdNew = int.Parse(historyDetails[i].NewValue);
                        historyDetails[i].NewValue = types.Find(p => p.Id == parsedIdNew).Name;
                    }
                    else if (historyDetails[i].Property == "AssignedToId")
                    {
                        var users = DbContext.Users.ToList();
                        var devUserNew = users.Find(p => p.Id == historyDetails[i].NewValue);
                        var devUserOld = users.Find(p => p.Id == historyDetails[i].OldValue);

                        historyDetails[i].Property = "Dev Assigned";
                        historyDetails[i].OldValue = devUserOld?.DisplayName;
                        historyDetails[i].NewValue = devUserNew?.DisplayName;

                        if (historyDetails[i].OldValue is null)
                            historyDetails[i].OldValue = "No dev assigned";
                        if (historyDetails[i].NewValue is null)
                            historyDetails[i].NewValue = "No dev assigned";
                    }
                    else if (historyDetails[i].Property == "ProjectId")
                    {
                        var projects = DbContext.Projects.ToList();

                        var devUserNew = projects.Find(p => p.Id.ToString() == historyDetails[i].NewValue);
                        var devUserOld = projects.Find(p => p.Id.ToString() == historyDetails[i].OldValue);

                        historyDetails[i].Property = "Assigned Project";
                        historyDetails[i].OldValue = devUserOld.Title;
                        historyDetails[i].NewValue = devUserNew.Title;
                    }
                }

                foreach (var detail in historyDetails)
                {
                    history.TicketHistoryDetails.Add(detail);
                }
            }

            // Ensures an empty history cannot be created.
            if (history.TicketHistoryDetails.Any())
                return history;
            else
                return null;
            
        }

        public static void SendNotificationsToUsers(Ticket ticket)
        {
            // If dev has already been notified of assignment -> notify them of edits/ comments changed/ files changed
            // else -> notify them of assignment (Only notify him once tho, might have to work off the changelog...)

            // Get Admins + PM's who are "tracking" this ticket. -> notify them of edits/ comments changed/ files changed
            NotifyUsersByEmail(
                $"Changes to a ticket have occurred",
                $"Changes to the ticket: '{ticket.Title}' have occured.",
                ticket.SubscribedUsers);
        }

        public static void SendNotificationsToUsers(Ticket ticket, string actionName)
        {
            // Notify dev of ticket assignment and set the history details.

            // If dev has already been notified of assignment -> notify them of edits/ comments changed/ files changed
            // else -> notify them of assignment (Only notify him once tho, might have to work off the changelog...)

            // Get Admins + PM's who are "tracking" this ticket. -> notify them of edits/ comments changed/ files changed
            NotifyUsersByEmail(
                $"Changes to a ticket have occurred",
                $"Changes to the ticket: '{ticket.Title}' have occured. " +
                $"Action: {actionName}",
                ticket.SubscribedUsers);
        }

        public static void NotifyUsersByEmail(string subject, string body, List<ApplicationUser> users)
        {
            var userEmails = users.Select(p => p.Email).ToList();
            var email = new EmailService();

            if (subject is null)
                email.SendToManyUsers(userEmails, body, $"Important Ticket Changes - {DateTime.Now}");
            else
                email.SendToManyUsers(userEmails, body, subject + " - " + DateTime.Now);
        }

        internal static void NotifyUserByEmail(ApplicationUser devUserNew, string ticketTitle)
        {
            var email = new EmailService();

            if (devUserNew != null)
                email.Send(devUserNew.Email, $"You've been assigned to the ticket: '{ticketTitle}'", "Ticket assignment");
        }

        internal static void ManageDevNotificationAssignment(ApplicationUser devUserOld, ApplicationUser devUserNew, Ticket ticket)
        {
            if (devUserOld != null && devUserNew != null)
            {
                // add new remove old
                ticket.SubscribedUsers.Add(devUserNew);
                ticket.SubscribedUsers.RemoveAll(p => p.Id == devUserOld.Id);
            } else if (devUserOld is null && devUserNew != null)
            {
                // Add new but you cannot remove null from a list sooo
                ticket.SubscribedUsers.Add(devUserNew);
            } else if (devUserOld != null && devUserNew is null)
            {
                // Remove old, but dont add new "null" to list.
                ticket.SubscribedUsers.RemoveAll(p => p.Id == devUserOld.Id);
            }
        }

        public static bool IsNewDevAssigned(TicketHistory ticketHistory)
        {
            foreach (var details in ticketHistory.TicketHistoryDetails)
            {
                if (details.Property == "Dev Assigned")
                    return true;
                else
                    return false;
            }
            return false;
        }
    }
}