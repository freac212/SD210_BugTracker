using SD210_BugTracker_DGrouette.Models.Domain;
using System;
using System.Collections.Generic;

namespace SD210_BugTracker_DGrouette.Models
{
    public class TicketHistoryViewModel
    {
        public string UserDisplayName { get; set; }
        public DateTime DateUpdated { get; set; }
        public List<TicketHistoryDetailsViewModel> HistoryDetails { get; set; }
    }
}