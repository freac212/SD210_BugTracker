﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SD210_BugTracker_DGrouette.Models.Domain;
using SD210_BugTracker_DGrouette.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;

namespace SD210_BugTracker_DGrouette.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
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

                projects = DbContext.Projects.ToList().Select(p => new ProjectsViewModel()
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
                .Where(project => project.Users.Any(p => p.Id == currentUserId))
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

            if (ProjectHelper.IsAdminOrManager(User))
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
    }
}