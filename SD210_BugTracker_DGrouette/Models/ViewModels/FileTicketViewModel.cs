using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.ViewModels
{
    public class FileTicketViewModel
    {
        public string MediaUrl { get; set; }
        public string MediaTitle { get; set; }
        public string MediaFileName { get; set; }
        public bool CanDeleteFile { get; set; }
        public int Id { get; set; }
    }
}