using SD210_BugTracker_DGrouette.Models.Domain;
using SD210_BugTracker_DGrouette.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class TicketViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public string ProjectTitle { get; set; }
        public int ProjectId { get; set; }
        public TicketStatuses TicketStatus { get; set; }
        public TicketPriorities TicketPriority { get; set; }
        public TicketTypes TicketType { get; set; }

        public string CreatedByDisplayName { get; set; }
        public string CreatedById { get; set; }

        public string AssignedToDisplayName { get; set; }
        public string AssignedToId { get; set; }

        public bool? CanEdit { get; set; }
        public bool? CanComment { get; set; }
        public bool? CanAddFile { get; set; }


        public List<CommentTicketViewModel> Comments { get; set; }
        public List<FileTicketViewModel> Files { get; set; }
        public List<TicketHistoryViewModel> Histories { get; set; }
    }
}