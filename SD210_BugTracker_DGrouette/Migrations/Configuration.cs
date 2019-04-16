namespace SD210_BugTracker_DGrouette.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using SD210_BugTracker_DGrouette.Models;
    using SD210_BugTracker_DGrouette.Models.Domain;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SD210_BugTracker_DGrouette.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        private ApplicationDbContext Context { get; set; }

        // Seeding Roles and Users to the database.
        private RoleManager<IdentityRole> RoleManager { get; set; }
        private UserManager<ApplicationUser> UserManager { get; set; }

        protected override void Seed(SD210_BugTracker_DGrouette.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.
            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data aswell, however, the long method
            //  with ifs is prefered.
            Context = context;
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // ================ SEEDED USERS ================
            const string adminUserName = "admin@mybugtracker.com";
            const string adminPS = "Password-1";

            const string userWithAllRolesUserName = "test@test.com";
            const string userWithAllRolesPS = "Password-1";

            const string managerUserName = "manager@mybugtracker.com";
            const string managerPS = "Password-1";

            const string developerUserName = "developer@mybugtracker.com";
            const string developerPS = "Password-1";

            const string submitterUserName = "submitter@mybugtracker.com";
            const string submitterPS = "Password-1";

            // Check if Roles exists, else create it
            context.Roles.AddOrUpdate(p => p.Name, new IdentityRole(ProjectConstants.AdminRole));
            context.Roles.AddOrUpdate(p => p.Name, new IdentityRole(ProjectConstants.ManagerRole));
            context.Roles.AddOrUpdate(p => p.Name, new IdentityRole(ProjectConstants.DeveloperRole));
            context.Roles.AddOrUpdate(p => p.Name, new IdentityRole(ProjectConstants.SubmitterRole));

            // ============ Admin user ==============
            ApplicationUser adminUser = SeedUser(adminUserName, adminUserName, adminPS);
            // Verify this users role/s
            VerifyUserRole(adminUser, ProjectConstants.AdminRole, UserManager);

            // ============ Other Users ==============
            ApplicationUser userWithAllRoles = SeedUser(userWithAllRolesUserName, userWithAllRolesUserName, userWithAllRolesPS);
            // Verify this users role/s
            VerifyUserRole(userWithAllRoles, new List<string>(){
                ProjectConstants.AdminRole,
                ProjectConstants.ManagerRole,
                ProjectConstants.DeveloperRole,
                ProjectConstants.SubmitterRole
            }, UserManager);

            ApplicationUser managerUser = SeedUser(managerUserName, managerUserName, managerPS);
            // Verify this users role
            VerifyUserRole(managerUser, ProjectConstants.ManagerRole, UserManager);

            ApplicationUser developerUser = SeedUser(developerUserName, developerUserName, developerPS);
            // Verify this users role
            VerifyUserRole(developerUser, ProjectConstants.DeveloperRole, UserManager);

            ApplicationUser submitterUser = SeedUser(submitterUserName, submitterUserName, submitterPS);
            // Verify this users role
            VerifyUserRole(submitterUser, ProjectConstants.SubmitterRole, UserManager);

            // ============== Seeded Projects ==============
            Projects project = new Projects()
            {
                Id = 1,
                Title = "Seeded Project",
                Users = new List<ApplicationUser> { adminUser, managerUser, developerUser, submitterUser }
            };

            if (!Context.Projects.ToList().Any(p => p.Title == project.Title))
            {
                Context.Projects.Add(project);
            }

            // ============== Seeded Ticket Properties ============== ++Q Can loop this later.
            // ============== Ticket Statuses ==============
            Context.TicketStatuses.AddOrUpdate(p => p.Name, new TicketStatuses() { Name = ProjectConstants.TicketStatusOpen });
            Context.TicketStatuses.AddOrUpdate(p => p.Name, new TicketStatuses() { Name = ProjectConstants.TicketStatusResolved });
            Context.TicketStatuses.AddOrUpdate(p => p.Name, new TicketStatuses() { Name = ProjectConstants.TicketStatusRejected });
            // ============== Ticket Priorities ==============
            Context.TicketPriorities.AddOrUpdate(p => p.Name, new TicketPriorities() { Name = ProjectConstants.TicketPriorityLow });
            Context.TicketPriorities.AddOrUpdate(p => p.Name, new TicketPriorities() { Name = ProjectConstants.TicketPriorityMedium });
            Context.TicketPriorities.AddOrUpdate(p => p.Name, new TicketPriorities() { Name = ProjectConstants.TicketPriorityHigh });
            // ============== Ticket Types ==============
            Context.TicketTypes.AddOrUpdate(p => p.Name, new TicketTypes() { Name = ProjectConstants.TicketTypeBug });
            Context.TicketTypes.AddOrUpdate(p => p.Name, new TicketTypes() { Name = ProjectConstants.TicketTypeDataBase });
            Context.TicketTypes.AddOrUpdate(p => p.Name, new TicketTypes() { Name = ProjectConstants.TicketTypeFeature });
            Context.TicketTypes.AddOrUpdate(p => p.Name, new TicketTypes() { Name = ProjectConstants.TicketTypeSupport });

            // Save changes here just incase
            Context.SaveChanges();
        }

        private void VerifyUserRole(ApplicationUser user, string role, UserManager<ApplicationUser> userManager)
        {
            // Verify this users role, if they're not assigned to the role given, assign them to it.
            if (!userManager.IsInRole(user.Id, role))
            {
                // if not, add the user to the role
                userManager.AddToRole(user.Id, role);
            }
        }

        private void VerifyUserRole(ApplicationUser user, List<string> roles, UserManager<ApplicationUser> userManager)
        {
            foreach (var role in roles)
            {
                // Verify this users role, if they're not assigned to the role given, assign them to it.
                if (!userManager.IsInRole(user.Id, role))
                {
                    // if not, add the user to the role
                    userManager.AddToRole(user.Id, role);
                }
            }
        }

        private ApplicationUser SeedUser(string email, string userName, string passPhrase)
        {
            // Getting/creating user. Ensures stability upon user seeding.

            ApplicationUser user;
            // Check if any users already have that userName
            if (!Context.Users.Any(p => p.UserName == userName))
            {
                // If that user doesn't exist, create the user.
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    DisplayName = userName
                };

                UserManager.Create(user, passPhrase);
            }
            else
            {
                // Getting the user in order to verify the role.
                user = Context.Users.First(p => p.UserName == userName);
            }

            return user;
        }
    }
}
