using SD210_BugTracker_DGrouette.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette.Models
{
    public class CreateTicketViewModel
    {
        public int ProjectId { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required(ErrorMessage = "Please choose a Ticket Priority")]
        public int TicketPriorityId { get; set; }
        [Required(ErrorMessage = "Please choose a Ticket Type")]
        public int TicketTypeId { get; set; }
        //[Required]
        //public List<ApplicationUser> AssignedTo { get; set; } // Only show this is the submitter is a project manager/ Admin
        public List<SelectListItem> TicketPriorities { get; set; } // Add greyed out "None" Button??
        public List<SelectListItem> TicketTypes { get; set; }
    }
}