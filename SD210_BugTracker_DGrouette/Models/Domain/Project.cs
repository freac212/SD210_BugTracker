using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsArchived { get; set; }

        public virtual List<ApplicationUser> Users { get; set; }

        public virtual List<Ticket> Tickets { get; set; }

        public Project()
        {
            Users = new List<ApplicationUser>();
            Tickets = new List<Ticket>();
        }
    }
}