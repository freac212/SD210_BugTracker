using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class ProjectsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int UserCount { get; set; }
    }
}