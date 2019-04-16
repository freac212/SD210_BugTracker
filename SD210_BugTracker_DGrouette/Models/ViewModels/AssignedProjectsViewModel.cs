using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class AssignedProjectsViewModel
    {
        public int? Id { get; set; }
        public string ProjectTitle { get; set; }
        public int UserCount { get; set; }
        public int TicketCount { get; set; }
    }
}