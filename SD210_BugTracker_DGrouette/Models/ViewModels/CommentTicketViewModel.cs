using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class CommentTicketViewModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public string CreatorDisplayName { get; set; }
        public string CreatorId { get; set; }
        public bool CanDeleteComment { get; set; }
        public DateTime DateCreated { get; set; }

    }
}