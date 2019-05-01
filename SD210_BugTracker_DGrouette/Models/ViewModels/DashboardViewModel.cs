namespace SD210_BugTracker_DGrouette.Models
{
    public class DashboardViewModel
    {
        public int ProjectCount { get; set; }
        public int TicketCount { get; set; }
        public int? OpenTicketCount { get; set; }
        public int? ResolvedTicketCount { get; set; }
        public int? RejectedTicketCount { get; set; }
    }
}