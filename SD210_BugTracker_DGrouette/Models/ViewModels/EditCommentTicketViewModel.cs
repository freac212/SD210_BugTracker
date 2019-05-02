using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class EditCommentTicketViewModel
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        [Required]
        public string Comment { get; set; }
        
    }
}