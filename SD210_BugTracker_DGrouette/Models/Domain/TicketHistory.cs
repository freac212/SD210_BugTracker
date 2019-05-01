using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public class TicketHistory
    {
        public int Id { get; set; }

        public virtual ApplicationUser User { get; set; }
        public int UserId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public int TicketId { get; set; }

        public DateTime DateUpdated { get; set; }
        public virtual List<TicketHistoryDetails> TicketHistoryDetails { get; set; }

        public TicketHistory()
        {
            TicketHistoryDetails = new List<TicketHistoryDetails>();
        }
    }
}