﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public class Comment
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string CommentData { get; set; }


        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public int TicketId { get; set; }
    }
}