using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public class ProjectConstants
    {
        public const string AdminRole = "Admin";
        public const string ManagerRole = "ProjectManager";
        public const string DeveloperRole = "Developer";
        public const string SubmitterRole = "Submitter";

        public const string TicketStatusOpen = "Open";
        public const string TicketStatusResolved = "Resolved";
        public const string TicketStatusRejected = "Rejected";

        public const string TicketPriorityLow = "Low";
        public const string TicketPriorityMedium = "Medium";
        public const string TicketPriorityHigh = "High";

        public const string TicketTypeBug = "Bug";
        public const string TicketTypeFeature = "Feature";
        public const string TicketTypeDataBase = "DataBase";
        public const string TicketTypeSupport = "Support";

    }
}