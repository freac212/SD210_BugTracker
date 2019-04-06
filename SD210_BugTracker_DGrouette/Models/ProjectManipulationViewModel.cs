using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class ProjectManipulationViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string UserId { get; set; }
    }
}