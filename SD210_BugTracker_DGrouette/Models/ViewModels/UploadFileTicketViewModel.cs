using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.ViewModels
{
    public class UploadFileTicketViewModel
    {
        public int TicketId { get; set; }

        // File/ Image uploading
        public HttpPostedFileBase Media { get; set; }
    }
}