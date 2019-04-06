using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SD210_BugTracker_DGrouette.Models.Domain;
using SD210_BugTracker_DGrouette.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette.Controllers
{
    public class ProjectsController : Controller
    {
        readonly static private ApplicationDbContext DbContext = new ApplicationDbContext();
        private RoleManager<IdentityRole> RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbContext));
        private UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(DbContext));

        // GET: Projects
        [HttpGet]
        public ActionResult Index()
        {
            List<ProjectsViewModel> projects = new List<ProjectsViewModel>();

            if (User.IsInRole(ProjectConstants.AdminRole) || User.IsInRole(ProjectConstants.ManagerRole))
            {
                projects = DbContext.Projects.ToList().Select(p => new ProjectsViewModel()
                {
                    Id = p.Id,
                    Title = p.Title,
                    UserCount = p.Users.Count
                }).ToList();

                return View(projects);
            }
            return View(projects);

            // Check for user role, 
            //  if admin/ manager
            //  then return all list, 
            //  else return nothing
        }

        // GET: Projects
        [HttpGet]
        public ActionResult AssignedProjects()
        {
            // Get projects that the current user is "in"
            // Then send the project titles, and user count.
            var currentUserId = User.Identity.GetUserId();

            var projects = DbContext.Projects.ToList()
                .Where(project => project.Users.Any(p => p.Id == currentUserId))
                .Select(p => new AssignedProjectsViewModel()
                    {
                        ProjectTitle = p.Title,
                        UserCount = p.Users.Count
            }).ToList();

            return View(projects);
        }

        // GET: 
        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult Create()
        {
            return View(new ProjectManipulationViewModel());
        }

        // POST: 
        [HttpPost]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult Create(ProjectManipulationViewModel newProject)
        {
            if (User.IsInRole(ProjectConstants.AdminRole) || User.IsInRole(ProjectConstants.ManagerRole))
            {
                var project = new Projects()
                {
                    Title = newProject.Title
                };

                DbContext.Projects.Add(project);
                DbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }



        // GET: 
        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult EditProject(int? id)
        {
            if (id is null)
                return RedirectToAction("Index");

            var project = DbContext.Projects.FirstOrDefault(p => p.Id == id);

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

            if (User.IsInRole(ProjectConstants.AdminRole) || User.IsInRole(ProjectConstants.ManagerRole))
            {
                var projectFromDb = DbContext.Projects.FirstOrDefault(p => p.Id == editedProject.Id);

                if (projectFromDb is null)
                    return RedirectToAction("Index");

                projectFromDb.Title = editedProject.Title;
                DbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }



        // User project Assignment
        // GET: 
        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole + "," + ProjectConstants.ManagerRole)]
        public ActionResult UserProjectAssignment(int? id)
        {
            if (id is null)
                return RedirectToAction("Index");

            var project = DbContext.Projects.FirstOrDefault(p => p.Id == id);

            if (project is null)
                return RedirectToAction("Index");

            var projectViewModel = new UserProjectAssignmentViewModel()
            {
                Title = project.Title,
                Id = project.Id,
                Users = DbContext.Users.Select(user => new UserInProject()
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

            if (User.IsInRole(ProjectConstants.AdminRole) || User.IsInRole(ProjectConstants.ManagerRole))
            {
                // Get the project we're working on
                var project = DbContext.Projects.FirstOrDefault(p => p.Id == assignedUsers.Id);

                if (project is null)
                    return RedirectToAction("Index");

                var userIds = assignedUsers.Users.Where(user => user.Selected).Select(user => user.UserId).ToList();
                var removedIds = assignedUsers.Users.Where(user => !user.Selected).Select(user => user.UserId).ToList();

                project.Users.RemoveAll(p => removedIds.Contains(p.Id));

                var UserListLocal = UserManager.Users.Where(user => userIds.Contains(user.Id)).ToList();

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



        // GET: RoleEditor/
        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole)]
        public ActionResult UsersAndRoles()
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            // List of Users, in that list of Users each contain a list of objects with role names and selected or not.
            // Get the list of Users,                   Convert to view Model.
            // ==BaseViewModel==
            // List- Users
            //      UserId
            //      UserDisplayName
            //      List- RolesTheyAreIn 
            //          RoleID
            //          Selected (Doesn't exist)
            // List- Roles
            //      RoleID
            //      RoleDisplayname

            var UserRoles = new UsersAndRolesViewModel()
            {
                Roles = RoleManager.Roles.ToList(),
                UserModels = UserManager.Users.Select(p => new RoleEditorViewModel()
                {
                    Id = p.Id,
                    DisplayName = p.DisplayName,
                    RolesUserIsIn = p.Roles.Select(prop => new SelectedRoles()
                    {
                        RoleId = prop.RoleId,
                        Selected = true
                    }).ToList()
                }).ToList()
            };

            return View(UserRoles);
        }

        [HttpGet]
        [Authorize(Roles = ProjectConstants.AdminRole)]
        public ActionResult RoleEditor(string userId)
        {
            if (userId is "")
            {
                return View();
            }
            // Get user, 
            // get roles they are assigned too,
            // if id is null -> redirect back to ???
            // otherwise redirect to edit roles page with
            //  -   User DisplayName
            //  -   Roles user is in
            //  -   Roles
            // Through a EditUserRolesViewModel

            var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);

            if (user is null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {

                return View(new RoleEditorViewModel()
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    RolesUserIsIn = RoleManager.Roles.ToList().Select(prop => new SelectedRoles()
                    {
                        RoleId = prop.Id,
                        Selected = user.Roles.Any(p => p.RoleId == prop.Id)
                    }).ToList(),
                    Roles = RoleManager.Roles.ToList()
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = ProjectConstants.AdminRole)]
        public ActionResult RoleEditor(string userId, RoleEditorViewModel formData)
        {
            // Ensure all the entityFrameWork tags requirements have been met
            if (!ModelState.IsValid || userId is "" || formData is null)
            {
                return View();
            }

            var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);
            foreach (var item in formData.RolesUserIsIn)
            {
                var role = RoleManager.Roles.FirstOrDefault(p => p.Id == item.RoleId);
                if (!item.Selected && role != null)
                {
                    UserManager.RemoveFromRole(user.Id, role.Name);
                }
                else
                {
                    UserManager.AddToRole(user.Id, role.Name);
                }
            }
            UserManager.Update(user);

            return RedirectToAction("UsersAndRoles");
        }
    }
}