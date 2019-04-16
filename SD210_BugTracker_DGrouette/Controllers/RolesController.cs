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
    public class RolesController : Controller
    {
        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationDbContext DbContext => HttpContext.GetOwinContext().Get<ApplicationDbContext>();
        public RoleManager<IdentityRole> RoleManager => new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(DbContext));


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
            // Literal List-ception...
            // Get the list of Users,                   Convert to view Model.
            // ==BaseViewModel==
            // List- Users
            //      UserId
            //      UserDisplayName
            //      List- RolesTheyAreIn 
            //          RoleID
            //          Selected
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
                    RolesUserIsIn = p.Roles.Select(prop => new UsersAssignedRolesViewModel()
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
                return RedirectToAction("Index", "Home");

            // Get user, 
            // get roles they are assigned too,
            // if id is null -> redirect back to home.
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
                    RolesUserIsIn = DbContext.Roles.ToList().Select(prop => new UsersAssignedRolesViewModel()
                    {
                        RoleId = prop.Id,
                        Selected = user.Roles.Any(p => p.RoleId == prop.Id)
                    }).ToList(),
                    Roles = DbContext.Roles.ToList() // Can use a viewModel for this. Actually it's best too.
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = ProjectConstants.AdminRole)]
        public ActionResult RoleEditor(string userId, RoleEditorViewModel formData)
        {
            if (!ModelState.IsValid || userId is "" || formData is null)
                return RedirectToAction("UsersAndRoles");
            //return View();

            var user = UserManager.Users.FirstOrDefault(p => p.Id == userId);

            if (user is null)
                return RedirectToAction("UsersAndRoles");

            foreach (var item in formData.RolesUserIsIn)
            {
                var role = DbContext.Roles.FirstOrDefault(p => p.Id == item.RoleId);

                if (role is null)
                {
                    throw new Exception("Role is null: Role Id is missing, check if you're sending the proper roles, or if a user role is missing.");
                }
                else
                {
                    if (item.Selected)
                        UserManager.AddToRole(user.Id, role.Name);
                    else
                        UserManager.RemoveFromRole(user.Id, role.Name);
                }

                //if (!item.Selected && role != null) // ++Q , technically if the roll is null, it will try to add the null role
                //{
                //    UserManager.RemoveFromRole(user.Id, role.Name);
                //}
                //else
                //{
                //    UserManager.AddToRole(user.Id, role.Name);
                //}
            }

            //UserManager.Update(user);

            return RedirectToAction("UsersAndRoles");
        }
    }
}