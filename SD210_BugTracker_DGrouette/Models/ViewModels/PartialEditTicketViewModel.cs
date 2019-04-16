using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette.Models
{
    public class PartialEditTicketViewModel
    {
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required(ErrorMessage = "Please choose a Ticket Priority")]
        public int TicketPriorityId { get; set; }
        [Required(ErrorMessage = "Please choose a Ticket Type")]
        public int TicketTypeId { get; set; }


        public virtual List<SelectListItem> Projects { get; set; }
        public virtual List<SelectListItem> TicketPriorities { get; set; } // Add greyed out "None" Button??
        public virtual List<SelectListItem> TicketTypes { get; set; }
    }
}