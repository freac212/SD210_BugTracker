using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class ListTicketsViewModel
    {
        public List<TicketViewModel> AssignedTickets { get; set; }
        public List<TicketViewModel> CreatedTickets { get; set; }
        public List<TicketViewModel> AllTickets { get; set; }
    }
}