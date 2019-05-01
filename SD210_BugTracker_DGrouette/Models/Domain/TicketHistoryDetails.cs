namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public class TicketHistoryDetails
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public TicketHistory TicketHistory { get; set; }
        public int TicketHistoryId { get; set; }

    }
}