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
    public class EditTicketViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }


        [Required(ErrorMessage = "Please assign this ticket to a project")]
        public int ProjectId { get; set; }

        //[Required(ErrorMessage = "Please choose a Ticket Status")]
        public int? TicketStatusId { get; set; }

        [Required(ErrorMessage = "Please choose a Ticket Priority")]
        public int TicketPriorityId { get; set; }

        [Required(ErrorMessage = "Please choose a Ticket Type")]
        public int TicketTypeId { get; set; }

        public string AssignedToId { get; set; }
        //public virtual ApplicationUser AssignedTo { get; set; }
        //public virtual Projects Project { get; set; } // Virtual for lazy loading



        // ++Q Should these use virtual? Noope
        public List<SelectListItem> Projects { get; set; }
        public List<SelectListItem> DevUsers { get; set; }
        public List<SelectListItem> TicketPriorities { get; set; } // Add greyed out "None" Button??
        public List<SelectListItem> TicketTypes { get; set; }
        public List<SelectListItem> TicketStatuses { get; set; }

    }
}