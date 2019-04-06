using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public class Projects
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public virtual List<ApplicationUser> Users { get; set; }

        public Projects()
        {
            Users = new List<ApplicationUser>();
        }
    }
}