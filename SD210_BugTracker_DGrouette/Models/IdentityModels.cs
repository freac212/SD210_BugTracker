using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using EntityFramework.DynamicFilters;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SD210_BugTracker_DGrouette.Models.Domain;

namespace SD210_BugTracker_DGrouette.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        // Lazy loading - Many to many relationship
        public virtual List<Project> Projects { get; set; }
        [InverseProperty(nameof(Ticket.CreatedBy))] // Used to connect certain properties when there's more than one of the same properties. (Like users.)
        public virtual List<Ticket> CreatedTickets { get; set; }
        [InverseProperty(nameof(Ticket.AssignedTo))]
        public virtual List<Ticket> AssignedTickets { get; set; }
        public virtual List<Comment> Comments { get; set; }
        public virtual List<Ticket> SubscribedTickets { get; set; }

        public virtual string DisplayName { get; set; }

        public ApplicationUser()
        {
            Projects = new List<Project>();
            CreatedTickets = new List<Ticket>();
            AssignedTickets = new List<Ticket>();
            Comments = new List<Comment>();
            SubscribedTickets = new List<Ticket>();
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

        // This modifies the query sent to the DB to get projects and tickets
        // It just ensures that anything on the DB that is archived is not "picked up"
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Filter("IsArchived", (Project d) => !d.IsArchived);
            modelBuilder.Filter("IsArchivedTicket", (Ticket d) => !d.Project.IsArchived);
            modelBuilder.Filter("IsArchivedTicketComment", (Comment d) => !d.Ticket.Project.IsArchived);
            modelBuilder.Filter("IsArchivedTicketFile", (TicketFile d) => !d.Ticket.Project.IsArchived);
            modelBuilder.Filter("IsArchivedTicketHistory", (TicketHistory d) => !d.Ticket.Project.IsArchived);
        }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<TicketFile> Files { get; set; }
        public DbSet<Project> Projects { get; set; } // Allows access of users projects from the DB

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }

        public DbSet<TicketStatuses> TicketStatuses { get; set; }
        public DbSet<TicketPriorities> TicketPriorities { get; set; }
        public DbSet<TicketTypes> TicketTypes { get; set; }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}