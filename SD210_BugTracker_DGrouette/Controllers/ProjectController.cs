using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SD210_BugTracker_DGrouette.Models.Domain;
using SD210_BugTracker_DGrouette.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using SD210_BugTracker_DGrouette.Models.Filters;

namespace SD210_BugTracker_DGrouette.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.GetOwinContext().Get<ApplicationDbContext>();
        public RoleManager<IdentityRole> RoleManager => new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbContext));


        // GET: Projects
        [HttpGet]
        public ActionResult Index()
        {
            // Check for user role, 
            //  if admin/ manager
            //  then return all list, 
            //  else if logged in
            //  goto assignedProjects view
            //  else return nothing

            if (ProjectHelper.IsAdminOrManager(User))
            {
                List<ProjectsViewModel> projects = new List<ProjectsViewModel>();

                projects = DbContext.Projects.ToList().Where(p => !p.IsArchived).Select(p => new ProjectsViewModel()
                {
                    Id = p.Id,
                    Title = p.Title,
                    UserCount = p.Users.Count
                }).ToList();

                return View(projects);
            }
            else if (User.Identity.IsAuthenticated)
            {
                // Just so they're not asked to log in, because the home page is all projects. 
                return RedirectToAction("AssignedProjects");
            }

            return View();
        }

        // GET: Projects
        [HttpGet]
        public ActionResult AssignedProjects()
        {
            // Get projects that the current user is "in"
            // Then send the project titles, and user count.
            var currentUserId = User.Identity.GetUserId();

            var projects = DbContext.Projects.ToList()
                .Where(project => project.Users.Any(p => p.Id == currentUserId) && !project.IsArchived)
                .Select(p => new AssignedProjectsViewModel()
                {
                    Id = p.Id,
                    ProjectTitle = p.Title,
                    UserCount = p.Users.Count,
                    TicketCount = p.Tickets.Count
                }).ToList();

            return View(projects);
        }

        // GET: 
        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult CreateProject()
        {
            return View(new ProjectManipulationViewModel());
        }

        // POST: 
        [HttpPost]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult CreateProject(ProjectManipulationViewModel newProject)
        {
            if (ProjectHelper.IsAdminOrManager(User))
            {
                var project = new Project()
                {
                    Title = newProject.Title,
                    IsArchived = false
                };

                DbContext.Projects.Add(project);
                DbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // GET: 
        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        [IdAuthentication("id")]
        public ActionResult EditProject(int? id)
        {
            var project = ProjectHelper.GetProjectById(DbContext, (int)id);

            if (project is null)
                return RedirectToAction("Index");

            var projectViewModel = new ProjectManipulationViewModel()
            {
                Title = project.Title,
                Id = project.Id
            };

            return View(projectViewModel);
        }

        // POST: 
        [HttpPost]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult EditProject(ProjectManipulationViewModel editedProject)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            if (ProjectHelper.IsAdminOrManager(User))
            {
                var projectFromDb = ProjectHelper.GetProjectById(DbContext, editedProject.Id);

                if (projectFromDb is null)
                    return RedirectToAction("Index");

                projectFromDb.Title = editedProject.Title;
                DbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // GET: 
        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        [IdAuthentication("id")]
        public ActionResult UserProjectAssignment(int? id)
        {
            var project = ProjectHelper.GetProjectById(DbContext, (int)id);

            if (project is null)
                return RedirectToAction("Index");

            var projectViewModel = new UserProjectAssignmentViewModel()
            {
                Title = project.Title,
                Id = project.Id,
                Users = DbContext.Users.Select(user => new UserInProjectViewModel()
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Selected = user.Projects.Any(prop => prop.Id == project.Id),
                    UserId = user.Id
                }).ToList()
            };

            return View(projectViewModel);
        }

        // POST: 
        [HttpPost]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult UserProjectAssignment(UserProjectAssignmentViewModel assignedUsers)
        {
            // No required tags are used so this isn't really neccessary.
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (ProjectHelper.IsAdminOrManager(User))
            {
                // Get the project we're working on
                var project = ProjectHelper.GetProjectById(DbContext, assignedUsers.Id);

                if (project is null)
                    return RedirectToAction("Index");

                var userIds = assignedUsers.Users
                    .Where(user => user.Selected)
                    .Select(user => user.UserId)
                    .ToList();
                var removedIds = assignedUsers.Users
                    .Where(user => !user.Selected)
                    .Select(user => user.UserId)
                    .ToList();

                // Bassically removing all users from the project, then re-adding the neccessary id's/ users.
                project.Users.RemoveAll(p => removedIds.Contains(p.Id));

                var UserListLocal = UserManager.Users
                    .Where(user => userIds.Contains(user.Id))
                    .ToList();

                foreach (var item in assignedUsers.Users)
                {
                    // If user exists on the project
                    // And is selected 
                    // do nothing

                    // if the user is NOT on the project and 
                    // is selected
                    // Add

                    // if the user is NOT on the project and 
                    // is not selected
                    // do nothing

                    // if the user is on the project and 
                    // is NOT selected
                    // remove it

                    if (item.Selected && !project.Users.Any(p => p.Id == item.UserId))
                    {
                        project.Users.Add(UserListLocal.FirstOrDefault(p => p.Id == item.UserId));
                    }
                    //else if (!item.Selected && project.Users.Any(p => p.Id == item.UserId))
                    //{
                    //    //project.Users.Remove(UserListLocal.FirstOrDefault(p => p.Id == item.UserId));
                    //    project.Users.RemoveAll(p => p.Id == item.UserId);
                    //}

                    // Can remove all users, then add the ones back 
                }

                DbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        [IdAuthentication("id")]
        public ActionResult ArchiveProject(int? Id)
        {
            var archiveProject = new ArchiveProjectViewModel()
            {
                ProjectId = (int)Id
            };

            return View(archiveProject);
        }

        [HttpPost]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult ArchiveProject(ArchiveProjectViewModel formData)
        {
            if (formData is null)
                return RedirectToAction("Index");

            var project = ProjectHelper.GetProjectById(DbContext, formData.ProjectId);

            if (project is null)
                return RedirectToAction("Index");

            project.IsArchived = true;

            DbContext.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}