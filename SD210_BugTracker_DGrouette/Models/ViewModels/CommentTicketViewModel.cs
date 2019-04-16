using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class CommentTicketViewModel
    {
        public string Comment { get; set; }
        public string CreatorDisplayName { get; set; }
        public DateTime DateCreated { get; set; }

    }
}