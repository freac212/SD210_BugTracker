using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SD210_BugTracker_DGrouette.Models.Domain;

namespace SD210_BugTracker_DGrouette.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        // Lazy loading - Many to many relationship
        public virtual List<Projects> Projects { get; set; }
        [InverseProperty(nameof(Tickets.CreatedBy))] // Used to connect certain properties when there's more than one of the same properties. (Like users.)
        public virtual List<Tickets> CreatedTickets { get; set; }
        [InverseProperty(nameof(Tickets.AssignedTo))]
        public virtual List<Tickets> AssignedTickets { get; set; }
        public virtual List<Comment> Comments { get; set; }

        public virtual string DisplayName { get; set; }

        public ApplicationUser()
        {
            Projects = new List<Projects>();
            CreatedTickets = new List<Tickets>();
            AssignedTickets = new List<Tickets>();
            Comments = new List<Comment>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Projects> Projects { get; set; } // Allows access of users projects from the DB
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<TicketStatuses> TicketStatuses { get; set; }
        public DbSet<TicketPriorities> TicketPriorities { get; set; }
        public DbSet<TicketTypes> TicketTypes { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}