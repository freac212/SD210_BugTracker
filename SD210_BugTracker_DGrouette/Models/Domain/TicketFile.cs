using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public class TicketFile
    {
        public int Id { get; set; }
        public string MediaUrl { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }


        public ApplicationUser User { get; set; }
        public string UserId { get; set; }

        public Ticket Ticket { get; set; }
        public int TicketId { get; set; }
    }
}