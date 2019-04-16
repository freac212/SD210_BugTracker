using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public class Tickets
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual Projects Project { get; set; } // Virtual for lazy loading
        public int ProjectId { get; set; }
        public virtual TicketStatuses TicketStatus { get; set; }
        public int TicketStatusId { get; set; }
        public virtual TicketPriorities TicketPriority { get; set; }
        public int TicketPriorityId { get; set; }
        public virtual TicketTypes TicketType { get; set; }
        public int TicketTypeId { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public string CreatedById { get; set; }

        public virtual ApplicationUser AssignedTo { get; set; }
        public string AssignedToId { get; set; }

        public virtual List<Comment> Comments { get; set; }
        public virtual List<TicketFile> Files { get; set; }

        public Tickets()
        {
            Comments = new List<Comment>();
            Files = new List<TicketFile>();
        }
    }
}